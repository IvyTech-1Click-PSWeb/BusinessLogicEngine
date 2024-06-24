using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Shared.Package;
using JGS.BusinessLogicEngine.Support;
using JGS.Shared;
using System.Configuration;
using System.Threading;

namespace JGS.BusinessLogicEngine
{
	public class Dispatcher
	{
		private Guid _guid = Guid.NewGuid();
		public Guid Guid { get { return _guid; } }

		public string Dispatch(string xmlString)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlString);

			return Dispatch(xmlDocument).InnerXml;
		}

		public XmlElement Dispatch(XmlElement xmlElement)
		{
			XmlDocument document = new XmlDocument();
			document.LoadXmlElement(xmlElement);

			return Dispatch(document).DocumentElement;
		}

		private XmlDocument Dispatch(XmlDocument document)
		{
			return Dispatch(new Package(document)).UnPack();
		}

		public Package Dispatch(Package package)
		{
			FillRoute(package);
			if (package.TransactionInformation.Status == TransactionStatus.Aborted)
			{
				return package;
			}

			Model.Process process;

			try
			{
				process = Model.Process.GetProcess(
					package.DestinationAddress.ProcessTypeName,
					package.DestinationAddress.LocationId,
					package.DestinationAddress.ClientId,
					package.DestinationAddress.ContractId,
					package.DestinationAddress.OrderProcessTypeId,
					package.DestinationAddress.WorkcenterId);
			}
			catch (Exception ex)
			{
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				package.TransactionInformation.Messages.Add("An exception occurred loading the process definition: " + ex.Message);
				return package;
			}

			if(process == null)
			{
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				package.TransactionInformation.Messages.Add("Unable to locate the process definition");
				return package;
			}

			try
			{
				Package result = process.Execute(package);
				return result;
			}
			catch (Exception ex)
			{
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				package.TransactionInformation.Messages.Add("An exception occurred executing the process: " + ex.Message);
				return package;
			}
		}

		private void FillRoute(Package package)
		{
			if(package.DestinationAddress.ProcessTypeId==0 & string.IsNullOrEmpty(package.DestinationAddress.ProcessTypeName)) {
				package.TransactionInformation.Status=TransactionStatus.Aborted;
				package.TransactionInformation.Messages.Add("No process type was specified");
				return;
			}

			if(
				(package.DestinationAddress.ProcessTypeId==0 | string.IsNullOrEmpty(package.DestinationAddress.ProcessTypeName))
				| (package.DestinationAddress.LocationId==0 | string.IsNullOrEmpty(package.DestinationAddress.LocationName))
				| (package.DestinationAddress.LocationId==0 | string.IsNullOrEmpty(package.DestinationAddress.LocationName))
				| (package.DestinationAddress.LocationId==0 | string.IsNullOrEmpty(package.DestinationAddress.LocationName))
				| (package.DestinationAddress.LocationId==0 | string.IsNullOrEmpty(package.DestinationAddress.LocationName))
				| (package.DestinationAddress.LocationId==0 | string.IsNullOrEmpty(package.DestinationAddress.LocationName))
				) {
					long processTypeId = package.DestinationAddress.ProcessTypeId;
					string processTypeName = package.DestinationAddress.ProcessTypeName;
					long locationId = package.DestinationAddress.LocationId;
					string locationName = package.DestinationAddress.LocationName;
					long clientId = package.DestinationAddress.ClientId;
					string clientName = package.DestinationAddress.ClientName;
					long contractId = package.DestinationAddress.ContractId;
					string contractName = package.DestinationAddress.ContractName;
					long orderProcessTypeId = package.DestinationAddress.OrderProcessTypeId;
					string orderProcessTypeName = package.DestinationAddress.OrderProcessTypeName;
					long workcenterId = package.DestinationAddress.WorkcenterId;
					string workcenterName = package.DestinationAddress.WorkcenterName;

					if (DbHelper.GetProcessRoute(
						ref processTypeId, ref processTypeName,
						ref locationId, ref locationName,
						ref clientId, ref clientName,
						ref contractId, ref contractName,
						ref orderProcessTypeId, ref orderProcessTypeName,
						ref workcenterId, ref workcenterName
						))
					{
						package.DestinationAddress.ProcessTypeId = processTypeId;
						package.DestinationAddress.ProcessTypeName = processTypeName;
						package.DestinationAddress.LocationId = locationId;
						package.DestinationAddress.LocationName = locationName;
						package.DestinationAddress.ClientId = clientId;
						package.DestinationAddress.ClientName = clientName;
						package.DestinationAddress.ContractId = contractId;
						package.DestinationAddress.ContractName = contractName;
						package.DestinationAddress.OrderProcessTypeId = orderProcessTypeId;
						package.DestinationAddress.OrderProcessTypeName = orderProcessTypeName;
						package.DestinationAddress.WorkcenterId = workcenterId;
						package.DestinationAddress.WorkcenterName = workcenterName;
					}
                    else
                    {
                        package.TransactionInformation.Status = TransactionStatus.Aborted;
                        package.TransactionInformation.Messages.Add("The process route is invalid");
                    }
			}
		}

		public void FlushCache()
		{
			Model.Process.FlushCache();
		}
	}
}
