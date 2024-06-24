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
    public class TRG_B2B_VALIDATIONS : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public TRG_B2B_VALIDATIONS()
        {
            this.Name = "TRG_B2B_VALIDATIONS";
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
            string orderProcessType;
            string SN;
            string BCN;
            int itemId;
            int workcenterId;
            string workcenterName;
            string partNumber;

            //Local variables for B2B calls //////////////////////////
            int n;
            string err;
            int DebugSetting = 1;

            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

                        
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

            //-- Get FAT
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            //{
            //    FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]).Trim();
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Fixed Asset Tag could not be found.");
            //}

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

            // New check FAT function
            //FAT = CommonChecks.CheckFAT(xmlIn, orderProcessType, contractName, this.ConnectionString);
            //if (FAT.StartsWith("Fixed Asset Tag check"))
            //{
            //    Functions.DebugOut("Fixed Asset Tag can not be found.");
            //    return SetXmlError(returnXml, "Fixed Asset Tag can not be found.");
            //}
                
                
            ///////////////////////// Display values for debug /////////////////////////////
            Functions.DebugOut("----------  Check B2B Variables  -------");
            Functions.DebugOut("GeoName:        " + geoName);
            Functions.DebugOut("ClientName:     " + clientName);
            Functions.DebugOut("ContractName:   " + contractName);
            Functions.DebugOut("OPT:            " + orderProcessType);
            Functions.DebugOut("SN:             " + SN);
            Functions.DebugOut("BCN:            " + BCN);
            Functions.DebugOut("ItemId:         " + itemId);
            Functions.DebugOut("PartNumber:     " + partNumber);
            Functions.DebugOut("workCenterId:   " + workcenterId);
            Functions.DebugOut("WorkCenterName: " + workcenterName);
            Functions.DebugOut("--------------------------------");


            //****************************************** Begin TRIGGER ***************************************/
           /****************************************** Check B2B - message flag ***************************************/
            
                Functions.DebugOut("-----  Inside of Del.#3 - B2B Timeout block --------> ");

                n = 1;
                //err := getB2BMsgFlag( itemId, SN, 'RECEIVE', n, isDebug );

                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("p_itemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("p_sn", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("p_msg", OracleDbType.Varchar2, "RECEIVE".Length, ParameterDirection.Input) { Value = "RECEIVE" });
                myParams.Add(new OracleParameter("n", OracleDbType.Int32, n.ToString().Length, ParameterDirection.Input) { Value = n });
                myParams.Add(new OracleParameter("isDebug", OracleDbType.Int32, DebugSetting.ToString().Length, ParameterDirection.Input) { Value = DebugSetting });

                err = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "getB2BMsgFlag", myParams);
                if ((err == "null") || (err == "NULL"))
                {
                    err = "";
                }

                Functions.DebugOut("<-----  after getB2BMsgFlag, n = 1, err: ' " + err + " ----->");


                if (!String.IsNullOrEmpty(err))
                {
                    //explisit reject
                    if (err.ToUpper() == "R")
                    {
                        Functions.DebugOut("Trigger Check (n=1) - Web Service Rejected receiving transaction, do not allow timeout/change part!");
                        return SetXmlError(returnXml, "Web Service Rejected receiving transaction, do not allow timeout/change part!");
                    }

                    if (err.ToUpper() != "P")
                    {
                        Functions.DebugOut("Trigger Check (n=1) - Unrecognized responce during receiving transaction, do not allow timeout/change part! Resp : " + err);
                        return SetXmlError(returnXml, "Unrecognized responce during receiving transaction, do not allow timeout/change part! Resp.: " + err);
                    }
                }
                else
                {
                    Functions.DebugOut("Trigger Check Problem in finding Web Service Message Flag!");
                    return SetXmlError(returnXml, "Problem in finding Web Service Message Flag!");
                }

                // 2nd check for change part message from the table
                n = 2;
                //err := getB2BMsgFlag( itemId, SN, 'CHANGEPART', n, isDebug );
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("p_itemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("p_sn", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("p_msg", OracleDbType.Varchar2, "CHANGEPART".Length, ParameterDirection.Input) { Value = "CHANGEPART" });
                myParams.Add(new OracleParameter("n", OracleDbType.Int32, n.ToString().Length, ParameterDirection.Input) { Value = n });
                myParams.Add(new OracleParameter("isDebug", OracleDbType.Int32, DebugSetting.ToString().Length, ParameterDirection.Input) { Value = DebugSetting });

                err = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "getB2BMsgFlag", myParams);
                if ((err == "null") || (err == "NULL"))
                {
                    err = "";
                }
                Functions.DebugOut("<-----  after getB2BMsgFlag, n = 2, err: ' " + err + " ----->");

                if (!String.IsNullOrEmpty(err))
                {
                    //explisit reject
                    if (err.ToUpper() == "R")
                    {
                        Functions.DebugOut("Trigger Check (n=2) - Web Service Rejected receiving transaction, do not allow timeout/change part!");
                        return SetXmlError(returnXml, "Web Service Rejected receiving transaction, do not allow timeout/change part!");
                    }

                    if (err.ToUpper() != "P")
                    {
                        Functions.DebugOut("Trigger Check (n=2) - Unrecognized responce during receiving transaction, do not allow timeout/change part! Resp : " + err);
                        return SetXmlError(returnXml, "Unrecognized responce during receiving transaction, do not allow timeout/change part! Resp.: " + err);
                    }
                }

                Functions.DebugOut("<-----  Exited Del.#3 - B2B Timeout block -------- ");

            //}            

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
