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
    public class DTVTROUBLESHOOT : TriggerProviderBase
    {
        DataTable dtDayRef;
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
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}
		};

        public override string Name { get; set; }

        public DTVTROUBLESHOOT()
        {
            this.Name = "DTVTROUBLESHOOT";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

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
            string opt;
            string nextWorkcenter;
            string strResult;
            string SN;
            string routeId;
            string rid;
            string part;
            string itemId;
            string diagCode;
            string partid;
            int workorderID;
            string RefOrder;
            DataTable dtResult;
            /* ECOValidation and NonMandatoryPCN Variables */
            string SNECOCodes = string.Empty;
            string SNRC = string.Empty;
            string ECOReq = string.Empty; /* 0 Requiere an ECO, 1 does not required an ECO */
            int ECOInd; /* To see if record already exist in Client_ECO_SN_History table */
            string ECOCode;
            string CloseECO;
            string SpecChar;
            string spc;
            string ReasonCode;
            string ManfRange;
            string ECOOPT;
            string SNRange;
            string[] ECOManRange;
            string ManRanges;
            string[] Dates;
            DateTime SNDate;
            DateTime InDate;
            DateTime EndDate;
            /* End ECOValidation and NonMandatoryPCN Variables */
            
            #region "Getting Variables"
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
            #endregion

            /************************************************** SET LANGUAGE INDICATOR **************************************************/

            Functions.DebugOut("-----  SET LANGUAGE INDICATOR --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("routeId", OracleDbType.Int32, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
            myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVTROUBLESHOOT".ToString().Length, ParameterDirection.Input) { Value = "DTVTROUBLESHOOT" });
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
            #region "getNextWorkcenterName"
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
            #endregion

            #region "Scrap ResultCode"
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
            #endregion

            if (workcenter.ToUpper().Contains("Troubleshoot".ToUpper()) && !result.ToUpper().Contains("SCRAP"))
            {
                if (!result.ToUpper().Contains("DIAGS"))
                {
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
                }

                #region "No mandatory PCNs Validation"
                if (!string.IsNullOrEmpty(diagCode))
                {
                    string DiagCodeList = GetMultipleDiagCodes(xmlIn);
                    string DiagVal = string.Empty;

                    Functions.DebugOut("-----  NonMandatoryPCNValidation  --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                    myParams.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                    myParams.Add(new OracleParameter("GeoId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("Queryclass", OracleDbType.Varchar2, "PCNVal".ToString().Length, ParameterDirection.Input) { Value = "PCNVal" });
                    myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                    dtResult = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + 
                        Package_name + "." + "GetNonMandatoryPCNs", myParams.ToArray());
                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            /* Getting the PCNs that can apply to the unit */
                            for (int rowdt = 0; rowdt < dtResult.Rows.Count; rowdt++)
                            {
                                string pcnclass = dtResult.Rows[rowdt]["pcnclass"].ToString();
                                string pcnname = dtResult.Rows[rowdt]["pcn"].ToString();
                                switch (pcnclass)
                                {
                                    case "All":
                                        DiagVal = pcnname + ", " + DiagVal;
                                        break;
                                    case "Special_Caracter":
                                        spc = SN.Substring(4, 1).ToUpper();
                                        SpecChar = dtResult.Rows[rowdt]["specchar"].ToString().ToUpper();
                                        int resSC = SpecChar.IndexOf(spc);
                                        if (resSC >= 0)
                                        {
                                            if (pcnname == "PCN0140P")
                                            {
                                                spc = SN.Substring(3, 1).ToUpper();
                                                if (spc.ToUpper() == "L" || spc.ToUpper() == "G")
                                                {
                                                    DiagVal = pcnname + ", " + DiagVal;
                                                }
                                            }
                                            else
                                            {
                                                DiagVal = pcnname + ", " + DiagVal;
                                            }
                                        }
                                        break;
                                    case "Date_Range":
                                        SNRange = SN.Substring(5, 3).ToUpper();
                                        ManRanges = SNRange + "," + dtResult.Rows[rowdt]["minran"].ToString().ToUpper().Trim() + "," + dtResult.Rows[rowdt]["maxran"].ToString().ToUpper().Trim();
                                        ManRanges = GetManufDate(ManRanges, part, SN, rid, LocationId, Schema_name, Package_name);
                                        Dates = ManRanges.Split(',');
                                        SNDate = Convert.ToDateTime(Dates[0]);
                                        InDate = Convert.ToDateTime(Dates[1]);
                                        EndDate = Convert.ToDateTime(Dates[2]);
                                        if ((SNDate >= InDate) && (SNDate <= EndDate))
                                        {
                                            DiagVal = pcnname + ", " + DiagVal;
                                        }
                                        break;
                                    case "Serial":
                                        DiagVal = pcnname + ", " + DiagVal;
                                        break;
                                    case "RID":
                                        DiagVal = pcnname + ", " + DiagVal;
                                        break;
                                }
                            }
                            /* End getting the PCNs that can apply to the unit */

                            /* Validating in the DiagCode captured can be in the unit */
                            string[] PCNSList = DiagCodeList.Split(',');
                            if (PCNSList.Count() > 0)
                                for (int pcn = 0; pcn < PCNSList.Count(); pcn++)
                                {
                                    int resPCN = DiagVal.IndexOf(PCNSList[pcn]);
                                    if (resPCN < 0)
                                    {
                                        if (LanguageInd == 0)
                                        {
                                            return SetXmlError(returnXml, "This unit does not apply for " + PCNSList[pcn] + ", this unit must not have this DiagCod");
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Esta unidad no aplica para el " + PCNSList[pcn] + ", no debe llevar este DiagCode");
                                        }
                                    }
                                }
                            /* End Validating in the DiagCode captured can be in the unit */
                        }
                        else
                        {
                            if (LanguageInd == 0)
                            {
                                return SetXmlError(returnXml, "This Model does not apply for any PCN, this unit must not have a DiagCod");
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Este modelo no aplica para ningun PCN, no debe llevar ningun DiagCode");
                            }
                        }
                    }
                    else
                    {
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "This Model does not apply for any PCN, this unit must not have a DiagCod");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Este modelo no aplica para ningun PCN, no debe llevar ningun DiagCode");
                        }
                    }
                }
                #endregion

                #region "Common Repair project update FF"
                Functions.DebugOut("-----  GetFFValue  --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ItemID", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "Com_Rpr".ToString().Length, ParameterDirection.Input) { Value = "Com_Rpr" });
                myParams.Add(new OracleParameter("InvAttribute", OracleDbType.Varchar2, 1.ToString().Length, ParameterDirection.Input) { Value = 1 });
                myParams.Add(new OracleParameter("workcenterId", OracleDbType.Varchar2, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetFFValue", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    if (strResult.ToUpper().Contains("YES"))
                    {
                        Functions.DebugOut("-----  UpdateFFValue  --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("ItemBCN", OracleDbType.Varchar2, bcn.ToString().Length, ParameterDirection.Input) { Value = bcn });
                        myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "Com_Rpr".ToString().Length, ParameterDirection.Input) { Value = "Com_Rpr" });
                        myParams.Add(new OracleParameter("FFValue", OracleDbType.Varchar2, "NO".ToString().Length, ParameterDirection.Input) { Value = "NO" });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "UpdateFFValue", myParams);
                        if (!strResult.ToUpper().Contains("SUCCESS"))
                        {
                            return SetXmlError(returnXml, "Error updating Com_Rpr FF");
                        }
                    }
                }
                #endregion
            }           

            #region "ForceResultbyDefect"
            if (!result.ToUpper().Contains("SCRAP") && !result.ToUpper().Contains("TO_ECO") && !result.ToUpper().Contains("BGA")
                && !result.ToUpper().Contains("HOLD") && !result.ToUpper().Contains("DIAGS") && !result.ToUpper().Contains("NPF_HC"))
            {
                string DefectList = string.Empty;
                string ResultCode = result;
                string Priority = string.Empty;
                Functions.DebugOut("-----  ForceResultbyDefect  --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("GeoId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                myParams.Add(new OracleParameter("ClientID", OracleDbType.Int32, client.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams.Add(new OracleParameter("ContractID", OracleDbType.Int32, contract.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                myParams.Add(new OracleParameter("wcId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                myParams.Add(new OracleParameter("pnId", OracleDbType.Int32, partid.ToString().Length, ParameterDirection.Input) { Value = partid });
                myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("WorkOrderId", OracleDbType.Int32, RefOrder.ToString().Length, ParameterDirection.Input) { Value = workorderID });
                myParams.Add(new OracleParameter("ResCode", OracleDbType.Varchar2, result.ToString().Length, ParameterDirection.Input) { Value = result });
                myParams.Add(new OracleParameter("DefectList", OracleDbType.Varchar2, 1000, DefectList, ParameterDirection.Output));
                myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                dtResult = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetResultByDefecttoForce", myParams.ToArray());
                DefectList = myParams[9].Value.ToString();
                if (dtResult != null) /* If there is no data in the table is not required to continue whit validation */
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(DefectList))
                        {
                            int row;
                            string[] Defect = DefectList.Split(',');
                            for (row = 0; row < Defect.Count(); row++)
                            {
                                var rowDef = (from rowdt in dtResult.AsEnumerable()
                                              where rowdt.Field<string>(dtResult.Columns["Name"]).ToUpper() == Defect[row].ToString().ToUpper()
                                              select rowdt);

                                if (rowDef.Count() > 0)
                                {
                                    DataTable dtRes = rowDef.CopyToDataTable();
                                    if (dtRes != null)
                                    {
                                        if (dtRes.Rows.Count > 0)
                                        {
                                            for (int rowRes = 0; rowRes < dtRes.Rows.Count; rowRes++)
                                            {
                                                if (dtRes.Rows[rowRes]["result_code"].ToString().ToUpper() != ResultCode.ToUpper())
                                                {
                                                    if (LanguageInd == 0)
                                                    {
                                                        return SetXmlError(returnXml, "ERROR: You must select result code " + dtRes.Rows[rowRes]["result_code"].ToString());
                                                    }
                                                    else
                                                    {
                                                        return SetXmlError(returnXml, "ERROR: Debe seleccionar el result code " + dtRes.Rows[rowRes]["result_code"].ToString());
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
            }
            #endregion

            #region "3258 Val for Rey Troubleshoot"
            /*On Rey troubleshoot verify if defect 3258 was captured on DTV_Heat_Chamber and if this is correct force, HDD replacement*/
            if (part.Equals("R15-500"))
            {
                if (workcenter.ToUpper().Contains("Troubleshoot".ToUpper()) && !result.ToUpper().Contains("SCRAP"))
                {
                    /*Verify if unit fail of 3258 on DTV_Heat_Chamber*/
                    
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("locationID", OracleDbType.Int32 , LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("ItemID", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("defCode", OracleDbType.Varchar2, "3258".ToString().Length, ParameterDirection.Input) { Value = "3258" });
                    myParams.Add(new OracleParameter("workcenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                    myParams.Add(new OracleParameter("PreviousWC", OracleDbType.Varchar2, "DTV_Heat_Chamber".ToString().Length, ParameterDirection.Input) { Value = "DTV_Heat_Chamber" });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString,Schema_name, "DTVHDDSwapVal", "ValDefectOnPreviousWC", myParams);
                    if (!string.IsNullOrEmpty(strResult))
                    {
                        if (strResult.ToUpper().Contains("TRUE"))
                        {/*if yes, verify if 3258+HDD was captured in Troubleshoot*/
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = geo });
                            myParams.Add(new OracleParameter("ItemID", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                            myParams.Add(new OracleParameter("defCode", OracleDbType.Varchar2, "3258".ToString().Length, ParameterDirection.Input) { Value = "3258" });
                            myParams.Add(new OracleParameter("workcenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                            myParams.Add(new OracleParameter("compName", OracleDbType.Varchar2, "COMP".ToString().Length, ParameterDirection.Input) { Value = "COMP" });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                            strResult = Functions.DbFetch(this.ConnectionString,Schema_name, "DTVHDDSwapVal", "ValHDDreplacementbyDefect", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                if (strResult.Equals("FALSE"))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], "Glb_Engineering");
                                    if (LanguageInd == 0)
                                    {
                                        return SetXmlError(returnXml, "Unit failed with 3258 en HC, replace HDD and capture this specific defect or get Supervisor approval");
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Unidad fallo de 3258 en HC, debe reemplazar el HDD y capturar el mismo defecto o consiga la aprobacion del Supervisor");
                                    }
                                }
                                if (strResult.StartsWith("ER"))
                                {
                                    return SetXmlError(returnXml, strResult);
                                }
                            }
                        }
                    }
                }
            }


            #endregion

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

        private String GetMultipleDiagCodes(XmlDocument XmlIn)
        {
            XmlNodeList nodes;
            string ffValue = null;
            nodes = XmlIn.SelectNodes("/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode");
            for (int i = 0; i < nodes.Count; i++)
            {
                ffValue = nodes.Item(i).FirstChild.InnerXml.ToUpper() + "," + ffValue;
            }
            return ffValue;
        }

        private static DateTime JulianToDateTime(int julianDate)
        {
            int RealJulian = julianDate;
            int Year = Convert.ToInt32(RealJulian.ToString().Substring(0, 4));
            int DoY = Convert.ToInt32(RealJulian.ToString().Substring(4, 3));
            DateTime dtOut = new DateTime(Year, 1, 1);
            return dtOut.AddDays(DoY - 1);
        }

        /// <summary>
        /// To get dates for PCN by manf_Range validation. 
        /// Returns 3 dates, SerialNo date, Minimun date range and maximun date range.
        /// </summary>
        /// <param name="MfCode"></param>
        /// <param name="part"></param>
        /// <param name="SN"></param>
        /// <param name="rid"></param>
        /// <param name="LocationId"></param>
        /// <param name="Schema_name"></param>
        /// <param name="Package_name"></param>
        /// <returns></returns>
        public string GetManufDate(string MfCode, string part, string SN, string rid, int LocationId, string Schema_name, string Package_name)
        {
            if (dtDayRef == null || dtDayRef.Rows.Count <= 0)
            {
                Functions.DebugOut("-----  Getting Table for Day Reference  --------> ");
                List<OracleParameter> myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                myParams.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("GeoId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                myParams.Add(new OracleParameter("Queryclass", OracleDbType.Varchar2, "DayRef".ToString().Length, ParameterDirection.Input) { Value = "DayRef" });
                myParams.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                dtDayRef = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetNonMandatoryPCNs", myParams.ToArray());
            }

            DateTime StrManufDR;
            int StrYear;
            string DMVar;
            int GetDateVar = 0;

            string[] Dates = MfCode.Split(',');

            if (Dates.Count() > 0)
            {
                for (int Dint = 0; Dint < Dates.Count(); Dint++)
                {
                    MfCode = Dates[Dint];
                    StrYear = Convert.ToInt32(MfCode.Substring(0, 1));
                    DMVar = MfCode.Substring(1, 2);
                    if (StrYear <= 4)
                    {
                        switch (StrYear)
                        {
                            case 0:
                                StrYear = 10;
                                break;
                            case 1:
                                StrYear = 11;
                                break;
                            case 2:
                                StrYear = 12;
                                break;
                            case 3:
                                StrYear = 13;
                                break;
                            case 4:
                                StrYear = 14;
                                break;
                        }
                    }

                    var qDayRef = (from row in dtDayRef.AsEnumerable()
                                   where row.Field<string>(dtDayRef.Columns["code"]).ToUpper() == DMVar.ToUpper()
                                   select row);

                    DataTable dt = qDayRef.CopyToDataTable();
                    int DayRef = Convert.ToInt32(dt.Rows[0]["day_of_year"].ToString());

                    /* Get manufacture date */
                    if (DayRef.ToString() != null)
                    {
                        if ((DayRef >= 1) && DayRef <= 9)
                        {
                            GetDateVar = Convert.ToInt32(StrYear + "00" + DayRef);
                        }
                        else
                        {
                            if ((DayRef >= 10) && (DayRef <= 99))
                            {
                                GetDateVar = Convert.ToInt32(StrYear + "0" + DayRef);
                            }
                            else
                            {
                                if ((DayRef >= 100) && (DayRef <= 366))
                                {
                                    GetDateVar = Convert.ToInt32(StrYear + "" + DayRef);
                                }
                                else
                                {
                                    StrManufDR = Convert.ToDateTime("17-Nov-1858");
                                }
                            }
                        }
                    }

                    GetDateVar = GetDateVar + 2000000;
                    StrManufDR = JulianToDateTime(GetDateVar);

                    Dates[Dint] = StrManufDR.ToString();
                }
            }
            return Dates[0] + "," + Dates[1] + "," + Dates[2];
        }

        /// <summary>
        /// InsertECORecord.- Use in ECOValidation region.
        /// If there is not record for ECOCode in Client_ECO_SN_History table, this function will insert one.
        /// </summary>
        /// <returns></returns>
        public string InsertECORecord(string Client, string partNumber, string SN, string ecoCode, string ecoClose, string UserName, string Schema_name, string Package_name)
        {
            try
            {
                Functions.DebugOut("-----  InsertingECORecord --------> ");
                List<OracleParameter> myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("Client", OracleDbType.Varchar2, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
                myParams.Add(new OracleParameter("partNumber", OracleDbType.Varchar2, partNumber.ToString().Length, ParameterDirection.Input) { Value = partNumber });
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("ecoCode", OracleDbType.Varchar2, ecoCode.ToString().Length, ParameterDirection.Input) { Value = ecoCode });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                myParams.Add(new OracleParameter("ecoClose", OracleDbType.Varchar2, ecoClose.ToString().Length, ParameterDirection.Input) { Value = ecoClose });
                string strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "InsertingECORecord", myParams);
                return strResult;
            }
            catch (Exception e)
            { return e.Message; }
        }
    }
}