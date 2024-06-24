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
//using System.Web.UI.WebControls;

namespace JGS.Web.TriggerProviders

{
    public class RIMTRIGGERBILLINGVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public RIMTRIGGERBILLINGVALIDATION()
        {
            this.Name = "RIMTRIGGERBILLINGVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            //string valor = string.Empty;
            string FFService_Level = string.Empty;
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


            if (Trigger.ToUpper() == "FAILUREANALYSIS")
            {
                if (WC.ToUpper() == "PRE-LOGISTC")
                {
                    if (RC.ToUpper() != "FAIL")
                    { 

                    //////////////////// Parameters List /////////////////////
                    List<OracleParameter> myParams;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                    myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                    //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                    FFService_Level = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                    
                    if (FFService_Level == null)
                    {
                        return SetXmlError(returnXml, "Flex Field Service Level empty, please verify.");
                    }
                    else
                    {
                        //////////////////// Parameters List /////////////////////
                        List<OracleParameter> myParams2;
                        myParams2 = new List<OracleParameter>();
                        myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                        myParams2.Add(new OracleParameter("ClientId", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                        myParams2.Add(new OracleParameter("ContractId", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });

                        myParams2.Add(new OracleParameter("WCName", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = WC });
                        myParams2.Add(new OracleParameter("WCID", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = WCid });
                        myParams2.Add(new OracleParameter("RC", OracleDbType.Varchar2, RC.Length, ParameterDirection.Input) { Value = RC });
                        myParams2.Add(new OracleParameter("Item", OracleDbType.Varchar2, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                        myParams2.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                        
                        myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //CalSerLev = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams2);
                        //CalSerLev = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams2);
                       CalSerLev = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams2);
                        
                        if (CalSerLev == null)
                        {
                            return SetXmlError(returnXml, "Calculate Service Level empty, please verify.");
                        }
                        else
                        {
                            if (FFService_Level != CalSerLev)
                            {
                                return SetXmlError(returnXml, "FF Service Level [ " + FFService_Level + " ] NOT math with Calculate Service Level [ " + CalSerLev + " ] ,Please verify.");
                            }
                        }

                    }

                    if (Convert.ToInt32(CalSerLev) == 0 && RC.ToUpper() != "BER-UR")
                    {
                        return SetXmlError(returnXml, "Service Level [ " + CalSerLev + " ] you should select RC [ " + "BER_UR" + " ] , please verify");
                    }


                    if (Convert.ToInt32(CalSerLev) == 0)
                    {

                        //////////////////// Parameters List /////////////////////
                        List<OracleParameter> myParams5;
                        myParams5 = new List<OracleParameter>();
                        myParams5.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                        myParams5.Add(new OracleParameter("ClientId", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                        myParams5.Add(new OracleParameter("ContractId", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                        myParams5.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                        myParams5.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //CountCompBilled = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "GetCompBilled", myParams5);
                        //CountCompBilled = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "GetCompBilled", myParams5);
                        CountCompBilled = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBILLINGVALIDATION", "GetCompBilled", myParams5);

                        if (CountCompBilled != "0" && CountCompBilled != "NOTHING" && CountCompBilled != null)
                        {
                            return SetXmlError(returnXml, "[ " + CountCompBilled + " ] Component(s) Billed, NOT Valid for Level 0, please verify");
                        }                       
                        
                    }   


                    if (RC.ToUpper() == "REPAIRED" && CalSerLev == "3B")
                    {
                        return SetXmlError(returnXml, "Service Level [ " + CalSerLev + " ] you should select different RC that [ " + RC + " ] , please verify");                        
                    }                    

                    //////////////////// Parameters List /////////////////////
                    List<OracleParameter> myParams4;
                    myParams4 = new List<OracleParameter>();
                    myParams4.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                    myParams4.Add(new OracleParameter("WCName", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = "Data_Entry" });
                    myParams4.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                    myParams4.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //DataEntry_RC = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getrc", myParams4);
                    //DataEntry_RC = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getrc", myParams4);
                    DataEntry_RC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getrc", myParams4);

                    if (DataEntry_RC == "NOTHING")
                    {
                        return SetXmlError(returnXml, "Data Entry RC is [ NULL ] you can not select RC [ " + "OOW" + " ] , please verify");
                    }
                    else
                    { 
                        // se modificó en una sola línea la validación
                        if ((RC.ToUpper() != "OOW" && DataEntry_RC.ToUpper() == "OOW") || (DataEntry_RC.ToUpper() != "OOW" && RC.ToUpper() == "OOW"))
                        {
                            return SetXmlError(returnXml, "Data Entry RC is [ " + DataEntry_RC + " ] you can not select RC [ " + RC.ToUpper() + " ] , please verify");
                        }                                            
                    }
                    
                    //////////////////// Parameters List /////////////////////
                    List<OracleParameter> myParams3;
                    myParams3 = new List<OracleParameter>();
                    myParams3.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                    myParams3.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                    myParams3.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams3.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams3.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, PartNo.Length, ParameterDirection.Input) { Value = PartNo });
                    myParams3.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                    myParams3.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //InvalidComp = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "GetValidComp", myParams3);
                    //InvalidComp = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "GetValidComp", myParams3);
                    InvalidComp = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBILLINGVALIDATION", "GetValidComp", myParams3);

                    if (InvalidComp != null)
                    {
                        return SetXmlError(returnXml, "The next components [ " + InvalidComp + " ] are NOT valid for Part No [ " + PartNo + " ] , please verify");
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