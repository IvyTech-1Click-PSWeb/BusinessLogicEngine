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
    public class DTVDEFECTCODEVAL : TriggerProviderBase
    {
        DataTable dtResult;
        /* This trigger will be used in submit button (FA) and TimeOut button.
             * The code 4Bxx could be use only at Troubleshoot, BGA and Failure Analisys WC's.
             * If code 4Bxx wants to be use it a FF must be fill out to capture the CRD. 
             * Validate if Defect is to force to HDD_SWAP and send to scrap */

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}            
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
            ,{"XML_DEFECTCODE","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/DefectCodeName"}
            ,{"XML_DAMAGE_CRD", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Damage_CRD']/Value"}
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}           
            ,{"XML_WORKORDERID","/Trigger/Detail/ItemLevel/WorkOrderID"} 
		};

        public override string Name { get; set; }

        public DTVDEFECTCODEVAL()
        {
            this.Name = "DTVDEFECTCODEVAL";
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
            string workcenter;
            int workcenterId;
            int LocationId;
            int LanguageInd = 0;
            string strResult;
            string routeId;
            string DefectCode = "";
            string UserName;
            string itemId;
            string TriggerType;
            string nextWorkcenter;
            string ResultCode = "";
            string SerialNo;
            string rid;
            string geo;
            string client;
            string contract;
            string opt= "";
            int workorderID;
            string partid;
            string RefOrder;

            #region "ExtractValue from xml"
            //-- Get WorkOrderId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKORDERID"]))
            {
                workorderID = Convert.ToInt32(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKORDERID"]).Trim());
            }
            else
            {
                return SetXmlError(returnXml, "WorkOrderId could not be found.");
            }

            //-- Get Order Process Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                opt = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
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

            //-- Get Order Process Type ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPTID"]))
            {
                routeId = Functions.ExtractValue(xmlIn, _xPaths["XML_OPTID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT ID cannot be empty.");
            }

            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item Id cannot be empty.");
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

            //-- Get TRIGGERTYPE
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                TriggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "TriggerType could not be found.");
            }

            //-- Get DefectCode if the trigger is FAILUREANALYSIS TYPE
            if (TriggerType.Contains("FAILURE"))
            {
                if (!Functions.IsNull(xmlIn, _xPaths["XML_DEFECTCODE"]))
                {
                    DefectCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DEFECTCODE"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "DefectCode could not be found.");
                }
            }

            if (TriggerType.Contains("TIMEOUT"))
            {
                //-- Get ResultCode
                if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                {
                    ResultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "ResultCode could not be found.");
                }
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SerialNo = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
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
            #endregion

            Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
            {
                {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
              , {Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
              , {Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
              , {Name="PartId", new OracleQuickQuery("INVENTORY1","ITEM","PART_ID","PartId","ITEM_ID = {PARAMETER}")}
              , {Name="RefOrder", new OracleQuickQuery("WC1","WORKORDER","reference_order_id","RefOrder","workorder_id = {PARAMETER}")}
            };

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

            /************************************************** SET LANGUAGE INDICATOR **************************************************/
            Functions.DebugOut("-----  SET LANGUAGE INDICATOR --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("routeId", OracleDbType.Int32, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
            myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVDEFECTCODEVAL".ToString().Length, ParameterDirection.Input) { Value = "DTVCUUTimeOutValidation" });
            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetLanguage", myParams);
            if (!string.IsNullOrEmpty(strResult))
            {
                LanguageInd = Int32.Parse(strResult);
            }
            else
            {
                LanguageInd = 0;
            }

            /****************************************** LOGIC START UP ***************************************/
            if (TriggerType.Contains("FAILURE"))
            {
                if (DefectCode.StartsWith("4B"))
                {
                    if (workcenter.ToUpper().Contains("TROUBLESHOOT") || workcenter.ToUpper().Trim().Equals("BGA") || workcenter.ToUpper().Contains("FAILURE"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], "Glb_Technician");
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "It is required a Technician role for use Defect Code 4Bxx");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Se requiere un Rol de Tecnico para utilizar el Defect Code 4Bxx");
                        }
                    }
                    else
                    {
                        if (LanguageInd == 0)
                        {
                            return SetXmlError(returnXml, "The DefectCode 4Bxx cannot be used in this workcenter");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "El codigo de defecto 4Bxx no puede ser utilizado en este workcenter");
                        }
                    }
                }

                if (workcenter.ToUpper().Contains("TROUBLESHOOT") || workcenter.ToUpper().Contains("HDD_SWAP"))
                {
                    string DefectList = string.Empty;
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
                    myParams.Add(new OracleParameter("ResCode", OracleDbType.Varchar2, "Defect".ToString().Length, ParameterDirection.Input) { Value = "Defect" });
                    myParams.Add(new OracleParameter("DefectList", OracleDbType.Varchar2, 1000, DefectList, ParameterDirection.Output));
                    myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                    dtResult = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetResultByDefecttoForce", myParams.ToArray());
                    if (dtResult != null) /* If there is no data in the table is not required to continue whit validation */
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(DefectCode))
                            {
                                var rowDef = (from rowdt in dtResult.AsEnumerable()
                                              where rowdt.Field<string>(dtResult.Columns["Name"]).ToUpper() == DefectCode.ToString().ToUpper()
                                              select rowdt);

                                if (rowDef.Count() > 0)
                                {
                                    DataTable dtRes = rowDef.CopyToDataTable();
                                    if (dtRes != null)
                                    {
                                        if (dtRes.Rows.Count > 0)
                                        {
                                            if (dtRes.Rows[0]["result_code"].ToString().ToUpper().Contains("SCRAP"))
                                            {
                                                if (!DefectCode.StartsWith("4"))
                                                {
                                                    if (LanguageInd == 0)
                                                    {
                                                        return SetXmlError(returnXml, "You must select a SCRAP Defect Code");
                                                    }
                                                    else
                                                    {
                                                        return SetXmlError(returnXml, "Debe seleccionar un Defect Code de SCRAP");
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
            else if (TriggerType.Contains("TIMEOUT"))
            {
                Functions.DebugOut("-----  GetDefectsByItemId --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                myParams.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                DataSet dsResult = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBADAPTERS1.QueryAdapter.GetUnitFAInfo", myParams.ToArray());
                if (dsResult != null)
                {
                    if (dsResult.Tables[0].Rows.Count > 0)
                    {
                        int defectCodeCount = (from rowFA in dsResult.Tables[0].AsEnumerable()
                                               where rowFA.Field<string>(dsResult.Tables[0].Columns["DefectCodeName"]).ToUpper().StartsWith("4B")
                                               select rowFA).Count();

                        if (defectCodeCount > 0)
                        {
                            if (workcenter.ToUpper().Contains("TROUBLESHOOT") || workcenter.ToUpper().Trim().Equals("BGA") || workcenter.ToUpper().Contains("FAILURE"))
                            {
                                if (Functions.NodeExists(xmlIn, _xPaths["XML_DAMAGE_CRD"]))
                                {
                                    if (Functions.IsNull(xmlIn, _xPaths["XML_DAMAGE_CRD"]))
                                    {
                                        if (LanguageInd == 0)
                                        {
                                            return SetXmlError(returnXml, "The Damage_CRD FF value cannot be null with this DefectCode");
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "El FF Damage_CRD no puede ser nulo");
                                        }
                                    }
                                }
                                else
                                {
                                    if (LanguageInd == 0)
                                    {
                                        return SetXmlError(returnXml, "The Damage_CRD FF does not exist for this workcenter");
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "El FF Damage_CRD no existe para este workcenter");
                                    }
                                }                               
                            }
                            else
                            {
                                if (LanguageInd == 0)
                                {
                                    return SetXmlError(returnXml, "The DefectCode 4Bxx cannot be used in this workcenter");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "El codigo de defecto 4Bxx no puede ser utilizado en este workcenter");
                                }
                            }
                        }
                    }
                }

                #region "getNextWorkcenterName"
                Functions.DebugOut("-----  getNextWorkcenterName --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("routeId", OracleDbType.Varchar2, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
                myParams.Add(new OracleParameter("workcenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                myParams.Add(new OracleParameter("result", OracleDbType.Varchar2, ResultCode.Length, ParameterDirection.Input) { Value = ResultCode });
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
                if (nextWorkcenter.Equals("ERWC") && ResultCode.ToUpper().Equals("SCRAP"))
                {

                    Functions.DebugOut("-----  Validate and Inactive TechnicalReturn --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("ridnumber", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                    myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SerialNo.ToString().Length, ParameterDirection.Input) { Value = SerialNo });
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
                    myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SerialNo.ToString().Length, ParameterDirection.Input) { Value = SerialNo });
                    myParams.Add(new OracleParameter("SCRAP-ERWC", OracleDbType.Varchar2, "SCRAP-ERWC".ToString().Length, ParameterDirection.Input) { Value = "SCRAP-ERWC" });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "InsertingScrapRecordAtTimeout", myParams);
                    if (!strResult.ToUpper().Contains("SUCCESS"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                }
                #endregion
            }

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
    }
}
