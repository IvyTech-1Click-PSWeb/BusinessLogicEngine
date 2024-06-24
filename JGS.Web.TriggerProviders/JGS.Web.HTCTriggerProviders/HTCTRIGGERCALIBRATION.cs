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
    public class HTCTRIGGERCALIBRATION : JGS.Web.TriggerProviders.TriggerProviderBase 
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

        public HTCTRIGGERCALIBRATION()
        {
            this.Name = "HTCTRIGGERCALIBRATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string Trigger = string.Empty;            
            string UserName = string.Empty;            
            string PartNo = string.Empty;
            string FFExtColor = string.Empty;           

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                PartNo = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number can not be found.");
            }

            ////-- Get FlexField External Color
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_FFEXTCOLOR"]))
            //{
            //    FFExtColor = Functions.ExtractValue(xmlIn, _xPaths["XML_FFEXTCOLOR"]).Trim().ToUpper();
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "FlexField External Color can not be found.");
            //}

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

            // comienza trigger

            if (Trigger.ToUpper() == "FAILUREANALYSIS")
            {                
                    //Extraer Descripción del número de parte...
                    string PartNoDesc = string.Empty;

                    List<OracleParameter> myParams;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("PartNumber", OracleDbType.Varchar2, PartNo.Length, ParameterDirection.Input) { Value = PartNo });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    //PartNoDesc = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "htccalibration", "getPNDesc", myParams);
                    PartNoDesc = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "htccalibration", "getPNDesc", myParams);

                    if (PartNoDesc == null || PartNoDesc == "NULO")
                    {
                        return SetXmlError(returnXml, "Trigger Error: Part Number no exist, please verify");
                    }
                    else
                    {

                        //-- Get FlexField External Color
                        if (!Functions.IsNull(xmlIn, _xPaths["XML_FFEXTCOLOR"]))
                        {
                            FFExtColor = Functions.ExtractValue(xmlIn, _xPaths["XML_FFEXTCOLOR"]).Trim().ToUpper();
                        }
                        else
                        {
                            FFExtColor = "";
                        }   
                       
                        if ((PartNoDesc.ToUpper().Contains("ESPRESSO")) == true )
                        {
                            if (FFExtColor == "")
                            {
                                return SetXmlError(returnXml, "Trigger Error: Debe seleccionar Flex Field EXTERNAL COLOR para unidad ESPRESSO");
                            }
                            
                            if ((FFExtColor.ToUpper().Contains(PartNoDesc)) != true)
                            {
                                return SetXmlError(returnXml, "Trigger Error: Flex Field EXTERNAL COLOR [ " + FFExtColor.ToUpper() + " ] NO corresponde al PN-IMEI [ " + PartNoDesc.ToUpper() + " ]");
                            }
                        }
                        else
                        {
                            if (FFExtColor != "" && ((PartNoDesc.ToUpper().Contains("ESPRESSO")) != true))
                            {
                                return SetXmlError(returnXml, "Trigger Error: Solo debe seleccionar valor para Flex Field EXTERNAL COLOR para unidades ESPRESSO, unidad [ " + PartNoDesc + " ]");
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

