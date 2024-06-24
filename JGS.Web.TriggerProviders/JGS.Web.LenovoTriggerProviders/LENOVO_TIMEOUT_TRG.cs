using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class LENOVO_TIMEOUT_TRG:TriggerProviderBase
    {
        public override string Name { get; set; }

        public LENOVO_TIMEOUT_TRG()
        {
            this.Name = "LENOVO_TIMEOUT_TRG";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            string Schema_name = "WEBAPP1";
            string Package_name = "LENOVO_WUR_TIMEOUT";

            int locationID;
            int clientID;
            int contractID;
            string triggerType;
            string workcenter;
            string opt;
            string resultCode;
            int itemId;
            string UserName;
            
            
            
            /**********************Getting values from xml*******************/
            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_LOCATIONID"]))
                locationID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_LOCATIONID"]));
            else 
                return SetXmlError(returnXml, "Geography Id can not be found.");
            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_CLIENTID"]))
                clientID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_CLIENTID"]));
            else
                return SetXmlError(returnXml, "Client Id can not be found.");
            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_CONTRACTID"]))
                contractID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_CONTRACTID"]));
            else
                return SetXmlError(returnXml, "Contract Id can not be found.");
            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_TRIGGERTYPE"]))
                triggerType = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_TRIGGERTYPE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Trigger type can not be found.");
            //-- Get WC
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_WORKCENTER"]))
                workcenter = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_WORKCENTER"]);
            else
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
            //-- Get OPT
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_OPT"]))
                opt = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_OPT"]);
            else
                return SetXmlError(returnXml, "OPT can not be found.");
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_RESULTCODE"]))
                resultCode = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_RESULTCODE"]);
            else 
                return SetXmlError(returnXml, "ResultCode can not be found.");
             //-- Get ItemId
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_ItemID"]))
                itemId = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_ItemID"]));
            else 
                return SetXmlError(returnXml, "ItemId can not be found.");
           
            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPathsTO["XML_USERNAME"]))
                UserName = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPathsTO["XML_USERNAME"]).Trim();
            else
                return SetXmlError(returnXml, "UserName could not be found.");

            if (triggerType.ToUpper() == "TIMEOUT")
            {
                if (opt.ToUpper() == "WRP")
                {
                    if (workcenter.ToUpper().Equals("LNV_PACK"))
                    {
                        //////////////////// Parameters List /////////////////////
                            String strResult = null;
                            List<OracleParameter> myParams;
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                            myParams.Add(new OracleParameter("resultCode", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode });
                            myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "lenovoGetERWCResultC", myParams);

                            if (!string.IsNullOrEmpty(strResult))
                            {
                                if (!strResult.Equals(resultCode ))
                                {
                                    XmlNode resultCNode = returnXml.SelectSingleNode(xLenovoDictionary._xPathsTO["XML_RESULTCODE"]);
                                    resultCNode.InnerText = strResult;
                                }
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Problems to retrieve data from DB [LENOVO_WUR_TIMEOUT.lenovoGetERWCResultC]");
                            }
                        
                    }//wc
                }//opt
            }//TriggerType
            SetXmlSuccess(returnXml);
            return returnXml;
        }//execute

        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPathsTO["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPathsTO["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPathsTO["XML_RESULT"], EXECUTION_OK);
        }
    }
}
