using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERNEWWARRANTYHTCVBS : JGS.Web.TriggerProviders.TriggerProviderBase
    {

    private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"} 
            ,{"XML_FAILCODE","/Trigger/Detail/TimeOut/FailureCodeList/FailureCode/Value"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}                          
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}                 
	    };


        public override string Name { get; set; }

        public BBYTRIGGERNEWWARRANTYHTCVBS()
            {
                this.Name = "BBYTRIGGERNEWWARRANTYHTCVBS";
            }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {

            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            
            //string UserName = string.Empty;
            string resultCode = string.Empty;
            string FailCode = string.Empty;            

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
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FAILCODE"]))
            {
                FailCode = Functions.ExtractValue(xmlIn, _xPaths["XML_FAILCODE"]).Trim().ToUpper();
            }
            else
            {
                FailCode = "";
            }            

            // Start Validations
            
            // RC IW_Rep
            if (resultCode.Trim().ToUpper() == "IW_REP") 
            {
                if (FailCode.Trim().ToUpper() == "")
                {
                    return SetXmlError(returnXml, "Seleccione un código de falla");
                }                
            }
            // RC NFF
            else if (resultCode.Trim().ToUpper() == "NFF") 
            {
                if (FailCode.Trim().ToUpper() != "41.3" && FailCode.Trim().ToUpper() != "42.3" && FailCode.Trim().ToUpper() != "43.3")
                {
                    return SetXmlError(returnXml, "Seleccione un código de falla NFF (41.3, 42.3 o 43.3)");
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

