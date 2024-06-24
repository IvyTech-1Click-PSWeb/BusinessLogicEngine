using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using JGS.Shared.Package;
using System.Data;
using Oracle.DataAccess.Client;
using JGS.DAL;
using System.Web.Configuration;

namespace JGS.BusinessLogicEngine.WCF
{
	[ServiceBehavior(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public class FieldOneClickService : IFieldOneClickService
	{
		private BLEService _service = new BLEService();

		#region IFieldOneClickService Members

		public bool Send(Package inPackage)
		{
			return _service.Send(inPackage);
		}

		public Package Receive(string inGuid)
		{
			return _service.Receive(inGuid);
		}

		public List<Package> ReceiveAll(string inGuid)
		{
			return _service.ReceiveAll(inGuid);
		}

		public string ExecuteDirect(string xmlString)
		{
			return _service.ExecuteByString(xmlString);
		}

        public DataSet ValidateTesterName(string TesterName)
        {

            List<OracleParameter> myParams = new List<OracleParameter>();
            DataSet DS = new DataSet();
            try
            {
                myParams.Add(new OracleParameter("TesterName", OracleDbType.Varchar2, TesterName, ParameterDirection.Input));                
                myParams.Add(new OracleParameter("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output));
                DS = ODPNETHelper.ExecuteDataset(WebConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString, CommandType.StoredProcedure, "WEBADAPTERS1.F1C_TESTER_LOG.ValidateTesterName", myParams.ToArray());

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                myParams = null;
            }
            return DS;
        }

        public string InsertTesterLog(string TesterName, string Part_NO, string ITEM_SERIAl_NO, string TEST_RESULT, string XML_DATA_CONTENT, string FILE_PATH,
                                    string DETAIL_DATA_TABLE, string ELAPSED_TIME, string MANUFACTURER, string PRODUCT_NAME, string USER_NAME, string USER_ID,
                                    string SCRIPT_NAME, string STATIONMA, string OSTYPE, string OSVERSION, string FIRMWAREVERSION, string TESTERWVERSION,
                                    string OEMINSTALL, string KERNELLVERSION, string FIXED_ASSET_TAG, string START_DATE, string END_DATE)
        {
            List<OracleParameter> myParams = new List<OracleParameter>();
            string strResult = string.Empty;
            try
            {
                myParams.Add(new OracleParameter("TesterName", OracleDbType.Varchar2, TesterName, ParameterDirection.Input));
                myParams.Add(new OracleParameter("Part_NO", OracleDbType.Varchar2, Part_NO, ParameterDirection.Input));
                myParams.Add(new OracleParameter("ITEM_SERIAl_NO", OracleDbType.Varchar2, ITEM_SERIAl_NO, ParameterDirection.Input));
                myParams.Add(new OracleParameter("TEST_RESULT", OracleDbType.Varchar2, TEST_RESULT, ParameterDirection.Input));
                myParams.Add(new OracleParameter("XML_DATA_CONTENT", OracleDbType.XmlType, XML_DATA_CONTENT, ParameterDirection.Input));
                myParams.Add(new OracleParameter("FILE_PATH", OracleDbType.Varchar2, FILE_PATH, ParameterDirection.Input));
                myParams.Add(new OracleParameter("DETAIL_DATA_TABLE", OracleDbType.Varchar2, DETAIL_DATA_TABLE, ParameterDirection.Input));
                myParams.Add(new OracleParameter("ELAPSED_TIME", OracleDbType.Varchar2, ELAPSED_TIME, ParameterDirection.Input));
                myParams.Add(new OracleParameter("MANUFACTURER", OracleDbType.Varchar2, MANUFACTURER, ParameterDirection.Input));
                myParams.Add(new OracleParameter("PRODUCT_NAME", OracleDbType.Varchar2, PRODUCT_NAME, ParameterDirection.Input));
                myParams.Add(new OracleParameter("USER_NAME", OracleDbType.Varchar2, USER_NAME, ParameterDirection.Input));
                myParams.Add(new OracleParameter("USER_ID", OracleDbType.Varchar2, USER_ID, ParameterDirection.Input));
                myParams.Add(new OracleParameter("SCRIPT_NAME", OracleDbType.Varchar2, SCRIPT_NAME, ParameterDirection.Input));
                myParams.Add(new OracleParameter("STATIONMA", OracleDbType.Varchar2, STATIONMA, ParameterDirection.Input));
                myParams.Add(new OracleParameter("OSTYPE", OracleDbType.Varchar2, OSTYPE, ParameterDirection.Input));
                myParams.Add(new OracleParameter("OSVERSION", OracleDbType.Varchar2, OSVERSION, ParameterDirection.Input));
                myParams.Add(new OracleParameter("FIRMWAREVERSION", OracleDbType.Varchar2, FIRMWAREVERSION, ParameterDirection.Input));
                myParams.Add(new OracleParameter("TESTERWVERSION", OracleDbType.Varchar2, TESTERWVERSION, ParameterDirection.Input));
                myParams.Add(new OracleParameter("OEMINSTALL", OracleDbType.Varchar2, OEMINSTALL, ParameterDirection.Input));
                myParams.Add(new OracleParameter("KERNELLVERSION", OracleDbType.Varchar2, KERNELLVERSION, ParameterDirection.Input));
                myParams.Add(new OracleParameter("FIXED_ASSET_TAG", OracleDbType.Varchar2, FIXED_ASSET_TAG, ParameterDirection.Input));
                myParams.Add(new OracleParameter("START_DATE", OracleDbType.Varchar2, ValidateDateFormat(START_DATE, "START_DATE"), ParameterDirection.Input));
                myParams.Add(new OracleParameter("END_DATE", OracleDbType.Varchar2, ValidateDateFormat(END_DATE, "END_DATE"), ParameterDirection.Input));
                myParams.Add(new OracleParameter("retval", OracleDbType.Varchar2, 100, strResult, ParameterDirection.Output));
                ODPNETHelper.ExecuteNonQuery(WebConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString, CommandType.StoredProcedure, "WEBADAPTERS1.F1C_TESTER_LOG.InsertTesterLog", myParams.ToArray());
                //Have to access the last paramater on the previous list in order to get the OutPut parameter value
                strResult = myParams[23].Value.ToString();                

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                myParams = null;
            }
            return strResult;
        }

        public string ValidateDateFormat(string Value2Check, string ParameterName)
        {
            string strResult;
            if (string.IsNullOrEmpty(Value2Check))
                strResult = null;
            else
                if (System.Text.RegularExpressions.Regex.IsMatch(Value2Check, @"^\d{1,2}\/\w{3}\/\d{4}\s\d{1,2}\:\d{2}\:\d{2}$"))
                    strResult = Value2Check;
                else
                    throw new System.ArgumentException("Format for Dates should be [DD/MON/YYYY HH24:MI:SS]", ParameterName);
            return strResult;
        }

		#endregion
	}
}
