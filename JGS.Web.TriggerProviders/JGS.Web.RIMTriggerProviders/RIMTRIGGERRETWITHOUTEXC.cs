using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using JGS.Web.TriggerProviders;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERRETWITHOUTEXC : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
        {
            {"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            //,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
        };

        public override string Name { get; set; }
        public RIMTRIGGERRETWITHOUTEXC()
        {
            this.Name = "RIMTRIGGERRETWITHOUTEXC";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string returnedValue;
            string SN, BCN, UserName, ResultCode,WorkCenter, ExceptionMessage, ExceptionWC, OPT;

            SetXmlSuccess(returnXml);

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Serial Number can not be found.");

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "BCN can not be found.");

            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            else
                return SetXmlError(returnXml, "User Name can not be found.");

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                ResultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Result Code can not be found.");
            
            //-- Get Work center
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
                WorkCenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "WorkCenter can not be found.");
            
            //-- Get OPT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
                OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Order Process Type can not be found.");

            returnedValue = CheckWCDestExceptions(BCN, UserName, ResultCode, WorkCenter, OPT);

            if (returnedValue.Contains("|"))
            {
                string[] excValues = returnedValue.Split(new[] { '|' });
                
                string rst = excValues[0].ToString(); 
                ExceptionMessage = excValues[1].ToString(); 
                //ExceptionWC = excValues[1].ToString();

                if (rst == "-1")
                {
                    return SetXmlError(returnXml, "Esta unidad no cuenta con un registro de excepción, no puede regresarla a una estación previa.");
                }
            }

            return returnXml;
        }

        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }

        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        private string CheckWCDestExceptions(string bcn, string username, string rc, string wc, string opt)
        {
            string rtn = "";

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("P_BCN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bcn });
            myParams.Add(new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
            myParams.Add(new OracleParameter("P_RC", OracleDbType.Varchar2, ParameterDirection.Input) { Value = rc });
            myParams.Add(new OracleParameter("P_WC", OracleDbType.Varchar2, ParameterDirection.Input) { Value = wc });
            myParams.Add(new OracleParameter("P_OPT", OracleDbType.Varchar2, ParameterDirection.Input) { Value = opt });
            rtn = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERRETWITHOUTEXC", "CheckWCDestinationExceptions", myParams);

            return rtn;
        }
    }
}