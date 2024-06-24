using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

[assembly: ContractNamespace("http://corporate.jabil.org/Shared", ClrNamespace = "JGS.Shared")]
namespace JGS.Shared
{
	/// <summary>
	/// Describes the current status of a distributed transaction.
	/// </summary>
	[DataContract(Namespace = "http://corporate.jabil.org/Shared", IsReference = false), Serializable]
	public enum TransactionStatus
	{
		/// <summary>
		/// The transaction has been created but no action has been taken yet
		/// </summary>
		[EnumMember]
		New,
		/// <summary>
		/// Processing of the transaction is in progress
		/// </summary>
		[EnumMember]
		Active,
		/// <summary>
		/// The transaction has been committed.
		/// </summary>
		[EnumMember]
		Committed,
		/// <summary>
		/// The transaction has been rolled back.
		/// </summary>
		[EnumMember]
		Aborted,
		/// <summary>
		/// The status of the transaction is unknown.
		/// </summary>
		[EnumMember]
		InDoubt
	}

	/// <summary>
	/// Provides additional information regarding a transaction.
	/// </summary>
	[DataContract(Namespace = "http://corporate.jabil.org/Shared", IsReference = false), Serializable]
	public class TransactionInformation
	{
		public TransactionInformation()
		{
			CreationTime = DateTime.Now.ToUniversalTime();
			Status = TransactionStatus.New;
		}

		/// <summary>
		/// Gets the creation time of the transaction.
		/// </summary>
		/// <value>A DateTime that contains the creation time of the transaction.</value>
		[DataMember()]
		public DateTime CreationTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a unique identifier of the escalated transaction.
		/// </summary>
		/// <value>A Guid that contains the unique identifier of the escalated transaction.</value>
		[DataMember()]
		public Guid DistributedIdentifier
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a unique identifier of the transaction.
		/// </summary>
		/// <value>A unique identifier of the transaction.</value>
		[DataMember()]
		public Guid LocalIdentifier
		{
			get;
			set;
		}

		/// <summary>
		/// Describes the current status of a distributed transaction.
		/// </summary>
		/// <value>A TransactionStatus that contains the status of the transaction.</value>
		[DataMember()]
		public TransactionStatus Status
		{
			get;
			set;
		}

		private List<string> _messages = new List<string>();
		/// <summary>
		/// A collection of messages provided by the various steps in processing
		/// </summary>
		[DataMember()]
		public List<string> Messages { get { return _messages; } set { _messages = value; } }
	}
}
