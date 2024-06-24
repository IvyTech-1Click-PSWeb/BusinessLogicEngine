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
    public class HTCTRIGGERDIGSL2 : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public HTCTRIGGERDIGSL2()
        {
            this.Name = "HTCTRIGGERDIGSL2";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            //string valor = string.Empty;
            string NumComp = string.Empty;
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
            int LocationID;
            string InvalidComp = string.Empty;
            string Msg = string.Empty;
            String FFName;
            String FFValue;
            String IMEI2 = string.Empty;
            // Set Return Code to Success
            SetXmlSuccess(returnXml);
            //-- Get Location ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }
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

//            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
  //          {
   //             SN=Functions.ExtractValue(xmlIn,xPaths "XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"};
    //        {
    //        }
    //        else
    //        {
    //        }

            if (Trigger.ToUpper() == "TIMEOUT")
            {
                // Get Result Code
                if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                {
                    RC = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "Result Code can not be found.");
                }

                //////////////////// Parameters List /////////////////////
                
                //if (RC.ToUpper() != "PURGE")
                //{
                    
                //}
                
                if ((PartNo == "99HLW021-00") || (PartNo == "99HKU001-00") || (PartNo == "99HKU06-00") || (PartNo == "99HKU007-00") || (PartNo == "99HKU001-01") || (PartNo == "99HKU06-01") || (PartNo == "99HKU007-01"))
                {
                    List<OracleParameter> myParams3;
                    myParams3 = new List<OracleParameter>();
                    myParams3.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams3.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams3.Add(new OracleParameter("LocationID", OracleDbType.Int32, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                    myParams3.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                    myParams3.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIGSL2", "GETFF", myParams3);
                    if (Msg != "OK")
                    {
                        if (Convert.ToInt32(Msg) >= 3)
                        {
                            if (RC.ToUpper() != "User_Dam")
                            {
                                Msg = "Debe seleccionar Result Code User_Dam";
                                return SetXmlError(returnXml, Msg);
                            }
                        }
                        if (!Functions.IsNull(xmlIn, _xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "OOW Description")))
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "OOW Description"), "Unauthorized Repair");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Work center flex field \"OOW Description\" cannot be found for update.");
                        }
                    }
                }
                if ((PartNo == "99HKY001-01") || (PartNo == "99HKY001-00") || (PartNo == "99HKY002-00"))
                {
                    List<OracleParameter> myParams5;
                    myParams5 = new List<OracleParameter>();
                    myParams5.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams5.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams5.Add(new OracleParameter("LocationID", OracleDbType.Int32, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                    myParams5.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                    myParams5.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIGSL2", "GETFF2", myParams5);
                    if (Msg != "OK")
                    {
                        if (Convert.ToInt32(Msg) >= 2)
                        {
                            if (RC.ToUpper() != "User_Dam")
                            {
                                Msg = "Debe seleccionar Result Code User_Dam";
                                return SetXmlError(returnXml, Msg);
                            }
                        }
                        if (!Functions.IsNull(xmlIn, _xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "OOW Description")))
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "OOW Description"), "Unauthorized Repair");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Work center flex field \"OOW Description\" cannot be found for update.");
                        }
                    }
                }



            }
            if (Trigger.ToUpper() == "FAILUREANALYSIS")
            {
                String DefectCodeName = String.Empty;
                String ActionCodeName = String.Empty;
                XmlNodeList xnList2 = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                if (xnList2.Count == 0)
                {
                    return SetXmlError(returnXml, "No Defect Code Found|Codigos de Defecto No Encontrados");
                }
                foreach (XmlNode xn in xnList2)
                {
                    if (!string.IsNullOrEmpty(xn["DefectCodeName"].InnerText))
                    {
                        DefectCodeName = xn["DefectCodeName"].InnerText;
                        XmlNodeList xnListAction = xn.SelectNodes("ActionCodeList/ActionCode");
                        foreach (XmlNode xnAction in xnListAction)
                        {
                            if (!string.IsNullOrEmpty(xnAction["ActionCodeName"].InnerText))
                            {
                                ActionCodeName = xnAction["ActionCodeName"].InnerText;
                            }
                        }
                    }
                }
                List<OracleParameter> myParams3;
                myParams3 = new List<OracleParameter>();
                myParams3.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                myParams3.Add(new OracleParameter("ActionCodeName", OracleDbType.Varchar2, ActionCodeName.Length, ParameterDirection.Input) { Value = ActionCodeName });
                myParams3.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIGSL2", "GETDEFACT", myParams3);
                if (Msg != "OK")
                {
                    return SetXmlError(returnXml, Msg);
                }
                

                XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/FAFlexFieldList/FlexField");

                foreach (XmlNode xn in xnList)
                {
                    FFName = xn["Name"].InnerText;
                    FFValue = xn["Value"].InnerText;
                    if (FFName == "S/N")
                    {
                        if (FFValue.Length != 13)
                        {
                            return SetXmlError(returnXml, "SN debe ser de 13 digitos");
                        }
                    }
                    if (FFName == "IMEI")
                    {
                        if (FFValue.Length != 15)
                        {

                            return SetXmlError(returnXml, "IMEI debe ser de 15 digitos");
                        }
                        else
                        {
                         IMEI2 = FFValue;
                        }
                    }
                }
               
                List<OracleParameter> myParams2;
                myParams2 = new List<OracleParameter>();
                myParams2.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams2.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams2.Add(new OracleParameter("LocationId", OracleDbType.Int32, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                myParams2.Add(new OracleParameter("Item", OracleDbType.Varchar2, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                myParams2.Add(new OracleParameter("Imei", OracleDbType.Varchar2, IMEI2.ToString().Length, ParameterDirection.Input) { Value =  IMEI2});
                myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIGSL2", "GETIMEIDUP", myParams2);
                if (Msg != "OK")
                {
                    return SetXmlError(returnXml, Msg);
                }
                
                //List<OracleParameter> myParams;
                //myParams = new List<OracleParameter>();
                //myParams.Add(new OracleParameter("LocationID", OracleDbType.Int32, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                //myParams.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                //myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIGSL2", "GETSNIMEI", myParams);
                //if (Msg != "OK")
                //{
                 //   return SetXmlError(returnXml, Msg);
                //}
                List<OracleParameter> myParams6;
                myParams6 = new List<OracleParameter>();
                myParams6.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams6.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams6.Add(new OracleParameter("LocationID", OracleDbType.Int32, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                myParams6.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, PartNo.ToString().Length, ParameterDirection.Input) { Value = PartNo });
                myParams6.Add(new OracleParameter("Item", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                myParams6.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                Msg = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIAGNOSIS", "GETIMEI", myParams6);
                if (Msg != "OK")
                {
                    return SetXmlError(returnXml, Msg);
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

    }
}