using System;
using System.Collections.Generic;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using System.Xml;


namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERBOUNCE : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
           ,{"XML_BOUNCE_DISPOSITION","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Bounce_Disposition']/Value"} 
           ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
           ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
           ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}

        };
        
        public override string Name { get; set; }

        public RIMTRIGGERBOUNCE()
        {
            this.Name = "RIMTRIGGERBOUNCE";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            System.Xml.XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string SN = string.Empty;
            string FFBounce = string.Empty;
            string ResulCode = string.Empty;
            string GetStatus = string.Empty;


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

            //-- Get FF Bounce Value
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]))
            {
                FFBounce = Functions.ExtractValue(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]).Trim().ToUpper();
            }
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                ResulCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim().ToUpper();
            }



            if (FFBounce == "VALID")
            {
                if (ResulCode != "SWAP_RED")
                {
                    return SetXmlError(returnXml, "Debe seleccionar SWAP_RED como Result Code /You must select SWAP_RED as Result Code");
                }
            }
            
            return returnXml;

        }

        private void SetXmlSuccess(System.Xml.XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }


        private XmlDocument SetXmlError(System.Xml.XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

    }
}