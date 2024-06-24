using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class TRG_CHECK_FAT : JGS.Web.TriggerProviders.TriggerProviderBase
    {
       
        public override string Name { get; set; }

        public TRG_CHECK_FAT()
        {
            this.Name = "TRG_CHECK_FAT";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
      
            string FAT;
                                    
            //BEGIN
            Functions.DebugOut("--------  TRG_CHECK_FAT  -------->");
            
            // Set Return Code to Success
            SetXmlSuccess(returnXml);
                                  

           //-- Get FAT
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_FIXEDASSETTAG"]))
            {
                FAT = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_FIXEDASSETTAG"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Fixed Asset Tag is required!");
            }


            Functions.DebugOut("<-----  Exited TRG_CHECK_FAT -------- ");

            //}            

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
            Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_MESSAGE"], message);
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
            Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_RESULT"], EXECUTION_OK);            
        }

    }
}
