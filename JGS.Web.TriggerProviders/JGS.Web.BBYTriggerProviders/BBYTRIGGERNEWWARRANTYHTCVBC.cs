using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERNEWWARRANTYHTCVBC : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"} 
            ,{"XML_SYMCODE","/Trigger/Detail/TimeOut/SymptomCodeList/SymptomCode/Value"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}                 
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            // Extraer valor de Flex Field OOWbyCondition
            ,{"XML_OOWbyCondition","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='OOWbyCondition']/Value"}			         
	    };


        public override string Name { get; set; }

        public BBYTRIGGERNEWWARRANTYHTCVBC()
            {
                this.Name = "BBYTRIGGERNEWWARRANTYHTCVBC";
            }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {

            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
                        
            string resultCode = string.Empty;
            string SymCode = string.Empty;
            string OOWbyCondition = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
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

            //-- Get FF OOWbyCondition Value
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OOWbyCondition"]))
            {
                OOWbyCondition = Functions.ExtractValue(xmlIn, _xPaths["XML_OOWbyCondition"]).Trim();
            }
            else
            {
                OOWbyCondition = "";
            }                                  

            // Start Validations

            // RC No Power
            if (resultCode.Trim().ToUpper() == "NO_POWER") 
            {                
                if (OOWbyCondition.Trim().ToUpper() != "FALSE")
                {
                    return SetXmlError(returnXml, "Unidad fuera de garantía por condición, no puede direccionar como ‘No Power’");
                }
                else 
                {
                    if (SymCode.Trim().ToUpper() != "H")
                    {
                        return SetXmlError(returnXml, "Seleccione el código de falla ‘H - No Power’ para este Result Code");
                    }
                }
            }
            // RC OOW
            else if (resultCode.Trim().ToUpper() == "OOW") 
            { 
                if (OOWbyCondition.Trim().ToUpper() != "TRUE")
                {
                    return SetXmlError(returnXml, "Unidad dentro de garantía por condición, no puede direccionar como ‘OOW’");
                }
                else 
                {
                    if (SymCode.Trim().ToUpper() == "")
                    {
                        return SetXmlError(returnXml, "Unidad fuera de garantía por condición, seleccione un código de síntoma");
                    }
                }
            }
            // RC PASS
            else if (resultCode.Trim().ToUpper() == "PASS")
            {
                if (OOWbyCondition.Trim().ToUpper() != "FALSE")
                {
                    return SetXmlError(returnXml, "No puede seleccionar este Result Code para unidades fuera de garantía");
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
