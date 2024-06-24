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
    public class RIMTRIGGERPGIVERIZONPOOL : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
        };

        public override string Name { get; set; }
        public RIMTRIGGERPGIVERIZONPOOL()
        {
            this.Name = "RIMTRIGGERPGIVERIZONPOOL";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string SN, BCN, UserName, PGIResult, TType, rcAtReceipt, WorkCenter;
            
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

            //-- Get WorkCenter
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
                WorkCenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Work Center can not be found.");

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                PGIResult = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Result Code can not be found.");

            if (TType.ToUpper() == "TIMEOUT")
            {
                rcAtReceipt = CheckReceiptsRC(UserName, BCN);
                rcAtReceipt = string.IsNullOrEmpty(rcAtReceipt) ? "" : rcAtReceipt.ToUpper().Replace(" ", "");

                if (WorkCenter.ToUpper() == "PGI")
                {
                    if (rcAtReceipt == "VERIZONPOOL")
                    {
                        if (PGIResult != "BER-URVZ" && PGIResult != "REPVZ") //|| PGIResult == "SCRAPVZ")
                        {
                            return SetXmlError(returnXml, "Esta unidad  es Verizon Pool, debe de ser direccionada con un Result Code de VRZ.");
                        }
                    }
                    else //was not VerizonPool
                    {
                        if (PGIResult == "BER-URVZ" || PGIResult == "REPVZ") //|| PGIResult == "SCRAPVZ")
                        {
                            return SetXmlError(returnXml, "Esta unidad no pertenece a Verizon Pool , favor de validar su condición.");
                        }
                    }
                }
                else
                {
                    if (rcAtReceipt == "VERIZONPOOL")
                    {
                        if (PGIResult != "SCRAPVZ" )
                        {
                            return SetXmlError(returnXml, "Esta unidad  es Verizon Pool, debe de ser direccionada con un Result Code de VRZ.");
                        }
                    }
                    else //was not VerizonPool
                    {
                        if (PGIResult == "SCRAPVZ")
                        {
                            return SetXmlError(returnXml, "Esta unidad no pertenece a Verizon Pool , favor de validar su condición.");
                        }
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

        private string CheckReceiptsRC (string username, string bcn)
        {
            string rc;
            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("P_BCN", OracleDbType.Varchar2, bcn.ToString().Length, ParameterDirection.Input) { Value = bcn });
            myParams.Add(new OracleParameter("P_USERNAME", OracleDbType.Varchar2, username.ToString().Length, ParameterDirection.Input) { Value = username });
            rc = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERPGIVERIZON", "GETRCATRECEIPT", myParams);

            return rc;
        }
    }
}