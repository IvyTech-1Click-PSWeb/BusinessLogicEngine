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
    public class TRG_CP_UPDATE_FFS_XML_WC : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        public override string Name { get; set; }

        //////////////////// Parameters List /////////////////////
        List<OracleParameter> myParams;

        ////////////////////////////////  Stored Procs calls ////////////////////////
        string Package_name = "TRG_NET_DELLRRTIMEOUT";

        public TRG_CP_UPDATE_FFS_XML_WC()
        {
            this.Name = "TRG_CP_UPDATE_FFS_XML_WC";
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
            int contractId;
            string BCN = string.Empty;
            string WarrantyTermsFF = string.Empty;

            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            // Get New Serial Number
            //-- New SN
            if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]))
            {
                newSN = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CP_NEW_SERIAL_NUM"]);
            }

            if (!string.IsNullOrEmpty(newSN.Trim()))
            {
                if (newSN.Length != 20)
                {
                    return SetXmlError(returnXml, "New Serial Number must be 20 character long.");
                }
                
                ////// Update values on Workcenter level to update CP 
                string NewFFDellPN = newSN.Substring(3, 5);
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "Dell PN")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "Dell PN"), NewFFDellPN);
                }
                else
                {
                    return SetXmlError(returnXml, "Work center flex field \"Dell PN\" cannot be found for update.");
                }
                
                string NewFFOEMWarr = string.Empty;
                
                //////////// Get all necessary date to calculate OEM warranty for new SN
                //-- Get Contract Id
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]))
                {
                    contractId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]));
                }
                else
                {
                    return SetXmlError(returnXml, "Contract Id cannot be found.");
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

                Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
                {
                    {Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
                };

                //Call the DB to get contract name ///////////////
                queries["ContractName"].ParameterValue = contractId.ToString();
                Functions.GetMultipleDbValues(this.ConnectionString, queries);

                contractName = queries["ContractName"].Result;

                if (String.IsNullOrEmpty(contractName))
                {
                    Functions.DebugOut("Contract Name can not be found.");
                    return SetXmlError(returnXml, "Contract Name cannot be found.");
                }
                /////////// Get Warranty Terms /////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, newSN.Length, ParameterDirection.Input) { Value = newSN });
                myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contractName.ToString().Length, ParameterDirection.Input) { Value = contractName });
                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, orderProcessType.Length, ParameterDirection.Input) { Value = orderProcessType });
                WarrantyTermsFF = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "GetLCDWarrantyTerms", myParams);

                if (WarrantyTermsFF.StartsWith("NOT_FOUND"))
                {
                    Functions.DebugOut("Could not get warranty term for SN  " + SN);
                    return SetXmlError(returnXml, "Could not get warranty terms for SN  " + SN);
                }
                if (WarrantyTermsFF.StartsWith("ERROR"))
                {
                    return SetXmlError(returnXml, "Change Part - Error while calling GetLCDWarrantyTerms for SN " + SN + " Err: " + WarrantyTermsFF);
                }

                // Subassebly LCD repair for change part requires work center FFs updates
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "Warranty Terms")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "Warranty Terms"), WarrantyTermsFF);
                }
                else
                {
                    return SetXmlError(returnXml, "Work center flex field \"Warranty Terms\" cannot be found for update.");
                }
                
                /////////// Get Warranty Status ///////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, newSN.Length, ParameterDirection.Input) { Value = newSN });
                myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contractName.ToString().Length, ParameterDirection.Input) { Value = contractName });
                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, orderProcessType.Length, ParameterDirection.Input) { Value = orderProcessType });
                NewFFOEMWarr = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "GetLCDWarrantyStatus", myParams);

                if (NewFFOEMWarr.StartsWith("NOT_FOUND"))
                {
                    Functions.DebugOut("Could not calculate OEM warranty for SN  " + newSN);
                    return SetXmlError(returnXml, "Could not calculate OEM warranty for new SN  " + newSN);
                }
                if (NewFFOEMWarr.StartsWith("ERROR"))
                {
                    return SetXmlError(returnXml, "Change Part - Error while calling GetLCDWarrantyStatus for new SN: " + newSN + " Err: " + NewFFOEMWarr);
                }
                // Subassebly LCD repair for change part requires work center FFs updates
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "OEM Warranty")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "OEM Warranty"), NewFFOEMWarr);
                }
                else
                {
                    return SetXmlError(returnXml, "Work center flex field \"OEM Warranty\" cannot be found for update.");
                }

                //Call Change part wrapper to update warranty
                //string err = UpdateFlexFieldsOnDB(BCN, NewFFOEMWarr, NewFFDellPN);
                //if (err != "SUCCESS")
                //{
                //    Functions.DebugOut("Trigger Check - Change Part Wrapper: failed to update Warranty/Dell PN for " + BCN + " " + err);
                //    return SetXmlError(returnXml, "Change Part Wrapper: failed to update Warranty/Dell PN for " + BCN + " " + err);
                //}
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


        /// <summary>
        /// Updates Warranty and Dell PN on database
        /// </summary>
        /// <param name="BCN"></param>
        /// <param name="WarrSts"></param>
        /// <param name="WarrTerms"></param>
        /// <param name="DellPN"></param>
        /// <returns></returns>
        protected string UpdateFlexFieldsOnDB(string BCN, string WarrSts, string DellPN)
        {
            string result = "";
            try
            {
                JGS.Web.TriggerProviders.CP.ChangePartWrapper CPobj = new JGS.Web.TriggerProviders.CP.ChangePartWrapper();
                JGS.Web.TriggerProviders.CP.ChangePartInfo cpi = new JGS.Web.TriggerProviders.CP.ChangePartInfo();

                cpi.SesCustomerID = "1";
                cpi.RequestId = "1";
                cpi.BCN = BCN;
                //'cpi.NewPartNo = "N1208"
                //'cpi.NewSerialNo = "356028035566620"
                //'cpi.BCN = "BCN91508162"
                //'cpi.NewSerialNo = "AIC028035566620"
                //'cpi.NewPartNo = "NBB0300000"
                //'cpi.NewRevisionLevel = "1"
                cpi.NewFixedAssetTag = "";
                cpi.Notes = "";
                cpi.MustBeOnHold = false;
                cpi.ReleaseIfHold = false;
                cpi.MustBeTimedIn = false;
                cpi.TimedInWorkCenterName = " ";
                cpi.userName = "DELL_TRG";

                CP.FlexFields ff0 = new CP.FlexFields();
                ff0.Name = "OEM Warranty";
                ff0.Value = WarrSts;
                                
                CP.FlexFields ff1 = new CP.FlexFields();
                ff1.Name = "Dell PN";
                ff1.Value = DellPN;

                cpi.FlexFieldList = new CP.FlexFields[2];
                cpi.FlexFieldList[0] = ff0;
                cpi.FlexFieldList[1] = ff1;
                
                result = CPobj.PerformChangePart(cpi, false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

    }
}

