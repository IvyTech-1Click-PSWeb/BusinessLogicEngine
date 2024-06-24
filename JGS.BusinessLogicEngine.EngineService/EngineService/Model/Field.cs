using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.BusinessLogicEngine.Support;
using JGS.DAL;
using JGS.Shared;
using System.Configuration;

namespace JGS.BusinessLogicEngine.Model
{
	public class Field
	{
		private static string _connectionString = ConfigurationManager.
			ConnectionStrings["OracleConnectionString"].ConnectionString;
		private static string _schemaAndPackage = 
			ConfigurationManager.AppSettings["BusinessLogicEngineSchemaAndPackage"];

		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int IsCollection { get; set; }
		public string XPath { get; set; }
		public long DbOrder { get; set; }
		public string DbCommand { get; set; }
		public string DbCommandType { get; set; }
		public string DbParams { get; set; }
		public string DbDataType { get; set; }

		public static List<Field> GetFields(long processId)
		{
			List<Field> newFields = new List<Field>();

         List<OracleParameter> myParams = new List<OracleParameter>() ;
         myParams.Add(new OracleParameter("ID", OracleDbType.Int64, 
                                          processId, ParameterDirection.Input));
         myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
         DataSet ds = ODPNETHelper.ExecuteDataset(_connectionString, CommandType.StoredProcedure, 
                                                         _schemaAndPackage + ".GetProcessFieldsByProcessId", myParams.ToArray());

         if( ds.Tables.Count != 1){
            throw new DataException("The database returned an invalid process field list");
			}
         foreach(DataRow row in ds.Tables[0].Rows) {
            Field newField =new Field();
            
               newField.Id = row.GetItem<long>("FIELD_ID");
               newField.Name = row.GetItem<string>("FIELD_NAME");
               newField.IsCollection = row.GetItem<int>("FIELD_IS_COLLECTION");
               newField.Description = row.GetItem<string>("FIELD_DESC");
               newField.XPath = row.GetItem<string>("FIELD_XPATH");
               newField.DbOrder = row.GetItem<long>("FIELD_DB_ORDER");
               newField.DbCommand = row.GetItem<string>("FIELD_DB_COMMAND");
               newField.DbCommandType = row.GetItem<string>("FIELD_DB_COMMANDTYPE");
               newField.DbParams = row.GetItem<string>("FIELD_DB_PARAMS");
               newField.DbDataType = row.GetItem<string>("FIELD_DB_DATATYPE");
            
            newFields.Add(newField);
         }

			return newFields;
		}

		public OracleParameter BuildParameter(ParameterDirection direction)
		{
			return new OracleParameter(this.Name, DbHelper.GetDbType(this.DbDataType), direction);
		}
		public OracleParameter BuildParameter(ParameterDirection direction, object value)
		{
			OracleParameter newParameter= new OracleParameter(this.Name, DbHelper.GetDbType(this.DbDataType), direction);
			newParameter.Value = value;
			return newParameter;
		}
	}
}
