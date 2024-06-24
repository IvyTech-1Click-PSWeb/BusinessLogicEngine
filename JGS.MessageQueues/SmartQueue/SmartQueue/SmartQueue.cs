using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.Transactions;
using System.Runtime.Serialization;
using JGS.MessageQueues.Support;

namespace JGS.MessageQueues.SmartQueue
{
	#region Enums
	public enum QueuePlatform
	{
		InMemoryQueue,
		MicrosoftMessageQueue,
		JavaMessageServiceQueue,
		OracleAdvancedQueue
	};
	#endregion

	/// <summary>
	/// Generic wrapper class with public properties and methods
	/// </summary>
	/// <remarks>
	/// This class requires a two phase construction and initialization. 
	/// The Smartqueue must be constructed using the Type of the object that the queue will contain. 
	/// Then the InitializeQueue method must be called with the queuePlatform, and if necessary the 
	/// QueuePath passed as parameters. SmartQueue supports 3 different queuePlatforms, an 
	/// InMemoryQueue, a MSMQQueue, and a JMSQueue.
	/// </remarks>
	public class SmartQueue<T>
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

		#region Constructors

		public SmartQueue(QueuePlatform queuePlatform, string queuePath)
		{
			this.InitializeQueue(queuePlatform, queuePath);
		}

		public SmartQueue(QueuePlatform queuePlatform, string queuePath, string Connectionstring)
		{
			this.InitializeQueue(queuePlatform, queuePath, Connectionstring);
		}

		public SmartQueue(string queuePlatform, string queuePath)
		{
			QueuePlatform platform = (QueuePlatform)Enum.Parse(typeof(QueuePlatform), queuePlatform);
			InitializeQueue(platform, queuePath);
		}

		public SmartQueue(string queuePlatform, string queuePath, string connectionString)
		{
			QueuePlatform platform = (QueuePlatform)Enum.Parse(typeof(QueuePlatform), queuePlatform);
			InitializeQueue(platform, queuePath, connectionString);
		}

		#endregion



		#region Private
		/// <summary>
		/// internal Queue
		/// </summary>
		/// <remarks>This private queue implements the IInternalQueue interface.</remarks>
		private IInternalQueue<T> _internalQueue = null;

		private QueuePlatform _queuePlatform = QueuePlatform.InMemoryQueue;

		private bool _isQueueInitialized = false;

		#endregion

		#region Properties
		/// <summary>
		/// A property designating the network path to a durable queue.
		/// </summary>
		public string QueuePath
		{
			get
			{
				if(_internalQueue == null) { throw new ApplicationException(@"The Queue has not been initialized"); }
				return _internalQueue.QueuePath;
			}
			set
			{
				if(_internalQueue == null) { throw new ApplicationException(@"The Queue has not been initialized"); }
				_internalQueue.QueuePath = value;
			}
		}

		/// <summary>
		/// A property determining if the queue will persist across SmartQueue instances.
		/// </summary>
		public bool IsDurable
		{
			get { if(_queuePlatform == QueuePlatform.InMemoryQueue) { return false; } else { return true; } }
		}

		/// <summary>
		/// A property determining if the Queue is ready for use.
		/// </summary>
		/// <remarks>The SmartQueue is properly initialized by calling the InitializeQueue method.</remarks>
		public bool IsQueueInitialized
		{
			get { return _isQueueInitialized; }

		}

		/// <summary>
		/// A property designating which queue platform is wrapped by the SmartQueue
		/// </summary>
		/// <remarks>Queue platforms include an InMemoryQueue, a MSMQQueue, and a JMSQueue</remarks>
		public QueuePlatform QueuePlatform
		{
			get { return _queuePlatform; }
			set { _queuePlatform = value; }
		}
		#endregion

		#region Methods

		/// <summary>
		/// Initializes the SmartQueue
		/// </summary>
		/// <remarks>This method must be successfully run before any other method will work.</remarks>
		public void InitializeQueue(QueuePlatform queuePlatform, string queuePath)
		{
			InitializeQueue(queuePlatform, queuePath, string.Empty);
		}

		/// <summary>
		/// Initializes the SmartQueue
		/// </summary>
		/// <remarks>This method must be successfully run before any other method will work.</remarks>
		public void InitializeQueue(QueuePlatform queuePlatform, string queuePath, string connectionString)
		{
			if(_isQueueInitialized) { throw new System.MethodAccessException(@"The queue is already initialized."); }

			try
			{
				switch(queuePlatform)
				{
					case QueuePlatform.MicrosoftMessageQueue:
						_internalQueue = new MSMQQueue<T>(queuePath, connectionString);
						break;
					case QueuePlatform.InMemoryQueue:
						_internalQueue = new InMemoryQueue<T>(queuePath, connectionString);
						break;
					case QueuePlatform.JavaMessageServiceQueue:
						_internalQueue = new JMSQueue<T>(queuePath, connectionString);
						break;
					case QueuePlatform.OracleAdvancedQueue:
						_internalQueue = new OAQueue<T>(queuePath, connectionString);
						break;
				}
				_internalQueue.MessageReady += new EventHandler(_internalQueue_MessageReady);
				_isQueueInitialized = true;
			}
			catch(MessageQueueException mqx)
			{
				throw new ApplicationException("Message queue initialization failed", mqx);
			}
			catch(ArgumentException ax)
			{
				throw new ApplicationException("Invalid message queue path", ax);
			}
		}

		void _internalQueue_MessageReady(object sender, EventArgs e)
		{
			OnMessageReady(e);
		}

		/// <summary>
		/// Places an object of type T into the SmartQueue.
		/// </summary>
		/// <remarks>The object, and any child objects contained within, must each have a blank, public constructor.</remarks>
		public void Send(T obj)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			_internalQueue.Send(obj);
		}

		public void Send(T obj, string correlationId)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			_internalQueue.Send(obj, correlationId);
		}

		/// <summary>
		/// Gets the next object in the queue.
		/// </summary>
		/// <remarks>Does remove first message from the queue.</remarks>
		/// <returns>Returns an object of type T representing the first object in the queue.</returns>
		public T Receive()
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return (T)_internalQueue.Receive();
		}

		public T Receive(string correlationId)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.Receive(correlationId);
		}

		/// <summary>
		/// Gets the next object in the queue.
		/// </summary>
		/// <remarks>Does not remove said message from the queue.</remarks>
		/// <returns>Returns an object of type T representing the first object in the queue.</returns>
		public T Peek()
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.Peek();
		}

		public T Peek(string correlationId)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.Peek(correlationId);
		}

		/// <summary>
		/// Gets a snapshot of all messages in the queue.
		/// </summary>
		/// <remarks>This method does not remove messages from the queue.</remarks>
		/// <returns>A List of type T representing all queue objects.</returns>
		public List<T> PeekAllMessages()
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.PeekAllMessages();
		}

		public List<T> PeekAllMessages(string correlationId)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.PeekAllMessages(correlationId);
		}

		/// <summary>
		/// Gets a list of the contents of the Queue
		/// </summary>
		/// <remarks>This method removes the messages from the Queue.</remarks>
		/// <returns>A List of type T containing objects contained in the Body of each queue Message</returns>
		public List<T> ReceiveAll()
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.ReceiveAll();
		}

		public List<T> ReceiveAll(string correlationId)
		{
			if(!IsQueueInitialized) { throw new ApplicationException(@"The Queue has not been initialized"); }
			return _internalQueue.ReceiveAll(correlationId);
		}

		/// <summary>
		/// Returns the current count of objects in the SmartQueue
		/// </summary>
		public Int32 QueueCount
		{ get { return _internalQueue.QueueCount; } }

		#endregion
	}
}
