using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using Oracle.DataAccess.Types;
using JGS.DAL;
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;


namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERSCRAP : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}            
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}  
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}                
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}            
	    };

        public override string Name { get; set; }

        public BBYTRIGGERSCRAP()
            {
                this.Name = "BBYTRIGGERSCRAP";
            }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {

            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            
            string UserName = string.Empty;
            string resultCode = string.Empty;            
            string SN = string.Empty;

            //List<OracleParameter> myParams;

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

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
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

            // Inicia desarrollo

            string Scrap = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocName", OracleDbType.Varchar2, "Reynosa".Length, ParameterDirection.Input) { Value = "Reynosa" });
            myParams.Add(new OracleParameter("ClientName", OracleDbType.Varchar2, "Best Buy".Length, ParameterDirection.Input) { Value = "Best Buy" });
            myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, "BBY Rapid Exchange".Length, ParameterDirection.Input) { Value = "BBY Rapid Exchange" });
            myParams.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, "WRP".Length, ParameterDirection.Input) { Value = "WRP" });
            myParams.Add(new OracleParameter("WCName", OracleDbType.Varchar2, "ERWC".Length, ParameterDirection.Input) { Value = "ERWC" });
            myParams.Add(new OracleParameter("ConditionName", OracleDbType.Varchar2, "SCRAP".Length, ParameterDirection.Input) { Value = "SCRAP" });
            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            //Scrap = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYSCRAP", "GetScrapHistory", myParams);
            //Scrap = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYSCRAP", "GetScrapHistory", myParams);
            Scrap = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSCRAP", "GetScrapHistory", myParams);

            if (Scrap != null && Scrap != "NULO" && Scrap != "0")
            {
                if (resultCode.ToUpper() != "OEM_SCRA")
                {
                    return SetXmlError(returnXml, "Esta unidad se embarcó como SCRAP, seleccione result code OEM_SCRA");
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
