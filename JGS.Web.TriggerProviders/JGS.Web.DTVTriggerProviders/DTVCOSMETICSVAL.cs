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
    public class DTVCOSMETICSVAL : TriggerProviderBase
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
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}
		};
        public override string Name { get; set; }

        public DTVCOSMETICSVAL ()
        {
            this.Name = "DTVCOSMETICSVAL";
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
            string Family;
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
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVCOSMETICSVAL".ToString().Length, ParameterDirection.Input) { Value = "DTVCOSMETICSVAL" });
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
            if (nextWorkcenter.Equals("ERWC"))
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
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "InsertingScrapRecordAtTimeout", myParams);
                    if (!strResult.ToUpper().Equals("SUCCESS"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        SetXmlSuccess(returnXml);
                        return returnXml;
                    }
                }
                #endregion
                #region "Convert Val"
                if (result.ToUpper().Equals("CONVERT"))
                {
                    /*Validate if model is D11-100, D11I-100, etc*/
                    if (part != "HR20-100" && part != "HR20I-100" && part != "D11-100" && part != "D11I-100" && part != "H23-600" && part != "COM23-600")
                    {
                        if (LanguageInd == 1)
                        {
                            return SetXmlError(returnXml, "Numero de parte " + part + " no se puede enviar a CONVERT!");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Part No "+ part+" can not be sent to CONVERT!");
                        }
                    }
                }

                #endregion

            }
            #region "CVT and Label Validation"
            /*1. if unit gets a replacement of bezel or label, force to select LABEL
              2. if unit gets cover replacement, force to select CVT(cosmetic verification test) result code 
              3. if unit was cleaned only, select result code NPF*/
            Functions.DebugOut("-----  getCosmeticsDefects --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter(":LocID", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) {Value = LocationId });
            myParams.Add(new OracleParameter(":WcID", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter(":ItemId", OracleDbType.Int32, itemId.Length , ParameterDirection.Input){Value = itemId});
            myParams.Add(new OracleParameter(":useName", OracleDbType.Varchar2, UserName.Length , ParameterDirection.Input){Value = UserName});
            myParams.Add(new OracleParameter(":p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            DataTable dtCosmeticDef = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTVCosmetics.GetDefectsbyWC", myParams.ToArray());
            if (dtCosmeticDef.Rows.Count >= 1)
            {
                string defect;
                bool label=false;
                bool cvt=false;

                for (int i = 0; i+1 <= dtCosmeticDef.Rows.Count; i++)
                {
                    defect = dtCosmeticDef.Rows[i]["DefName"].ToString();
                    if (defect.Substring(0, 2).Equals("21") || defect.Substring(0, 2).Equals("26"))
                    {
                        label = true;
                    }
                    else if(defect.Substring(0, 2).Equals("23"))
                    {
                        cvt = true;
                    }

                }
                if (label)
                {
                    if(!result.ToUpper().Equals("LABEL"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "LABEL");
                    }
                }
                else if (cvt)
                {
                    if (!result.ToUpper().Equals("REPAIRED"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "REPAIRED");
                    }
                }
            }


            #endregion

            #region "Validate Cosmetic Repair Vs Cosmetic Inspection"
            if (String.IsNullOrEmpty(Functions.ExtractValue(xmlIn, _xPaths["XML_ROLENAME"])))
            {
                Functions.DebugOut("-----  getCosmeticsInspectionDefects --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter(":LocID", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                myParams.Add(new OracleParameter(":ItemId", OracleDbType.Int32, itemId.Length, ParameterDirection.Input) { Value = itemId });
                myParams.Add(new OracleParameter(":useName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                myParams.Add(new OracleParameter(":p_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                DataTable dtCosmeticInspDef = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTVCosmetics.GetDefectsbyCosmeticIns", myParams.ToArray());            
                if (dtCosmeticInspDef.Rows.Count >= 1  )
                {
                    string defectbyInsp;
                    string DefectbyCos;
                    bool getaproval= false;
                    
                    for (int iDx = 0; iDx+1 <= dtCosmeticInspDef.Rows.Count; iDx++)
                    {
                        bool found = false;
                        defectbyInsp = dtCosmeticInspDef.Rows[iDx]["DefName"].ToString();
                        for (int i = 0; i+1 <= dtCosmeticDef.Rows.Count; i++)
                        {
                            DefectbyCos = dtCosmeticDef.Rows[i]["DefName"].ToString();
                            if (defectbyInsp.Equals(DefectbyCos))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            getaproval = true;
                            break;
                        }
                    }
                    if (getaproval)
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], "Glb_Team_Lead");
                        if (LanguageInd == 1)
                        {
                            return SetXmlError(returnXml, "Defectos en cosmeticos no coinciden con defectos en Inspection, se requiere autorizacion de Supervisor");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Defects of Cosmetics are not equal than Defects of Inspection, Supervisor approval is required");
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
    }
}