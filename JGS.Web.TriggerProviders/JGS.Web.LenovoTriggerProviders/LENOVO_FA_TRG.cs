using System;
using System.Collections.Generic;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.DAL;
using System.Text.RegularExpressions;

namespace JGS.Web.TriggerProviders
{
    public class LENOVO_FA_TRG:TriggerProviderBase
    {
        private string remCode = null;
        private string cid = null;
        private string repairable = null;
        private string warrantyStatus = null;
        private string manuDate = null;
        private string dispositionCode = null;
        private string dateCode = null;
        private string destWhr = null;
        private string destZone = null;
        private string destBin = null;
        private string warranty = null;
        private string productClass = null;
        private string productSubClass = null;
        private string locAbbr = null;
        private string sapLineNo = null;
        

        public override string Name { get; set; }

        public LENOVO_FA_TRG()
        {
            this.Name = "LENOVO_FA_TRG";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string Schema_name = "WEBAPP1";
            string Package_name = "LENOVO_WUR_FA";
            int locationID;
            int clientID;
            int contractID;
            int optId;
            int wcId;
            string locationName;
            string clientName;
            string contractName;
            string wholeUnitSn;
            string workcenter;
            string triggerType;
            string UserName;
            bool foundRemCode = false;

            /**********************Getting values from xml*******************/
            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_LOCATIONID"]))
                locationID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_LOCATIONID"]));
            else 
                return SetXmlError(returnXml, "Geography Id can not be found.");
            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_CLIENTID"]))
                clientID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_CLIENTID"]));
            else
                return SetXmlError(returnXml, "Client Id can not be found.");
            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_CONTRACTID"]))
                contractID = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_CONTRACTID"]));
            else
                return SetXmlError(returnXml, "Contract Id can not be found.");
            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_TRIGGERTYPE"]))
                triggerType = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Trigger type can not be found.");
            //-- Get Unit serial number
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_SN"]))
                wholeUnitSn = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_SN"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Unit Serial Number can not be found.");
            //-- Get OPTId
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_OPTID"]))
                optId = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_OPTID"]));
            else
                return SetXmlError(returnXml, "OPTID can not be found.");
            //-- Get WCId
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_WORKCENTERID"]))
                wcId = Int32.Parse(Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_WORKCENTERID"]));
            else
                return SetXmlError(returnXml, "wcId can not be found.");
            //-- Get WC
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_WORKCENTER"]))
                workcenter = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_WORKCENTER"]);
            else
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, xLenovoDictionary._xPaths["XML_USERNAME"]))
                UserName = Functions.ExtractValue(xmlIn, xLenovoDictionary._xPaths["XML_USERNAME"]).Trim();
            else
                return SetXmlError(returnXml, "UserName could not be found.");

            /*************************************************Starts FA Trigger*************************************************/
            if (triggerType.ToUpper() == "FAILUREANALYSIS")
            {

                /********************Getting geo,client & contract****************************/
                Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
                {
                  {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
                 ,{Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
                 ,{Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
                };

                //Call the DB to get necessary data from Oracle 
                queries["GeoName"].ParameterValue = locationID.ToString();
                queries["ClientName"].ParameterValue = clientID.ToString();
                queries["ContractName"].ParameterValue = contractID.ToString();

                Functions.GetMultipleDbValues(this.ConnectionString, queries);
                locationName = queries["GeoName"].Result;
                clientName = queries["ClientName"].Result;
                contractName = queries["ContractName"].Result;

                if (String.IsNullOrEmpty(locationName))
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

                /***getting SAPLineNo from custom table***/
                this.sapLineNo = getSAPLineNo(Schema_name, Package_name, locationID, clientID, contractID, optId, wcId, "N/A", "N/A",
                                  "N/A", "N/A", "N/A", "N/A", "N/A", UserName);
                
                
                /*********************Loop through component defective parts*************************************/
                StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
                XmlNodeList defList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                foreach (XmlNode defectCode in defList)
                {
                 string defect = defectCode["DefectCodeName"].InnerText;
                 XmlNodeList actList = defectCode.SelectNodes("ActionCodeList/ActionCode");
                 foreach (XmlNode actionCode in actList)
                 {
                  string action = actionCode["ActionCodeName"].InnerText;
                  XmlNodeList compList = actionCode.SelectNodes("ComponentCodeList/DefectiveList/Component");
                  foreach (XmlNode defComp in compList)
                  {
                   string sbi = defComp["SerializedBulkIndicator"].InnerText;
                   if (sbi.Equals("BCN", ignoreCase))
                   {
                       string compPN = defComp["ComponentPartNo"].InnerText;
                       string mPN = defComp["ManuFacturerPart"].InnerText; //must change
                       string manufacturer = defComp["Manufacturer"].InnerText;//must change
                       string owner = defComp["Owner"].InnerText;
                       string CompCond = defComp["Condition"].InnerText;
                       string serialNo = defComp["ComponentSerialNo"].InnerText;
                       string strResult = null;
                       string ffName = null;
                       

                       //getting FF Value from FA
                       XmlNodeList faFF = defComp.SelectNodes("FAFlexFieldList/FlexField");
                       foreach (XmlNode ff in faFF)
                       {
                        ffName = ff["Name"].InnerText;
                        if (ffName.Equals("CID Found"))
                            this.cid = ff["Value"].InnerText;
                        else if (ffName.Equals("Repairable"))
                            this.repairable = ff["Value"].InnerText;
                        else if (ffName.Equals("Date Code"))
                            this.dateCode = ff["Value"].InnerText;
                       }

                       //getting redemption code from LUCI file - Lenovo PN
                       List<OracleParameter> myParams = new List<OracleParameter>();
                       myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                       myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                       strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getRedemptionCode", myParams);

                       if (!string.IsNullOrEmpty(strResult))
                       {
                           if (strResult.StartsWith("Error") == true)
                               return SetXmlError(returnXml, strResult);
                           else
                               this.remCode = strResult;
                       }
                       else { return SetXmlError(returnXml, "Error - Problems to get the RemCode"); }

                       DataSet spDataSet = new DataSet();
                       OracleParameter[] myParam = new OracleParameter[8];
                       myParam[0] = new OracleParameter("locationID", OracleDbType.Int32, locationID, ParameterDirection.Input);
                       myParam[1] = new OracleParameter("clientID", OracleDbType.Int32, clientID, ParameterDirection.Input);
                       myParam[2] = new OracleParameter("contractID", OracleDbType.Int32, contractID, ParameterDirection.Input);
                       myParam[3] = new OracleParameter("optId", OracleDbType.Int32, optId, ParameterDirection.Input);
                       myParam[4] = new OracleParameter("wcId", OracleDbType.Int32, wcId, ParameterDirection.Input);
                       myParam[5] = new OracleParameter("remCode", OracleDbType.Varchar2, this.remCode.ToUpper(), ParameterDirection.Input);
                       myParam[6] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                       myParam[7] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                       spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".removeFAValidationsNoWarr", myParam);

                       if (spDataSet.Tables[0].Rows.Count > 0)
                       {
                         foreach (DataRow DR in spDataSet.Tables[0].Rows)
                          {
                           try
                            {
                               strResult = DR["ERRORMSG"].ToString();
                               if (!string.IsNullOrEmpty(strResult))
                               {
                                   if (strResult.Equals("SUCCESS", ignoreCase))
                                   {
                                       this.destWhr = DR["Warehouse"].ToString();
                                       this.destZone = DR["Zone"].ToString();
                                       this.destBin = DR["Bin"].ToString();
                                       this.dispositionCode = DR["dispCode"].ToString();
                                       this.warrantyStatus = DR["WarrantyStatus"].ToString();
                                       CompCond = DR["conditionName"].ToString();
                                       foundRemCode = true;

                                   }
                                   else { return SetXmlError(returnXml, strResult); }

                                   
                                }
                            }catch (Exception ex)
                             {
                                return SetXmlError(returnXml, ex.ToString());
                             }
                               
                          }
                        }

                       if (foundRemCode==false) //other remCodes
                       {

                           //getting product class and subclass - MPN
                           strResult = null;
                           spDataSet = new DataSet();
                           myParam = new OracleParameter[3];
                           myParam[0] = new OracleParameter("partno", OracleDbType.Varchar2, compPN, ParameterDirection.Input);
                           myParam[1] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                           myParam[2] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                           spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".getProductClassNSubClass", myParam);

                           if (spDataSet.Tables[0].Rows.Count > 0)
                           {
                               foreach (DataRow DR in spDataSet.Tables[0].Rows)
                               {
                                   try
                                   {
                                       strResult = DR["ERRORMSG"].ToString();
                                       if (!string.IsNullOrEmpty(strResult))
                                       {
                                           return SetXmlError(returnXml, strResult);
                                       }
                                       else
                                       {
                                           productClass = DR["PC"].ToString();
                                           productSubClass = DR["PSC"].ToString();
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       return SetXmlError(returnXml, ex.ToString());
                                   }
                               }
                           }

                           string groupDisp = null;
                           strResult = null;
                           myParams = new List<OracleParameter>();
                           myParams.Add(new OracleParameter("geoId", OracleDbType.Int32, locationID.ToString().Length, ParameterDirection.Input) { Value = locationID });
                           myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientID.ToString().Length, ParameterDirection.Input) { Value = clientID });
                           myParams.Add(new OracleParameter("productClass", OracleDbType.Varchar2, productClass.Length, ParameterDirection.Input) { Value = productClass });
                           myParams.Add(new OracleParameter("productSubClass", OracleDbType.Varchar2, productSubClass.Length, ParameterDirection.Input) { Value = productSubClass });
                           myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                           strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getGroupDisp", myParams);

                           if (!string.IsNullOrEmpty(strResult))
                           {
                               if (strResult.StartsWith("Error") == true)
                               {
                                   return SetXmlError(returnXml, strResult);
                               }
                               else
                               {
                                   groupDisp = strResult;
                               }
                           }

                           if (groupDisp.Equals("SCRAP-ALL", ignoreCase))
                           {
                               this.warranty = "OOW";
                           }
                           else
                           {

                           //warranty validation
                           strResult = null;
                           string nDateName = null;
                           myParams = new List<OracleParameter>();
                           myParams.Add(new OracleParameter("location",OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input){ Value= locationName});
                           myParams.Add(new OracleParameter("client", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
                           myParams.Add(new OracleParameter("contract", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
                           myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                           myParams.Add(new OracleParameter("mpn", OracleDbType.Varchar2, mPN.Length, ParameterDirection.Input) { Value = mPN});
                           strResult = Functions.DbFetch(this.ConnectionString, "CUSTOM1", "lenovo_warranty", "get_manufacture_date_name", myParams);
                           //strResult="11 S Serial Number";
                           if (string.IsNullOrEmpty(strResult))
                           {
                               return SetXmlError(returnXml, "Error - Problems determining manufacture_date_name");
                           }
                           else 
                           {
                               if (strResult.IndexOf("ERROR", 0) != -1)
                               {
                                   return SetXmlError(returnXml, strResult);
                               }
                           }

                           if (strResult.StartsWith("11S Serial Number", ignoreCase))
                           {
                               if (string.IsNullOrEmpty(serialNo)||!serialNo.ToUpper().StartsWith("11S"))
                               {
                                   return SetXmlError(returnXml, "Error - Please enter " + strResult);
                               }
                               else { nDateName = serialNo; }
                           }
                           else if (strResult.StartsWith("Serial Number", ignoreCase))
                           {
                               if (string.IsNullOrEmpty(serialNo))
                               {
                                   return SetXmlError(returnXml, "Error - Please enter " + strResult);
                               }
                               else { nDateName = serialNo; }
                           }
                           else if (strResult.StartsWith("Date Code", ignoreCase))
                           {
                               if (string.IsNullOrEmpty(this.dateCode))
                               {
                                   return SetXmlError(returnXml, "Error - Please enter " + strResult);
                               }
                               else { nDateName = this.dateCode; }
                           }
                           else {
                               this.warranty = strResult;//cnd
                           }

                           if (!string.IsNullOrEmpty(nDateName))
                           {
                               bool found = false;
                               string error = strResult;
                               int chr = strResult.IndexOf("(", 0);
                               strResult = strResult.Substring(chr);
                               char[] arr = new char[] { '(', ')' };
                               strResult = strResult.TrimStart(arr);
                               strResult = strResult.TrimEnd(arr);
                               strResult = strResult.Replace(" ", string.Empty);
                               string[] patterns = Regex.Split(strResult,"or");
                               foreach (string pattern in patterns)
                               {
                                   if (pattern.Length == nDateName.Length)
                                   {
                                       found = true;
                                   }
                               }
                               if (found==false)
                               {
                                   return SetXmlError(returnXml, "Error - Please verify the length of the following value:" + error);
                               }
                           }
                           if (!strResult.Equals("CND", ignoreCase))
                           {
                               strResult = null;
                               spDataSet = new DataSet();
                               myParam = new OracleParameter[9];
                               myParam[0] = new OracleParameter("location", OracleDbType.Varchar2, locationName, ParameterDirection.Input);
                               myParam[1] = new OracleParameter("client", OracleDbType.Varchar2, clientName, ParameterDirection.Input);
                               myParam[2] = new OracleParameter("contract", OracleDbType.Varchar2, contractName, ParameterDirection.Input);
                               myParam[3] = new OracleParameter("owner", OracleDbType.Varchar2, owner, ParameterDirection.Input);
                               myParam[4] = new OracleParameter("nDateName", OracleDbType.Varchar2, nDateName, ParameterDirection.Input);
                               myParam[5] = new OracleParameter("compPN", OracleDbType.Varchar2, compPN, ParameterDirection.Input);
                               myParam[6] = new OracleParameter("mpn", OracleDbType.Varchar2, mPN, ParameterDirection.Input);
                               myParam[7] = new OracleParameter("format", OracleDbType.Varchar2, "MM/DD/YYYY", ParameterDirection.Input);
                               myParam[8] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                               spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure,"CUSTOM1.lenovo_warranty.get_warranty_details", myParam);

                               if (spDataSet.Tables[0].Rows.Count > 0)
                               {
                                   foreach (DataRow DR in spDataSet.Tables[0].Rows)
                                   {
                                       try
                                       {
                                           strResult = DR["warranty_status"].ToString();
                                           if (string.IsNullOrEmpty(strResult))
                                           {
                                               return SetXmlError(returnXml, "Error - Problems determining warranty status");
                                           }
                                           else {
                                               if (strResult.IndexOf("ERROR", 0) != -1)
                                               {
                                                   return SetXmlError(returnXml, strResult);
                                               }
                                               else
                                               {
                                                   this.warranty = strResult;
                                                   if (!this.warranty.Equals("CND", ignoreCase))
                                                   {
                                                       this.manuDate = DR["manufacture_date"].ToString();
                                                   }
                                               }

                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           return SetXmlError(returnXml, ex.ToString());
                                       }
                                   }
                               }
                             }
                           }// else scrap-all check

                           spDataSet = new DataSet();
                           myParam = new OracleParameter[12];
                           myParam[0] = new OracleParameter("locationID", OracleDbType.Int32, locationID, ParameterDirection.Input);
                           myParam[1] = new OracleParameter("clientID", OracleDbType.Int32, clientID, ParameterDirection.Input);
                           myParam[2] = new OracleParameter("contractID", OracleDbType.Int32, contractID, ParameterDirection.Input);
                           myParam[3] = new OracleParameter("optId", OracleDbType.Int32, optId, ParameterDirection.Input);
                           myParam[4] = new OracleParameter("wcId", OracleDbType.Int32, wcId, ParameterDirection.Input);
                           myParam[5] = new OracleParameter("warrranty", OracleDbType.Varchar2, this.warranty.ToUpper(), ParameterDirection.Input);
                           myParam[6] = new OracleParameter("cid", OracleDbType.Varchar2, this.cid.ToUpper() , ParameterDirection.Input);
                           myParam[7] = new OracleParameter("repairable", OracleDbType.Varchar2, this.repairable.ToUpper(), ParameterDirection.Input);
                           myParam[8] = new OracleParameter("groupDisp", OracleDbType.Varchar2, groupDisp, ParameterDirection.Input);
                           myParam[9] = new OracleParameter("remCode", OracleDbType.Varchar2, this.remCode.ToUpper(), ParameterDirection.Input);
                           myParam[10] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                           myParam[11] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                           spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".removeFAValidations", myParam);

                           if (spDataSet.Tables[0].Rows.Count > 0)
                           {
                               foreach (DataRow DR in spDataSet.Tables[0].Rows)
                               {
                                   try
                                   {
                                       strResult = DR["ERRORMSG"].ToString();
                                       if (!string.IsNullOrEmpty(strResult))
                                       {
                                           return SetXmlError(returnXml, strResult);
                                       }
                                       else
                                       {
                                           this.destWhr = DR["Warehouse"].ToString();
                                           this.destZone = DR["Zone"].ToString();
                                           this.destBin = DR["Bin"].ToString();
                                           this.dispositionCode = DR["dispCode"].ToString();
                                           this.warrantyStatus = DR["WarrantyStatus"].ToString();
                                           this.remCode = DR["remCode"].ToString(); 
                                           CompCond = DR["conditionName"].ToString();
                                       }

                                   }
                                   catch (Exception ex)
                                   {
                                       return SetXmlError(returnXml, ex.ToString());
                                   }

                               }
                           }


                           
                        }//other remCodes


                       //updating the contidion
                       if (!string.IsNullOrEmpty(CompCond))
                       {
                           defComp["Condition"].InnerText = CompCond;
                       }

                       //Insert FFs
                       XmlNode remCodeNode = null;
                       XmlNode dispCodeNode = null;
                       XmlNode warrStatusNode = null;
                       XmlNode warrMFGNode = null;
                       XmlNode sapLineNoNode = null;
                       XmlNodeList faFFClone = defComp.SelectNodes("FAFlexFieldList/FlexField");
                       XmlNode singleNode = defComp.SelectSingleNode("FAFlexFieldList");

                       foreach (XmlNode ff in faFFClone)
                       {
                           if (!String.IsNullOrEmpty(this.remCode))
                           {
                               remCodeNode = ff.Clone();
                               remCodeNode["Name"].InnerText = "Redemption Code";
                               remCodeNode["Value"].InnerText = this.remCode;
                               singleNode.AppendChild(remCodeNode);
                           }
                           if (!String.IsNullOrEmpty(this.dispositionCode))
                           {
                               dispCodeNode = ff.Clone();
                               dispCodeNode["Name"].InnerText = "Disposition Code";
                               dispCodeNode["Value"].InnerText = this.dispositionCode;
                               singleNode.AppendChild(dispCodeNode);
                           }
                           if (!String.IsNullOrEmpty(this.warrantyStatus))
                           {
                               warrStatusNode = ff.Clone();
                               warrStatusNode["Name"].InnerText = "Warranty Status";
                               warrStatusNode["Value"].InnerText = this.warrantyStatus;
                               singleNode.AppendChild(warrStatusNode);
                           }
                           if (!String.IsNullOrEmpty(this.manuDate))
                           {
                               warrMFGNode = ff.Clone();
                               warrMFGNode["Name"].InnerText = "MFG Date";
                               warrMFGNode["Value"].InnerText = this.manuDate;
                               singleNode.AppendChild(warrMFGNode);
                           }
                           if (!String.IsNullOrEmpty(this.sapLineNo))
                           {
                               sapLineNoNode = ff.Clone();
                               sapLineNoNode["Name"].InnerText = "SAP_LINE_NO";
                               sapLineNoNode["Value"].InnerText = this.sapLineNo;
                               singleNode.AppendChild(sapLineNoNode);
                           }
                           break;
                       }

                       //changing the destination location for the removed comp
                       XmlNodeList sourceList = defComp.SelectNodes("Source");
                       foreach (XmlNode source in sourceList)
                       {
                           string whs = source["Warehouse"].InnerText;
                           string stkl = source["StockingLoc"].InnerText;
                           string bin = source["Bin"].InnerText;

                           if (!string.IsNullOrEmpty(whs) && !string.IsNullOrEmpty(this.destWhr))
                               source["Warehouse"].InnerText = this.destWhr;
                           else
                               return SetXmlError(returnXml, "Error - Problems setting destination Warehouse");

                           if (!string.IsNullOrEmpty(stkl) && !string.IsNullOrEmpty(this.destZone))
                               source["StockingLoc"].InnerText = this.destZone;
                           else
                               return SetXmlError(returnXml, "Error - Problems setting destination Zone");

                           if (!string.IsNullOrEmpty(bin) && !string.IsNullOrEmpty(this.destBin))
                               source["Bin"].InnerText = this.destBin;
                           else
                               return SetXmlError(returnXml, "Error - Problems setting destination Bin");
                       }
                   }else if (sbi.Equals("NONBCN", ignoreCase))
                    {
                        string CompCond = defComp["Condition"].InnerText;
                        int condId = 0;
                        string strResult = null;
                        string errorMsg;

                        List<OracleParameter> myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("conditionName", OracleDbType.Varchar2, CompCond.Length, ParameterDirection.Input) { Value = CompCond });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, "COMP_ISSUEREMOVE_LOCATION", "getConditionId", myParams);

                        if (!string.IsNullOrEmpty(strResult))
                        {
                            if (strResult.StartsWith("Error") == true)
                                return SetXmlError(returnXml, strResult);
                            else
                                condId = Int32.Parse(strResult);
                        }
                        else { return SetXmlError(returnXml, "Error - Problems to get the Condition ID"); }

                        Comp_IssueRemove_Location.getCompIssueRemoveLoc(this.ConnectionString, locationID, clientID, contractID, optId, wcId, condId, "N/A", "N/A",
                            "N/A", "N/A", "N/A", "REMOVE", UserName, ref this.destWhr, ref this.destZone, ref this.destBin, out errorMsg);
                        
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            return SetXmlError(returnXml, errorMsg);
                        }

                       XmlNode sapLineNoNode = null;
                       XmlNodeList faFFClone = defComp.SelectNodes("FAFlexFieldList/FlexField");
                       XmlNode singleNode = defComp.SelectSingleNode("FAFlexFieldList");

                       foreach (XmlNode ff in faFFClone)
                       {
                           if (!String.IsNullOrEmpty(this.sapLineNo))
                           {
                               sapLineNoNode = ff.Clone();
                               sapLineNoNode["Name"].InnerText = "SAP_LINE_NO";
                               sapLineNoNode["Value"].InnerText = this.sapLineNo;
                               singleNode.AppendChild(sapLineNoNode);
                           }
                           break;
                       }
                        //changing the destination location for the removed comp
                        XmlNodeList sourceList = defComp.SelectNodes("Source");
                        foreach (XmlNode source in sourceList)
                        {
                            string whs = source["Warehouse"].InnerText;
                            string stkl = source["StockingLoc"].InnerText;
                            string bin = source["Bin"].InnerText;

                            if (!string.IsNullOrEmpty(whs) && !string.IsNullOrEmpty(this.destWhr))
                                source["Warehouse"].InnerText = this.destWhr;
                            else
                                return SetXmlError(returnXml, "Error - Problems setting destination Warehouse");

                            if (!string.IsNullOrEmpty(stkl) && !string.IsNullOrEmpty(this.destZone))
                                source["StockingLoc"].InnerText = this.destZone;
                            else
                                return SetXmlError(returnXml, "Error - Problems setting destination Zone");

                            if (!string.IsNullOrEmpty(bin) && !string.IsNullOrEmpty(this.destBin))
                                source["Bin"].InnerText = this.destBin;
                            else
                                return SetXmlError(returnXml, "Error - Problems setting destination Bin");
                        }
                   } //sbi foreach
                  } //def comp foreach
                  XmlNodeList newCompList = actionCode.SelectNodes("ComponentCodeList/NewList/Component");
                  foreach (XmlNode newComp in newCompList)
                  {
                      if (!string.IsNullOrEmpty(this.sapLineNo))
                      {
                          XmlNode singleNode = newComp.SelectSingleNode("FAFlexFieldList");
                          XmlElement ffElement = returnXml.CreateElement("FlexField");
                          XmlElement ffName = returnXml.CreateElement("Name");
                          XmlElement ffValue = returnXml.CreateElement("Value");
                          ffName.InnerText = "SAP_LINE_NO";
                          ffValue.InnerText = this.sapLineNo;
                          ffElement.AppendChild(ffName);
                          ffElement.AppendChild(ffValue);
                          singleNode.AppendChild(ffElement);  
                      }
                   }//new comp foreach
                 } //action foreach   
                } //defect foreach
            }//failureAnalysis trigger type

            SetXmlSuccess(returnXml);
            return returnXml;
        }//execute
        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, xLenovoDictionary._xPaths["XML_RESULT"], EXECUTION_OK);
        }
        private void setRemCode(string remStr)
        {
            if (remStr.IndexOf(this.remCode) != -1) this.remCode = "TBD";
        }
        private void setLocation(string locStr)
        {
            int setCtn = 0;
            string[] loc = locStr.Split('/');
            foreach (string value in loc)
            {
                switch (setCtn)
                {
                    case 0: 
                        this.destWhr = value; 
                        break;
                    case 1: 
                        this.destZone= value;
                        break;
                    case 2: 
                        this.destBin = value;
                        break;
                }
                setCtn=setCtn+1;
            }
        }
        private string getSAPLineNo(string schemaName, string packageName, int locationID, int clientID, int contractID, int optID, int workCenterID, string warrStatus, string cidFound,
                                    string repairable, string groupDisp, string dispCode, string remCode, string warrStatusCheck, string username)
        {
            string strResult = null;
            List<OracleParameter> myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationID", OracleDbType.Int32, locationID.ToString().Length, ParameterDirection.Input) { Value = locationID });
            myParams.Add(new OracleParameter("clientID", OracleDbType.Int32, clientID.ToString().Length, ParameterDirection.Input) { Value = clientID });
            myParams.Add(new OracleParameter("contractID", OracleDbType.Int32, contractID.ToString().Length, ParameterDirection.Input) { Value = contractID });
            myParams.Add(new OracleParameter("optID", OracleDbType.Int32, optID.ToString().Length, ParameterDirection.Input) { Value = optID });
            myParams.Add(new OracleParameter("wcID", OracleDbType.Int32, workCenterID.ToString().Length, ParameterDirection.Input) { Value = workCenterID });
            myParams.Add(new OracleParameter("warrStatus", OracleDbType.Varchar2, warrStatus.Length, ParameterDirection.Input) { Value = warrStatus });
            myParams.Add(new OracleParameter("cidFound", OracleDbType.Varchar2, cidFound.Length, ParameterDirection.Input) { Value = cidFound });
            myParams.Add(new OracleParameter("repairable", OracleDbType.Varchar2, repairable.Length, ParameterDirection.Input) { Value = repairable });
            myParams.Add(new OracleParameter("groupDisp", OracleDbType.Varchar2, groupDisp.Length, ParameterDirection.Input) { Value = groupDisp });
            myParams.Add(new OracleParameter("dispCode", OracleDbType.Varchar2, dispCode.Length, ParameterDirection.Input) { Value = dispCode });
            myParams.Add(new OracleParameter("remCode", OracleDbType.Varchar2, remCode.Length, ParameterDirection.Input) { Value = remCode });
            myParams.Add(new OracleParameter("warrStatusCheck", OracleDbType.Varchar2, warrStatusCheck.Length, ParameterDirection.Input) { Value = warrStatusCheck });
            myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, username.Length, ParameterDirection.Input) { Value = username });
            strResult = Functions.DbFetch(this.ConnectionString, schemaName, packageName, "getSAPLineNo", myParams);
            return strResult;
         }
    }
}
