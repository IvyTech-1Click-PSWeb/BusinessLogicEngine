using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
   public class RIMTRIGGERBERUR : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}  
            ,{"XML_SERVICE_LEVEL","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='SERVICE_LEVEL']/Value"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
	    };

        public override string Name { get; set; }

        public RIMTRIGGERBERUR()
        {
            this.Name = "RIMTRIGGERBERUR";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string diagCode = string.Empty;
            List<OracleParameter> myParams;
            string qty = string.Empty;
            string BCN = string.Empty;
            string UserName = string.Empty;
            string resultCode = string.Empty;
            string Service_Level = string.Empty;


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

            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERVICE_LEVEL"]))
            {
                Service_Level = Functions.ExtractValue(xmlIn, _xPaths["XML_SERVICE_LEVEL"]).Trim().ToUpper();
            }

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



            if (resultCode == "BER-UR")
            {
                 if (Service_Level != "0")
                    {
                        return SetXmlError(returnXml, "Debe seleccionar Service Level 0 / Must select Service Level 0");
                    }
                          
                              
                //////////////////// Get que Qyt of components /////////////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                qty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSTRIGGERBERUR", "GETQTYOFCOMPONENTS", myParams);

                if (qty != null)
                {
                    if (Convert.ToInt16(qty) > 0)
                    {
                        return SetXmlError(returnXml, "Debe remover componentes para este Result code/Please remove the components for this Result Code");
                    }
                }

            }
            else 
            {
                if (Service_Level == "0" || Service_Level == "L3B" || Service_Level == "DOA1")           
                {
                    return SetXmlError(returnXml, "Debe seleccionar Service Level 0, L3B o DOA1 / Must select Service Level 0, L3B or DOA1");
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
