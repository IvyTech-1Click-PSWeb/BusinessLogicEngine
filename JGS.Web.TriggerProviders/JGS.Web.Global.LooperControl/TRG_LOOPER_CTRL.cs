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

    public class TRG_LOOPER_CTRL : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        public override string Name { get; set; }

        //////////////////// Parameters List /////////////////////
        List<OracleParameter> myParams;

        ////////////////////////////////  Stored Procs calls ////////////////////////
        string Package_name = "BLE_GLB_TRG_LOOPCTRL";
        //string Schema_name = "GREENSTM";

        public TRG_LOOPER_CTRL()
        {
            this.Name = "TRG_LOOPER_CTRL";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            int locationId;
            int clientId;
            int contractId;
            string orderProcessType = string.Empty;
            int itemId;
            int workcenterId;
            string workcenterName;
            string BCN = string.Empty;
            string UserName = string.Empty;
            string OverridePwd = string.Empty;

            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_LOCATIONID"]))
            {
                locationId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CLIENTID"]))
            {
                clientId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CLIENTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]))
            {
                contractId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
            }

            //-- Get Order Process Type
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_OPT"]))
            {
                orderProcessType = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT cannot be empty.");
            }

            //-- Get Workcenter Id
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WORKCENTERID"]))
            {
                workcenterId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_WORKCENTERID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Id can not be found.");
            }

            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
            }

            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_ItemID"]))
            {
                itemId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_ItemID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "Item Id cannot be empty.");
            }

            //-- Get UserName
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }
            //-- Get_PASSWORD
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_PASSWORD"]))
            {
                OverridePwd = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_PASSWORD"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Password can not be found.");
            }

            /////////// Call Looper Proc ///////////
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("v_LOCATION_ID", OracleDbType.Int32, locationId.ToString().Length, ParameterDirection.Input) { Value = locationId });
            myParams.Add(new OracleParameter("v_CLIENT_ID", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("v_CONTRACT_ID", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("v_OPT", OracleDbType.Varchar2, orderProcessType.Length, ParameterDirection.Input) { Value = orderProcessType });
            myParams.Add(new OracleParameter("v_WORKCENTER_ID", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("v_WORKCENTER_Name", OracleDbType.Varchar2, workcenterName.Length, ParameterDirection.Input) { Value = workcenterName });
            myParams.Add(new OracleParameter("v_ITEM_ID", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
            myParams.Add(new OracleParameter("v_USER_NAME", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            myParams.Add(new OracleParameter("v_OVERRIDE_PWD", OracleDbType.Varchar2, OverridePwd.Length, ParameterDirection.Input) { Value = OverridePwd });
            errMsg = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "ValidateNumberOfLoops", myParams);
            if (errMsg == null)
            {
                return SetXmlError(returnXml, " Err: error while calling PL/SQL package!"); 
            }
            else
            {
                if (errMsg.StartsWith("ERROR"))
                {
                    return SetXmlError(returnXml, " Err: " + errMsg);
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
