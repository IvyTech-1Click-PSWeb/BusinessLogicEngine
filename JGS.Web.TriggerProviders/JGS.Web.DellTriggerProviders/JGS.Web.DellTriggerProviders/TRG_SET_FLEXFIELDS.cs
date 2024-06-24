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
    public class TRG_SET_FLEXFIELDS : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
			//FA Fields
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			//To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalisys/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES", "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
                 "DefectCode[DefectCodeName='{DEFECTCODE}']"}
            ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
                 "DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
                        
		};

        public override string Name { get; set; }

        public TRG_SET_FLEXFIELDS()
        {
            this.Name = "TRG_SET_FLEXFIELDS";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Package_name = "TRG_NET_DELLRRTIMEOUT";

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            int locationId;
            int clientId;
            int contractId;
            string geoName = string.Empty;
            string clientName = string.Empty;
            string contractName = string.Empty;
            string resultCode;
            string orderProcessType;
            string SN;
            string BCN;
            int itemId;
            int workcenterId;
            string workcenterName;
            string partNumber;

           //Local variables for B2B calls //////////////////////////
            //string err;
           
            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            //FF Variable
            string DellPartNumberFF;
            string WarrantyTermsFF;
            string WarrStatusFF;

        
            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                locationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
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

            //-- Get Order Process Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                orderProcessType = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT cannot be empty.");
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }

            if (SN.Length != 20)
            {
                return SetXmlError(returnXml, "Serial Number must be 20 character long.");
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

            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "Item Id cannot be empty.");
            }
                        
            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                partNumber = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.");
            }

            //-- Get Workcenter
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                workcenterId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Id can not be found.");
            }
            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
            }

            Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
            {
                {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
              , {Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
              , {Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
            };

            //Call the DB to get necessary data from Oracle ///////////////

            queries["GeoName"].ParameterValue = locationId.ToString();
            queries["ClientName"].ParameterValue = clientId.ToString();
            queries["ContractName"].ParameterValue = contractId.ToString();
            Functions.GetMultipleDbValues(this.ConnectionString, queries);

            geoName = queries["GeoName"].Result;
            clientName = queries["ClientName"].Result;
            contractName = queries["ContractName"].Result;

            if (String.IsNullOrEmpty(geoName))
            {
                Functions.DebugOut("Geography Name can not be found.");
                return SetXmlError(returnXml, "Geography Name can not be found.");
            }
            if (String.IsNullOrEmpty(clientName))
            {
                Functions.DebugOut("Client Name can not be found.");
                return SetXmlError(returnXml, "Client Name can not be found.");
            }
            if (String.IsNullOrEmpty(contractName))
            {
                Functions.DebugOut("Contract Name can not be found.");
                return SetXmlError(returnXml, "Contract Name can not be found.");
            }

            ///////////////////////// Display values for debug /////////////////////////////
            Functions.DebugOut("----------  Set Flex Fields Variables  -------");
            Functions.DebugOut("GeoName:        " + geoName);
            Functions.DebugOut("ClientName:     " + clientName);
            Functions.DebugOut("ContractName:   " + contractName);
            Functions.DebugOut("ResultCode:     " + resultCode);
            Functions.DebugOut("OPT:            " + orderProcessType);
            Functions.DebugOut("SN:             " + SN);
            Functions.DebugOut("BCN:            " + BCN);
            Functions.DebugOut("ItemId:         " + itemId);
            Functions.DebugOut("PartNumber:     " + partNumber);
            Functions.DebugOut("workCenterId:   " + workcenterId);
            Functions.DebugOut("WorkCenterName: " + workcenterName);
            Functions.DebugOut("--------------------------------");


            //****************************************** Begin TRIGGER ***************************************/

            /************************************** Deliverable #4 OEM Warranty Upgrade ***************************************/
            
                Functions.DebugOut("-----  Inside of Del.#4 - Calculate OEM Warranty and set 3 FFs  --------> ");
                /////////// Get Warranty Status ///////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contractName.ToString().Length, ParameterDirection.Input) { Value = contractName });
                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, orderProcessType.Length, ParameterDirection.Input) { Value = orderProcessType });
                WarrStatusFF = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "GetLCDWarrantyStatus", myParams);

                if (WarrStatusFF.StartsWith("NOT_FOUND"))
                {
                    Functions.DebugOut("Could not calculate OEM warranty for SN  " + SN);
                    return SetXmlError(returnXml, "Could not calculate OEM warranty for SN  " + SN);
                }
                if (WarrStatusFF.StartsWith("ERROR"))
                {
                    return SetXmlError(returnXml, "Oracle Error while calling GetLCDWarrantyStatus for SN " + SN + " Err: " + WarrStatusFF);
                }

                ////// Get Warranty Terms //////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
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
                    return SetXmlError(returnXml, "Oracle Error while calling GetLCDWarrantyTerms for SN " + SN + " Err: " + WarrantyTermsFF);
                }
                /////////// Get Dell Part Number ////////////
                DellPartNumberFF = SN.Substring(3, 5);
                if (string.IsNullOrEmpty(DellPartNumberFF) || DellPartNumberFF.Length != 5)
                {
                    Functions.DebugOut("Bad Dell PN for SN  " + SN);
                    return SetXmlError(returnXml, "Bad Dell PN for SN  " + SN);
                }

                // Call Change part wrapper to update warranty
                //err = UpdateFlexFieldsOnDB(BCN, WarrStatusFF, WarrantyTermsFF, DellPartNumberFF);
                //if (err != "SUCCESS")
                //{
                //    Functions.DebugOut("Trigger Check - Change Part Wrapper: failed to update Warranty/Dell PN for " + BCN + " " + err);
                //    return SetXmlError(returnXml, "Change Part Wrapper: failed to update Warranty/Dell PN for " + BCN + " " + err);
                //}
                /// Set Warranty /////////
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "OEM Warranty")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "OEM Warranty"), WarrStatusFF);
                }
                else
                {
                    return SetXmlError(returnXml, "Work Center flex field \"OEM Warranty\" can not be found for update.");
                }
                /// Set Warranty Terms
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "Warranty Terms")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "Warranty Terms"), WarrantyTermsFF);
                }
                else
                {
                    return SetXmlError(returnXml, "Work Center flex field \"Warranty Terms\" can not be found for update.");
                }
                /// Set Dell PN
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "Dell PN")))
                {
                    Functions.UpdateXml(ref returnXml, xPathDictionary._xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "Dell PN"), DellPartNumberFF);
                }
                else
                {
                    return SetXmlError(returnXml, "Work Center flex field \"Dell PN\" can not be found for update.");
                }

                Functions.DebugOut("<-----  Exited Del.#4 -  Calculate OEM Warranty and set 3 FFs  -------- ");
                                    
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
        /// Update the various fields in the XmlDocument after all the validation is completed.
        /// </summary>
        private void UpdateFields()
        {
        }

        protected string UpdateFlexFieldsOnDB(string BCN, string WarrSts, string WarrTerms, string DellPN)
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
                ff1.Name = "Warranty Terms";
                ff1.Value = WarrTerms;

                CP.FlexFields ff2 = new CP.FlexFields();
                ff2.Name = "Dell PN";
                ff2.Value = DellPN;

                cpi.FlexFieldList = new CP.FlexFields[3];
                cpi.FlexFieldList[0] = ff0;
                cpi.FlexFieldList[1] = ff1;
                cpi.FlexFieldList[2] = ff2;

                result = CPobj.PerformChangePart(cpi, false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

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

