using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JGS.MessageQueues.SmartQueue
{
	

	/// <summary>
	/// Internal Queue interface
	/// </summary>
	/// <remarks>3 classes implement this interface, InMemoryQueue, MSMQQueue, or JMSQueue.</remarks>
	internal interface IInternalQueue<T>
		where T :  new() 
	{
		event EventHandler MessageReady;

		string QueuePath { get; set; }
		string Connectionstring { get; set; }
		Int32 QueueCount { get; }

		T Peek();

		T Peek(string correlationId);

		List<T> PeekAllMessages();

		List<T> PeekAllMessages(string correlationId);

		T Receive();

		T Receive(string correlationId);

		List<T> ReceiveAll();

		List<T> ReceiveAll(string correlationId);

		void Send(T Obj);

		void Send(T obj, string correlationId);
	}
}
