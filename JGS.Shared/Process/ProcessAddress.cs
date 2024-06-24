using System;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://corporate.jabil.org/Shared/Process", ClrNamespace = "JGS.Shared.Process")]
namespace JGS.Shared.Process
{
	/// <summary>
	/// The address information for delivery to a Business Logic Process
	/// </summary>
	[DataContract(Namespace="http://corporate.jabil.org/Shared/Process",IsReference=false),Serializable]
	public class ProcessAddress
	{
		/// <summary>
		/// The Location ID of the destination Process
		/// </summary>
		/// <value>An RnR Location ID</value>
		[DataMember]
		public long LocationId
		{
			get;
			set;
		}

		/// <summary>
		/// The Client ID of the destination process
		/// </summary>
		/// <value>An RnR Client ID</value>
		[DataMember]
		public long ClientId
		{
			get;
			set;
		}

		/// <summary>
		/// The Contract ID of the destination process
		/// </summary>
		/// <value>An RnR Contract ID</value>
		[DataMember]
		public long ContractId
		{
			get;
			set;
		}

		/// <summary>
		/// The OPT ID of the destination process.
		/// </summary>
		/// <value>An RnR Order Process Type ID</value>
		[DataMember]
		public long OrderProcessTypeId
		{
			get;
			set;
		}

		/// <summary>
		/// The Workcenter ID of the destination process
		/// </summary>
		/// <value>An RnR Workcenter ID</value>
		[DataMember]
		public long WorkcenterId
		{
			get;
			set;
		}

		/// <summary>
		/// The type name of the destination process.
		/// </summary>
		/// <value>A Process Type Name</value>
		[DataMember]
		public string ProcessTypeName
		{
			get;
			set;
		}

		/// <summary>
		/// The ID of the destination process.
		/// </summary>
		/// <value>A process ID</value>
		[DataMember]
		public long ProcessId
		{
			get;
			set;
		}

		/// <summary>
		/// The Client Name of the destination process
		/// </summary>
		[DataMember]
		public string ClientName
		{
			get;
			set;
		}

		/// <summary>
		/// The Contract Name of the destination process
		/// </summary>
		[DataMember]
		public string ContractName
		{
			get;
			set;
		}

		/// <summary>
		/// The Name of the destination process
		/// </summary>
		[DataMember]
		public string ProcessName
		{
			get;
			set;
		}

		/// <summary>
		/// The Workcenter Name of the destination process
		/// </summary>
		[DataMember]
		public string WorkcenterName
		{
			get;
			set;
		}

		/// <summary>
		/// The Process Type ID of the destination process
		/// </summary>
		[DataMember]
		public long ProcessTypeId
		{
			get;
			set;
		}

		/// <summary>
		/// The Order Process Type Name of the destination process
		/// </summary>
		[DataMember]
		public string OrderProcessTypeName
		{
			get;
			set;
		}

		/// <summary>
		/// The Location Name of the destination process
		/// </summary>
		[DataMember]
		public string LocationName
		{
			get;
			set;
		}
	}
}
