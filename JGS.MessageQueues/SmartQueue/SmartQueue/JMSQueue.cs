using System;
using System.Collections.Generic;
using JGS.MessageQueues.Support;
using System.Runtime.Serialization;

namespace JGS.MessageQueues.SmartQueue
{
    public class JMSQueue<T> : IInternalQueue<T>
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

		 public JMSQueue(string queuePath, string connectionString)
		 {
			 this.QueuePath = queuePath;
			 this.Connectionstring = connectionString;
		 }

		 #endregion

		 #region IInternalQueue<T> Members


		 public string QueuePath
		 {
			 get
			 {
				 throw new NotImplementedException();
			 }
			 set
			 {
				 throw new NotImplementedException();
			 }
		 }

		 public string Connectionstring
		 {
			 get
			 {
				 throw new NotImplementedException();
			 }
			 set
			 {
				 throw new NotImplementedException();
			 }
		 }

		 public int QueueCount
		 {
			 get { throw new NotImplementedException(); }
		 }

		 public T Peek()
		 {
			 throw new NotImplementedException();
		 }

		 public T Peek(string correlationId)
		 {
			 throw new NotImplementedException();
		 }

		 public List<T> PeekAllMessages()
		 {
			 throw new NotImplementedException();
		 }

		 public List<T> PeekAllMessages(string correlationId)
		 {
			 throw new NotImplementedException();
		 }

		 public T Receive()
		 {
			 throw new NotImplementedException();
		 }

		 public T Receive(string correlationId)
		 {
			 throw new NotImplementedException();
		 }

		 public List<T> ReceiveAll()
		 {
			 throw new NotImplementedException();
		 }

		 public List<T> ReceiveAll(string correlationId)
		 {
			 throw new NotImplementedException();
		 }

		 public void Send(T Obj)
		 {
			 throw new NotImplementedException();
		 }

		 public void Send(T obj, string correlationId)
		 {
			 throw new NotImplementedException();
		 }

		 #endregion
	 }
}
