using System;
using System.Collections.Generic;
using System.Messaging;
using System.Transactions;
using JGS.MessageQueues.Support;
using System.Runtime.Serialization;


namespace JGS.MessageQueues.SmartQueue
{
	internal class MSMQQueue<T> : IInternalQueue<T>
		where T :  new() 
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

		#region Private Members

		private MessageQueue TheQueue = new MessageQueue();

		#endregion
		
		#region Constructors
		
		public MSMQQueue(string queuePath, string connectionString)
		{
			this.Connectionstring = connectionString;
			this.QueuePath = queuePath;
			InitializeQueue();
		}

		#endregion

		#region IInternalQueue Members
		public string Connectionstring { get; set; }
		public string QueuePath { get; set; }

		T IInternalQueue<T>.Peek()
		{
			return (T)TheQueue.Peek().Body;
		}

		T IInternalQueue<T>.Peek(string correlationId)
		{
			return (T)TheQueue.PeekByCorrelationId(correlationId).Body;
		}

		List<T> IInternalQueue<T>.PeekAllMessages()
		{
			Message[] msgs = TheQueue.GetAllMessages();
			List<T> AllObject = new List<T>();
			foreach(Message m in msgs)
			{
				AllObject.Add((T)m.Body);
			}
			return AllObject;
		}

		List<T> IInternalQueue<T>.PeekAllMessages(string correlationId)
		{
			Message[] msgs = TheQueue.GetAllMessages();
			List<T> AllObject = new List<T>();
			foreach(Message m in msgs)
			{
				if(m.CorrelationId == correlationId)
				{
					AllObject.Add((T)m.Body);
				}
			}
			return AllObject;
		}

		T IInternalQueue<T>.Receive()
		{
			return (T)TheQueue.Receive().Body;
		}

		T IInternalQueue<T>.Receive(string correlationId)
		{
			return (T)TheQueue.ReceiveByCorrelationId(correlationId).Body;
		}

		List<T> IInternalQueue<T>.ReceiveAll()
		{
			List<T> AllObject = new List<T>();
			TimeSpan timeout = new TimeSpan(0, 0, 0);
			try
			{
				while(true)
				{ AllObject.Add((T)TheQueue.Receive(timeout).Body); }
			}
			catch(MessageQueueException)
			{ }

			return AllObject;
		}

		List<T> IInternalQueue<T>.ReceiveAll(string correlationId)
		{
			List<T> AllObject = new List<T>();
			TimeSpan timeout = new TimeSpan(0, 0, 0);
			try
			{
				while(true)
				{ AllObject.Add((T)TheQueue.ReceiveByCorrelationId(correlationId, timeout).Body); }
			}
			catch(MessageQueueException)
			{ }

			return AllObject;
		}

		void IInternalQueue<T>.Send(T obj)
		{
			Message msg = new Message(obj);

			if(TheQueue.Transactional == true)
			{
				// Create a transaction.
				MessageQueueTransaction myTransaction = new MessageQueueTransaction();

				myTransaction.Begin();
				try
				{
					TheQueue.Send(msg, myTransaction);
				}
				catch(Exception ex)
				{
					myTransaction.Abort();
					throw new ApplicationException("Transaction Aborted", ex);
				}

				myTransaction.Commit();
			}
			else
			{
				TheQueue.Send(msg);
			}

		}

		void IInternalQueue<T>.Send(T obj, string correlationId)
		{
			Message msg = new Message(obj);
			msg.CorrelationId = correlationId;

			if(TheQueue.Transactional == true)
			{
				// Create a transaction.
				MessageQueueTransaction myTransaction = new MessageQueueTransaction();

				myTransaction.Begin();
				try
				{
					TheQueue.Send(msg, myTransaction);
				}
				catch(Exception ex)
				{
					myTransaction.Abort();
					throw new ApplicationException("Transaction Aborted", ex);
				}

				myTransaction.Commit();
			}
			else
			{
				TheQueue.Send(msg);
			}
		}

		Int32 IInternalQueue<T>.QueueCount
		{
			get
			{
				if(TheQueue.Transactional)
				{
					int count = 0;
					using(TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
					{
						MessageEnumerator enumerator = TheQueue.GetMessageEnumerator2();
						while(enumerator.MoveNext(new TimeSpan(0, 0, 0)))
						{
							count++;
						}
					}

					return count;
				}
				else
				{
					int count = 0;
					using(TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
					{
						MessageEnumerator enumerator = TheQueue.GetMessageEnumerator2();
						while(enumerator.MoveNext(new TimeSpan(0, 0, 0)))
						{
							count++;
						}
					}

					return count;
				}
			}
		}
		#endregion

		private void InitializeQueue()
		{
			//TODO MSMQQueue.InitializeQueue()
			if(MessageQueue.Exists(QueuePath))
			{
				TheQueue.Path = QueuePath;
			}
			else
			{
				TheQueue = MessageQueue.Create(QueuePath, true);

			}
			TheQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
		}

	}
}
