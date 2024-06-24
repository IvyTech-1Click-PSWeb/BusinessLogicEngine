using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERSYMPTOMCODE : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SYMPTOM_CODE","/Trigger/Detail/TimeOut/SymptomCodeList/SymptomCode/Value"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}


	    };


        public override string Name { get; set; }

        public RIMTRIGGERSYMPTOMCODE()
        {
            this.Name = "RIMTRIGGERSYMPTOMCODE";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string UserName = string.Empty;
            string SymptomCode = string.Empty;
            string resultCode = string.Empty;
            string workcenterName = string.Empty;
                         
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

            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SYMPTOM_CODE"]))
            {
                SymptomCode = Functions.ExtractValue(xmlIn, _xPaths["XML_SYMPTOM_CODE"]).Trim();
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

            if (workcenterName == "Bounce")
            {
                if (resultCode == "Ope_IN")
                {
                    if ((SymptomCode == "") || (SymptomCode == null))
                    {
                        return SetXmlError(returnXml, "Debe seleccionar un código de falla/ Please fill a SymptomCode.");
                    }
                }
            }
            else
            {
                if ((resultCode == "DataEnt") || (resultCode == "Dbg Rew") || (resultCode == "VMI_Insp"))
                {                
                        if ((SymptomCode == "") || (SymptomCode == null))
                        {
                            return SetXmlError(returnXml, "Debe seleccionar un código de falla,para este result Code seleccionado/ Please fill a SymptomCode.");
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


        public string getSHA1Hash(string input)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider SHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] data = SHA1Hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(input));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            int i = 0;
            for (i = 0; i <= data.Length - 1; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }      
    }
}
