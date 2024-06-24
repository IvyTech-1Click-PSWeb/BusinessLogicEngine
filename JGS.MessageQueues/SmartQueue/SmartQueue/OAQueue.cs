using System;
using System.Collections.Generic;
using JGS.MessageQueues.Support;
using Oracle.DataAccess.Client;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;
using Oracle.DataAccess.Types;
using System.Linq;
using System.Data;

namespace JGS.MessageQueues.SmartQueue
{
	internal class OAQueue<T> : IInternalQueue<T>
		where T : new()
	{
		#region Events
		private EventHandler _messageReady;
		private readonly Lock _messageReadyLock = new Lock();
		public event EventHandler MessageReady
		{
			add
			{
				lock(_messageReadyLock)
				{
					_messageReady += value;
				}
			}
			remove
			{
				lock(_messageReadyLock)
				{
					_messageReady -= value;
				}
			}
		}

		private void OnMessageReady(EventArgs e)
		{
			EventHandler handler;
			lock(_messageReadyLock)
			{
				handler = _messageReady;
			}
			if(handler != null)
				handler(this, e);
		}
		#endregion

		#region Private members

		private Lock _connectionLock = new Lock();
		private OracleConnection _connection = new OracleConnection();
		private OracleAQQueue _queue = null;
		private XmlSerializer _serializer = new XmlSerializer(typeof(T));

		#endregion

		#region Constructors

		public OAQueue(string queuePath, string connectionString)
		{
			lock(_connectionLock)
			{
				_connection.ConnectionString = connectionString;
				CheckConnection();

				_queue = new OracleAQQueue(queuePath, _connection);
				_queue.MessageType = OracleAQMessageType.Xml;
				_queue.EnqueueOptions.DeliveryMode = OracleAQMessageDeliveryMode.Persistent;
				_queue.EnqueueOptions.Visibility = OracleAQVisibilityMode.OnCommit;
				_queue.DequeueOptions.DeliveryMode = OracleAQMessageDeliveryMode.Persistent;
				_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Browse;
				_queue.DequeueOptions.NavigationMode = OracleAQNavigationMode.FirstMessageMultiGroup;
				_queue.DequeueOptions.Visibility = OracleAQVisibilityMode.OnCommit;
				_queue.DequeueOptions.ProviderSpecificType = true;
				_queue.DequeueOptions.Wait = 0;

				_queue.MessageAvailable += new OracleAQMessageAvailableEventHandler(_queue_MessageAvailable);
			}
		}

		~OAQueue()
		{
			if(_connection.State == ConnectionState.Open) { _connection.Close(); }
		}

		void _queue_MessageAvailable(object sender, OracleAQMessageAvailableEventArgs e)
		{
			OnMessageReady(new EventArgs());
		}

		#endregion

		#region IInternalQueue<T> Members


		public string QueuePath
		{
			get
			{
				lock(_connectionLock)
				{
					return _queue.Name;
				}
			}
			set
			{
				lock(_connectionLock)
				{
					_queue = new OracleAQQueue(value, _connection);
				}
			}
		}

		public string Connectionstring
		{
			get
			{
				lock(_connectionLock)
				{
					return _connection.ConnectionString;
				}
			}
			set
			{
				if(_connection.State != ConnectionState.Closed)
				{
					throw new InvalidOperationException(@"The connection string of the queue cannot be changed while the queue is in use.");
				}
				lock(_connectionLock)
				{
					_connection.ConnectionString = value;
				}
			}
		}

		public int QueueCount
		{
			get
			{
				lock(_connectionLock)
				{
					CheckConnection();
					_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Browse;
					_queue.DequeueOptions.Correlation = null;

					using(OracleTransaction _txn = _connection.BeginTransaction())
					{

						try
						{
							OracleAQMessage[] msgs = _queue.DequeueArray(Int32.MaxValue);
							_txn.Commit();
							return msgs.Count();
						}
						catch(OracleException oex)
						{
							if(oex.ErrorCode != -2147467259)
							{
								throw (oex);
							}
							return 0;
						}
						catch(Exception ex)
						{
							_txn.Rollback();
							throw (ex);
						}
					}
				}
			}
		}

		public T Peek()
		{
			return Peek(null);
		}

		public T Peek(string correlationId)
		{
			OracleAQMessage msg;
			lock(_connectionLock)
			{
				CheckConnection();
				_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Browse;
				_queue.DequeueOptions.Correlation = correlationId;

				using(OracleTransaction _txn = _connection.BeginTransaction())
				{
					try
					{
						msg = _queue.Dequeue();
						if(msg != null)
						{
							OracleXmlType payload = (OracleXmlType)msg.Payload;
							T item = (T)_serializer.Deserialize(payload.GetXmlReader());
							_txn.Commit();
							return item;
						}
						else
						{
							_txn.Commit();
							return default(T);
						}
					}
					catch(OracleException oex)
					{
						if(oex.ErrorCode != -2147467259)
						{
							throw (oex);
						}
						return default(T);
					}
					catch(Exception ex)
					{
						_txn.Rollback();
						throw (ex);
					}
				}
			}

		}

		private void CheckConnection()
		{
			if(_connection.State != ConnectionState.Open) { _connection.Open(); }
		}

		public List<T> PeekAllMessages()
		{
			return PeekAllMessages(null);
		}

		public List<T> PeekAllMessages(string correlationId)
		{
			OracleAQMessage[] msgs = null;
			lock(_connectionLock)
			{
				CheckConnection();
				_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Browse;
				_queue.DequeueOptions.Correlation = correlationId;

				using(OracleTransaction _txn = _connection.BeginTransaction())
				{
					try
					{
						msgs = _queue.DequeueArray(8000);

						List<T> items = new List<T>();
						foreach(OracleAQMessage msg in msgs)
						{
							OracleXmlType payload = (OracleXmlType)msg.Payload;
							T item = (T)_serializer.Deserialize(payload.GetXmlReader());
							items.Add(item);
						}
						_txn.Commit();
						return items;
					}
					catch(OracleException oex)
					{
						if(oex.ErrorCode != -2147467259)
						{
							throw (oex);
						}
						return new List<T>();
					}
					catch(Exception ex)
					{
						_txn.Rollback();
						throw (ex);
					}
				}
			}
		}

		public T Receive()
		{
			return Receive(null);
		}

		public T Receive(string correlationId)
		{
			OracleAQMessage msg;
			lock(_connectionLock)
			{
				CheckConnection();
				_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Remove;
				_queue.DequeueOptions.Correlation = correlationId;

				using(OracleTransaction _txn = _connection.BeginTransaction())
				{
					try
					{
						msg = _queue.Dequeue();
						if(msg != null)
						{
							OracleXmlType payload = (OracleXmlType)msg.Payload;
							T item = (T)_serializer.Deserialize(payload.GetXmlReader());
							_txn.Commit();
							return item;
						}
						else
						{
							return default(T);
						}
					}
					catch(OracleException oex)
					{
						if(oex.ErrorCode != -2147467259)
						{
							throw (oex);
						}
						return default(T);
					}
					catch(Exception ex)
					{
						_txn.Rollback();
						throw (ex);
					}
				}
			}
		}

		public List<T> ReceiveAll()
		{
			return ReceiveAll(null);
		}

		public List<T> ReceiveAll(string correlationId)
		{
			lock(_connectionLock)
			{
				CheckConnection();
				_queue.DequeueOptions.DequeueMode = OracleAQDequeueMode.Remove;
				_queue.DequeueOptions.Correlation = correlationId;

				using(OracleTransaction _txn = _connection.BeginTransaction())
				{
					try
					{
						OracleAQMessage[] msgs = _queue.DequeueArray(8000);
						List<T> items = new List<T>();
						foreach(OracleAQMessage msg in msgs)
						{
							OracleXmlType payload = (OracleXmlType)msg.Payload;
							T item = (T)_serializer.Deserialize(payload.GetXmlReader());
							items.Add(item);
						}
						_txn.Commit();
						return items;
					}
					catch(OracleException oex)
					{
						if(oex.ErrorCode != -2147467259)
						{
							throw (oex);
						}
						return new List<T>();
					}
					catch(Exception ex)
					{
						_txn.Rollback();
						throw (ex);
					}
				}
			}

		}

		public void Send(T obj)
		{
			this.Send(obj, null);
		}

		public void Send(T obj, string correlationId)
		{
			StringWriter sw = new StringWriter();
			_serializer.Serialize(sw, obj);

			lock(_connectionLock)
			{
				CheckConnection();
				using(OracleTransaction _txn = _connection.BeginTransaction())
				{
					try
					{
						OracleXmlType payload = new OracleXmlType(_connection, sw.ToString());
						OracleAQMessage msg = new OracleAQMessage(payload);
						msg.Correlation = correlationId;
						_queue.Enqueue(msg);
						_txn.Commit();
					}
					catch(Exception ex)
					{
						_txn.Rollback();
						throw (ex);
					}
				}
			}
		}

		#endregion
	}
}