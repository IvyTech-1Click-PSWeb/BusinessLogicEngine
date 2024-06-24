using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERSYMINBER : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}        
	    };

        public override string Name { get; set; }

        public RIMTRIGGERSYMINBER()
        {
            this.Name = "RIMTRIGGERSYMINBER";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string NumberOfReceipt = string.Empty;
            string Itemid = string.Empty;
            string UserName = string.Empty;
            string resultCode = string.Empty;
            string SymptomCode = string.Empty;
            int LocationId;
            List<OracleParameter> myParams;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

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

            //-- Get ItemID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                Itemid = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //////////////////// Get the Service Level /////////////////////
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("Itemid", OracleDbType.Varchar2, Itemid.Length, ParameterDirection.Input) { Value = Itemid });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            SymptomCode = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGRRIMPOPULATESYM", "GetOPEINSYM", myParams);

            if (SymptomCode != null)
            {
                XmlDocument returnXmlUpdate = returnXml;
                XmlNode oldCd;
                XmlElement root = returnXmlUpdate.DocumentElement;
                oldCd = root.SelectSingleNode("/Trigger/Detail/TimeOut/SymptomCodeList");
                oldCd.InnerXml = "<SymptomCode><Name>" + SymptomCode + "</Name><Value>" +
                                SymptomCode + "</Value></SymptomCode>";

                returnXml = returnXmlUpdate;
                _xPaths.Add("XML_SYMTOM", "/Trigger/Detail/TimeOut/SymptomCodeList/SymptomCode[Name='" + SymptomCode + "']/Value");
                Functions.UpdateXml(ref returnXml, _xPaths["XML_SYMTOM"], SymptomCode);
            }
            else 
            {
                return SetXmlError(returnXml, "Symptom Code no fue agregado en la estación de Ope_IN.");
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
