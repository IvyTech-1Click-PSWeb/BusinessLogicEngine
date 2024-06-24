using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.DAL;
using JGS.BusinessLogicEngine.Support;
using JGS.Shared;
using System.Configuration;

namespace JGS.BusinessLogicEngine.Model
{
	internal class Step
	{
		private static string _connectionString = ConfigurationManager.
			ConnectionStrings["OracleConnectionString"].ConnectionString;
		private static string _schemaAndPackage = ConfigurationManager.AppSettings["BusinessLogicEngineSchemaAndPackage"];

		public long ProcessId { get; set; }
		public long StepProcessId { get; set; }
		public long ParentStepProcessId { get; set; }
		public int IsPassStep { get; set; }

		public static List<Step> GetSteps(long processId)
		{
			List<Step> newSteps = new List<Step>();

			List<OracleParameter> myParams = new List<OracleParameter>();
			myParams.Add(new OracleParameter("ID", OracleDbType.Int64, processId, ParameterDirection.Input));
			myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

			DataSet ds = ODPNETHelper.ExecuteDataset(_connectionString,
																			CommandType.StoredProcedure,
																			_schemaAndPackage + ".GetProcessStepsByProcessId", myParams.ToArray());
			if(ds.Tables.Count != 1)
			{
				throw new DataException("The database returned an invalid process step list");
			}

			foreach(DataRow row in ds.Tables[0].Rows)
			{
				Step newStep = new Step();

				newStep.ProcessId = processId;
				newStep.StepProcessId = row.GetItem<long>("STEP_PROCESS_ID");
				newStep.ParentStepProcessId = row.GetItem<long>("PARENT_STEP_PROCESS_ID");
				newStep.IsPassStep = row.GetItem<int>("PASS_FLAG");
				newSteps.Add(newStep);
			}

			return newSteps;
		}
	}
}
