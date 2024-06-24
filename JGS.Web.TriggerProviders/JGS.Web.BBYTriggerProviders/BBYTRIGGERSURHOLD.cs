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
    public class BBYTRIGGERSURHOLD : JGS.Web.TriggerProviders.TriggerProviderBase 
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
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}            
            // Extraer valor de Flex Field External Color
            ,{"XML_FFEXTCOLOR","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='EXTERNAL COLOR']/Value"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_PNCHANGEPART","/Trigger/Detail/ChangePart/NewPartNo"}
            ,{"XML_SNCHANGEPART","/Trigger/Detail/ChangePart/NewSerialNo"}
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

        public BBYTRIGGERSURHOLD()
        {
            this.Name = "BBYTRIGGERSURHOLD";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string Trigger = string.Empty;
            string UserName = string.Empty;
            string BCN = string.Empty;
            string RC = string.Empty;
 
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

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                RC = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Result Code can not be found.");
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

            // Validaciones

            // Release Validation

            string PlaceOnHold = string.Empty;
            string ExistSOR = string.Empty;
            string AprovedAR = string.Empty;

            List<OracleParameter> myParamshr1;
            myParamshr1 = new List<OracleParameter>();
            myParamshr1.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParamshr1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            //PlaceOnHold = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "PlaceOnHold", myParamshr1);
            //PlaceOnHold = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "PlaceOnHold", myParamshr1);
            PlaceOnHold = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "PlaceOnHold", myParamshr1);

            if (PlaceOnHold == null || PlaceOnHold == "NULO") 
            {
                PlaceOnHold = "";
            }
            
            if (PlaceOnHold.ToUpper() == "PLACE ON HOLD")
            {
                List<OracleParameter> myParamsR;
                myParamsR = new List<OracleParameter>();
                myParamsR.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParamsR.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //ExistSOR = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "ExistSO", myParamsR);
                //ExistSOR = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "ExistSO", myParamsR);
                ExistSOR = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "ExistSO", myParamsR);

                if (ExistSOR != null && ExistSOR != "NULO")
                {
                    List<OracleParameter> myParamsAR;
                    myParamsAR = new List<OracleParameter>();
                    myParamsAR.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                    myParamsAR.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //AprovedAR = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "GetAprove", myParamsAR);
                    //AprovedAR = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "GetAprove", myParamsAR);
                    AprovedAR = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "GetAprove", myParamsAR);

                    if (AprovedAR == "" || AprovedAR == null || AprovedAR == "NULO")
                    {
                       return SetXmlError(returnXml, "Do not release this unit");
                    }

                }
            }
            
            // 21 DAYS AND JUNK OUT VALIDATIONS

            string ExistSO = string.Empty;
            string DifDays = string.Empty;
            string Aproved = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();           
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            //ExistSO = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "ExistSO", myParams);
            //ExistSO = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "ExistSO", myParams);
            ExistSO = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "ExistSO", myParams);

            if (ExistSO == null || ExistSO == "NULO")
            {

                List<OracleParameter> myParams2;
                myParams2 = new List<OracleParameter>();
                myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //DifDays = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "GetSODays", myParams2);
                //DifDays = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "GetSODays", myParams2);
                DifDays = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "GetSODays", myParams2);

                if (DifDays == null || DifDays == "NULO")
                {
                    return SetXmlError(returnXml, "Trigger Error: BCN without Reference Order Date");
                }
                else
                {
                    if (Convert.ToInt32(DifDays) >= 21)
                    {
                        return SetXmlError(returnXml, "Unidad con más de 21 días, [ " + DifDays + " ] mandar a HOLD");
                    }
                    else 
                    {
                        // escoger cualquier RC excepto Junk Out
                        if (RC.ToUpper() == "JUNK-OUT") 
                        {
                            return SetXmlError(returnXml, "Solo puede seleccionar RC diferente de JUNK-OUT");
                        }
                    }
                }

            }
            else 
            {

                List<OracleParameter> myParams3;
                myParams3 = new List<OracleParameter>();
                myParams3.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParams3.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //Aproved = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSURHOLD", "GetAprove", myParams3);
                //Aproved = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSURHOLD", "GetAprove", myParams3);
                Aproved = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSURHOLD", "GetAprove", myParams3);

                if (ExistSO.ToUpper() == "JRQ")
                {
                    if (Aproved == "Y")
                    {
                        // escoger solo RC Junk Out
                        if (RC.ToUpper() != "JUNK-OUT")
                        {
                            return SetXmlError(returnXml, "Solo puede seleccionar RC JUNK-OUT");
                        }                        
                    }
                    else
                    {
                        // escoger solo RC RNR
                        if (RC.ToUpper() != "RNR")
                        {
                            return SetXmlError(returnXml, "Solo puede seleccionar RC RNR");
                        }     
                    }
                }
                else 
                {
                    if (Aproved == "Y")
                    {
                        // escoger cualquier RC excepto RNR o Junk Out
                        if (RC.ToUpper() == "JUNK-OUT")
                        {
                            return SetXmlError(returnXml, "Solo puede seleccionar RC diferente JUNK-OUT");
                        }
                    }
                    else 
                    {
                        // escoger solo RC RNR
                        if (RC.ToUpper() != "RNR")
                        {
                            return SetXmlError(returnXml, "Solo puede seleccionar RC RNR");
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

    }
}
