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
    public class DTVHDDSWAPVAL : TriggerProviderBase
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
		};
        public override string Name { get; set; }

        public DTVHDDSWAPVAL()
        {
            this.Name = "DTVHDDSWAPVAL";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTVCUUTOValidationQueries";

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
            string Family;

            #region "GetVariables"
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
            };

            //Call the DB to get necessary data from Oracle ///////////////
            queries["GeoName"].ParameterValue = LocationId.ToString();
            queries["ClientName"].ParameterValue = clientId.ToString();
            queries["ContractName"].ParameterValue = contractId.ToString();
            queries["PartId"].ParameterValue = itemId.ToString();
            Functions.GetMultipleDbValues(this.ConnectionString, queries);
            geo = queries["GeoName"].Result;
            client = queries["ClientName"].Result;
            contract = queries["ContractName"].Result;
            partid = queries["PartId"].Result;
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
            //Get Mayra Changes
            Dictionary<string, OracleQuickQuery> queries2 = new Dictionary<string, OracleQuickQuery>() 
            {
                {Name="Family", new OracleQuickQuery("INVENTORY1","PART_VW","COMMODITY_CLASS_NAME","Family","PART_ID={PARAMETER}")}
            };
            queries2["Family"].ParameterValue = partid;
            Functions.GetMultipleDbValues(this.ConnectionString, queries2);

            Family = queries2["Family"].Result;
            if (String.IsNullOrEmpty(Family))
            {
                Functions.DebugOut("Family Name can not be found.");
                return SetXmlError(returnXml, "Family Name can not be found.");
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
                //if DiagCode is more than one
                diagCode = GetMultipleDiagCodes(xmlIn);
                //diagCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DIAGCODE"]).Trim();
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
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVHDDSWAPVAL".ToString().Length, ParameterDirection.Input) { Value = "DTVHDDSWAPVAL" });
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
            if (nextWorkcenter.Equals("ERWC") || (result.ToUpper().Equals("SCRAP")))
            {
                #region "Insert Scrap Records"
                if (result.ToUpper().Equals("SCRAP"))
                {
                    Functions.DebugOut("-----  validateRecordScrapAtTimeout --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                    myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.Length, ParameterDirection.Input) { Value = client });
                    myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contract.Length, ParameterDirection.Input) { Value = contract });
                    myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("SCRAP-ERWC", OracleDbType.Varchar2, "SCRAP-ERWC".ToString().Length, ParameterDirection.Input) { Value = "SCRAP-ERWC" });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateRecordScrapAtTimeout", myParams);
                    if (!string.IsNullOrEmpty(strResult))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                }
                #endregion
                #region "TechReturnValidation"
                Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateTechnicalReturn", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    if (!strResult.ToUpper().Equals("TRUE") && !strResult.ToUpper().Equals("FALSE"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        if (strResult.ToUpper().Equals("TRUE"))
                        {
                            // need to inactive record in technicalReturntable
                            Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("ridnumber", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                            myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "inactivateTechnicalreturn", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                return SetXmlError(returnXml, strResult);
                            }
                        }
                    }
                }
                #endregion
            }

            #region "HDD SWAP WC Valdiation"
            if(workcenter.ToUpper().Equals("HDD_SWAP") && !result.ToUpper().Equals("SCRAP"))
            {
                if (String.IsNullOrEmpty(diagCode) || !diagCode.Contains("HDD_SWAP"))
                {
                    if (LanguageInd == 1)
                    { return SetXmlError(returnXml, "Codigo de Diagnostico obligatorio favor de seleccionar HDD SWAP"); }
                    else { return SetXmlError(returnXml, "Diagnostic Code is null, please select  HDD SWAP"); }
                }
            }

            #endregion
            
            #region "DTV_PREP, Pre-Test validations"
            if ((workcenter.ToUpper().Equals("Pre-Test".ToUpper()) || workcenter.ToUpper().Equals("DTV_PREP".ToUpper())) && !result.ToUpper().Equals("SCRAP"))
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
                        return SetXmlError(returnXml, "This units is not active for ECO, you must not select TO_ECO resultcode");
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Esta unidad NO esta activa en ECO, NO debe seleccionar el ResultCode TO_ECO");
                    }
                }
                #endregion

                if (workcenter.ToUpper().Equals("Pre-Test".ToUpper()))
                {
                    if (!result.ToUpper().Equals("TO_ECO") && !result.ToUpper().Contains("DEFECT") && !result.ToUpper().Contains("HDD_SWAP"))
                    {
                        #region "TechReturnValidation"
                        Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                        myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateTechnicalReturn", myParams);
                        if (!string.IsNullOrEmpty(strResult))
                        {
                            if (!strResult.ToUpper().Equals("TRUE") && !strResult.ToUpper().Equals("FALSE"))
                            {
                                return SetXmlError(returnXml, strResult);
                            }
                            else
                            {
                                if (strResult.ToUpper().Equals("TRUE") && !result.Equals("NPF_HC"))
                                {
                                    if (LanguageInd == 1) { return SetXmlError(returnXml, "Unidad es tech Return debe selecionar NPF_HC"); }
                                    else { return SetXmlError(returnXml, "Unit is Tech Return, you must select NPF_HC "); }
                                }
                                else if (strResult.ToUpper().Equals("FALSE"))
                                {
                                    if (Family.ToUpper().Equals("DVR") || Family.ToUpper().Equals("HD-DVR"))
                                    {
                                        if (!result.Equals("NPF_HC"))
                                        {
                                            if (LanguageInd == 1) { return SetXmlError(returnXml, "Unidad es HDDVR o DVR debe selecionar NPF_HC"); }
                                            else { return SetXmlError(returnXml, "Unit is HDDVR or DVR, you must select NPF_HC "); }
                                        }
                                    }
                                    else
                                    {
                                        if (result.Equals("NPF_HC"))
                                        {
                                            if (LanguageInd == 1) { return SetXmlError(returnXml, "Este resultCode es invalido, seleccione uno diferente"); }
                                            else { return SetXmlError(returnXml, "Unit is not Tech Return or HDDVR or DVR, you must select another resultCode "); }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #region "CaptureEscalationSN"
                    /*Deactivate record in client_escalation_sn table, if unit found in the table by  sn*/
                    Functions.DebugOut("------ Validate Capture SN Escalation -----");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                    myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                    myParams.Add(new OracleParameter("fieldName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = SN  });
                    myParams.Add(new OracleParameter("N", OracleDbType.Int32, ParameterDirection.Input) { Value = 2 });
                    myParams.Add(new OracleParameter("USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, "AutoTestTriggers","isTimeoutCapture", myParams);
                    if (!String.IsNullOrEmpty(strResult))
                    {
                        if (strResult.ToUpper().Equals("TRUE"))
                        {
                            Functions.DebugOut("------ Deactive Capture SN Escalation -----");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                            myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                            myParams.Add(new OracleParameter("fieldName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = SN });
                            myParams.Add(new OracleParameter("N", OracleDbType.Int32, ParameterDirection.Input) { Value = 2 });
                            myParams.Add(new OracleParameter("USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = UserName });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, "AutoTestTriggers", "setCaptureInactive", myParams);
                            if (!String.IsNullOrEmpty(strResult))
                            {
                                return SetXmlError(returnXml, "ER Capture Function" + strResult);
                            }
                        }
                    }
                    #endregion
                }

                #region "INSP HDDSwapValidation"
                if ((workcenter.ToUpper().Equals("Pre-Test".ToUpper()) || workcenter.ToUpper().Equals("DTV_PREP".ToUpper())) && (result.ToUpper().Trim().Equals("DEFECT") || result.ToUpper().Trim().Equals("DEFECTRP") || result.ToUpper().Trim().Equals("HDD_SWAP")))
                {
                    string DefectList = GetMultipleDefectsCodes(xmlIn);
                    if (String.IsNullOrEmpty(DefectList)|| DefectList.Equals("+"))
                    {
                        if (LanguageInd == 1)
                        {
                            return SetXmlError(returnXml, "Error en el resulado seleccionado no existen defectos por lo tanto  " + result + " es incorrecto  ");
                        }
                        else { return SetXmlError(returnXml, "Error on result code selected  " + result + " is not correct, you must select defects codes"); }

                    }
                    else
                    {
                    #region HDDSwap Validation Time out
                        Functions.DebugOut("--------- HDD SWAP VALIDATION ----------");
                        //(LocId IN NUMBER, Client IN VARCHAR2, ClientId IN NUMBER, ContractId IN NUMBER, OPT IN VARCHAR2, wcId IN NUMBER, 
                       //Part IN VARCHAR2, partId IN NUMBER, Serial IN VARCHAR2, itemId IN VARCHAR2, DefectList IN VARCHAR2, 
                        //UserName IN VARCHAR2, ResultCode OUT VARCHAR2);
                        try
                        {
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                            myParams.Add(new OracleParameter("Client", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                            myParams.Add(new OracleParameter("ClientID", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                            myParams.Add(new OracleParameter("ContractID", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                            myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                            myParams.Add(new OracleParameter("wcId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                            myParams.Add(new OracleParameter("Part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                            myParams.Add(new OracleParameter("PartId", OracleDbType.Int32, partid.ToString().Length, ParameterDirection.Input) { Value = Convert.ToInt32(partid) });
                            myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                            myParams.Add(new OracleParameter("itemId", OracleDbType.Varchar2, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                            myParams.Add(new OracleParameter("DefectList", OracleDbType.Varchar2, DefectList.ToString().Length, ParameterDirection.Input) { Value = DefectList });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                            myParams.Add(new OracleParameter("ResultCode", OracleDbType.Varchar2, 100, strResult, ParameterDirection.Output));

                            ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTVHDDSWAPVAL.HDDSwapValTO", myParams.ToArray());
                            strResult = myParams[12].Value.ToString();

                            if (!String.IsNullOrEmpty(strResult))
                            {
                                if (strResult != result)
                                {
                                    if (LanguageInd == 1)
                                    { return SetXmlError(returnXml, "Result Code " + result + " es incorrecto, debe seleccionar " + strResult); }
                                    else { return SetXmlError(returnXml, "Result Code " + result + " is incorrect, you must select " + strResult); }
                                }
                            }
                            else
                            {
                                if (LanguageInd == 1)
                                {
                                    return SetXmlError(returnXml, "Problemas en validacion de DTVHDDSWAPVALIDATIONS ");
                                }
                                else { return SetXmlError(returnXml, "Problems with DTVHDDSWAPVALIDATIONS package "); }
                            }
                        }
                        catch (Exception ex)
                        {
                            return SetXmlError(returnXml, "Err: DTVHDDSWAPVALIDATIONS "+ex);
                        }
                    #endregion
                    }
                }
                #endregion


            }
            #endregion

            #region "getResultCodetoSet"
            //Validate ResultCode configured on DTV_Model_Released table.
            if (result.ToUpper().Equals("COSMETIC") || result.ToUpper().Contains("PASS") || result.ToUpper().Equals("NPF"))
            {
                Functions.DebugOut("-----  getResultCodetoSet --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("LocName", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                myParams.Add(new OracleParameter("ClientName", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                myParams.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, contract.ToString().Length, ParameterDirection.Input) { Value = contract });
                myParams.Add(new OracleParameter("opt", OracleDbType.Varchar2, opt.ToString().Length, ParameterDirection.Input) { Value = opt });
                myParams.Add(new OracleParameter("wc", OracleDbType.Varchar2, workcenter.ToString().Length, ParameterDirection.Input) { Value = workcenter });
                myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                myParams.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                strResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "AutoTestTriggers", "GetResultCodetoSet", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    if (!strResult.ToUpper().Contains("SUCCESS"))
                    {
                        if (!result.ToUpper().Equals(strResult.ToUpper()))
                        {
                            UpdateXml(xmlIn, _xPaths["XML_RESULTCODE"], strResult);
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
        private String GetMultipleDefectsCodes(XmlDocument XMLIn)
        {
            XmlNodeList nodes;
            string defects = null;
            nodes = XMLIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
            for (int i = 0; i < nodes.Count; i++)
            {                
                if (String.IsNullOrEmpty(defects))
                {
                    defects = nodes.Item(i).FirstChild.InnerText.ToUpper();
                }
                else
                {
                    defects = defects + "+" + nodes.Item(i).FirstChild.InnerText.ToUpper();
                }
            }
            return defects;
        }

        /// <summary>
        /// Update the various fields in the XmlDocument after all the validation is completed.
        /// </summary>
        private void UpdateXml(XmlDocument returnXml, string path, string value)
        {
            Functions.UpdateXml(ref returnXml, path, value);
        }

        private XmlDocument AddWCFFXmlNode(XmlDocument XmlIn, string NodeName, string NodeValue)
        {
            XmlElement ffEle = XmlIn.CreateElement("ResultCodetoSet");
            //ffEle.AppendChild(XmlIn.CreateNode(XmlNodeType.Element, null, null));

            ffEle.InnerText = NodeValue;
            XmlIn.DocumentElement["Detail"]["TimeOut"].AppendChild(ffEle);

            return XmlIn;
        }  
    }
}