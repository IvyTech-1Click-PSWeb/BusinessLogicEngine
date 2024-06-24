using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using System.Web;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Linq;
using JGS.DAL;

namespace JGS.Web.TriggerProviders
{
    public class DTVCUUTIMEOUTVALIDATION : TriggerProviderBase
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
            ,{"XML_ORDERID","/Trigger/Detail/ItemLevel/OrderID"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
            ,{"XML_DIAGCODE","/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode/Name"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_PASSWORD","/Trigger/Header/UserObj/PassWord"}
            ,{"XML_WORKORDERID","/Trigger/Detail/ItemLevel/WorkOrderID"}
            ,{"XML_COM_REP_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Com_Rpr']/Value"}
            ,{"XML_OBAREQUIRED", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='OBA_Required']/Value"}
		};

        public override string Name { get; set; }

        public DTVCUUTIMEOUTVALIDATION()
        {
            this.Name = "DTVCUUTIMEOUTVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTVCUUTOVALIDATIONQUERIES";

            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string geo;
            string client;
            string bcn;
            string contract;
            string workcenter;
            int workcenterId;
            int LocationId;
            string result;
            int LanguageInd = 0; //0 English, 1 Espanol            
            string UserName;
            string Password;
            string newPart = null;
            string opt;
            string nextWorkcenter;
            string strResult;
            string SN;
            string routeId;
            string rid;
            string part;
            string itemId;
            string diagCode;
            string Recycled_CAM;
            string partid;
            int workorderID;
            string RefOrder;
            string response;

            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item Id cannot be empty.");
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

            //-- Get Workcenter Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                workcenterId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTERID can not be found.");
            }

            //-- Get Workcenter
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]);
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                bcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                part = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.");
            }

            //-- Get WorkOrderId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKORDERID"]))
            {
                workorderID = Convert.ToInt32(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKORDERID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "WorkOrderId could not be found.");
            }

            Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
            {
                {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
              , {Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
              , {Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
              , {Name="PartId", new OracleQuickQuery("INVENTORY1","ITEM","PART_ID","PartId","ITEM_ID = {PARAMETER}")}
              , {Name="RefOrder", new OracleQuickQuery("WC1","WORKORDER","reference_order_id","RefOrder","workorder_id = {PARAMETER}")}
            };

            //Call the DB to get necessary data from Oracle ///////////////
            queries["GeoName"].ParameterValue = LocationId.ToString();
            queries["ClientName"].ParameterValue = clientId.ToString();
            queries["ContractName"].ParameterValue = contractId.ToString();
            queries["PartId"].ParameterValue = itemId.ToString();
            queries["RefOrder"].ParameterValue = workorderID.ToString();
            Functions.GetMultipleDbValues(this.ConnectionString, queries);
            geo = queries["GeoName"].Result;
            client = queries["ClientName"].Result;
            contract = queries["ContractName"].Result;
            partid = queries["PartId"].Result;
            RefOrder = queries["RefOrder"].Result;

            if (String.IsNullOrEmpty(geo))
            {
                Functions.DebugOut("Geography Name can not be found.");
                return SetXmlError(returnXml, "Geography Name can not be found.");
            }
            if (String.IsNullOrEmpty(client))
            {
                Functions.DebugOut("Client Name can not be found.");
                return SetXmlError(returnXml, "Client Name can not be found.");
            }
            if (String.IsNullOrEmpty(contract))
            {
                Functions.DebugOut("Contract Name can not be found.");
                return SetXmlError(returnXml, "Contract Name can not be found.");
            }

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                result = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
            }

            //-- Get Order Process Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                opt = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT cannot be empty.");
            }

            //-- Get Order Process Type ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPTID"]))
            {
                routeId = Functions.ExtractValue(xmlIn, _xPaths["XML_OPTID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT ID cannot be empty.");
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

            //-- Get FAT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            {
                rid = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Fixed Asset Tag could not be found.");
            }

            //-- Get DIAGCODE
            if (!Functions.IsNull(xmlIn, _xPaths["XML_DIAGCODE"]))
            {
                diagCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DIAGCODE"]).Trim();
            }
            else
            {
                //return SetXmlError(returnXml, "Diagnostic Code could not be found.");
                diagCode = null;
            }

            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "UserName could not be found.");
            }
            //-- Get Password
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PASSWORD"]))
            {
                Password = Functions.ExtractValue(xmlIn, _xPaths["XML_PASSWORD"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Password could not be found.");
            }
            if (string.IsNullOrEmpty(opt) || opt.Length != 0)
            {
                if (opt.Length > 5)
                {
                    return SetXmlError(returnXml, "Order Process Type more than 5 Characters");
                }
            }
            else
            {
                return SetXmlError(returnXml, "Order Process Type is Blank!");
            }

            /************************************************** SET LANGUAGE INDICATOR **************************************************/

            Functions.DebugOut("-----  SET LANGUAGE INDICATOR --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("routeId", OracleDbType.Int32, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
            myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVCUUTimeOutValidation".ToString().Length, ParameterDirection.Input) { Value = "DTVCUUTimeOutValidation" });
            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetLanguage", myParams);
            if (!string.IsNullOrEmpty(strResult))
            {
                LanguageInd = Int32.Parse(strResult);
            }
            else
            {
                LanguageInd = 0;
            }

            /****************************************************************************************************************************/

            ///////////////////////// Display values for debug /////////////////////////////
            Functions.DebugOut("----------  Check B2B Variables  -------");
            Functions.DebugOut("GeoName:        " + geo);
            Functions.DebugOut("ClientName:     " + client);
            Functions.DebugOut("ContractName:   " + contract);
            Functions.DebugOut("ResultCode:     " + result);
            Functions.DebugOut("OPT:            " + opt);
            Functions.DebugOut("SN:             " + SN);
            Functions.DebugOut("BCN:            " + bcn);
            Functions.DebugOut("ItemId:         " + itemId);
            Functions.DebugOut("PartNumber:     " + part);
            Functions.DebugOut("workCenterId:   " + workcenterId);
            Functions.DebugOut("WorkCenterName: " + workcenter);
            Functions.DebugOut("PartId:         " + partid);
            Functions.DebugOut("--------------------------------");

            /****************************************** LOGIC START UP ***************************************/

            Functions.DebugOut("-----  getNextWorkcenterName --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("routeId", OracleDbType.Varchar2, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
            myParams.Add(new OracleParameter("workcenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("result", OracleDbType.Varchar2, result.Length, ParameterDirection.Input) { Value = result });
            myParams.Add(new OracleParameter("RES", OracleDbType.Varchar2, "RES".ToString().Length, ParameterDirection.Input) { Value = "RES" });
            nextWorkcenter = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getNextWorkcenterName", myParams);
            if (!string.IsNullOrEmpty(nextWorkcenter))
            {
                if (nextWorkcenter.Length > 16)
                {
                    return SetXmlError(returnXml, "Next workcenter more than 16 characters");
                }
                nextWorkcenter = nextWorkcenter.ToUpper();
                if (nextWorkcenter.Equals("NULL"))
                {
                    return SetXmlError(returnXml, "Next workcenter can't be determinated!");
                }
            }
            else
            {
                return SetXmlError(returnXml, "Next workcenter can't be determinated!");
            }

            if (nextWorkcenter.Equals("ERWC") && result.ToUpper().Equals("SCRAP"))
            {
                Functions.DebugOut("-----  Validate and Inactive TechnicalReturn --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ridnumber", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "UpdateTechReturntoInactivate", myParams);
                if (!strResult.ToUpper().Contains("SUCCESS"))
                {
                    return SetXmlError(returnXml, strResult);
                }

                Functions.DebugOut("-----  validateRecordScrapAtTimeout --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.Length, ParameterDirection.Input) { Value = client });
                myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contract.Length, ParameterDirection.Input) { Value = contract });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("SCRAP-ERWC", OracleDbType.Varchar2, "SCRAP-ERWC".ToString().Length, ParameterDirection.Input) { Value = "SCRAP-ERWC" });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "InsertingScrapRecordAtTimeout", myParams);
                if (!strResult.ToUpper().Contains("SUCCESS"))
                {
                    return SetXmlError(returnXml, strResult);
                }
            }

            #region "CONVERT WC Valdiation"
            if (workcenter.ToUpper().Equals("CONVERT"))
            {
                string newSN = null;

                if (string.IsNullOrEmpty(diagCode) || diagCode.Length == 0)
                {
                    if (LanguageInd == 1)
                    {
                        strResult = "Serie " + SN + " Requiere un codigo de diagnostico, favor de seleccionar el correspondiente";
                    }
                    else
                    {
                        strResult = "Serie " + SN + " Required a diag Code please select the correct one";
                    }
                    return SetXmlError(returnXml, strResult);
                }
                else
                {
                    if (!diagCode.ToUpper().Equals("PCN0840P"))
                    {
                        if (LanguageInd == 1)
                        {
                            strResult = "PCN " + diagCode + " Es incorrecto no se refiere a CONVERSION";
                        }
                        else
                        {
                            strResult = "PCN " + diagCode + " is incorrect is not related to CONVERSION";
                        }
                        return SetXmlError(returnXml, strResult);
                    }
                }

                if (part.ToUpper().Equals("D11-100") || part.ToUpper().Equals("HR20-100") || part.ToUpper().Equals("H23-600"))
                {
                    if (part.ToUpper().Equals("D11-100"))
                    {
                        newPart = "D11I-100";
                        if (SN.Length == 14)
                        {
                            newSN = "A13" + SN.Substring(3);
                        }
                    }
                    else if (part.ToUpper().Trim().Equals("HR20-100"))
                    {
                        newPart = "HR20I-100";
                        if (SN.Length == 14)
                        {
                            newSN = "A14" + SN.Substring(3);
                        }
                    }
                    else if (part.ToUpper().Trim().Equals("H23-600"))
                    {
                        newPart = "COM23-600";
                        if (SN.Length == 14)
                        {
                            newSN = "E18" + SN.Substring(3);
                        }
                    }

                    if (!string.IsNullOrEmpty(newPart) && !string.IsNullOrEmpty(newSN))
                    {
                        // CALL THE CHANGE PART API
                        response = string.Empty;
                        Functions.DebugOut("-----  Call Change Part Wrapper --------> ");
                        try
                        {
                            JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper();
                            JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo();
                            cpi.SesCustomerID = "1";
                            cpi.RequestId = "1";
                            cpi.BCN = bcn;
                            cpi.NewPartNo = newPart;
                            cpi.NewSerialNo = newSN;
                            cpi.MustBeOnHold = false;
                            cpi.ReleaseIfHold = false;
                            cpi.MustBeTimedIn = false;
                            cpi.TimedInWorkCenterName = " ";
                            cpi.userName = UserName;
                            cpi.Password = Password;
                            response = CPobj.PerformChangePart(cpi, false);
                        }
                        catch (Exception ex)
                        {
                            response = ex.ToString();
                        }
                        if (!response.ToUpper().Equals("SUCCESS"))
                        {
                            return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                        }
                    }
                }
            }
            #endregion

            #region "PackOut WC validation"
            if (workcenter.ToUpper().Equals("Pack Out".ToUpper()))
            {
                #region "Global Validations"   
                #region "SetOBARequiredByRandomIntOBAPercent"
                /* SetOBARequiredByRandomIntOBAPercent.- 
                 * At Timeout work center Pack Out, 1. get PGA flex field OBA_Percent value and compare with Random generated integer
                   if Random generated integer <= OBA_Percent will auto set work center Pack Out flex field OBA_Required to true else set to false.
                 * 2. For Memphis, OPT INSP, Client DirecTV, Contract RECEIVER, if OBA_Required = true, and Result code is NPF, auto set result to
                   TO_OBA if OBA_Required = false, and Result Code is TO_OBA auto set result code to NPF.
                 * If valueOBARequired == 100 always set OBA_Required to true */
                /*
				 * if Memphis, if flex field OBA_Required is Yes auto set result to TO_OBA, if flex field
				 * OBA_Required is No, auto set result to NPF (auto ERWC). */
                string OBARequired = string.Empty;
                Functions.DebugOut("-----  getPGAOBAPercent --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getPGAOBAPercent", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    if (!strResult.Equals("NA") && !strResult.Equals("0"))
                    {
                        if (strResult.Length > 3)
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "The OBA_Percent value is not valid");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "El valor del FlexField OBA_Percent no es valido");
                            }
                        }
                        else
                        {
                            if (strResult == "100")
                            {
                                OBARequired = "True";
                            }
                            else
                            {
                                Random generator = new Random();
                                int i = generator.Next(100) + 1;
                                if (i <= Convert.ToInt32(strResult))
                                {
                                    OBARequired = "True";
                                }
                                else
                                {
                                    OBARequired = "False";
                                }
                            }
                        }
                    }
                    else
                    {
                        OBARequired = "False";
                    }
                }
                else
                {
                    OBARequired = "False";
                }

                /*Filling OBA_Required FF */
                if (OBARequired.Contains("True") || OBARequired.Contains("False"))
                {
                    if (Functions.NodeExists(xmlIn, _xPaths["XML_OBAREQUIRED"]))
                    {
                        if (_xPaths["XML_OBAREQUIRED"] != null)
                            UpdateXml(xmlIn, _xPaths["XML_OBAREQUIRED"], OBARequired);
                    }
                }

                /* Setting Auto-ResultCode */
                if (geo.Contains("Memphis") && opt.Contains("WRP1") || opt.Contains("INSP"))
                {
                    if (OBARequired.Contains("True") && result.Contains("NPF"))
                    {
                        UpdateXml(xmlIn, _xPaths["XML_RESULTCODE"], "TO_OBA");
                    }
                    else if (OBARequired.Contains("False") && result.Contains("TO_OBA"))
                    {
                        result = "NPF";
                    }

                }
                else
                {
                    if (OBARequired.Contains("True") && !result.Contains("TO_OBA"))
                    {
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "OBA_Required is True, you should select TO_OBA result code");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "OBA_Required es True, debe seleccionar resultcode TO_OBA");
                        }
                    }
                    else if (OBARequired.Contains("False") && result.Contains("TO_OBA"))
                    {
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "OBA_Required is False, you should not select TO_OBA result code");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "OBA_Required es Falso, no debe seleccionar resultcode TO_OBA");
                        }
                    }
                }
                #endregion

                #region "ValidateActionCodeComponent"
                /* used to validate Action Code if it exist to get validation rules from the table custom1.FA_Components
                 * then based on the rule to validate each required field, if missing any value in required field
                 * then throw trigger error. There could be multiple action codes, and each action code could have multiple components.
                 * Validate fields include Assembly Code, Defective Component, its location, serial number, description, 
                 * manufacturer, manufacturer part no etc. Same feilds for new components.
                 * 2. compare client/contract/opt with geo/client/contract get from submit form context.getAttribute(),
                 * 1) if not found match no further validation.
                 * 2) if fund match, get current defect code, action code list by work order id and item id.
                 * 3) get component, component location, serial number requirements information by client/contract/opt/action code from table
                 * custom1.fa_require_components.
                 * 4) get serial number requirements information, check removed/issued component from wc1.component_codes by item Id, action code
                 * Id and inactive_ind.
                 * 5) compare issued/removed components with is those requirements from custom1.fa_require_component, throw error if no found match.*/
                Functions.DebugOut("-----  ValidateActionCodeComponent --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                myParams.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                DataSet dsUnitFAInfo = DbFetchds(this.ConnectionString, "WEBADAPTERS1", "QueryAdapter", "GetUnitFAInfo", myParams);

                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ClientName", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });                                
                myParams.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                DataSet dsFACompReq = DbFetchds(this.ConnectionString, Schema_name, Package_name, "GetFAInfReq", myParams);

                if (dsUnitFAInfo != null)
                {
                    if (dsUnitFAInfo.Tables.Count > 0 && dsUnitFAInfo.Tables[0].Rows.Count > 0)
                    {
                        for (int row = 0; row < dsUnitFAInfo.Tables[0].Rows.Count; row++)
                        {
                            string AssemblyNAME = dsUnitFAInfo.Tables[0].Rows[row]["AssemblyNAME"].ToString().Trim().ToUpper();
                            string DefCodeName = dsUnitFAInfo.Tables[0].Rows[row]["DefectCodeName"].ToString().Trim().ToUpper();
                            string ActionCode = dsUnitFAInfo.Tables[0].Rows[row]["ActionCodeName"].ToString().Trim().ToUpper();
                            string Component = dsUnitFAInfo.Tables[0].Rows[row]["ComponentPartNumber"].ToString().Trim().ToUpper();
                            string CompLoc = dsUnitFAInfo.Tables[0].Rows[row]["LOCATION"].ToString().Trim().ToUpper();
                            string CompSn = dsUnitFAInfo.Tables[0].Rows[row]["SERIAL_NO"].ToString().Trim().ToUpper();
                            string CompDesc = dsUnitFAInfo.Tables[0].Rows[row]["DESCRIPTION"].ToString().Trim().ToUpper();
                            string CompMfg = dsUnitFAInfo.Tables[0].Rows[row]["MANUFACTURER"].ToString().Trim().ToUpper();
                            string CompMfgPart = dsUnitFAInfo.Tables[0].Rows[row]["MANUFACTURER_PART_NO"].ToString().Trim().ToUpper();
                            string isadded = dsUnitFAInfo.Tables[0].Rows[row]["ISADDED"].ToString().Trim().ToUpper();
                            string Def = string.Empty;
                            string New = string.Empty;

                            if (dsFACompReq != null)
                            {
                                if (dsFACompReq.Tables.Count > 0 && dsFACompReq.Tables[0].Rows.Count > 0)
                                {
                                    var dcFACompReq = (from rowFA in dsFACompReq.Tables[0].AsEnumerable()
                                                       where rowFA.Field<string>(dsFACompReq.Tables[0].Columns["ACName"]).ToUpper() == ActionCode.ToUpper()
                                                       select rowFA);

                                    if (dcFACompReq != null)
                                    {
                                        if (dcFACompReq.Count() > 0)
                                        {
                                            DataTable dtFA = dcFACompReq.CopyToDataTable();

                                            if (dtFA.Rows[0]["DefComp"].ToString().Contains("Y") &&
                                                    dtFA.Rows[0]["NewComp"].ToString().Contains("Y"))
                                            {
                                                for (int rowDN = 0; rowDN < dsUnitFAInfo.Tables[0].Rows.Count; rowDN++)
                                                {
                                                    if (dsUnitFAInfo.Tables[0].Rows[rowDN]["DefectCodeName"].ToString().ToUpper().Contains(DefCodeName))
                                                    {
                                                        switch (dsUnitFAInfo.Tables[0].Rows[rowDN]["ISADDED"].ToString().Trim().ToUpper())
                                                        {
                                                            case "0":
                                                                Def = "OK";
                                                                break;
                                                            case "1":
                                                                New = "OK";
                                                                break;
                                                        }
                                                        if (Def.Contains("OK") && New.Contains("OK"))
                                                        { break; }
                                                    }
                                                }

                                                if (!Def.Contains("OK"))
                                                {

                                                    if (LanguageInd == 0)
                                                    {
                                                        return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                            " for Defect Code " + DefCodeName + " requires defective component");
                                                    }
                                                    else
                                                    {
                                                        return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                            " con el Defect Code " + DefCodeName + " requiere componente defectuoso");
                                                    }
                                                }

                                                if (!New.Contains("OK"))
                                                {
                                                    if (LanguageInd == 0)
                                                    {
                                                        return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                            " for Defect Code " + DefCodeName + " requires new component");
                                                    }
                                                    else
                                                    {
                                                        return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                            " con el Defect Code " + DefCodeName + " requiere componente nuevo");
                                                    }
                                                }
                                            }

                                            for (int FAcol = 0; FAcol < dtFA.Columns.Count; FAcol++)
                                            {
                                                string FAVal = dtFA.Rows[0][FAcol].ToString().ToUpper();
                                                string FAColName = dtFA.Columns[FAcol].ColumnName.ToString().ToUpper();

                                                if (FAVal.Contains("Y"))
                                                {
                                                    switch (FAColName)
                                                    {
                                                        case "ASSEMBLY":
                                                            if (string.IsNullOrEmpty(AssemblyNAME))
                                                            {
                                                                if (LanguageInd == 0)
                                                                {
                                                                    return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                        " for Defect Code " + DefCodeName + " need Assembly Code");
                                                                }
                                                                else
                                                                {
                                                                    return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                        " con el Defect Code " + DefCodeName + " requiere Assembly Code");
                                                                }
                                                            }
                                                            break;
                                                        case "COMPLOC":
                                                            if (string.IsNullOrEmpty(CompLoc))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires defective location");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere locacion para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires new location");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere locacion para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "SNCOMP":
                                                            if (string.IsNullOrEmpty(CompSn))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires defective component Serial number");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere numero de serie para el componente defectuoso");
                                                                        }

                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires new component Serial number");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere numero de serie para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "DESCCOMP":
                                                            if (string.IsNullOrEmpty(CompDesc))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires defective component description");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere descripcion para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires new component description");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere descripcion para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "MFG":
                                                            if (string.IsNullOrEmpty(CompMfg))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + "  requires defective component manufacturer");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere manufacturero para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + "  requires new component manufacturer");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere manufacturero para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "MFGPART":
                                                            if (string.IsNullOrEmpty(CompMfgPart))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + "  requires defective component manufacturer part");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere numero de parte del manufacturero para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + "  requires defective component manufacturer part");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere numero de parte del manufacturero para el componente defectuoso");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                else if (FAVal.Contains("X"))
                                                {
                                                    switch (FAColName)
                                                    {
                                                        case "ASSEMBLY":
                                                            if (!string.IsNullOrEmpty(AssemblyNAME))
                                                            {
                                                                if (LanguageInd == 0)
                                                                {
                                                                    return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                        " for Defect Code " + DefCodeName + " does not need Assembly Code");
                                                                }
                                                                else
                                                                {
                                                                    return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                        " con el Defect Code " + DefCodeName + " no requiere Assembly Code");
                                                                }
                                                            }
                                                            break;
                                                        case "DEFCOMP":
                                                            if (!string.IsNullOrEmpty(Component))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires defective component");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere componente defectuoso");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "NEWCOMP":
                                                            if (!string.IsNullOrEmpty(Component))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + "does not requires new component");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "COMPLOC":
                                                            if (!string.IsNullOrEmpty(CompLoc))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires defective location");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere locacion para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires new location");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere locacion para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "SNCOMP":
                                                            if (!string.IsNullOrEmpty(CompSn))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires defective component Serial number");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere numero de serie para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " requires new component Serial number");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " requiere numero de serie para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "DESCCOMP":
                                                            if (!string.IsNullOrEmpty(CompDesc))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires defective component description");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere descripcion para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires new component description");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere descripcion para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "MFG":
                                                            if (!string.IsNullOrEmpty(CompMfg))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires defective component manufacturer");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere manufacturero para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode +
                                                                                " for Defect Code " + DefCodeName + " does not requires new component manufacturer");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode +
                                                                                " con el Defect Code " + DefCodeName + " no requiere manufacturero para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                        case "MFGPART":
                                                            if (!string.IsNullOrEmpty(CompMfgPart))
                                                            {
                                                                switch (isadded)
                                                                {
                                                                    case "0":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode + " for Defect Code "
                                                                                + DefCodeName + " does not requires defective component manufacturer part");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode + " con el Defect Code "
                                                                                + DefCodeName + " no requiere numero de parte del manufacturero para el componente defectuoso");
                                                                        }
                                                                    case "1":
                                                                        if (LanguageInd == 0)
                                                                        {
                                                                            return SetXmlError(returnXml, "Action Code " + ActionCode + " for Defect Code "
                                                                                + DefCodeName + " does not requires new component manufacturer part");
                                                                        }
                                                                        else
                                                                        {
                                                                            return SetXmlError(returnXml, "El Action Code " + ActionCode + " con el Defect Code "
                                                                                + DefCodeName + " no requiere numero de parte del manufacturero para el componente nuevo");
                                                                        }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }                                             
                #endregion
                #endregion

                #region "ECOValidation"
                Functions.DebugOut("-----  ECOVALIDATION --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ECOVALIDATION", myParams);
                if (strResult.Equals("0") && !result.ToUpper().Equals("TO_ECO"))
                {
                    if (LanguageInd == 0)
                    {
                        return SetXmlError(returnXml, "This units has an active ECO, you must select TO_ECO resultcode");
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Esta unidad esta activa en ECO, debe seleccionar el ResultCode TO_ECO");
                    }
                }
                else if (strResult.Equals("1") && result.ToUpper().Equals("TO_ECO"))
                {
                    if (LanguageInd == 0)
                    {
                        return SetXmlError(returnXml, "This units has not an active ECO, you must not select TO_ECO resultcode");
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Esta unidad NO esta activa en ECO, NO debe seleccionar el ResultCode TO_ECO");
                    }                    
                }
                #endregion

                #region "TechReturnValidation"
                Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ridnumber", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "UpdateTechReturntoInactivate", myParams);
                if (!strResult.ToUpper().Contains("SUCCESS"))
                {
                    return SetXmlError(returnXml, strResult);
                }
                #endregion

                #region "RIDValidation"
                Functions.DebugOut("-----  RID Validation VALIDATION --------> ");
                string ridFFValue;
                ridFFValue = getWCFFValue(xmlIn, "RID");
                if (string.IsNullOrEmpty(ridFFValue) && opt.ToUpper().Equals("WRP"))
                {
                    return SetXmlError(returnXml, "Debe introducir el Receiver ID en Flex Field RID!");
                }
                else
                {
                    if (!string.IsNullOrEmpty(ridFFValue))  //&& (opt.ToUpper().Equals("INSP") || opt.ToUpper().Equals("WRP")))
                    {
                        if (ridFFValue.Trim().Length != 12)
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "Invalid RID, the RID should have 12 digits");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "RID Invalido, debe de tener 12 digitos!");
                            }
                        }
                        else
                        {
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, ridFFValue.ToString().Length, ParameterDirection.Input) { Value = ridFFValue });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateCamId", myParams);
                            if (strResult.ToUpper().Equals("FALSE"))
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "Invalid RID");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "RID Invalido!");
                                }
                            }
                        }
                    }
                }
                #endregion

                #region "ValidateResultByDefect WRP"
                if (opt.ToUpper().Equals("WRP") && (result.ToUpper().Equals("NPF") || result.ToUpper().Equals("REPAIRED") || result.ToUpper().Equals("COM_RPR")))
                {
                    Functions.DebugOut("-----  GETRESULTBYDEFECT --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                    myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                    myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                    myParams.Add(new OracleParameter("opt", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                    myParams.Add(new OracleParameter("workcenter", OracleDbType.Varchar2, workcenter.ToString().Length, ParameterDirection.Input) { Value = workcenter });
                    myParams.Add(new OracleParameter("result", OracleDbType.Varchar2, result.ToString().Length, ParameterDirection.Input) { Value = result });
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("LanguageInd", OracleDbType.Int32, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LanguageInd });
                    strResult = Functions.DbFetch(this.ConnectionString, "CUSTOM1", "JGSVALIDATIONS", "GETRESULTBYDEFECT", myParams);
                    if (!strResult.ToUpper().Equals("SUCCESS"))
                    {
                        /* "Common Repair Project Result Code" */
                        if (strResult.ToUpper().Contains("REPAIRED") && (result.ToUpper().Equals("COM_RPR") || result.ToUpper().Equals("REPAIRED")))
                        {
                            Functions.DebugOut("-----  GetFFValue --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("itemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                            myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "Com_Rpr".ToString().Length, ParameterDirection.Input) { Value = "Com_Rpr" });
                            myParams.Add(new OracleParameter("InvAttribute", OracleDbType.Int32, "1".ToString().Length, ParameterDirection.Input) { Value = 1 });
                            myParams.Add(new OracleParameter("workcenterId", OracleDbType.Varchar2, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetFFValue", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                string Com_Rpr = strResult;
                                Functions.DebugOut("-----  GetWCLoops --------> ");
                                myParams = new List<OracleParameter>();
                                myParams.Add(new OracleParameter("workcenter", OracleDbType.Varchar2, "Troubleshoot".ToString().Length, ParameterDirection.Input) { Value = "Troubleshoot" });
                                myParams.Add(new OracleParameter("LocationId", OracleDbType.Int32, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                                myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetWCLoops", myParams);
                                if ((Convert.ToInt32(strResult) == 0) && (Com_Rpr.Contains("YES")))
                                {
                                    if (!result.ToUpper().Equals("COM_RPR"))
                                    {
                                        if (LanguageInd == 0)
                                        {
                                            return SetXmlError(returnXml, "You must select COM_RPR Result Code");
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Debe seleccionar el Result Code COM_RPR");
                                        }
                                    }
                                }
                                else if (Convert.ToInt32(strResult) > 0)
                                {
                                    if (!result.ToUpper().Equals("REPAIRED"))
                                    {
                                        if (LanguageInd == 0)
                                        {
                                            return SetXmlError(returnXml, "You must select REPAIRED Result Code");
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Debe seleccionar el Result Code REPAIRED");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        { return SetXmlError(returnXml, strResult); }
                    }
                }
                #endregion

                #region "ValidatePartNoByResultCode"
                if (!result.ToUpper().Equals("NPF") && !result.ToUpper().Equals("REPAIRED") && !result.ToUpper().Equals("SCRAP") && !result.ToUpper().Equals("TO_OBA") && !result.ToUpper().Equals("COM_RPR"))
                {
                    Functions.DebugOut("-----  GetChangedPartNo --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("ItemID", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetChangedPartNo", myParams);

                    if (string.IsNullOrEmpty(strResult))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        int Index = strResult.Length - 1;
                        if (strResult.ToUpper().Substring(Index, 1) == "C")
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "You should delete prefix C before sent to WIP");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Debe remover la letra C, antes de enviar a WIP");
                            }
                        }
                    }
                }
                #endregion

                #region "H25 PSU Validation"
                if (part.StartsWith("H25"))
                {
                    if (result.ToUpper().Trim().Equals("NPF") || result.ToUpper().Trim().Equals("REPAIRED") || result.ToUpper().Trim().Equals("COM_RPR") || result.ToUpper().Trim().Equals("TO_OBA"))
                    {
                        string FFH25PSU;
                        FFH25PSU = getWCFFValue(xmlIn, "PS_H25");
                        if (string.IsNullOrEmpty(FFH25PSU))
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "The PS_H25 FF cannot be null");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "El FF PS_H25 no puede estar en blanco! ");
                            }
                        }
                        else
                        {
                            if (FFH25PSU.Trim().Length != 14)
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "The PS_H25 FF value should have 14 digits");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "El valor del FF PS_H25 debe ser de 14 digitos");
                                }
                            }
                            if (FFH25PSU.Equals(SN))
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "The PS_H25 FF value cannot be the same that serial number");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "El FF PS_H25 no puede ser igual al numero de serie");
                                }
                            }
                            //3rd and 4th digits must be "10" or "13", if not the PSU serial is invalid. 08/18/2011
                            String FFDigits = FFH25PSU.Substring(2, 2).Trim();
                            if (!FFDigits.Equals("10") && !FFDigits.Equals("13"))
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "The PS_H25 FF value is invalid");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "El valor del FF PS_H25 es invalido");
                                }
                            }
                        }
                    }
                }
                #endregion

                #region "ValidateResultByDefect CNVRT"
                if (opt.ToUpper().Equals("CNVRT") && (result.ToUpper().Trim().Equals("NPF") || result.ToUpper().Trim().Equals("REPAIRED")))
                {
                    Functions.DebugOut("-----  GETRESULTBYDEFECT --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    strResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "DTVCUUTimeOutValidation", "GETRESULTBYDEFECT", myParams);

                    if (!strResult.ToUpper().Equals("NPF") && !strResult.ToUpper().Equals("REPAIRED"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        if (!result.ToUpper().Trim().Equals(strResult))
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "The resultcode is not correct, you must select " + strResult);
                            }
                            else
                            {
                                return SetXmlError(returnXml, "El resultcode que selecciono es incorrecto, debe seleccionar " + strResult);
                            }
                        }
                    }
                }
                #endregion

                #region "INSP HDDSwapValidation"
                if ((opt.Equals("INSP") || opt.Equals("WRP1")) && (result.ToUpper().Trim().Equals("NPF") || result.ToUpper().Trim().Equals("REPAIRED") || result.ToUpper().Trim().Equals("TO_OBA")))
                {
                    Functions.DebugOut("-----  getHDDSwapResult --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    strResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "DTVHDDSWAPVAL", "getHDDSwapResult", myParams);
                    if (Convert.ToInt32(strResult) > 0)
                    {
                        strResult = "REPAIRED";
                    }
                    else
                    {
                        strResult = "NPF";
                    }
                    if (!result.Equals(strResult))
                    {
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "The resultcode is not correct, you must select " + strResult);
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Codigo de resultado incorrecto seleccione " + strResult);
                        }
                    }
                }
                #endregion

                #region "CamIdValidation, Include changepart process and Cam Card process"
                Functions.DebugOut("-----  CAM ID Validation VALIDATION --------> ");
                string camFFValue;
                camFFValue = getWCFFValue(xmlIn, "CAM ID");
                if (string.IsNullOrEmpty(camFFValue))
                {
                    if (LanguageInd == 0)
                    {
                        return SetXmlError(returnXml, "CAM ID FF value cannot be null");
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Debe introducir un numero de camid!");
                    }
                }
                else
                {
                    if (camFFValue.ToUpper().Equals("0"))
                    {
                        if (nextWorkcenter == "ERWC" && !result.ToUpper().Equals("SCRAP")) //result.ToUpper().Equals("NPF") || result.ToUpper().Equals("REPAIRED"))
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "For the resultcode that you select the CAM ID cannot be 0");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Para este Result Code el valor de la camid no puede ser 0!");
                            }
                        }
                    }
                    else
                    {
                        /* added if camId is not "0" but result code is TO_ECO or TO_PRET, throw error */
                        if (!nextWorkcenter.ToUpper().Trim().Equals("ERWC") && (!result.ToUpper().Equals("NPF") && !result.ToUpper().Equals("REPAIRED") && !result.ToUpper().Equals("TO_OBA") && !result.ToUpper().Equals("COM_RPR")))
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "CAM ID shoul be 0 if the unit will not ERWC");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "CamId debe cambiar a 0 si la unidad no dara ERWC!");
                            }
                        }

                        if (camFFValue.Length != 12)
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "The CAM ID should have 12 digits");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "CamId debe tener 12 digitos!");
                            }
                        }

                        if (camFFValue.ToUpper().Equals("000000000000"))
                        {
                            return SetXmlError(returnXml, "CamId no puede ser 000000000000!!!");
                        }

                        Functions.DebugOut("-----  validateCamId --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("camId", OracleDbType.Varchar2, camFFValue.ToString().Length, ParameterDirection.Input) { Value = camFFValue });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateCamId", myParams);
                        if (strResult.ToUpper().Equals("FALSE"))
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "Invalid CAM ID");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "CamId Invalida!");
                            }
                        }
                        else if (camFFValue == rid || camFFValue == ridFFValue)
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "The CAM ID value and RID value cannot be the same");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "El valor del CamId no puede ser igual al del RID");
                            }
                        }
                    }

                    Functions.DebugOut("-----  checkCamIdUniqueness --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("camId", OracleDbType.Varchar2, camFFValue.ToString().Length, ParameterDirection.Input) { Value = camFFValue });
                    myParams.Add(new OracleParameter("LanguageInd", OracleDbType.Int32, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LanguageInd });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "checkCamIdUniqueness", myParams);
                    if (!string.IsNullOrEmpty(strResult))
                    {
                        return SetXmlError(returnXml, strResult);
                    }

                    if (result.ToUpper().Equals("NPF") || result.ToUpper().Equals("REPAIRED") || result.ToUpper().Equals("COM_RPR") || result.ToUpper().Equals("TO_OBA"))
                    {
                        Functions.DebugOut("-----  check Cam Card Process / Cam Card Exceptions --------> ");
                        JGS.Web.DTVTriggerProviders.QA.QueryAdapterWrapper qaObj = new JGS.Web.DTVTriggerProviders.QA.QueryAdapterWrapper();
                        DataSet dtInfo = qaObj.GetAppControls(LocationId, workcenterId, clientId,
                                                           contractId.ToString(), routeId, UserName);
                        JGS.Web.DTVTriggerProviders.CamId.CamIdValidationService CamId = new JGS.Web.DTVTriggerProviders.CamId.CamIdValidationService();
                        string CamProcess = string.Empty;
                        for (int idx = 0; idx < dtInfo.Tables[0].Rows.Count; idx++)
                        {
                            if (dtInfo.Tables[0].Rows[idx]["PROCESS_NAME"].ToString().Contains("CAM_PROCESS"))
                            {
                                CamProcess = "TRUE";
                                string camIDUpdateResult = CamId.UpdateCamIdANDSerial(camFFValue, SN, UserName, geo, part);
                                if (camIDUpdateResult.Contains("ERROR"))
                                {
                                    return SetXmlError(returnXml, camIDUpdateResult);
                                }
                            }
                        }

                        if (!CamProcess.Contains("TRUE"))
                        {                            
                            string CamExeptVal = CamId.ValCamExceptvsModel(camFFValue, UserName, part);
                            if (!CamExeptVal.Contains("SUCCESS"))
                            {
                                return SetXmlError(returnXml, CamExeptVal);
                            }
                        }

                        Functions.DebugOut("-----  checkPGAFFRecycledCAM --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                        myParams.Add(new OracleParameter("LocationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                        myParams.Add(new OracleParameter("LanguageInd", OracleDbType.Int32, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LanguageInd });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "checkPGAFFRecycledCAM", myParams);
                        if (strResult.IndexOf("[RECYCLED_CAM]") >= 0)
                        {
                            return SetXmlError(returnXml, strResult);
                        }
                        else
                        {
                            Recycled_CAM = strResult;
                        }

                        if (Recycled_CAM.ToUpper().Equals("NO"))
                        {
                            if (!part.ToString().ToUpper().Contains("C"))
                            {
                                Functions.DebugOut("-----  getNewPartNo --------> ");
                                myParams = new List<OracleParameter>();
                                myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                                newPart = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getNewPartNo", myParams);
                                if (string.IsNullOrEmpty(newPart))
                                {
                                    if (LanguageInd == 1)
                                    {
                                        strResult = "Parte " + part + " no esta en la tabla change_part_ref!";
                                    }
                                    else
                                    {
                                        strResult = "Part " + part + " is not in the change_part_ref table!";
                                    }
                                    return SetXmlError(returnXml, strResult);
                                }
                                else
                                {
                                    // CALL THE CHANGE PART API
                                    response = string.Empty;
                                    Functions.DebugOut("-----  Call Change Part Wrapper --------> ");
                                    try
                                    {
                                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper();
                                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo();
                                        cpi.SesCustomerID = "1";
                                        cpi.RequestId = "1";
                                        cpi.BCN = bcn;
                                        if (!string.IsNullOrEmpty(ridFFValue) && !rid.Equals(ridFFValue))
                                        {
                                            cpi.NewFixedAssetTag = ridFFValue;
                                        }
                                        cpi.NewPartNo = newPart;
                                        cpi.MustBeOnHold = false;
                                        cpi.ReleaseIfHold = false;
                                        cpi.MustBeTimedIn = false;
                                        cpi.TimedInWorkCenterName = " ";
                                        cpi.userName = UserName;
                                        cpi.Password = Password;
                                        response = CPobj.PerformChangePart(cpi, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        response = ex.ToString();
                                    }
                                    if (!response.ToUpper().Equals("SUCCESS"))
                                    {
                                        return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region "ECO WC Validation"
            if (workcenter.ToUpper().Equals("ECO"))
            {
                string DiagCodeList = GetMultipleDiagCodes(xmlIn);

                #region "GetResultByDefect"
                //Process for Common Repair Project Dec 9, 2011
                if (opt.ToUpper().Equals("WRP"))
                {
                    Functions.DebugOut("----- GetResultByDefect --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                    myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                    myParams.Add(new OracleParameter("opt", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                    myParams.Add(new OracleParameter("workcenterid", OracleDbType.Varchar2, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                    myParams.Add(new OracleParameter("partid", OracleDbType.Varchar2, partid.ToString().Length, ParameterDirection.Input) { Value = partid });
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("reforder", OracleDbType.Varchar2, RefOrder.ToString().Length, ParameterDirection.Input) { Value = RefOrder });
                    myParams.Add(new OracleParameter("result", OracleDbType.Varchar2, result.ToString().Length, ParameterDirection.Input) { Value = result });
                    myParams.Add(new OracleParameter("languageind", OracleDbType.Varchar2, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LanguageInd });
                    strResult = Functions.DbFetch(this.ConnectionString, "CUSTOM1", "DTVJGSValidations", "FORCERESULTBYDEFECT", myParams);
                    if (!strResult.ToUpper().Equals("SUCESS"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                }
                #endregion

                #region "Update COM_RPR FF Value, Common Repair Project"
                if ((result.ToUpper() != "TROUBLE" && result.ToUpper() != "SCRAP") && opt.ToUpper().Equals("WRP") && LocationId == 1605)
                {
                    Functions.DebugOut("----- GetResultCodeValidation --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("clientId", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("contractId", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("opt", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                    myParams.Add(new OracleParameter("workcenterid", OracleDbType.Varchar2, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                    myParams.Add(new OracleParameter("partid", OracleDbType.Varchar2, partid.ToString().Length, ParameterDirection.Input) { Value = partid });
                    myParams.Add(new OracleParameter("result", OracleDbType.Varchar2, result.ToString().Length, ParameterDirection.Input) { Value = result });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, "DTVCUUTimeOutValidation", "GetResultCode", myParams);
                    if (strResult.ToUpper().Contains("SUCCESS"))
                    {
                        if (result.ToUpper().Contains("COM_RPR"))
                        {
                            Functions.DebugOut("----- UpdateFFValue --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("ItemBCN", OracleDbType.Varchar2, bcn.ToString().Length, ParameterDirection.Input) { Value = bcn });
                            myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "Com_Rpr".ToString().Length, ParameterDirection.Input) { Value = "Com_Rpr" });
                            myParams.Add(new OracleParameter("FFValue", OracleDbType.Varchar2, "YES".ToString().Length, ParameterDirection.Input) { Value = "YES" });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, "DTVCUUTimeOutValidation", "UpdateFFValue", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                if (!strResult.ToUpper().Equals("SUCCESS"))
                                {
                                    return SetXmlError(returnXml, strResult);
                                }
                            }
                            //else
                            //{
                            //    returnXml = AddWCFFXmlNode(returnXml, "Com_Rpr", "YES");
                            //}
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                }
                #endregion

                #region "Inactivating ECO's"
                if (!result.ToUpper().Equals("DEFECT") && !result.ToUpper().Equals("SCRAP"))
                {
                    Functions.DebugOut("----- ECOwcTMOValidation --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                    myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                    myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("diagCode", OracleDbType.Varchar2, DiagCodeList.ToString().Length, ParameterDirection.Input) { Value = DiagCodeList });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ECOwcTMOValidation", myParams);

                    if (!strResult.ToUpper().Equals("SUCCESS"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                }
                #endregion

                #region "Conversion Project"
                //R22 Conversion project, need to call changepart API before timeout
                if (part.Substring(0, 3) == "R22")
                {
                    string NewDigit = "";
                    string NewPart = "";
                    string numbers = SN.Trim().Substring(1, 2);
                    string NewSerial = SN.ToUpper().Trim();
                    NewSerial = SN.Substring(0, 1) + "17" + NewSerial.Substring(3);
                    string digit = NewSerial.ToUpper().Trim().Substring(4, 1);
                    switch (digit)
                    {
                        case "A":
                            {
                                NewDigit = "F";
                                break;
                            }
                        case "B":
                            {
                                NewDigit = "G";
                                break;
                            }
                        case "C":
                            {
                                NewDigit = "H";
                                break;
                            }
                        case "D":
                            {
                                NewDigit = "J";
                                break;
                            }
                        case "E":
                            {
                                NewDigit = "K";
                                break;
                            }
                    }

                    NewSerial = NewSerial.Substring(0, 4) + NewDigit + NewSerial.Substring(5);

                    if (part.Trim().ToUpper() == "R22-100")
                    {
                        NewPart = "HR21-100";
                    }
                    else if (part.Trim().ToUpper() == "R22-200")
                    {
                        NewPart = "HR21-200";
                    }

                    //Calling changepart API
                    response = string.Empty;
                    Functions.DebugOut("-----  Call Change Part Wrapper --------> ");
                    try
                    {
                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper();
                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo();
                        cpi.SesCustomerID = "1";
                        cpi.RequestId = "1";
                        cpi.BCN = bcn;
                        cpi.NewPartNo = NewPart;
                        cpi.NewSerialNo = NewSerial;
                        cpi.MustBeOnHold = false;
                        cpi.ReleaseIfHold = false;
                        cpi.MustBeTimedIn = false;
                        cpi.TimedInWorkCenterName = " ";
                        cpi.userName = UserName;
                        cpi.Password = Password;
                        response = CPobj.PerformChangePart(cpi, false);
                    }
                    catch (Exception ex)
                    {
                        response = ex.ToString();
                    }
                    if (!response.ToUpper().Equals("SUCCESS"))
                    {
                        return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                    }
                    //End Call ChangePart API 
                }
                #endregion
            }
            #endregion

            #region "FVT BGA WC Validation"
            if (workcenter.ToUpper().Equals("FVT BGA"))
            {
                Functions.DebugOut("-----  RID Validation VALIDATION --------> ");
                string ridFFValue;
                ridFFValue = getWCFFValue(xmlIn, "RID");
                if (string.IsNullOrEmpty(ridFFValue))
                {
                    return SetXmlError(returnXml, "Debe introducir el Receiver ID en Flex Field RID!");
                }
                else
                {
                    if (!string.IsNullOrEmpty(ridFFValue))  //&& (opt.ToUpper().Equals("INSP") || opt.ToUpper().Equals("WRP")))
                    {
                        if (ridFFValue.Trim().Length != 12)
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "Invalid RID, the RID should have 12 digits");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "RID Invalido, debe de tener 12 digitos!");
                            }
                        }
                        else
                        {
                            Functions.DebugOut("----- Validate RID --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, ridFFValue.ToString().Length, ParameterDirection.Input) { Value = ridFFValue });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateCamId", myParams);
                            if (strResult.ToUpper().Equals("FALSE"))
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "Invalid RID");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "RID Invalido!");
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ridFFValue) && !rid.Equals(ridFFValue))
                                {
                                    response = string.Empty;
                                    Functions.DebugOut("-----  Call Change Part Wrapper --------> ");
                                    try
                                    {
                                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper();
                                        JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo();
                                        cpi.SesCustomerID = "1";
                                        cpi.RequestId = "1";
                                        cpi.BCN = bcn;
                                        cpi.NewFixedAssetTag = ridFFValue;
                                        cpi.MustBeOnHold = false;
                                        cpi.ReleaseIfHold = false;
                                        cpi.MustBeTimedIn = false;
                                        cpi.TimedInWorkCenterName = " ";
                                        cpi.userName = UserName;
                                        //cpi.Password = Password;
                                        response = CPobj.PerformChangePart(cpi, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        response = ex.ToString();
                                    }
                                    if (!response.ToUpper().Equals("SUCCESS"))
                                    {
                                        return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            // Set Return Code to Success
            SetXmlSuccess(returnXml);
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
        private void UpdateXml(XmlDocument returnXml, string path, string value)
        {
            Functions.UpdateXml(ref returnXml, path, value);
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

        private String getWCFFValue(XmlDocument XmlIn, string flexFieldName)
        {
            XmlNodeList nodes;
            string ffValue = null;
            nodes = XmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes.Item(i).FirstChild.InnerXml.ToUpper().Equals(flexFieldName.ToUpper()))
                {
                    ffValue = nodes.Item(i).LastChild.InnerXml.ToString();
                    break;
                }
            }
            return ffValue;
        }

        private String GetMultipleDiagCodes(XmlDocument XmlIn)
        {
            XmlNodeList nodes;
            string ffValue = null;
            nodes = XmlIn.SelectNodes("/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode");
            for (int i = 0; i < nodes.Count; i++)
            {
                ffValue = ffValue + "+" + nodes.Item(i).FirstChild.InnerXml.ToUpper();
            }
            return ffValue;
        }

        private XmlDocument AddWCFFXmlNode(XmlDocument XmlIn, string NodeName, string NodeValue)
        {
            XmlElement ffEle = XmlIn.CreateElement("FlexField");
            ffEle.AppendChild(XmlIn.CreateNode(XmlNodeType.Element, "Name", null));
            ffEle.AppendChild(XmlIn.CreateNode(XmlNodeType.Element, "Value", null));

            ffEle.ChildNodes[0].InnerText = NodeName;
            ffEle.ChildNodes[1].InnerText = NodeValue;
            XmlIn.DocumentElement["Detail"]["TimeOut"]["WCFlexFields"].AppendChild(ffEle);

            return XmlIn;
        }  
        
        /// <summary>
        /// Execute an Oracle Dataset function and return the results as a dataset.
        /// </summary>
        /// <param name="connectionString">The connection string to the data server</param>
        /// <param name="schema">The schema of the function</param>
        /// <param name="package">The name of the package containing the function</param>
        /// <param name="functionName">The name of the function to execute</param>
        /// <param name="params">The array of parameters to the function</param>
        /// <returns>The DataSet result, Dataset null on failure</returns>
        /// <remarks></remarks>
        private DataSet DbFetchds(String connectionString, String schema, String package, String functionName, List<OracleParameter> parameters)
        {
            try
            {
                DataSet dsResult = ODPNETHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, schema + "." + package + "." + functionName, parameters.ToArray());
                if (dsResult.Tables.Count > 0)
                {
                    if (dsResult.Tables[0].Rows.Count > 0)
                    {
                        return dsResult;
                    }
                }
                return dsResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }    
    }
}
