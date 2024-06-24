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
	public class TRG_SCRAP_APPROVAL : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public TRG_SCRAP_APPROVAL()
		{
            this.Name = "TRG_SCRAP_APPROVAL";
		}

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
           
            //////////////////////////////// package name for Stored Procs calls ////////////////////////
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
            
          
            //FlexField
            string ActionFF;

           
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

            ///////////////////////// Display values for debug /////////////////////////////
            Functions.DebugOut("----------  Scrap Approval Variables  -------");
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
            
                Functions.DebugOut("-----  Inside of Del.#5 - Scrap/Salvage Check  --------> ");
                
                //-- Get Action Flex Field and compare it to result code.
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("dellPartNumber", OracleDbType.Varchar2, partNumber.ToString().Length, ParameterDirection.Input) { Value = partNumber });
                ActionFF = Functions.DbFetch(this.ConnectionString, CommontSettings.Schema_name, Package_name, "GetActionFFforDellPart", myParams);

                if (ActionFF.StartsWith("NOT_FOUND"))
                {
                    Functions.DebugOut("Trigger Error - could not find action for the part number ");
                    return SetXmlError(returnXml, "Could not find action for the part number " + partNumber);
                }
                if (ActionFF.StartsWith("ERROR"))
                {
                    return SetXmlError(returnXml, "Oracle Error while selecting action for the part number " + partNumber);
                }

                Functions.DebugOut("ACTION FF : " + ActionFF);

                if ((ActionFF.ToUpper() == "SCRAP" || ActionFF.ToUpper() == "AUTO-SCRAP") && resultCode.ToUpper() == "SALVAGE")
                {
                    Functions.DebugOut("Trigger Check - This unit/part number can only be timed out as SCRAP.");
                    return SetXmlError(returnXml, "This unit/part number can only be timed out as SCRAP.");
                }

                if ((ActionFF.ToUpper() == "SALVAGE" || ActionFF.ToUpper() == "AUTO-SALVAGE") && resultCode.ToUpper() == "SCRAP")
                {
                    Functions.DebugOut("Trigger Check - This unit/part number can only be timed out as SALVAGE.");
                    return SetXmlError(returnXml, "This unit/part number can only be timed out as SALVAGE.");
                }
                Functions.DebugOut("<-----  Exited Del.#5 - Scrap/Salvage Check -------- ");
                          
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

