using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using Oracle.DataAccess.Types;
using JGS.DAL;
//using System.Web.Configuration;
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders
{
    public class HTCTRIGGERREYFUNCTIONAL : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
			//FA Fields
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			//To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalisys/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES", 
				 "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
         ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']"}
         ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
             		};

        public override string Name { get; set; }

        public HTCTRIGGERREYFUNCTIONAL()
        {
            this.Name = "HTCTRIGGERREYFUNCTIONAL";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            //string valor = string.Empty;
            string WCL2 = string.Empty;
            string DataEntry_RC = string.Empty;
            string CalSerLev = string.Empty;
            string Trigger = string.Empty;
            int ItemID;
            string SN;
            string UserName;
            string BCN;
            string WC;
            string OPT;
            string RC;
            string PartNo = string.Empty;
            string CountCompBilled;
            int clientId;
            int contractId;
            int WCid;
            string InvalidComp = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Item ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                PartNo = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number can not be found.");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
            }

            //-- Get OPT Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "OPT can not be found.");
            }

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                RC = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Result Code can not be found.");
            }

            //-- Get WC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                WC = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "WC can not be found.");
            }

            //-- Get WCID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WCid = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WC ID can not be found.");
            }

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                Trigger = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }

            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }


            if (Trigger.ToUpper() == "TIMEOUT")
            {
                if (RC.ToUpper() == "FAIL_COS")
                {
                    XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
                    foreach (XmlNode xn in xnList)
                    {
                        if (xn["Name"].InnerText == "Defect list_Cosm")
                            if (xn["Value"].InnerText == String.Empty)
                            {
                                return SetXmlError(returnXml, "Flex Field Defect list_Cosm empty, please verify.");
                            }
                    }
                }
                if (RC.ToUpper() == "FUN_FAIL")
                {
                    XmlNodeList xnList2 = xmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
                    foreach (XmlNode xn in xnList2)
                    {
                        if (xn["Name"].InnerText == "Defect List Func")
                            if (xn["Value"].InnerText == String.Empty)
                            {
                                return SetXmlError(returnXml, "Flex Field Defect list Func empty, please verify.");
                            }
                    }
                }

                if (RC.ToUpper() == "FUN_FAIL" || RC.ToUpper() == "WIP" ) 
                {
                    //////////////////// Parameters List /////////////////////
                    List<OracleParameter> myParams;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                    myParams.Add(new OracleParameter("WC", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = WC });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    WCL2 = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCREYFUNCTIONAL", "GETCOUNT", myParams);

                    if (Convert.ToInt32(WCL2) >= 3)
                    {
                        if (RC.ToUpper() != "ENG")
                        {
                            return SetXmlError(returnXml, "The result code must be ENG, please Verify  /El result code debe ser ENG, verifique");
                        }
                    }
                }
            }
            return returnXml;
        }

        /// <summary>
        /// Set the Result to EXECUTION_ERROR and the Message to the specified message
        /// </summary>
        /// <param name="returnXml">The XmlDocument to update</param>
        /// <param name="message">The error message to set</param>
        /// <returns>The modified XmlDocument</returns>
        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        /// <summary>
        /// Set Return XML to Success before validation begin.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }


        /// <summary>
        /// Set Return XML to change Result code according validation.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlResultCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "DIAGNOSIS");
        }

        /// <summary>
        /// Set Return XML to assing defect code according validation.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlDefectCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_DEFECTCODES"], "F100");

        }

    }
}