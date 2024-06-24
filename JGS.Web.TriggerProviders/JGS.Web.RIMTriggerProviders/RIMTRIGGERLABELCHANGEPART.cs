using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;


namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERLABELCHANGEPART : JGS.Web.TriggerProviders.TriggerProviderBase
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
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_PNCHANGEPART","/Trigger/Detail/ChangePart/NewPartNo"}
            ,{"XML_SNCHANGEPART","/Trigger/Detail/ChangePart/NewSerialNo"}
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalisys/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES", "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"}
         ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
       };


         public override string Name { get; set; }

         public RIMTRIGGERLABELCHANGEPART()
        {
            this.Name = "RIMTRIGGERLABELCHANGEPART";
        }


         public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
         {
             XmlDocument returnXml = xmlIn;

             //Build the trigger code here
             ////////////////////////////// Variables ///////////////////////////////////////////////////
             string SN = string.Empty;
             string TriggerType = string.Empty;
             string WC = string.Empty;
             string SNChangePart = string.Empty;
             //int StringCampare;
             List<OracleParameter> myParams;
             string UserName = string.Empty;
             string WIPData = string.Empty;

             //-- Get User Name
             if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
             {
                 UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
             }
             else
             {
                 return SetXmlError(returnXml, "User Name can not be found.");
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

             //-- Get Trigger Type
             if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
             {
                 TriggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
             }
             else
             {
                 return SetXmlError(returnXml, "User Name can not be found.");
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
             //-- Get Change Part SN
             if (!Functions.IsNull(xmlIn, _xPaths["XML_SNCHANGEPART"]))
             {
                 SNChangePart = Functions.ExtractValue(xmlIn, _xPaths["XML_SNCHANGEPART"]).Trim().ToUpper();
             }
             else
             {
                 SNChangePart = "";
                 //                return SetXmlError(returnXml, "Serial Number Change Part can not be found.");
             }

               //LABEL Validation..............
             if (TriggerType.ToUpper() == "TIMEOUT")
             {
                 if (WC.ToUpper() == "LABEL")
                 {
                     if (SN.StartsWith("RIM") & SN.Length == 24)
                     {
                         return SetXmlError(returnXml, "Trigger Error: Debe realizar el Change Serial Number a este BCN antes de direccionar la unidad a el siguiente WC" +
                             " / You must perform the Change Serial Number to the BCN before addressing the unit to the next WC");
                     }

                     if (SN.Substring(0, 1).ToUpper() == "1")
                     {
                         if (SN.Length == 10)
                         {
                             return SetXmlError(returnXml, "Trigger Error: Debe realizar el Change Serial Number a este BCN antes de direccionar la unidad a el siguiente WC" +
                             " / You must perform the Change Serial Number to the BCN before addressing the unit to the next WC");                             
                         }
                     }
                 }
             }                          
                          
             //CHANGESERIAL Validation...............
             if (TriggerType.ToUpper() == "CHANGEPART")
             {
                 if (SNChangePart.Length > 0)
                 {
                     if (SNChangePart.Length > 10)
                     {
                         // Validaciones de nuevo número de serie
                         if (SNChangePart.Substring(0, 2).ToUpper() == "07")
                         {
                             if (SNChangePart.Length != 11)
                             {
                                 return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 07, este debe de contener 11 dígitos.");
                             }
                         }
                         else if (SNChangePart.Substring(0, 6).ToUpper() == "A00000")
                         {
                             if (SNChangePart.Length != 14)
                             {
                                 return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con A0, este debe de contener 14 dígitos.");
                             }
                             else
                             {
                                 int SNLen;

                                 SNLen = SNChangePart.Length;

                                 if (System.Text.RegularExpressions.Regex.IsMatch(SNChangePart.Substring(6, SNLen - 6).ToUpper(), "^[a-fA0-F9]+$")) { }
                                 else
                                 {
                                     return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con A0, este debe de contener solo números o letras de la A a la F.");
                                 }
                             }
                         }
                         else if (SNChangePart.Substring(0, 2).ToUpper() == "35")
                         {
                             if (SNChangePart.Length != 15)
                             {
                                 return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 35, este debe de contener 15 dígitos.");
                             }
                             else if (System.Text.RegularExpressions.Regex.IsMatch(SNChangePart.Trim().ToUpper(), "^[0-9]*$")) { }
                             else
                             {
                                 return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 35, este debe de contener solo números");
                             }
                         }

                         else if (SNChangePart.Substring(0, 3).ToUpper() == "RIM")
                         {
                             if (SNChangePart.Length != 24)
                             {
                                 return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con RIM, este debe de contener 24 dígitos.");

                             }

                             string RIMDate = SNChangePart.Substring(3, 21).ToUpper();
                             DateTime temp;

                             if (!DateTime.TryParse(RIMDate.ToString(), out temp))
                             {
                                 return SetXmlError(returnXml, "Trigger Error: Ingresó un numero de serial No Valido favor de checarlo");
                             }

                         }
                         else
                         {
                             return SetXmlError(returnXml, "Trigger Error: Ingresó un número de serie no válido. Los números de serie deben de empezar con 07, 35, A00000 o RIM.");
                         }
                     }
                     else
                     {
                         return SetXmlError(returnXml, "Trigger Error: Ingresó un número de serie con longitud inválida / Invalid Serial Number Length");
                     }


                     //////////////////// GCheck if serial is in WIP  /////////////////////
                     myParams = new List<OracleParameter>();
                     myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, SNChangePart.Length, ParameterDirection.Input) { Value = SNChangePart });
                     myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                     WIPData = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERSNINWIP", "CHECKIFEXISTSERIAL", myParams);

                     if (WIPData != null)
                     {
                         return SetXmlError(returnXml, "Trigger Error: El serial number que ingreso está actualmente activo en el WIP, no puede ser utilizado para el cambio. Verifique nuevamente el serial.");
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
