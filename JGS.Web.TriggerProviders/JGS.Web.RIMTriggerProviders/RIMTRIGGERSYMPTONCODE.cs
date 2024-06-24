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
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders 
{
    public class RIMTRIGGERSYMPTONCODE : JGS.Web.TriggerProviders.TriggerProviderBase        
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
            ,{"XML_SYMCODE","/Trigger/Detail/TimeOut/SymptomCodeList/SymptomCode/Value"}
            ,{"XML_DIACODE","/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode/Value"}     
            ,{"XML_DiagFF","/Trigger/Detail/TimeOut/WCFlexFields/FlexField/Value"}   
			//FA Fields
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			//To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalisys/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES", "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}        
            ,{"XML_FFWCDiagnostic","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Diagnostic']/Value"}
        };          

        public override string Name { get; set; }

        public RIMTRIGGERSYMPTONCODE()
        {
            this.Name = "RIMTRIGGERSYMPTONCODE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {

            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;           
            string SymCode = string.Empty;
            string CountSymCod = string.Empty;
            string Trigger = string.Empty;
            string UserName;
            string BCN;
            string WC;
            string OPT;
            string RC;
            string DiaCode = string.Empty;
            string FFWCDiagnostic = string.Empty;
            string CountHistory = string.Empty;
            
            // Set Return Code to Success
            SetXmlSuccess(returnXml);
                     
            
            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
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
                       
            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                Trigger = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Trigger Type can not be found.");
            }

            //-- Get Symtomp Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SYMCODE"]))
            {
                SymCode = Functions.ExtractValue(xmlIn, _xPaths["XML_SYMCODE"]).Trim().ToUpper();
            }
            else
            {
                SymCode = "";
            }

            //-- Get Diagnostic Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_DIACODE"]))
            {
                DiaCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DIACODE"]).Trim().ToUpper();
            }
            else
            {
                DiaCode = "";
            }

            //-- Get FF WC Diagnostic
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFWCDiagnostic"]))
            {
                FFWCDiagnostic = Functions.ExtractValue(xmlIn, _xPaths["XML_FFWCDiagnostic"]).Trim().ToUpper();
            }
            else
            {
                FFWCDiagnostic = "";
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
                                 
                if (WC.ToUpper() == "OMFT" || WC.ToUpper() == "LABEL" || WC.ToUpper() == "ZAUD")
                {
                    if (OPT.ToUpper() == "WRP" || OPT.ToUpper() == "ZWRP" || OPT.ToUpper() == "ZRICK") 
                    {
                        if (RC.ToUpper() == "DATAENT" || RC.ToUpper() == "DBG REW" || RC.ToUpper() == "VMI_INSP")
                        {                            
                            if (SymCode == "")
                            {
                                return SetXmlError(returnXml, "Trigger Error: Debe seleccionar un código de falla");                           
                            }                           
                        }                        
                    }
                }

                if (WC.ToUpper() == "DEBUG-REW")
                {

                    if (OPT.ToUpper() == "WRP" || OPT.ToUpper() == "ZWRP" || OPT.ToUpper() == "ZRICK")
                    {

                        //////////////////// Parameters List /////////////////////
                        // Extraer consulta de Xelus si tiene symton code agregado en los WC's OMFT/ LABEL or ZAUD 
                        List<OracleParameter> myParams;
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //CountSymCod = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);
                        //CountSymCod = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);
                        CountSymCod = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);

                        if (CountSymCod == null || CountSymCod == "")
                        {
                            CountSymCod = "0";
                        }

                        if (CountSymCod != "0")
                        {
                            if (DiaCode == "")
                            {
                                return SetXmlError(returnXml, "Trigger Error: Debe seleccionar un Diagnostic Code");
                            }
                        } 
                    }                               
                }
            }

             if (Trigger.ToUpper() == "TIMEOUT") 
            {
                if (WC.ToUpper() == "DATA_ENTRY")
                {

                    //////////////////// Parameters List /////////////////////
                    // Extraer consulta de Xelus si tiene symton code agregado en los WC's OMFT/ LABEL or ZAUD 
                    List<OracleParameter> myParams;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //CountSymCod = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);
                    //CountSymCod = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);
                    CountSymCod = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMSYMPTONCODE", "havesymptoncode", myParams);

                    if (CountSymCod == null || CountSymCod == "")
                    {
                        CountSymCod = "0";
                    }

                    if (CountSymCod == "0")
                    {
                        SetXmlFFDia(returnXml, "NO");
                    }
                    else
                    {
                        SetXmlFFDia(returnXml, "YES");
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
        /// Set Return XML to change FlexField Diagnostic.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void  SetXmlFFDia(XmlDocument returnXml, String FFDiaValue)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFWCDiagnostic"], FFDiaValue);
        }        

    }
}
