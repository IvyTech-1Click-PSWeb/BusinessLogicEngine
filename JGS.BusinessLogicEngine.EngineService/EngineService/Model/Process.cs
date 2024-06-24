using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.DAL;
using JGS.BusinessLogicEngine.Support;
using JGS.Shared.Package;
using System.Xml;
using JGS.Shared;
using System.Configuration;

namespace JGS.BusinessLogicEngine.Model
{
	internal class Process
	{
		private static string _connectionString = ConfigurationManager
			.ConnectionStrings["OracleConnectionString"].ConnectionString;
		private static string _schemaAndPackage = ConfigurationManager
			.AppSettings["BusinessLogicEngineSchemaAndPackage"];

		private static List<Process> _cache = new List<Process>();

		public static int CacheDuration
		{
			get;
			set;
		}

		static Process()
		{
			CacheDuration = 6;
		}

		public long Id
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}
		public long ProcessTypeId
		{
			get;
			set;
		}
		public string ProcessTypeName
		{
			get;
			set;
		}
		public long LocationId
		{
			get;
			set;
		}
		public long ClientId
		{
			get;
			set;
		}
		public long ContractId
		{
			get;
			set;
		}
		public long OrderProcessTypeId
		{
			get;
			set;
		}
		public long WorkcenterId
		{
			get;
			set;
		}
		public int IsInactive
		{
			get;
			set;
		}
		public long FieldGroupId
		{
			get;
			set;
		}

		private List<Rule> _rules = new List<Rule>();
		internal List<Rule> Rules
		{
			get
			{
				return _rules;
			}
			set
			{
				_rules = value;
			}
		}

		private List<Field> _fields = new List<Field>();
		internal List<Field> Fields
		{
			get
			{
				return _fields;
			}
			set
			{
				_fields = value;
			}
		}

		private List<Step> _steps = new List<Step>();
		internal List<Step> Steps
		{
			get
			{
				return _steps;
			}
			set
			{
				_steps = value;
			}
		}

		private DateTime _expiration = DateTime.Now.AddHours(CacheDuration);
		protected DateTime Expiration
		{
			get
			{
				return _expiration;
			}
		}

		public static Process GetProcess(long processId)
		{
			Process cachedProcess = GetProcessFromCache(processId);
			if (cachedProcess != null)
			{
				return cachedProcess;
			}

			List<OracleParameter> parameters = new List<OracleParameter>();
			parameters.Add(new OracleParameter("ID", OracleDbType.Int64,
														processId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

			DataSet ds = ODPNETHelper.ExecuteDataset(_connectionString, CommandType.StoredProcedure,
																			_schemaAndPackage + ".GetProcessById", parameters.ToArray());
			if (ds.Tables.Count != 1 || ds.Tables[0].Rows.Count != 1)
			{
				return null;
			}
			return CreateProcess(ds);
		}

		public static Process GetProcess(string processTypeName, long locationId, long clientId,
			long contractId, long orderProcessTypeId, long workcenterId)
		{
			Process cachedProcess = GetProcessFromCache(
				processTypeName, locationId, clientId, contractId, orderProcessTypeId, workcenterId);
			if (cachedProcess != null)
			{
				return cachedProcess;
			}

			List<OracleParameter> parameters = new List<OracleParameter>();
			parameters.Add(new OracleParameter("processTypeName", OracleDbType.Varchar2,
														processTypeName, ParameterDirection.Input));
			parameters.Add(new OracleParameter("locationId", OracleDbType.Int64, locationId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("clientId", OracleDbType.Int64, clientId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("contractId", OracleDbType.Int64, contractId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("orderProcessTypeId", OracleDbType.Int64,
														orderProcessTypeId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("workcenterId", OracleDbType.Int64,
														workcenterId, ParameterDirection.Input));
			parameters.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

			DataSet ds = ODPNETHelper.ExecuteDataset(_connectionString,
																			CommandType.StoredProcedure,
																			_schemaAndPackage + ".GetProcess", parameters.ToArray());
			if (ds.Tables.Count != 1 || ds.Tables[0].Rows.Count != 1)
			{
				return null;
			}
			return CreateProcess(ds);
		}

		private static Process GetProcessFromCache(long processId)
		{
			if ((from Process p in _cache
				 where p.Id == processId
				 select p).Count() > 0)
			{
				Process cachedProcess = (from Process p in _cache
										 where p.Id == processId
										 select p).First();
				if (cachedProcess.Expiration > DateTime.Now)
				{
					return cachedProcess;
				}
				_cache.Remove(cachedProcess);
			}
			return null;
		}

		private static Process GetProcessFromCache(
			string processTypeName, long locationId, long clientId,
			long contractId, long orderProcessTypeId, long workcenterId)
		{
			if ((from Process p in _cache
				 where p.ProcessTypeName == processTypeName
				 && p.LocationId == locationId
				 && p.ClientId == clientId
				 && p.ContractId == contractId
				 && p.OrderProcessTypeId == orderProcessTypeId
				 && p.WorkcenterId == workcenterId
				 select p).Count() > 0)
			{
				Process cachedProcess = (from Process p in _cache
										 where p.ProcessTypeName == processTypeName
										 && p.LocationId == locationId
										 && p.ClientId == clientId
										 && p.ContractId == contractId
										 && p.OrderProcessTypeId == orderProcessTypeId
										 && p.WorkcenterId == workcenterId
										 select p).First();
				if (cachedProcess.Expiration > DateTime.Now)
				{
					return cachedProcess;
				}
				_cache.Remove(cachedProcess);
			}
			return null;
		}

		private static Process CreateProcess(DataSet ds)
		{
			Process newProcess = new Process();

			newProcess.Id = ds.Tables[0].Rows[0].GetItem<long>("PROCESS_ID");
			newProcess.Name = ds.Tables[0].Rows[0].GetItem<String>("PROCESS_NAME");

			newProcess.LocationId = ds.Tables[0].Rows[0].GetItem<long>("LOCATION_ID");
			newProcess.ClientId = ds.Tables[0].Rows[0].GetItem<long>("CLIENT_ID");
			newProcess.ContractId = ds.Tables[0].Rows[0].GetItem<long>("CONTRACT_ID");
			newProcess.OrderProcessTypeId = ds.Tables[0].Rows[0].GetItem<long>("ORDER_PROCESS_TYPE_ID");
			newProcess.WorkcenterId = ds.Tables[0].Rows[0].GetItem<long>("WORKCENTER_ID");
			newProcess.ProcessTypeName = ds.Tables[0].Rows[0].GetItem<String>("PROCESS_TYPE_NAME");
			newProcess.FieldGroupId = ds.Tables[0].Rows[0].GetItem<long>("FIELD_GROUP_ID");
			newProcess.IsInactive = ds.Tables[0].Rows[0].GetItem<int>("INACTIVE_IND");

			newProcess.Steps = Step.GetSteps(newProcess.Id);
			newProcess.Rules = Rule.GetRules(newProcess.Id);
			newProcess.Fields = Field.GetFields(newProcess.Id);

			if (CacheDuration != 0)
			{
				_cache.Add(newProcess);
			}

			return newProcess;
		}

		public Package Execute(Package package)
		{
			if (package.TransactionInformation.Status == TransactionStatus.New)
			{
				package.TransactionInformation.DistributedIdentifier = Guid.NewGuid();
				package.TraceEvent("Beginning transaction " + package.TransactionInformation.DistributedIdentifier.ToString(), PackageTraceLevel.Info);
				package.TransactionInformation.Status = TransactionStatus.Active;
			}
			else if (package.TransactionInformation.Status == TransactionStatus.Aborted)
			{
				package.TraceEvent("Process " + this.Name + " was skipped because the transaction has been aborted", PackageTraceLevel.Warning);
				return package;
			}
			else if (package.TransactionInformation.Status == TransactionStatus.InDoubt)
			{
				package.TraceEvent("Unable to execute process " + this.Name + " because the transaction status is in doubt", PackageTraceLevel.Warning);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return package;
			}

			package = Execute(package, package.TransactionInformation.DistributedIdentifier);

			if (package.TransactionInformation.Status == TransactionStatus.Active)
			{
				package.TraceEvent("Commiting transaction " + package.TransactionInformation.DistributedIdentifier.ToString(), PackageTraceLevel.Info);
				package.TransactionInformation.Status = TransactionStatus.Committed;
			}
			else if (package.TransactionInformation.Status == TransactionStatus.Aborted)
			{
				package.TraceEvent("Unable to roll back transaction " + package.TransactionInformation.DistributedIdentifier.ToString(), PackageTraceLevel.Warning);
			}
			else if (package.TransactionInformation.Status == TransactionStatus.InDoubt)
			{
				package.TraceEvent("The final state of the transaction " + package.TransactionInformation.DistributedIdentifier.ToString() + " is unknown:",
					PackageTraceLevel.Warning);
			}

			return package;
		}

		private Package Execute(Package package, Guid transactionId)
		{
			if (!ContinueTransaction(package, "Process " + this.Name + " was skipped", PackageTraceLevel.Warning))
			{
				return package;
			}

			package.TraceEvent("Executing process " + this.Name + " on Transaction " + package.TransactionInformation.DistributedIdentifier.ToString()
				, PackageTraceLevel.Info);

			ExecuteFetch(package);
			if (!ContinueTransaction(package, "Process " + this.Name + " was stopped", PackageTraceLevel.Warning))
			{
				return package;
			}

			RuleResult ruleResult = Rules[0].Evaluate(package, Fields);
			if (!ContinueTransaction(package, "Process " + this.Name + " was stopped", PackageTraceLevel.Warning))
			{
				return package;
			}

			if (ruleResult.IsContinue)
			{
				package = ExecuteSteps(package, ruleResult.IsPass);
			}
			else
			{
				package.TraceEvent("STOP action called, aborting transaction", PackageTraceLevel.Info);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
			}

			if (!ContinueTransaction(package, "Process " + this.Name + " was stopped", PackageTraceLevel.Warning))
			{
				return package;
			}

			return package;
		}

		private Package ExecuteSteps(Package package, bool isPass)
		{
			Step nextStep = GetNextStep(Id, (isPass == false ? 0 : 1));
			while (nextStep != null && package.TransactionInformation.Status == TransactionStatus.Active)
			{
				Process nextProc = GetProcess(nextStep.StepProcessId);
				package.TraceEvent("Executing step " + nextProc.Name, PackageTraceLevel.Verbose);
				package = nextProc.Execute(package, package.TransactionInformation.DistributedIdentifier);
				if (!ContinueTransaction(package, "Process " + this.Name + " was stopped at step " + nextProc.Name, PackageTraceLevel.Warning))
				{
					return package;
				}

				nextStep = GetNextStep(nextStep.StepProcessId, (isPass == false ? 0 : 1));
			}
			return package;
		}

		private Step GetNextStep(long stepId, int isPass)
		{
			return (
				from p in Steps
				where p.ParentStepProcessId == stepId && p.IsPassStep == isPass
				select p).DefaultIfEmpty(null).First();
		}

		private void ExecuteFetch(Package package)
		{
			package.TraceEvent("Process " + this.Name + " beginning fetch", PackageTraceLevel.Verbose);
			try
			{
				foreach (Field field in (from p in this.Fields
										 orderby p.DbOrder
										 select p))
				{
					if (field.IsCollection == 1)
					{
						DbHelper.ExecuteDbFetchCollection(package.Document, field, this.Fields);
					}
					else
					{
						DbHelper.ExecuteDbFetch(package.Document, field, this.Fields);
					}
				}
			}
			catch (Exception ex)
			{
				package.TraceEvent("An exception occurred during fetch: " + ex.Message, PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			package.TraceEvent("Process " + this.Name + " completed fetch", PackageTraceLevel.Verbose);
		}

		private static bool ContinueTransaction(Package package, string stopMessage, PackageTraceLevel eventTraceLevel)
		{
			if (package.TransactionInformation.Status == TransactionStatus.Aborted || package.TransactionInformation.Status == TransactionStatus.InDoubt)
			{
				package.TraceEvent(stopMessage + " (The Transaction is " + package.TransactionInformation.Status.ToString() + ")", eventTraceLevel);
				return false;
			}
			return true;
		}

		internal static void FlushCache()
		{
			_cache.Clear();
		}
	}
}
