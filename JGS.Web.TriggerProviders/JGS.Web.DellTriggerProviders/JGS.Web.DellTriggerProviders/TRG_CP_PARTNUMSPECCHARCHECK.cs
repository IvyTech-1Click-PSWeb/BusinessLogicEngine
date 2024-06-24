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
	public class TRG_CP_PARTNUMSPECCHARCHECK : JGS.Web.TriggerProviders.TriggerProviderBase
	{		
		public override string Name { get; set; }

        //////////////////// Parameters List /////////////////////
       
        public TRG_CP_PARTNUMSPECCHARCHECK()
		{
            this.Name = "TRG_CP_PARTNUMSPECCHARCHECK";
		}

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
           
           ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            string geoName = string.Empty;
            string clientName = string.Empty;
            string contractName = string.Empty;
            string orderProcessType = string.Empty;
            string SN = string.Empty;
            string newPart = string.Empty;
            string newSN = string.Empty;
            //int contractId;
            
            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);


            // Get Serial Number
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }

            if (!string.IsNullOrEmpty(SN.Trim()))
            {
                if (SN.Length != 20)
                {
                    return SetXmlError(returnXml, "Serial Number must be 20 character long.");
                }
            }            
            
            // Get New Serial Number
            //-- New SN
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]))
            {
                newSN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]);
            }
            //****************************************** Begin TRIGGER ***************************************/

            if (!string.IsNullOrEmpty(newSN))
            {   // check S/N length
                if (newSN.Length != 20)
                {
                    return SetXmlError(returnXml, "New Serial Number must be 20 character long.");
                }
                
                String result = checkSpecialCharacterInString(newSN.Substring(0,2).ToUpper(),
                                         "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                if (result != null)
                {
                    return SetXmlError(returnXml, result + "first two characters of PPID must be alpha!");
                }
                result = checkSpecialCharacterInString(newSN.ToUpper(),
                                         "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                if (result != null)
                {
                    return SetXmlError(returnXml, result + "PPID should contain only alpha-numeric characters!");
                }
                ///////// Date Code check 
                // Year
                result = checkSpecialCharacterInString(newSN.Substring(13, 1).ToUpper(),
                                        "0123456789");
                if (result != null)
                {
                    return SetXmlError(returnXml, result + "Invalid Year Code in PPID: " + newSN.Substring(13, 1).ToUpper() + ". Should be in {0123456789}");
                }
                // Month
                result = checkSpecialCharacterInString(newSN.Substring(14, 1).ToUpper(),
                                        "123456789ABC");
                if (result != null)
                {
                    return SetXmlError(returnXml, result + "Invalid Month Code in PPID: " + newSN.Substring(14, 1).ToUpper() + ". Should be in {123456789ABC}");
                }
                // Day
                result = checkSpecialCharacterInString(newSN.Substring(15, 1).ToUpper(),
                                        "123456789ABCDEFGHIJKLMNOPQRSTUV");
                if (result != null)
                {
                    return SetXmlError(returnXml, result + "Invalid Day Code in PPID: " + newSN.Substring(15, 1).ToUpper() + ". Should be in {123456789ABCDEFGHIJKLMNOPQRSTUV}");
                }
            }

                                                          
            //-- New Part Number
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_PART_NUM"]))
            {
                newPart = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_PART_NUM"]);
            }
                       
                       
                Functions.DebugOut("-----  Inside of Change Part Trigger  --------> ");
                if ( !string.IsNullOrEmpty(newPart.Trim()))
                {
                    String result = checkSpecialCharacterInString(newPart.Trim().ToUpper(),
                                         "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-$");
                    if (result != null)
                    {
                        Functions.DebugOut(result);
                        return SetXmlError(returnXml, result + "please enter part number in standard format!");
                        
                    }
                }
                
                Functions.DebugOut("<-----  Exited Change Part trigger -------- ");
                          
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

		public String checkSpecialCharacterInString( String TestString, String controlSet ) {
				
        foreach (char ch in TestString)
        {			
			Functions.DebugOut(" Char at position: " + ch);
			if ( controlSet.IndexOf( ch ) < 0 ) 
            {
				return "Invalid format - ";
			}
		}
		return null;
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

