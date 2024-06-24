using System.Collections.Generic;
using JGS.MessageQueues.SmartQueue;
using JGS.Shared.Package;
using System.Configuration;
using System.ServiceModel;

namespace JGS.BusinessLogicEngine.WCF
{
	/// <summary>
	/// The implementation of the WCF webservice
	/// </summary>
	/// <remarks>This class implements the IProcess and IProcessManagement interfaces.</remarks>
	[ServiceBehavior(Namespace="http://corporate.jabil.org/BusinessLogicEngine")]
	public class BLEService : IBLEService
	{
		private static Dispatcher _dispatcher = new Dispatcher();
		private static string _toProcessQueuePath = 
			ConfigurationManager.AppSettings["BusinessLogicEngineIncomingQueuePath"];
		private static string _fromProcessQueuePath =
			ConfigurationManager.AppSettings["BusinessLogicEngineOutgoingQueuePath"];
		private static string _connectionString = 
			ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
		private static string _queueType =
			ConfigurationManager.AppSettings["BusinessLogicEngineQueueType"];
		private static SmartQueue<Package> _queueToProcess =
			new SmartQueue<Package>(_queueType,_toProcessQueuePath, BLEService._connectionString);

		private static SmartQueue<Package> _queueFromProcess=
			new SmartQueue<Package>(_queueType,_fromProcessQueuePath, BLEService._connectionString);

		public BLEService()
		{

		}

		#region IProcess Members
		public void FlushCache()
		{
			_dispatcher.FlushCache();
		}

		public bool Send(Package inPackage)
		{
			bool result = true;
			if(_queueType.ToUpper() == "INMEMORYQUEUE")
			{
				inPackage = _dispatcher.Dispatch(inPackage);
			}
			try
			{

				_queueToProcess.Send(inPackage,inPackage.SourceAddress.DispatcherGuid.ToString());
			}
			catch
			{
				result = false;
			}
			return result;
		}

		public Package Receive(string inGuid)
		{
			return _queueFromProcess.Receive(inGuid);
		}

		public List<Package> ReceiveAll(string inGuid)
		{
			return _queueFromProcess.ReceiveAll(inGuid);
		}

		public Package Peek(string inGuid)
		{
			return _queueFromProcess.Peek(inGuid);
		}

		public List<Package> PeekAll(string inGuid)
		{
			return _queueFromProcess.PeekAllMessages(inGuid);
		}

		public string ExecuteByString(string xmlString)
		{
			return _dispatcher.Dispatch(xmlString);
		}

		public System.Xml.XmlElement ExecuteByXmlElement(System.Xml.XmlElement xmlElement)
		{
			return _dispatcher.Dispatch(xmlElement);
		}

		public Package ExecuteByPackage(Package package)
		{
			return _dispatcher.Dispatch(package);
		}
		#endregion
	}
}
