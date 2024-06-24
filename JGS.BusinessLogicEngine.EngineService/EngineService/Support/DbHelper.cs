using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.DAL;
using System.Data;
using Oracle.DataAccess.Client;
using JGS.BusinessLogicEngine.Model;
using System.Xml;
using System.Configuration;
using JGS.Shared;

namespace JGS.BusinessLogicEngine.Support
{
    internal static class DbHelper
    {
        private static string _connectionString = ConfigurationManager.
            ConnectionStrings["OracleConnectionString"].ConnectionString;
        private static string _schemaAndPackage = ConfigurationManager
            .AppSettings["BusinessLogicEngineSchemaAndPackage"];

        /// <summary>
        /// Executes the specified Scalar command and fetches the return value
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="connectionString">The connection string to the data store</param>
        /// <param name="commandText">The command to execute</param>
        /// <param name="commandType">The command type to execute</param>
        /// <param name="parameters">The parameters to pass to the command. 
        /// The return value parameter will be inserted into this collection.</param>
        /// <param name="dbType">The type of the return parameter</param>
        /// <returns>The scalar value fetched from the data store</returns>
        public static T DbFetch<T>(string commandType, string commandText,
            List<OracleParameter> parameters, string dbType)
            where T : IConvertible
        {
            return DbFetch<T>(GetCommandType(commandType), commandText, parameters, GetDbType(dbType));
        }

        /// <summary>
        /// Executes the specified Scalar command and fetches the return value
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="connectionString">The connection string to the data store</param>
        /// <param name="commandText">The command to execute</param>
        /// <param name="commandType">The command type to execute</param>
        /// <param name="parameters">The parameters to pass to the command. 
        /// The return value parameter will be inserted into this collection.</param>
        /// <param name="dbType">The type of the return parameter</param>
        /// <returns>The scalar value fetched from the data store</returns>
        public static T DbFetch<T>(CommandType commandType, string commandText,
            List<OracleParameter> parameters, OracleDbType dbType)
            where T : IConvertible
        {
            parameters.Insert(0,
                new OracleParameter("returnValue", dbType, ParameterDirection.ReturnValue));
            if (MapDbType(dbType) == typeof(string))
            {
                parameters[0].Size = 8000;
            }

            ODPNETHelper.ExecuteScalar(_connectionString, commandType,
                commandText, parameters.ToArray());

            if (parameters[0].Value.ToString().ToUpper() == "NULL")
            {
                return default(T);
            }

            return (T)parameters[0].Value.ToString().ChangeTypeTo<T>();
        }

        /// <summary>
        /// Converts the specified command type name into the CommandType enum
        /// </summary>
        /// <param name="commandType">The string representation of the System.Data.CommandType</param>
        /// <returns>The System.Data.CommandType enum value</returns>
        public static CommandType GetCommandType(string commandType)
        {
            return (CommandType)Enum.Parse(typeof(CommandType), commandType);
        }

        /// <summary>
        /// Converts the specified oracle db type name into the OracleDbType enum
        /// </summary>
        /// <param name="dbTypeName">The oracle db type name</param>
        /// <returns>The Oracle.DataAccess.Client.OracleDbType enum value</returns>
        public static OracleDbType GetDbType(string dbTypeName)
        {
            if (string.IsNullOrEmpty(dbTypeName))
            {
                return OracleDbType.NVarchar2;
            }
            return (OracleDbType)Enum.Parse(typeof(OracleDbType), dbTypeName);
        }

        /// <summary>
        /// Returns the .Net type for the specified Oracle DB Type
        /// </summary>
        /// <param name="dbType">The Oracle Data Type</param>
        /// <returns>A .Net Type</returns>
        public static Type MapDbType(string dbType)
        {
            return MapDbType(GetDbType(dbType));
        }

        /// <summary>
        /// Returns the .Net type for the specified Oracle DB Type
        /// </summary>
        /// <param name="dbType">The Oracle Data Type</param>
        /// <returns>A .Net Type</returns>
        public static Type MapDbType(OracleDbType dbType)
        {
            switch (dbType)
            {
                case OracleDbType.BFile:
                case OracleDbType.Blob:
                case OracleDbType.LongRaw:
                case OracleDbType.Raw:
                    return typeof(byte[]);
                case OracleDbType.Char:
                case OracleDbType.Clob:
                case OracleDbType.Long:
                case OracleDbType.NClob:
                case OracleDbType.NChar:
                case OracleDbType.NVarchar2:
                case OracleDbType.Varchar2:
                case OracleDbType.XmlType:
                    return typeof(string);
                case OracleDbType.Date:
                case OracleDbType.TimeStamp:
                case OracleDbType.TimeStampLTZ:
                case OracleDbType.TimeStampTZ:
                    return typeof(DateTime);
                case OracleDbType.IntervalDS:
                    return typeof(TimeSpan);
                case OracleDbType.IntervalYM:
                case OracleDbType.Int64:
                    return typeof(long);
                case OracleDbType.Int16:
                    return typeof(short);
                case OracleDbType.Int32:
                    return typeof(int);
                case OracleDbType.Byte:
                    return typeof(byte);
                case OracleDbType.Decimal:
                    return typeof(decimal);
                case OracleDbType.Single:
                    return typeof(Single);
                case OracleDbType.Double:
                    return typeof(double);
                default:
                    throw new NotImplementedException(dbType.ToString() + " is not yet implemented");
            }
        }

        public static void ExecuteDbFetch(XmlNode workingXml, Field field, List<Field> fields)
        {

            if (!string.IsNullOrEmpty(field.DbCommand)
                && !string.IsNullOrEmpty(field.DbCommandType)
                && !string.IsNullOrEmpty(field.DbDataType))
            {
                List<OracleParameter> parameters = Parser.ParseDatabaseParameters(
                    field.DbParams, fields, workingXml);
                ExecuteDbFetch(workingXml, field, fields,
                    field.DbCommandType, field.DbCommand, parameters);
            }
        }

        public static void ExecuteDbFetch(XmlNode document, Field field,
            List<Field> fields, string dbCommandType, string dbCommand, string parametersString)
        {
            List<OracleParameter> parameters = Parser.ParseDatabaseParameters(
                    parametersString, fields, document);
            ExecuteDbFetch(document, field, fields,
                    dbCommandType, dbCommand, parameters);
        }

        public static void ExecuteDbFetch(XmlNode document, Field field,
            List<Field> fields, string dbCommandType, string dbCommand, List<OracleParameter> parameters)
        {
            string retVal = DbHelper.DbFetch<string>(
                dbCommandType, dbCommand, parameters, field.DbDataType);
            document.SetValue(field.XPath, retVal);
        }

        public static void ExecuteDbFetchCollection(XmlNode document, Field field, List<Field> fields)
        {
            if (!string.IsNullOrEmpty(field.DbCommand)
                && !string.IsNullOrEmpty(field.DbCommandType))
            {
                List<OracleParameter> parameters = Parser.ParseDatabaseParameters(
                    field.DbParams, fields, document);
                parameters.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

                ExecuteDbFetchCollection(document, field, fields, field.DbCommandType, field.DbCommand, parameters);
            }
        }

        public static void ExecuteDbFetchCollection(XmlNode document, Field field,
            List<Field> fields, string dbCommandType, string dbCommand, string parametersString)
        {
            List<OracleParameter> parameters = Parser.ParseDatabaseParameters(
                    parametersString, fields, document);
            parameters.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
            ExecuteDbFetchCollection(document, field, fields, dbCommandType, dbCommand, parameters);
        }

        public static void ExecuteDbFetchCollection(XmlNode document, Field field,
            List<Field> fields, string dbCommandType, string dbCommand, List<OracleParameter> parameters)
        {
            DataSet retDs = ODPNETHelper.ExecuteDataset(_connectionString
                , DbHelper.GetCommandType(dbCommandType)
                , dbCommand,
                parameters.ToArray());

            //Empty dataset, we're done here
            if (retDs.Tables.Count != 1 || retDs.Tables[0].Rows.Count == 0)
            {
                return;
            }

            //The base XPath is that of the parent field, each chile will be written relative to that path
            Dictionary<string, string> dataMap = new Dictionary<string, string>();
            foreach (DataColumn column in retDs.Tables[0].Columns)
            {
                if ((from p in fields
                     where p.Name == column.ColumnName
                     select p).Count() == 1)
                {
                    dataMap.Add(column.ColumnName,
                        (from p in fields
                         where p.Name == column.ColumnName
                         select p).First().XPath);
                }
            }

            foreach (DataRow row in retDs.Tables[0].Rows)
            {
                XmlNode newNode = document.CreateXPathNode(field.XPath);
                foreach (KeyValuePair<string, string> children in dataMap)
                {
                    XmlNode newChild = (document.OwnerDocument == null ? (XmlDocument)document : document.OwnerDocument).CreateElement(children.Key);
                    newChild.InnerText = row[children.Key].ToString();
                    newNode.AppendChild(newChild);
                }
            }
        }

        public static void ExecuteDbDirect(XmlNode document, List<Field> fields, string dbCommandType,
            string dbCommand, string parametersString)
        {
            List<OracleParameter> parameters = Parser.ParseDatabaseParameters(
                    parametersString, fields, document);
            ExecuteDbDirect(document, fields, dbCommandType, dbCommand, parameters);
        }

        public static void ExecuteDbDirect(XmlNode document,
            List<Field> fields, string dbCommandType, string dbCommand, List<OracleParameter> parameters)
        {
            ODPNETHelper.ExecuteNonQuery(_connectionString
                , DbHelper.GetCommandType(dbCommandType)
                , dbCommand,
                parameters.ToArray());
        }


        private static Dictionary<long, string> _processTypes = new Dictionary<long, string>();
        private static Dictionary<long, string> _locations = new Dictionary<long, string>();
        private static Dictionary<long, string> _clients = new Dictionary<long, string>();
        private static Dictionary<long, string> _contracts = new Dictionary<long, string>();
        private static Dictionary<long, string> _orderProcessTypes = new Dictionary<long, string>();
        private static Dictionary<long, string> _workcenters = new Dictionary<long, string>();
        public static bool GetProcessRoute(
            ref long processTypeId, ref string processTypeName,
            ref long locationId, ref string locationName,
            ref long clientId, ref string clientName,
            ref long contractId, ref string contractName,
            ref long orderProcessTypeId, ref string orderProcessTypeName,
            ref long workcenterId, ref string workcenterName)
        {
            bool routeFound = false;

            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref processTypeId, ref processTypeName);
            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref locationId, ref locationName);
            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref clientId, ref clientName);
            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref contractId, ref contractName);
            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref orderProcessTypeId, ref orderProcessTypeName);
            routeFound &= GetProcessRouteElementFromCache(_processTypes, ref workcenterId, ref workcenterName);

            if (!routeFound)
            {
                routeFound = GetProcessRouteElementsFromDb(
                    ref processTypeId, ref processTypeName,
                    ref locationId, ref locationName,
                    ref clientId, ref clientName,
                    ref contractId, ref contractName,
                    ref orderProcessTypeId, ref orderProcessTypeName,
                    ref workcenterId, ref  workcenterName);
            }

            return routeFound;
        }

        private static bool GetProcessRouteElementFromCache(Dictionary<long, string> cache, ref long id, ref string name)
        {
            if (id == 0 || string.IsNullOrEmpty(name))
            {
                if (id == 0 && string.IsNullOrEmpty(name))
                {
                    //Apply the All routing
                    name = "ALL";
                    return true;
                }
                if (id == 0)
                {
                    string lname = name;
                    id = cache.Where(p => p.Value == lname).FirstOrDefault().Key;
                    return (id != 0);
                }
                if (string.IsNullOrEmpty(name))
                {
                    Int64 lid = id;
                    name = cache.Where(p => p.Key == lid).FirstOrDefault().Value;
                    return (!string.IsNullOrEmpty(name));
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        private static bool GetProcessRouteElementsFromDb(
            ref long processTypeId, ref string processTypeName,
            ref long locationId, ref string locationName,
            ref long clientId, ref string clientName,
            ref long contractId, ref string contractName,
            ref long orderProcessTypeId, ref string orderProcessTypeName,
            ref long workcenterId, ref string workcenterName)
        {
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("ProcessTypeId", OracleDbType.Int64, processTypeId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("ProcessTypeName", OracleDbType.Varchar2, processTypeName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("LocationId", OracleDbType.Int64, locationId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("LocationName", OracleDbType.Varchar2, locationName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("ClientId", OracleDbType.Int64, clientId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("ClientName", OracleDbType.Varchar2, clientName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("ContractId", OracleDbType.Int64, contractId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contractName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("OrderProcessTypeId", OracleDbType.Int64, orderProcessTypeId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("OrderProcessTypeName", OracleDbType.Varchar2, orderProcessTypeName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("WorkcenterId", OracleDbType.Int64, workcenterId, ParameterDirection.Input));
            parameters.Add(new OracleParameter("WorkcenterName", OracleDbType.Varchar2, workcenterName, ParameterDirection.Input));
            parameters.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            DataSet ds = JGS.DAL.ODPNETHelper.ExecuteDataset(
                _connectionString, CommandType.StoredProcedure, _schemaAndPackage + ".GetProcessRoute", parameters.ToArray());

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return false;
            }
            DataRow row = ds.Tables[0].Rows[0];

            processTypeId = long.Parse(row["PROCESS_TYPE_ID"].ToString());
            locationId = long.Parse(row["LOCATION_ID"].ToString());
            clientId = long.Parse(row["CLIENT_ID"].ToString());
            contractId = long.Parse(row["CONTRACT_ID"].ToString());
            orderProcessTypeId = long.Parse(row["ORDER_PROCESS_TYPE_ID"].ToString());
            workcenterId = long.Parse(row["WORKCENTER_ID"].ToString());

            processTypeName = row["PROCESS_TYPE_NAME"].ToString();
            locationName = row["LOCATION_NAME"].ToString();
            clientName = row["CLIENT_NAME"].ToString();
            contractName = row["CONTRACT_NAME"].ToString();
            orderProcessTypeName = row["ORDER_PROCESS_TYPE_NAME"].ToString();
            workcenterName = row["WORKCENTER_NAME"].ToString();

            if (!_processTypes.ContainsKey(long.Parse(row["PROCESS_TYPE_ID"].ToString())))
            {
                _processTypes.Add(long.Parse(row["PROCESS_TYPE_ID"].ToString()), row["PROCESS_TYPE_NAME"].ToString());
            }
            if (!_locations.ContainsKey(long.Parse(row["LOCATION_ID"].ToString())))
            {
                _locations.Add(long.Parse(row["LOCATION_ID"].ToString()), row["LOCATION_NAME"].ToString());
            }
            if (!_clients.ContainsKey(long.Parse(row["CLIENT_ID"].ToString())))
            {
                _clients.Add(long.Parse(row["CLIENT_ID"].ToString()), row["CLIENT_NAME"].ToString());
            }
            if (!_contracts.ContainsKey(long.Parse(row["CONTRACT_ID"].ToString())))
            {
                _contracts.Add(long.Parse(row["CONTRACT_ID"].ToString()), row["CONTRACT_NAME"].ToString());
            }
            if (!_orderProcessTypes.ContainsKey(long.Parse(row["ORDER_PROCESS_TYPE_ID"].ToString())))
            {
                _orderProcessTypes.Add(long.Parse(row["ORDER_PROCESS_TYPE_ID"].ToString()), row["ORDER_PROCESS_TYPE_NAME"].ToString());
            }
            if (!_workcenters.ContainsKey(long.Parse(row["WORKCENTER_ID"].ToString())))
            {
                _workcenters.Add(long.Parse(row["WORKCENTER_ID"].ToString()), row["WORKCENTER_NAME"].ToString());
            }

            return true;

        }
    }
}
