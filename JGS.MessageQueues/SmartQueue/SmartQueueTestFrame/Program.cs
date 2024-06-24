using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.MessageQueues.SmartQueue;

namespace JGS.MessageQueues.SmartQueueTestFrame
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.CursorVisible = false;

			Console.WriteLine(@"QueueWrapper TestFrame");
			Console.WriteLine();

			TestInMemoryQueue();

			TestMSMQQueue();

			TestOracleAdvancedQueue();

			TestJMSQueue();

			Console.Write("\nPress Enter to continue:");
			Console.Read();
		}

		protected static bool TestInMemoryQueue()
		{
			Console.WriteLine(string.Empty.PadLeft(Console.WindowWidth, '-'));

			bool retvalue = true;
			TestObject1 to1 = new TestObject1();
			to1.TestObj2 = new TestObject2();
			try
			{
				SmartQueue<TestObject1> q = new SmartQueue<TestObject1>(
					JGS.MessageQueues.SmartQueue.QueuePlatform.InMemoryQueue, @"TestInMemoryQueue");

				Console.WriteLine("Testing In Memory Queue");
				Console.WriteLine();

				retvalue |= TestSingle(q);
				retvalue |= TestMultiple(q);
				retvalue |= StressTest(q);
			}
			catch(Exception ex)
			{
				retvalue = false;
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			Console.ForegroundColor = (retvalue ? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine("Test result = " + retvalue.ToString());
			Console.ForegroundColor = ConsoleColor.White;

			return retvalue;
		}

		protected static bool TestMSMQQueue()
		{
			Console.WriteLine(string.Empty.PadLeft(Console.WindowWidth, '-'));

			bool retvalue = true;

			TestObject1 to1 = new TestObject1();
			to1.TestObj2 = new TestObject2();
			to1.TestObj2 = new TestObject2();
			try
			{

				SmartQueue<TestObject1> q = new SmartQueue<TestObject1>(
					JGS.MessageQueues.SmartQueue.QueuePlatform.MicrosoftMessageQueue, @".\Private$\SmartQueue");

				Console.WriteLine("Testing Microsoft Message Queue");

				retvalue |= TestSingle(q);
				retvalue |= TestMultiple(q);
				retvalue |= StressTest(q);


			}
			catch(Exception ex)
			{
				retvalue = false;
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			Console.ForegroundColor = (retvalue ? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine("Test result = " + retvalue.ToString());
			Console.ForegroundColor = ConsoleColor.White;

			return retvalue;
		}

		protected static bool TestJMSQueue()
		{
			Console.WriteLine(string.Empty.PadLeft(Console.WindowWidth, '-'));

			bool retvalue = true;
			try
			{
				throw new NotImplementedException();
			}
			catch(Exception ex)
			{
				retvalue = false;
				Console.WriteLine("false");
				Console.WriteLine();
				Console.WriteLine(ex.Message);
				Console.WriteLine();
				Console.WriteLine(ex.StackTrace);
			}
			return retvalue;
		}
		protected static bool TestOracleAdvancedQueue()
		{
			Console.WriteLine(string.Empty.PadLeft(Console.WindowWidth, '-'));

			bool retvalue = true;

			try
			{
				SmartQueue<TestObject1> q = new SmartQueue<TestObject1>(
					JGS.MessageQueues.SmartQueue.QueuePlatform.OracleAdvancedQueue,
					 @"BISHOPT1.PROCESS_QUEUE_PENDING",
					 "Data Source=RNRSTG;Persist Security Info=True;User ID=WEBADAPTERS1;Password=CXDS1TERS9!@!$;ENLIST=DYNAMIC");

				Console.WriteLine("Testing Oracle Advanced Queue");

				retvalue |= TestSingle(q);
				retvalue |= TestMultiple(q);
				retvalue |= StressTest(q);


			}
			catch(Exception ex)
			{
				retvalue = false;
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			Console.ForegroundColor = (retvalue ? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine("Test result = " + retvalue.ToString());
			Console.ForegroundColor = ConsoleColor.White;

			return retvalue;
		}

		private static bool TestSingle(SmartQueue<TestObject1> q)
		{
			Console.WriteLine("\tTesting single send, peek, and receive");

			TestObject1 to1 = new TestObject1();
			to1.TestObj2 = new TestObject2();
			to1.TestProperty1 = Guid.NewGuid().ToString();
			Console.WriteLine("\t\tSend single\t" + to1.TestProperty1);

			q.Send(to1);

			TestObject1 Peeked = q.Peek();
			Console.WriteLine("\t\tPeek single\t" + Peeked.TestProperty1);

			TestObject1 Received = q.Receive();

			Console.ForegroundColor = ((to1.TestProperty1 == Peeked.TestProperty1 && to1.TestProperty1 == Received.TestProperty1) 
				? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine("\t\tReceive single\t" + Received.TestProperty1);
			Console.ForegroundColor = ConsoleColor.White;

			return (to1.TestProperty1 == Peeked.TestProperty1 && to1.TestProperty1 == Received.TestProperty1);
		}

		private static bool TestMultiple(SmartQueue<TestObject1> q)
		{
			bool retValue = true;
			int sendCount = 10;
			TestObject1 to1 = new TestObject1();
			to1.TestObj2 = new TestObject2();
			to1.TestProperty1 = Guid.NewGuid().ToString();

			Console.WriteLine("\tTesting multiple send, peek, and receive");

			retValue |= SendMultiple(q, sendCount, to1);

			retValue |= PeekMultiple(q, sendCount, to1);

			retValue |= ReceiveMultiple(q, sendCount, to1);

			Console.ForegroundColor = (retValue ? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine(retValue ? "\tPassed" : "\tFailed");
			Console.ForegroundColor = ConsoleColor.White;

			return retValue;
		}

		private static bool StressTest(SmartQueue<TestObject1> q)
		{
			bool retValue = true;
			int sendCount = 1000;
			TestObject1 to1 = new TestObject1();
			to1.TestObj2 = new TestObject2();
			to1.TestProperty1 = Guid.NewGuid().ToString();

			Console.WriteLine("\tStress testing at " + sendCount.ToString());
			DateTime startTime = DateTime.Now;

			retValue |= SendMultiple(q, sendCount, to1);

			retValue |= PeekMultiple(q, sendCount, to1);

			retValue |= ReceiveMultiple(q, sendCount, to1);

			Console.Write("\tRound Trip Time per Message = ");
			Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds / (sendCount * 3));

			Console.ForegroundColor = (retValue ? ConsoleColor.Green : ConsoleColor.Red);
			Console.WriteLine(retValue ? "\tPassed" : "\tFailed");
			Console.ForegroundColor = ConsoleColor.White;

			return retValue;
		}

		private static bool ReceiveMultiple(SmartQueue<TestObject1> q, int numberExpected, TestObject1 to1)
		{
			bool retValue = true;
			try
			{
				List<TestObject1> allGet = q.ReceiveAll();
				Console.WriteLine("\t\tReceived = " + allGet.Count().ToString());
				if(numberExpected != allGet.Count)
				{
					retValue = false;
					Console.WriteLine("\t\tLess messages received than expected");
				}
				if(!VerifyReceived(to1.TestProperty1, allGet))
				{
					retValue = false;
					Console.WriteLine("\t\tReceived messages did not verify");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				retValue = false;
			}
			return retValue;
		}

		private static bool PeekMultiple(SmartQueue<TestObject1> q, int numberExpected, TestObject1 to1)
		{
			bool retValue = true;
			try
			{
				List<TestObject1> allGet = q.PeekAllMessages();
				Console.WriteLine("\t\tPeeked = " + allGet.Count().ToString());
				if(numberExpected != allGet.Count)
				{
					retValue = false;
					Console.WriteLine("\t\tLess messages peeked than expected");
				}
				if(!VerifyReceived(to1.TestProperty1, allGet))
				{
					retValue = false;
					Console.WriteLine("\t\tPeeked messages did not verify");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				retValue = false;
			}
			return retValue;
		}

		private static bool SendMultiple(SmartQueue<TestObject1> q, int sendCount, TestObject1 to1)
		{
			bool retValue = true;

			Console.Write("\t\tSent = ");
			try
			{
				int x, y;
				x = Console.CursorLeft;
				y = Console.CursorTop;
				for(int i = 1; i <= sendCount; i++)
				{
					q.Send(to1);
					Console.SetCursorPosition(x, y);
					Console.Write(i.ToString() + "          ");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine(ex.Message);
				retValue = false;
			}
			Console.WriteLine();
			return retValue;
		}

		private static bool VerifyReceived(string guid, List<TestObject1> messages)
		{
			foreach(TestObject1 to in messages)
			{
				if(to.TestProperty1 != guid) { return false; }
			}
			return true;
		}
	}
}
