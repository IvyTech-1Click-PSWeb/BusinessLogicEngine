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
    public class BBYTRIGGERTAT : JGS.Web.TriggerProviders.TriggerProviderBase 
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
            {"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}		         
            // Extraer valor de Flex Field FF_TATConfirm
            ,{"XML_FFTATConfirm","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='FF_TATConfirm']/Value"}			         
       };

        public override string Name { get; set; }

        public BBYTRIGGERTAT()
        {
            this.Name = "BBYTRIGGERTAT";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string UserName = string.Empty;
            string BCN = string.Empty;            
           
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

            string TAT = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();           
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            //TAT = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYTAT", "TATDAYS", myParams);
            //TAT = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYTAT", "TATDAYS", myParams);
            TAT = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYTAT", "TATDAYS", myParams);

            if (TAT == null || TAT == "NULO")
            {
                return SetXmlError(returnXml, "Trigger Error: Unit NO recibida, favor de verificar");
            }
            else 
            {
                if (Convert.ToInt32(TAT) > 7)
                {                       
                    string FF_TAT = string.Empty;

                    //-- Get Result Code
                    if (!Functions.IsNull(xmlIn, _xPaths["XML_FFTATConfirm"]))
                    {
                        FF_TAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FFTATConfirm"]).Trim();
                    }
                    else
                    {
                        FF_TAT = "";
                    }

                    if (FF_TAT == "" || FF_TAT == "false") 
                    {
                            return SetXmlError(returnXml, "Trigger Error – Unidad con alto TAT, dar prioridad, días en proceso: " + TAT);
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
