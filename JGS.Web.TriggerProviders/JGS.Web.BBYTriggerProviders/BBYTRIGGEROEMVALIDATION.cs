using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.Web.TriggerProviders;

namespace JGS.Web.TriggerProviders
{
   public class BBYTRIGGEROEMVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}  
            ,{"XML_FA_COMP","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList"}
            ,{"XML_PASSWORD","/Trigger/Header/UserObj/PassWord"}
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}
            ,{"XML_OOWBYCONDITION","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='OOWbyCondition']/Value"}
            ,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
	    };
       
        public override string Name { get; set; }

        public BBYTRIGGEROEMVALIDATION()
        {
            this.Name = "BBYTRIGGEROEMVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            List<OracleParameter> myParams;
            string UserName = string.Empty;
            string LocationId = string.Empty;
            string SerialNo = string.Empty;
            string FACOMP = string.Empty;
            string res = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

   

            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }

            //-- Get SerialNo
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SerialNo = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SerialNo.Length, ParameterDirection.Input) { Value = SerialNo });//new parameter
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "BBYTRIGGEROEMVALIDATION", "getOEM", myParams);

            if (res != null)
            {
                if (res != "false")
                {
                    if (Functions.IsNull(xmlIn, _xPaths["XML_OOWBYCONDITION"]))
                    {
                        return SetXmlError(returnXml, "Unidad OEM: " + res + "; por favor llene el FF OOWBYCONDITION/EOM Unit " + res + "; please fill the OOWBYCONDITION FF");
                    }
                }
            }
            else
            {
                return SetXmlError(returnXml, "La unidad no esta activa o tiene un problema para sacar el OEM/The unit is not active or had a problem to get the OEM");

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

        private XmlDocument SetXmlRole(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

    }



}
