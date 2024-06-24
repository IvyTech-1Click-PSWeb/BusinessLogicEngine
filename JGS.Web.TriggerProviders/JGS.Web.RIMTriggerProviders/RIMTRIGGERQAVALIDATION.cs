using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERQAVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
            {"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_ITEMID","/Trigger/Detail/ItemLevel/ItemID"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_SYMCODE","/Trigger/Detail/TimeOut/SymptomCodeList/SymptomCode/Value"}
        };

        public override string Name { get; set; }

        public RIMTRIGGERQAVALIDATION()
        {
            this.Name = "RIMTRIGGERQAVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {

            XmlDocument returnXml = xmlIn;

            //Valores Requeridos para JGSRIMQAVALIDATION
            string SymptomCode = string.Empty;
            string ResultCode = string.Empty;
            string UserName = string.Empty;
            string Bcn = string.Empty;
            string SerialNo = string.Empty;
            string Workcenter = string.Empty;
            string Trigger = string.Empty;

            //runtime variables
            string Result = string.Empty;
            string ComponentsCount = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);



            //-- Get Symptom Code.
            // Notar que si es NULL no genera un error,
            // esta validacion es realizada solo para cierta combinacion WC/RC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SYMCODE"]))
            {
                SymptomCode = Functions.ExtractValue(xmlIn, _xPaths["XML_SYMCODE"]).Trim();
            }

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                ResultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Result Code can not be found.");
            }

            //-- Get WC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                Workcenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
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


            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                Bcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            if (Trigger.ToUpper() == "TIMEOUT")
            {
                //IT Project Request 1291
                if (Workcenter.ToUpper() == "QA") //if (Workcenter.ToUpper() == "DATA_ENTRY")
                {
                    //IT Project Request 1291 - Objective 1
                    if (ResultCode.ToUpper() == "DBG REW"
                        || ResultCode.ToUpper() == "VMI_INSP"
                        || ResultCode.ToUpper() == "FAIL")
                    {
                        if (SymptomCode == String.Empty || SymptomCode == null)
                        {
                            return SetXmlError(returnXml, "Trigger Error - Debe seleccionar un codigo de falla, para este result Code seleccionado.");
                        }
                    }

                    //IT Project Request 1291 - Objective 3
                    if (ResultCode.ToUpper() == "BER-UR") //if (ResultCode.ToUpper() == "OMFT")
                    {
                        List<OracleParameter> myParams;
                        // Verifico historial y componentes cargados en la misma funcion...
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("p_item_bcn", OracleDbType.Varchar2, Bcn.Length, ParameterDirection.Input) { Value = Bcn.ToUpper() });
                        myParams.Add(new OracleParameter("p_workcenter_name", OracleDbType.Varchar2, Workcenter.Length, ParameterDirection.Input) { Value = "BER" });
                        myParams.Add(new OracleParameter("p_result_code", OracleDbType.Varchar2, ResultCode.Length, ParameterDirection.Input) { Value = ResultCode });
                        myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        Result = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMQAVALIDATION", "checkQaRules", myParams);
                        if (Result != "SUCCESS")
                        {
                            return SetXmlError(returnXml, "Trigger Error - Esta unidad no cumple las reglas para ser direccionada como BER, favor de validar.");
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

        ////-- Get BCN
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
        //{
        //    BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
        //}
        //else
        //{
        //    return SetXmlError(returnXml, "BCN can not be found.");
        //}

        ////-- Get OPT Name
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
        //{
        //    OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim();
        //}
        //else
        //{
        //    return SetXmlError(returnXml, "OPT can not be found.");
        //}

        ////-- Get Symtomp Code
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_SYMCODE"]))
        //{
        //    SymCode = Functions.ExtractValue(xmlIn, _xPaths["XML_SYMCODE"]).Trim().ToUpper();
        //}
        //else
        //{
        //    SymCode = "";
        //}

        ////-- Get Diagnostic Code
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_DIACODE"]))
        //{
        //    DiaCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DIACODE"]).Trim().ToUpper();
        //}
        //else
        //{
        //    DiaCode = "";
        //}

        ////-- Get FF WC Diagnostic
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_FFWCDiagnostic"]))
        //{
        //    FFWCDiagnostic = Functions.ExtractValue(xmlIn, _xPaths["XML_FFWCDiagnostic"]).Trim().ToUpper();
        //}
        //else
        //{
        //    FFWCDiagnostic = "";
        //}

        ////-- Get Serial Number
        //if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
        //{
        //    SerialNo = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim();
        //}
        //else
        //{
        //    return SetXmlError(returnXml, "Serial Number can not be found.");
        //}
    }
}
