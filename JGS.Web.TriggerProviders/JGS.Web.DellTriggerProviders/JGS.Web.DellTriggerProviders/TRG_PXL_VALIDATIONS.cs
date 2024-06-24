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
	public class TRG_PXL_VALIDATIONS : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public TRG_PXL_VALIDATIONS()
		{
            this.Name = "TRG_PXL_VALIDATIONS";
		}

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
                       

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
             

            string err_msg;

          
            //BEGIN
            Functions.DebugOut("--------  Inside of Execute Function  -------->");

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //List<OracleParameter> myParams = new List<OracleParameter>();
            //myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, "KR0TR217397929AP5033".Length, ParameterDirection.Input) { Value = "KR0TR217397929AP5033" });
            //myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, "LCD REPAIR".Length, ParameterDirection.Input) { Value = "LCD REPAIR" });
            //myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, "WRP".Length, ParameterDirection.Input) { Value = "WRP" });
            //myParams.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            //DataSet dataIn = JGS.DAL.ODPNETHelper.ExecuteDataset(this.ConnectionString,CommandType.StoredProcedure,
            //    "GREENSTM.TRG_NET_DELLRRTIMEOUT.GetLCDWarrantyTerms", myParams.ToArray());

            //List<OracleParameter> myParams = new List<OracleParameter>();
            //myParams.Add(new OracleParameter("p_itemId", OracleDbType.Varchar2, "86634825".Length, ParameterDirection.Input) { Value = "86634825" });
            //myParams.Add(new OracleParameter("p_sn", OracleDbType.Varchar2, "KR0TR217397929AP5033".Length, ParameterDirection.Input) { Value = "KR0TR217397929AP5033" });
            //myParams.Add(new OracleParameter("p_msg", OracleDbType.Varchar2, "RECEIVE".Length, ParameterDirection.Input) { Value = "RECEIVE" });
            //myParams.Add(new OracleParameter("n", OracleDbType.Int32, "1".Length, ParameterDirection.Input) { Value = "1" });
            //myParams.Add(new OracleParameter("isDebug", OracleDbType.Int32, "1".Length, ParameterDirection.Input) { Value = "1" });


            //string test = Functions.DbFetch(this.ConnectionString, "GREENSTM", "TRG_NET_DELLRRTIMEOUT", "getB2BMsgFlag", myParams);

            //string test = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
            //    .Replace("{DEFECTCODE}", "BLU-BRK_BLB")
            //    .Replace("{FLEXFIELDNAME}", "BrightPixelQTY"));                       

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
            Functions.DebugOut("----------  Variables  -------");
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
            
                Functions.DebugOut("----- Inside of Del. #2 - enforce_dell_pixel_count ----------->");
                err_msg = EnforceDellPixelCounts(xmlIn);
                if (err_msg != "OK")
                {
                    Functions.DebugOut("Trigger Check -  " + err_msg);
                    return SetXmlError(returnXml, err_msg);
                }
                Functions.DebugOut("<----- Exited Del. #2 - enforce_dell_pixel_count ----------- ");
            
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

        public String EnforceDellPixelCounts(XmlDocument xmlIn)
        {
            String v_return_string = "OK";

            // bright variables
            String f_BrightPixelQTY;
            String f_SingleBrightDot;
            String f_AdjacentBrightDot;
            String f_PartialBrightPixelQTY;

            //   dark variables
            String f_DarkPixelQTY;
            String f_SingleDarkDot;
            String f_AdjacentDarkDot;

            ///////// int variables ////////
            int i_BrightPixelQTY;
            int i_SingleBrightDot;
            int i_AdjacentBrightDot;
            int i_PartialBrightPixelQTY;

            //   dark variables
            int i_DarkPixelQTY;
            int i_SingleDarkDot;
            int i_AdjacentDarkDot;

            try
            {
                // Check if Pixel Bright Dot Defect is Set.
                if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_DEFECTCODE_NAME_SEARCH"]
                    .Replace("{DEFECTCODE}", "PXL-BRT_DOT")))
                {
                    f_BrightPixelQTY = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                     .Replace("{DEFECTCODE}", "PXL-BRT_DOT")
                     .Replace("{FLEXFIELDNAME}", "BrightPixelQTY"));

                    if (String.IsNullOrEmpty(f_BrightPixelQTY))
                    {
                        v_return_string = " \"BrightPixelQTY\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    f_SingleBrightDot = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                   .Replace("{DEFECTCODE}", "PXL-BRT_DOT")
                   .Replace("{FLEXFIELDNAME}", "SingleBrightDot"));

                    if (String.IsNullOrEmpty(f_SingleBrightDot))
                    {
                        v_return_string = " \"SingleBrightDot\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    f_AdjacentBrightDot = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                   .Replace("{DEFECTCODE}", "PXL-BRT_DOT")
                   .Replace("{FLEXFIELDNAME}", "AdjacentBrightDot"));

                    if (String.IsNullOrEmpty(f_AdjacentBrightDot))
                    {
                        v_return_string = " \"AdjacentBrightDot\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    f_PartialBrightPixelQTY = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                   .Replace("{DEFECTCODE}", "PXL-BRT_DOT")
                   .Replace("{FLEXFIELDNAME}", "PartialBrightPixelQTY"));

                    if (String.IsNullOrEmpty(f_PartialBrightPixelQTY))
                    {
                        v_return_string = " \"PartialBrightPixelQTY\" flex field value is missing or empty";
                        return v_return_string;
                    }
                    // Check for all 0
                    if (f_BrightPixelQTY == "0" && f_SingleBrightDot == "0" && f_AdjacentBrightDot == "0" && f_PartialBrightPixelQTY == "0")
                    {
                        v_return_string = "Bright Pixel fields in Failure Analysis cannot all equal 0 (zero) for the defect \"PXL-BRT_DOT\".";
                        return v_return_string;
                    }
                    // Check for all negative
                    int.TryParse(f_BrightPixelQTY, out i_BrightPixelQTY);
                    int.TryParse(f_SingleBrightDot, out i_SingleBrightDot);
                    int.TryParse(f_AdjacentBrightDot, out i_AdjacentBrightDot);
                    int.TryParse(f_PartialBrightPixelQTY, out i_PartialBrightPixelQTY);
                    if (i_BrightPixelQTY < 0 || i_SingleBrightDot < 0 || i_AdjacentBrightDot < 0 || i_PartialBrightPixelQTY < 0)
                    {
                        v_return_string = "Bright Pixel fields in Failure Analysis cannot contain negative values.";
                        return v_return_string;
                    }

                }

                // Check if Pixel Dark Dot Defect is Set.
                if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_DEFECTCODE_NAME_SEARCH"]
                    .Replace("{DEFECTCODE}", "PXL-DRK_DOT")))
                {
                    f_DarkPixelQTY = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                    .Replace("{DEFECTCODE}", "PXL-DRK_DOT")
                    .Replace("{FLEXFIELDNAME}", "DarkPixelQTY"));

                    if (String.IsNullOrEmpty(f_DarkPixelQTY))
                    {
                        v_return_string = " \"DarkPixelQTY\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    f_SingleDarkDot = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                  .Replace("{DEFECTCODE}", "PXL-DRK_DOT")
                  .Replace("{FLEXFIELDNAME}", "SingleDarkDot"));

                    if (String.IsNullOrEmpty(f_SingleDarkDot))
                    {
                        v_return_string = " \"SingleDarkDot\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    f_AdjacentDarkDot = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODE_FLEX_FIELD_VAL"]
                  .Replace("{DEFECTCODE}", "PXL-DRK_DOT")
                  .Replace("{FLEXFIELDNAME}", "AdjacentDarkDot"));

                    if (String.IsNullOrEmpty(f_AdjacentDarkDot))
                    {
                        v_return_string = " \"AdjacentDarkDot\" flex field value is missing or empty";
                        return v_return_string;
                    }

                    // Check for all 0
                    if (f_DarkPixelQTY == "0" && f_SingleDarkDot == "0" && f_AdjacentDarkDot == "0")
                    {
                        v_return_string = "Dark Pixel fields in Failure Analysis cannot all equal 0 (zero) for the defect \"PXL-DRK_DOT\".";
                        return v_return_string;
                    }
                    // Check for negative values
                    int.TryParse(f_DarkPixelQTY, out i_DarkPixelQTY);
                    int.TryParse(f_SingleDarkDot, out i_SingleDarkDot);
                    int.TryParse(f_AdjacentDarkDot, out  i_AdjacentDarkDot);
                    if (i_DarkPixelQTY < 0 || i_SingleDarkDot < 0  || i_AdjacentDarkDot < 0)
                    {
                        v_return_string = "Dark Pixel fields in Failure Analysis cannot contain negative values.";
                        return v_return_string;
                    }

                }

                return v_return_string;
            }
            catch (Exception ex)
            {
                v_return_string = "An exception occured in TRG_PXL_VALIDATIONS trigger: " + ex.ToString();
                return v_return_string;
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
