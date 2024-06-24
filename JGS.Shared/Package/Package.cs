using System;
using System.Runtime.Serialization;
using JGS.Shared.Process;
using System.Xml;
using JGS.Shared.Validation;
using JGS.Shared;
using System.Globalization;

[assembly: ContractNamespace("http://corporate.jabil.org/Shared/Package", ClrNamespace = "JGS.Shared.Package")]
namespace JGS.Shared.Package
{
	/// <summary>
	/// A black box package object for use in transfrerring data between components in an application chain.
	/// </summary>

	[DataContract(Namespace = "http://corporate.jabil.org/Shared/Package", IsReference = true), Serializable]
	public class Package
	{
		#region Private Methods
		private T GetPackValue<T>(DocumentDefinition definition, XmlDocument document, string fieldName)
			where T : IConvertible
		{
			if (definition.GetField(fieldName) != null)
			{
				return document.GetValue<T>(definition.GetField(fieldName).XPath);
			}
			return default(T);
		}

		private void SetDocumentFromPayload(string value)
		{
			try
			{
				_document = new XmlDocument();
				_document.LoadXml(_payload);
				_payload = _document.OuterXml;
			}
			catch
			{
				_document = null;
				_payload = value;
			}
		}
		#endregion

		#region Properties 
		private Guid _packageGuid = Guid.NewGuid();
		/// <summary>
		/// The GUID of this package.
		/// </summary>
		/// <remarks>This GUID is automatically assigned at package creation and should never be changed.</remarks>
		/// <value>A GUID generated at package creation.</value>
		[DataMember]
		public Guid PackageGuid
		{
			get
			{
				return _packageGuid;
			}
			set
			{
				_packageGuid = value;
			}
		}

		/// <summary>
		/// The address for routing the package back to the requestor.
		/// </summary>
		/// <value>A Package Address structure to be filled in at each step of the routing.</value>
		[DataMember]
		public PackageAddress SourceAddress
		{
			get;
			set;
		}

		/// <summary>
		/// The information for delivery to the destination process.
		/// </summary>
		[DataMember]
		public ProcessAddress DestinationAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Retrieves additional information about a the transaction this package is participating in.
		/// </summary>
		/// <value>A TransactionInformation that contains additional information about the transaction.</value>
		[DataMember]
		public TransactionInformation TransactionInformation
		{
			get;
			set;
		}

		XmlDocument _document = null;
		string _payload = null;
		/// <summary>
		/// The XML to be passed to the process.
		/// </summary>
		/// <value>Implementation specific.</value>
		[DataMember]
		public string Payload
		{
			get
			{
				if (_document != null)
				{
					return _document.OuterXml;
				}
				return _payload;
			}
			set
			{
				SetDocumentFromPayload(value);
			}
		}

		/// <summary>
		/// The XmlDocument represented by the payload
		/// </summary>
		public XmlDocument Document
		{
			get
			{
				//In case the document was not recreated after deserialization, rebuild it
				if(_document==null && !string.IsNullOrEmpty(_payload)) {
					SetDocumentFromPayload(_payload);
				}
				return _document;
			}
			set
			{
				_document = value;
				_payload = _document.OuterXml;
			}
		}

		private XmlNode _currentNode = null;
		/// <summary>
		/// The node currently selected for use
		/// </summary>
		public XmlNode CurrentNode
		{
			get
			{
				if (_currentNode == null)
				{
					return Document;
				}
				else
				{
					return _currentNode;
				}
			}
			set { _currentNode=value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public Package()
		{
			SourceAddress = new PackageAddress();
			DestinationAddress = new ProcessAddress();
			TransactionInformation = new TransactionInformation();
		}

		/// <summary>
		/// Create a fully prevalidated package from a document and build the destination address from the fields of the document
		/// </summary>
		/// <param name="document">An XmlDocument to package</param>
		public Package(XmlDocument document)
		{
			Pack(document);
		}
		#endregion

		#region Puclic Methods
		/// <summary>
		/// Ensures that internal objects are created on deserialization, if necessary
		/// </summary>
		/// <param name="context">The StreamingContext</param>
		[OnDeserializing]
		public void OnDeserializing(StreamingContext context)
		{
			if(SourceAddress == null) { SourceAddress = new PackageAddress(); }
			if(DestinationAddress == null) { DestinationAddress = new ProcessAddress(); }
			if(TransactionInformation == null) { TransactionInformation = new TransactionInformation(); }
		}

		/// <summary>
		/// Converts an XML document into the package payload
		/// </summary>
		/// <param name="document">The XML document to pack</param>
		public void Pack(XmlDocument document)
		{
			if (!document.IsValid())
			{
				throw new FormatException(document.Validate());
			}
			SourceAddress = new PackageAddress();
			DestinationAddress = new ProcessAddress();
			TransactionInformation = new TransactionInformation();

			DocumentDefinition definition = Validator.GetDocumentDefinition(document.DocumentElement.LocalName);

			DestinationAddress.ProcessTypeId = GetPackValue<long>(definition, document, "ProcessTypeId");
			DestinationAddress.LocationId = GetPackValue<long>(definition, document, "LocationId");
			DestinationAddress.ClientId = GetPackValue<long>(definition, document, "ClientId");
			DestinationAddress.ContractId = GetPackValue<long>(definition, document, "ContractId");
			DestinationAddress.OrderProcessTypeId = GetPackValue<long>(definition, document, "OrderProcessTypeId");
			DestinationAddress.WorkcenterId = GetPackValue<long>(definition, document, "WorkcenterId");

			DestinationAddress.ProcessTypeName = GetPackValue<string>(definition, document, "ProcessTypeName");
			DestinationAddress.LocationName = GetPackValue<string>(definition, document, "LocationName");
			DestinationAddress.ClientName = GetPackValue<string>(definition, document, "ClientName");
			DestinationAddress.ContractName = GetPackValue<string>(definition, document, "ContractName");
			DestinationAddress.OrderProcessTypeName = GetPackValue<string>(definition, document, "OrderProcessTypeName");
			DestinationAddress.WorkcenterName = GetPackValue<string>(definition, document, "WorkcenterName");

			Payload = document.InnerXml;
		}

		/// <summary>
		/// Unpacks the XmlDocument from the Payload and adds the TransactionInformation
		/// </summary>
		/// <returns>The XmlDocument carried in this package as Payload with the Transaction Information injected</returns>
		public XmlDocument UnPack()
		{
			XmlDocument document = new XmlDocument();
			document.LoadXml(this.Payload);

			string rootElementName = document.DocumentElement.LocalName;
			string transactionXPath = rootElementName + @"/TransactionInformation";
			string creationXPath = transactionXPath + @"/CreationTime";
			string distributedXPath = transactionXPath + @"/DistributedIdentifier";
			string statusXPath = transactionXPath + @"/Status";
			string messagesXPath = transactionXPath + @"/Messages";
			string messageXPath = @"Message";

			document.SetValue(creationXPath, TransactionInformation.CreationTime.ToUniversalTime()
				.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", DateTimeFormatInfo.InvariantInfo));
			document.SetValue(distributedXPath, TransactionInformation.DistributedIdentifier.ToString());
			document.SetValue(statusXPath, TransactionInformation.Status.ToString());

			XmlNode newNode = document.CreateXPathNode(messagesXPath);
			foreach(string message in TransactionInformation.Messages)
			{
				XmlNode newChild = document.CreateElement(messageXPath);
				newChild.InnerText = message;
				newNode.AppendChild(newChild);
			}

			return document;
		}
		#endregion
	}
}
