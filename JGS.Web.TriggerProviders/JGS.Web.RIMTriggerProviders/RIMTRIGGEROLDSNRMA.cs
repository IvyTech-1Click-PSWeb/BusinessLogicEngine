using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using JGS.Web.TriggerProviders;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGEROLDSNRMA : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
        {
            {"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            //,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            //,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
            ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
        };

        public override string Name { get; set; }
        public RIMTRIGGEROLDSNRMA()
        {
            this.Name = "RIMTRIGGEROLDSNRMA";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string RMAFlexFieldName = "OLD_SN_RMA";
            string returnedValue;
            string SN, BCN, UserName, ContractID, ClientID, TType;

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

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
                TType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Trigger Type can not be found.");

            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
                ClientID = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Client ID can not be found.");

            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
                ContractID = Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Contract ID can not be found.");

            if (TType.ToUpper() == "TIMEOUT")
            {
                returnedValue = GetRMA(BCN, UserName, ContractID, ClientID);

                if (!string.IsNullOrEmpty(returnedValue))
                {
                    //SET FLEX FIELD
                    if (!Functions.IsNull(xmlIn, _xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", RMAFlexFieldName)))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", RMAFlexFieldName), returnedValue);
                    }
                    else
                    {
                        return SetXmlError(returnXml, "WC Flex Field \"" + RMAFlexFieldName + "\" can not be found for update.");
                    }
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

        private string GetRMA(string bcn, string username, string contract, string client)
        {
            string rtn = "";

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("P_CONTRACTID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = contract });
            myParams.Add(new OracleParameter("P_CLIENTID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = client });
            myParams.Add(new OracleParameter("P_BCN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bcn });
            myParams.Add(new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
            rtn = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGEROLDSNRMA", "GetRMAFromOldSN", myParams);

            return rtn;
        }
    }
}
