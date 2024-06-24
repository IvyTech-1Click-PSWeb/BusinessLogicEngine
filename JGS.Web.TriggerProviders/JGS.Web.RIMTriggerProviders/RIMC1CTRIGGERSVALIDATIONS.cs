using System;
using System.Collections.Generic;
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
    public class RIMC1CTRIGGERSVALIDATIONS : JGS.Web.TriggerProviders.TriggerProviderBase
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
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalysis/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			//To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES","/Trigger/Detail/FailureAnalysis/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']"}
            ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_BOUNCE_DISPOSITION","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Bounce_Disposition']/Value"} 
  		    ,{"XML_LOOP_COUNT","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Loop_Count']/Value"}
            ,{"XML_SERVICE_LEVEL","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='SERVICE_LEVEL']/Value"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            };

        
            public override string Name { get; set; }

            public RIMC1CTRIGGERSVALIDATIONS()
            {
                this.Name = "RIMC1CTRIGGERSVALIDATIONS";
            }


            public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
            {
                XmlDocument returnXml = xmlIn;

                //Build the trigger code here
                ////////////////////////////// Variables ///////////////////////////////////////////////////
                string errMsg = string.Empty;
                string BounceDisp = string.Empty;
                string LoopCount = string.Empty;
                string CallBounce = string.Empty;
                string CallLoop = string.Empty;
                string CallRC = string.Empty;
                string CallKit = string.Empty;
                string CallPartKit = string.Empty;
                string Call3Comp  ;
                string partNumber;
                string workcenterName;
                string Trigger = string.Empty;
                string resultCode;
                string SN;
                string BCN = string.Empty;
                string FA_COMP_PN = string.Empty;
                int LocationId;
                int clientId;
                int contractId;
                string WCId;
                string Itemid;
                string UserName;
                string CalSerLev = string.Empty;
                string CallKitNo = string.Empty;
                string BerCodes = string.Empty;
                string OrderProcessType = string.Empty;
                string res = string.Empty;
                string Service_Level = string.Empty;
                string FA_Defect = string.Empty;
                List<OracleParameter> myParams;

                // Set Return Code to Success
                SetXmlSuccess(returnXml);

                //-- Get Serial Number
                if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
                {
                    SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
                }
                else
                {
                    return SetXmlError(returnXml, "Serial Number can not be found.");
                }

                //-- Get PartNumber
                if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
                {
                    partNumber = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "Part Number could not be found.");
                }

                //-- Get OPT
                if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
                {
                    OrderProcessType = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "Part Number could not be found.");
                }
                // - Get Work Center Name
                if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
                {
                    workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "Work Center Name can not be found.");
                }

                //-- Get Location Id
                if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
                {
                    LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
                }
                else
                {
                    return SetXmlError(returnXml, "Geography Id can not be found.");
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

                //-- Get Result Code
                if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                {
                    resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
                }
                else
                {
                    return SetXmlError(returnXml, "Result Code could not be found.");
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

                   if (!Functions.IsNull(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]))
                {
                    BounceDisp = Functions.ExtractValue(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]).Trim().ToUpper();
                }


                     if (!Functions.IsNull(xmlIn, _xPaths["XML_LOOP_COUNT"]))
                {
                    LoopCount = Functions.ExtractValue(xmlIn, _xPaths["XML_LOOP_COUNT"]).Trim().ToUpper();
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

                 //-- Get WCID
                 if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
                 {
                     WCId = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]).Trim().ToUpper();
                 }
                 else
                 {
                     return SetXmlError(returnXml, "WORKCENTERID can not be found.");
                 }

                 //-- Get ItemID
                 if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
                 {
                     Itemid = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();
                 }
                 else
                 {
                     return SetXmlError(returnXml, "ItemID can not be found.");
                 }

                 if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_PN"]))
                 {
                     FA_COMP_PN = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_PN"]).Trim().ToUpper();
                 }

                 if (!Functions.IsNull(xmlIn, _xPaths["XML_SERVICE_LEVEL"]))
                 {
                     Service_Level = Functions.ExtractValue(xmlIn, _xPaths["XML_SERVICE_LEVEL"]).Trim().ToUpper();
                 }

                 if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_DEFECTCODES"]))
                 {
                     FA_Defect = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODES"]).Trim().ToUpper();
                 }
                                       


             if (Trigger.ToUpper() == "FAILUREANALYSIS")
                {
                    if (FA_COMP_PN.Contains("KIT"))
                        {
                            //////////////////// Not more than the same Kit /////////////////////
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                            myParams.Add(new OracleParameter("FA_COMP_PN", OracleDbType.Varchar2, FA_COMP_PN.ToString().Length, ParameterDirection.Input) { Value = FA_COMP_PN });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                            //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                            CallKitNo = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetIfExistTheKit", myParams);

                            if (CallKitNo != null )
                            {
                                return SetXmlError(returnXml, "No Debe cobrar mas de un mismo KIT/Should not charge more of the same KIT");

                            }
                        }

                    if (FA_Defect != null & FA_Defect.Contains("F002"))
                    {
                        //////////////////// Not more than two BER's codes /////////////////////
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("BNC", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                        //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                        BerCodes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetBERCodes", myParams);

                        if (BerCodes != null)
                        {
                            return SetXmlError(returnXml, "No Debe agregar mas de un codigo BER/Not to add more than one BER code");

                        }
                    }



                    //////////////////// No more than 3 components /////////////////////
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("FA_COMP_PN", OracleDbType.Varchar2, FA_COMP_PN.ToString().Length, ParameterDirection.Input) { Value = FA_COMP_PN });
                    myParams.Add(new OracleParameter("itemid", OracleDbType.Varchar2, Itemid.ToString().Length, ParameterDirection.Input) { Value = Itemid });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                    //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                    Call3Comp = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "ThreeXComponent", myParams);


                    if (Call3Comp == Convert.ToString(true))
                    {
                        return SetXmlError(returnXml, "No se puede agregar por tercera vez el componente " + FA_COMP_PN + "/ You can't add more than 3 times the component " + FA_COMP_PN);

                    } 
                  
                }

             if (Trigger.ToUpper() == "TIMEOUT") 
             {
                 if (workcenterName == "Debug_Rew" & resultCode == "SwapAzul") 
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_SERVICE_LEVEL"], "INSP");
                         
                 }
                 else
                 {
                     if (Service_Level != null)
                     {
                         //////////////////// Get the Service Level /////////////////////
                         myParams = new List<OracleParameter>();
                         myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                         myParams.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                         myParams.Add(new OracleParameter("CONTRACTID", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                         myParams.Add(new OracleParameter("ACTUALWCNAME", OracleDbType.Varchar2, workcenterName.Length, ParameterDirection.Input) { Value = workcenterName });//new parameter
                         myParams.Add(new OracleParameter("WCId", OracleDbType.Varchar2, WCId.Length, ParameterDirection.Input) { Value = WCId });//new parameter
                         myParams.Add(new OracleParameter("ACTUALRC", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode });//new parameter
                         myParams.Add(new OracleParameter("Itemid", OracleDbType.Varchar2, Itemid.Length, ParameterDirection.Input) { Value = Itemid });//new parameter
                         myParams.Add(new OracleParameter("OrderProcessType", OracleDbType.Varchar2, OrderProcessType.Length, ParameterDirection.Input) { Value = OrderProcessType });//new parameter
                         myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });


                         //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function

                         res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams);
                         if (!string.IsNullOrEmpty(res))
                         {
                             Functions.UpdateXml(ref returnXml, _xPaths["XML_SERVICE_LEVEL"], res);
                         }
                         else
                         {
                             return SetXmlError(returnXml, "Service Level Could not be Calculate|Nivel de Servicio no puede ser calculado");
                         }

                     }
                     else
                     {
                         return SetXmlError(returnXml, "Debe seleccionar un Repair Level/Must be select a Repair Level");
                     }

                    }

                 //////////////////// Bounce Disposition /////////////////////
                 myParams = new List<OracleParameter>();
                 myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                 myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                 //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                 //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                 CallBounce = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetBounceDisp", myParams);

                 if (CallBounce != null )
                 {
                     Functions.UpdateXml(ref returnXml, _xPaths["XML_BOUNCE_DISPOSITION"], CallBounce);

                 }
                 else
                 {
                     if (BounceDisp == null)
                     {
                         return SetXmlError(returnXml, "Debe llenar el FF Bounce Disposition/Must be fill the FF Bounce Disposition");
                     }
                 }


                 if (LoopCount == null)
                 {
                     //////////////////// Loop Count /////////////////////
                     myParams = new List<OracleParameter>();
                     myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.ToString().Length, ParameterDirection.Input) { Value = BCN });
                     myParams.Add(new OracleParameter("WCId", OracleDbType.Varchar2, WCId.ToString().Length, ParameterDirection.Input) { Value = WCId });
                     myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                     //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                     //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                     CallLoop = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetLoop", myParams);

                     if (CallLoop == null)
                     {
                         Functions.UpdateXml(ref returnXml, _xPaths["XML_LOOPCOUNT"], "0");
                     }
                     else
                     {
                         Functions.UpdateXml(ref returnXml, _xPaths["XML_LOOPCOUNT"], "1");
                      }
                 }


                 if (resultCode != "BER")
                 {

                     if (resultCode != "OOW")
                     {

                         //////////////////// Get the Result Code /////////////////////
                          myParams = new List<OracleParameter>();
                          myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.ToString().Length, ParameterDirection.Input) { Value = BCN });
                          myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                         myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                         //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                         //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                         CallRC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetReceiptRC", myParams);



                         if (CallRC != "Return Customer")
                         {

                             //////////////////// Get if the Item has the Kit mandatory  ///////////////////// 
                             myParams = new List<OracleParameter>();
                             myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.ToString().Length, ParameterDirection.Input) { Value = BCN });
                             myParams.Add(new OracleParameter("clientId", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                             myParams.Add(new OracleParameter("OrderProcessType", OracleDbType.Varchar2, OrderProcessType.ToString().Length, ParameterDirection.Input) { Value = OrderProcessType });
                             myParams.Add(new OracleParameter("contractId", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                             myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                             //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                             //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                             CallKit = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetPartNoKit", myParams);


                             if (CallKit == null)
                             {
                                 //////////////////// Get the Kit Mandatory /////////////////////
                                 myParams = new List<OracleParameter>();
                                 myParams.Add(new OracleParameter("partNumber", OracleDbType.Varchar2, partNumber.ToString().Length, ParameterDirection.Input) { Value = partNumber });
                                 myParams.Add(new OracleParameter("OrderProcessType", OracleDbType.Varchar2, OrderProcessType.ToString().Length, ParameterDirection.Input) { Value = OrderProcessType });
                                 myParams.Add(new OracleParameter("contractId", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                                 myParams.Add(new OracleParameter("clientId", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                                 myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                                 myParams.Add(new OracleParameter("workcenterName", OracleDbType.Varchar2, workcenterName.ToString().Length, ParameterDirection.Input) { Value = workcenterName });
                                 myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                 //FFService_Level = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                                 //FFService_Level = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMBILLINGVALIDATION", "getffvalue", myParams);
                                 CallPartKit = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetKitNo", myParams);

                                 if (CallPartKit != null)
                                 {
                                     return SetXmlError(returnXml, "Debe por lo menos cobrar el KIT " + CallPartKit + " /Must collect at least KIT " + CallPartKit );

                                 }
                             }
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


            private void ReturnReceivingFlexfield()
            {

            }
        }    
}
