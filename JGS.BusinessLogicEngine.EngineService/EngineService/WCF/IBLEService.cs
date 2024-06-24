using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Shared;
using System.ServiceModel;
using JGS.Shared.Package;

namespace JGS.BusinessLogicEngine.WCF
{
	[ServiceContract(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public interface IBLEService
	{
		[OperationContract]
		void FlushCache();

		[OperationContract]
		bool Send(Package inPackage);

		[OperationContract]
		Package Receive(String inGuid);

		[OperationContract]
		List<Package> ReceiveAll(String inGuid);

		[OperationContract]
		Package Peek(string inGuid);

		[OperationContract]
		List<Package> PeekAll(string inGuid);

		[OperationContract]
		string ExecuteByString(string xmlString);

		[OperationContract]
		XmlElement ExecuteByXmlElement(XmlElement xmlElement);

		[OperationContract]
		Package ExecuteByPackage(Package package);
	}
}
