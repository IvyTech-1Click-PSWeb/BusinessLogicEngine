using System;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://corporate.jabil.org/Shared/Package", ClrNamespace = "JGS.Shared.Package")]
namespace JGS.Shared.Package
{
	/// <summary>
	/// The delivery routing information for a package.
	/// </summary>
	[DataContract(Namespace = "http://corporate.jabil.org/Shared/Package",IsReference=false),Serializable]
	public class PackageAddress
	{
		/// <summary>
		/// The identity of the iniator of the process.
		/// </summary>
		/// <value>The Guid of a Requestor</value>
		[DataMember]
		public Guid RequestorGuid
		{
			get;
			set;
		}

		/// <summary>
		/// The identity of the Dispatcher responsible for receipt from and delivery to the clerk.
		/// </summary>
		/// <value>The Guid of a Dispatcher</value>
		[DataMember]
		public Guid DispatcherGuid
		{
			get;
			set;
		}

		/// <summary>
		/// The routing identity of the Clerk handling receipt and delivery of this package.
		/// </summary>
		/// <value>The Guid of the Clerk</value>
		[DataMember]
		public Guid ClerkGuid
		{
			get;
			set;
		}
	}
}
