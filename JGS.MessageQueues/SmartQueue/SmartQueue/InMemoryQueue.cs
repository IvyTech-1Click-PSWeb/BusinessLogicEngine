using System;
using System.Collections.Generic;
using System.Linq;
using JGS.MessageQueues.Support;
using System.Runtime.Serialization;

namespace JGS.MessageQueues.SmartQueue
{
	internal class InMemoryQueue<T> : IInternalQueue<T>
		where T :  new() 
	{
		internal class MemoryQueueItem<T>
		{
			public string CorrelationId { get; set; }
			private T _body = default(T);
			public T Body { get; set; }
		}

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


		#region Private Member Variables

		private static Dictionary<string, List<MemoryQueueItem<T>>> _queues = 
			new Dictionary<string, List<MemoryQueueItem<T>>>();

		#endregion

		#region Constructors 

		public InMemoryQueue(string queuePath, string connectionString)
		{
			this.Connectionstring = connectionString;
			this.QueuePath = queuePath;
		}

		#endregion

		#region IInternalQueue Members
		public string Connectionstring { get; set; }
		
		private string _queuePath = string.Empty;
		public string QueuePath
		{
			get
			{
				return _queuePath;
			}
			set
			{
				_queuePath = value;
				if(!_queues.ContainsKey(_queuePath))
				{
					_queues.Add(_queuePath, new List<MemoryQueueItem<T>>());
				}
			}

		}

		Int32 IInternalQueue<T>.QueueCount
		{
			get { return _queues[QueuePath].Count; }
		}

		T IInternalQueue<T>.Peek()
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				MemoryQueueItem<T> returnItem = (from p in _queues[_queuePath] select p).DefaultIfEmpty(null).First();
				return returnItem.Body;
			}
			else
			{
				return default(T);
			}
		}

		T IInternalQueue<T>.Peek(string correlationId)
		{
			if((from p in _queues[_queuePath] where p.CorrelationId == correlationId select p).Count() > 0)
			{
				MemoryQueueItem<T> returnItem = (from p in _queues[_queuePath] where p.CorrelationId==correlationId select p)
					.DefaultIfEmpty(null).First();
				return returnItem.Body;
			}
			else
			{
				return default(T);
			}
		}

		T IInternalQueue<T>.Receive()
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				MemoryQueueItem<T> returnItem = (from p in _queues[_queuePath] select p).DefaultIfEmpty(null).First();
				_queues[_queuePath].Remove(returnItem);
				return returnItem.Body;
			}
			else
			{
				return default(T);
			}
		}

		T IInternalQueue<T>.Receive(string correlationId)
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				MemoryQueueItem<T> returnItem = (from p in _queues[_queuePath] where p.CorrelationId==correlationId select p)
					.DefaultIfEmpty(null).First();
				_queues[_queuePath].Remove(returnItem);
				return returnItem.Body;
			}
			else
			{
				return default(T);
			}
		}

		List<T> IInternalQueue<T>.PeekAllMessages()
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				List<T> returnItems = (from p in _queues[_queuePath] select p.Body).ToList();
				return returnItems;
			}
			else
			{
				return new List<T>();
			}
		}

		List<T> IInternalQueue<T>.PeekAllMessages(string correlationId)
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				List<T> returnItems = (from p in _queues[_queuePath] where p.CorrelationId==correlationId select p.Body).ToList();
				return returnItems;
			}
			else
			{
				return new List<T>();
			}
		}

		List<T> IInternalQueue<T>.ReceiveAll()
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				List<T> returnItems = (from p in _queues[_queuePath] select p.Body).ToList();
				_queues[_queuePath].Clear();				
				return returnItems;
			}
			else
			{
				return new List<T>();
			}
		}

		List<T> IInternalQueue<T>.ReceiveAll(string correlationId)
		{
			if((from p in _queues[_queuePath] select p).Count() > 0)
			{
				List<T> returnItems = (from p in _queues[_queuePath] select p.Body).ToList();
				_queues[_queuePath].RemoveAll(p => p.CorrelationId == correlationId);
				return returnItems;
			}
			else
			{
				return new List<T>();
			}
		}

		void IInternalQueue<T>.Send(T MsgObject)
		{
			_queues[QueuePath].Add(new MemoryQueueItem<T>() { CorrelationId = null, Body = MsgObject });
			OnMessageReady(new EventArgs());	
		}

		void IInternalQueue<T>.Send(T obj, string correlationId)
		{
			_queues[QueuePath].Add(new MemoryQueueItem<T>() { CorrelationId = correlationId, Body = obj });
			OnMessageReady(new EventArgs());
		}

		#endregion
	}
}
