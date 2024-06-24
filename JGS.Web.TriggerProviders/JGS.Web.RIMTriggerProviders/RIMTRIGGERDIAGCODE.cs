using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
   public class RIMTRIGGERDIAGCODE : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}  
            ,{"XML_DIAGCODE","/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode"}
            ,{"XML_Diagnostic","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Diagnostic']/Value"} 
            ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}

            
	    };


        public override string Name { get; set; }

        public RIMTRIGGERDIAGCODE()
        {
            this.Name = "RIMTRIGGERDIAGCODE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
              string diagCode = string.Empty;
              string SymptomCode = string.Empty;
              string Itemid = string.Empty;
              string UserName = string.Empty;
              int LocationId;
              string workcenterName = string.Empty;

              List<OracleParameter> myParams;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);


            //-- Get Item ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                Itemid = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();

            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
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
            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
            }

            //-- Get DIAGCODE

            //////////////////// Get the Service Level /////////////////////
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("Itemid", OracleDbType.Varchar2, Itemid.Length, ParameterDirection.Input) { Value = Itemid });
            myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            SymptomCode = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMDIGTRIGGER", "GetSymptom", myParams);

            if (workcenterName == "Data_Entry")
            {
                if (SymptomCode != null)
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_Diagnostic"], "yes");
                }
                else
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_Diagnostic"], "no");
                }            
            
            }

            if (workcenterName == "Debug-Rew")
            {
                if (SymptomCode != null)
                {
                    if (Functions.IsNull(xmlIn, _xPaths["XML_DIAGCODE"]))
                    {
                        return SetXmlError(returnXml, "Favor de seleccionar un Diagnostic Code/ Please fill a Diagnostic Code.");
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
