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
    public class TRG_CP_PNNOTEQUALDELLPN : JGS.Web.TriggerProviders.TriggerProviderBase
    {
       
        public override string Name { get; set; }

        public TRG_CP_PNNOTEQUALDELLPN()
        {
            this.Name = "TRG_CP_PNNOTEQUALDELLPN";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;

            string SN = string.Empty;
            string partNumber = string.Empty;
            string newPart = string.Empty;
            string DellPN = string.Empty;
            string newSN = string.Empty;


            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }

            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_PARTNO"]))
            {
                partNumber = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_PARTNO"]).Trim();
            }

            //-- New Part Number
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_PART_NUM"]))
            {
                newPart = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_PART_NUM"]);
            }

            //-- New SN
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]))
            {
                newSN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]);
            }

            //-- Dell Part Number
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_ITEMLEVEL_FLEX_FIELD"].Replace("{FLEXFIELDNAME}", "Dell PN")))
            {
                DellPN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_ITEMLEVEL_FLEX_FIELD"]
                    .Replace("{FLEXFIELDNAME}", "Dell PN"));
            }
            
            //*************************** if new part number field is filled use it *****************************/
            if (!string.IsNullOrEmpty(newPart))
            {
                partNumber = newPart;
            }

            //****************************************** Begin TRIGGER Logic ***********************************/

            Functions.DebugOut("-----  Inside of TRG_CP_PNNOTEQUALDELLPN block --------> ");

            if ( !string.IsNullOrEmpty(partNumber.Trim()) && !string.IsNullOrEmpty(DellPN.Trim()) && !string.IsNullOrEmpty(SN.Trim()))
            {
                // New Part number should NOT be the same as Dell PN
                if (partNumber.ToUpper() == DellPN.ToUpper())
                {
                    return SetXmlError(returnXml, "Verify Line Part Number field is populated with OEM Part Number!");
                }
                //////// check SN
                if (!string.IsNullOrEmpty(newSN))
                {
                    SN = newSN;
                }
                if (partNumber.ToUpper() == SN.Substring(3, 5).ToUpper())
                {
                    return SetXmlError(returnXml, "New Part Number should not match part number embedded in the Serial Number field!");
                }
                // Check to ensure Dell PN is the same as embedded one in SN
                if (DellPN.ToUpper() != SN.Substring(3, 5).ToUpper())
                {
                    return SetXmlError(returnXml, "The part number from Serial Number must match Dell PN field!");
                }
                Functions.DebugOut("<-----  Exited TRG_CP_PNNOTEQUALDELLPN block -------- ");

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
