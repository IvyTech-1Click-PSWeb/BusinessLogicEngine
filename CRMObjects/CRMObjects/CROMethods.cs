using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRMObjects.CRMInput;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CRMObjects;
using CRMObjects.ReceiptWrapper;
using Oracle.DataAccess.Client;
using JGS.DAL;
using System.Data;
using System.Configuration;
using CRMObjects.RightNowSync;

namespace CRMObjects.Methods
{
    public class CROMethods
    {

        #region "Public Methods and Variables for Service Interactions"

        private static string WarrantyFlag = "Yes";
        private static string ConnString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ToString(); 
        //private static string ConnString = ""; 
       

        public static string obj2String(object objIn)
        {
            XmlSerializer ser = new XmlSerializer(objIn.GetType());
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            ser.Serialize(xw, objIn);

            return sw.ToString();
        }

        //**************************************************************************************************
        //                                      ORGANIZATION CHANGES
        //**************************************************************************************************
        private static string GetName(string id)
        {
            string result = "Error: No parent found.";
            if (id != null)
            {
                if (id.Length > 0)
                {
                    Organization organization = new Organization();
                    Organization organization2 = new Organization();

                    ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
                    clientInfoHeader.AppID = "Basic Get";

                    String queryString = "Select * from Organization O where O.ID = '" + id + "';";

                    RNObject[] objects = new RNObject[] { organization };

                    RightNowSyncPortClient _client;
                    _client = GetClient();

                    try
                    {
                        QueryResultData[] queryObjects = _client.QueryObjects(clientInfoHeader, queryString, objects, 10000);
                        RNObject[] rnObjects = queryObjects[0].RNObjectsResult;
                        if (rnObjects.Count() > 0)
                        {
                            organization2 = (Organization)rnObjects[0];
                            result = organization2.Name;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error: " + ex.Message;
                    }
                }
            }
            return result;
        }

        private static RightNowSyncPortClient GetClient()
        {
            RightNowSyncPortClient _client;
            _client = new RightNowSyncPortClient
            {
                ClientCredentials =
                {
                    UserName =
                    {
                        UserName = Properties.Settings.Default.UName,
                        Password = Properties.Settings.Default.PassW
                    }
                }
            };
            return _client;
        }
        //**************************************************************************************************
        //                                    END ORGANIZATION CHANGES
        //**************************************************************************************************

        public static CRM1ClickService CreateOrder(CRM1ClickService CRM)
        {
            int binId;
            string debugEnabled;
            try
            {
                debugEnabled = System.Configuration.ConfigurationManager.AppSettings.GetValues("debugEnabled")[0].ToString();
            }
            catch //(Exception exc)
            {
                debugEnabled = "FALSE";
            }

            try
            {
                if (debugEnabled == "TRUE")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(obj2String(CRM));
                    doc.Save("C:\\CRMSvc\\FinalXML.xml");
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, FinalXML", System.Diagnostics.TraceEventType.Information, doc.OuterXml);
                }  

                createReferenceOrderInfo master = new createReferenceOrderInfo();

                master.CallSource = "CRM";
                master.IP = CRM.header.userobj.hostname;
                master.APIUsage_ClientName = CRM.header.client;
                master.APIUsage_LocationName = CRM.header.location;

                master.OrderInfo = GetRefOrderInfo(new ReferenceOrderInfo(), CRM);               
                master.OrderInfo.shipToDo = new ContactAddressInfo();
                master.OrderInfo.shipToDo = GetShippingAddress(new ContactAddressInfo(), CRM);
                if (master.OrderInfo.shipToDo.AddressInfo.line4 != null)
                {
                    if (master.OrderInfo.shipToDo.AddressInfo.line4.Contains("ERROR:"))
                    {
                        CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                        CRM.detail.createorderinfo.returninfo.failmessage = master.OrderInfo.shipToDo.AddressInfo.line4;
                        return CRM;
                    }                    
                }
                master.OrderInfo.billToDo = new ContactAddressInfo(); // GetShippingAddress(new ContactAddressInfo(), CRM);
                master.OrderInfo.billToDo.ContactInfo = new ContactInfo();
                master.OrderInfo.billToDo.AddressInfo = new AddressInfo();
            
                try
                {
                    //master.UserName = "CRMEDI";
                    //master.Password = "CRMEDI";
                    //master.UserName = Properties.Settings.Default.RnrLogin;
                    master.UserName = System.Configuration.ConfigurationManager.AppSettings.GetValues("RnrLogin")[0].ToString();  //"CRMEDI";
                    master.Password = System.Configuration.ConfigurationManager.AppSettings.GetValues("RnrPassword")[0].ToString();  //"CRMEDI";
                    Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, RnR Credentials Configuration settings: ", System.Diagnostics.TraceEventType.Information, master.UserName);
                }
                catch (Exception exc)
                {
                    CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                    CRM.detail.createorderinfo.returninfo.failmessage = "CreateOrder, Get RnR Credentials Configuration settings Error - " + exc.Message;
                    Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Get RnR Credentials Configuration settings Error.", exc);
                    throw exc;
                }

                master.OrderInfo.btType = CRM.detail.createorderinfo.orderheader.btt;

                master.OrderInfo.clientRefNo1 = CRM.detail.contactinfo.contactphone.phoneno;
                master.OrderInfo.clientRefNo2 = CRM.detail.crmincidentno;
                master.OrderInfo.documentNo = "CRM-" + CRM.detail.crmincidentno;
                master.OrderInfo.clientAbbr = CRM.header.client;
                //master.OrderInfo.customerAbbr = CRM.detail.createorderinfo.orderheader.customertradingpartner;
                //*************************
                // ORGANIZATION CHANGE
                //*************************
                String TPName = GetName(CRM.detail.createorderinfo.orderheader.tradingpartner);
                if (!TPName.StartsWith("Error: "))
                {
                    master.OrderInfo.customerAbbr = TPName;
                }
                else
                {
                    master.OrderInfo.customerAbbr = CRM.detail.createorderinfo.orderheader.customertradingpartner;
                }
                //*************************

                //// Need to get Warehouse by Disposition type and Info from the First pass
                try
                {
                    binId = int.Parse(CRM.detail.createorderinfo.orderlinedetail.bin);
                    //GetWarehouseName(string dispositionType, int clientId, int contractId, int binId)                    
                }
                catch (Exception exc)
                {
                    Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, No BinID found.  ", exc);
                    //throw exc;
                    binId = 0;
                }
                master.OrderInfo.warehouse = GetWarehouseName(CRM.detail.dispositiontype, CRM.header.client, CRM.header.contract, binId);
                
                //if (CRM.detail.dispositiontype == "Customer Replacable Unit")
                //{
                //    master.OrderInfo.warehouse = "ANK-MAIN BUILDING"; // CRM.detail.partavailabilityinfo.returninfo.warehouse;
                //}
                //else
                //{
                //    //master.OrderInfo.warehouse = "ANK-MAIN BUILDING";
                //    master.OrderInfo.warehouse = "IST-MAIN BUILDING";
                //}

                master.OrderInfo.headerFlexFields = GetFlexFields(CRM.header.HeaderFlexFields);
                master.OrderInfo.geography = CRM.header.location;
                master.OrderInfo.contract = CRM.header.contract;
                master.OrderInfo.custOrderNo = CRM.detail.customerrma;
                master.OrderInfo.newLineList = GetRefOrderLines(CRM).ToArray(); // To be mapped

                //Trading partner Data
                //master.OrderInfo.OrdTradPart_BillShipToSameContactAndAddress = "false";
                //master.OrderInfo.OrdTradPart_Name = CRM.detail.createorderinfo.orderheader.customertradingpartner; 
                //master.OrderInfo.OrdTradPart_Note = "NET 45"; 
                //master.OrderInfo.OrdTradPart_OrderPaymentTerms =  "NET 45";

                master.OrderInfo.status = CRM.detail.createorderinfo.orderheader.orderstatus;
                master.OrderInfo.notes = CRM.detail.createorderinfo.orderheader.note;
                string partProdLine = GetProductLineByPartNo(CRM.detail.createorderinfo.orderlinedetail.part_no);
                string partBusinessLine = ConvertProdLineToBusiness(partProdLine);
                CRM.detail.partavailabilityinfo.productline = partBusinessLine;                
                
                ReceiptWrapperSoapClient svc = new ReceiptWrapperSoapClient();
                

                if (debugEnabled == "TRUE")
                {
                    //comment out before Release
                    XmlDocument adoc = new XmlDocument();
                    adoc.LoadXml(CROMethods.obj2String(master));
                    adoc.Save("C:\\CRMSvc\\xml\\PreWrapper_SN.xml");
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, PreWrapper", System.Diagnostics.TraceEventType.Information, adoc.OuterXml);

                    XmlDocument idoc = new XmlDocument();
                    string s = svc.CreateCROV2(master, true);                    
                    idoc.LoadXml(s);
                    idoc.Save("C:\\CRMSvc\\xml\\Wrapper_SN.xml");
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Wrapper", System.Diagnostics.TraceEventType.Information, idoc.OuterXml);

                    //
                    XmlDocument cdoc = new XmlDocument();
                    cdoc.LoadXml(CROMethods.obj2String(CRM));
                    adoc.Save("C:\\CRMSvc\\xml\\PreWrapper.xml");
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Wrapper, CRM", System.Diagnostics.TraceEventType.Information, cdoc.OuterXml);
                }

                string y = svc.CreateCROV2(master, false);
                int i;
                bool isNum = Int32.TryParse(y, out i);
                if (isNum == true)
                {
                    CRM.detail.createorderinfo.returninfo.result = "Success";
                    CRM.detail.createorderinfo.returninfo.rnrreferenceorderid = y;
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Success, svc.CreateCROV2(master, false) ", System.Diagnostics.TraceEventType.Information, idoc.OuterXml);
                }
                else
                {
                    CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                    CRM.detail.createorderinfo.returninfo.failmessage = y;
                    //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Unsuccessful, svc.CreateCROV2(master, false): "+y, System.Diagnostics.TraceEventType.Information, idoc.OuterXml);
                }
            }
            catch (Exception exc)
            {                
                CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                CRM.detail.createorderinfo.returninfo.failmessage = exc.Message;
                Logger.AddEntry(Logger.EventSource.Process, "CreateOrder", exc);
            }
            return CRM;
        }

        public static CRM1ClickService UpdateOrder(CRM1ClickService CRM)
        {
            try
            {
                createReferenceOrderInfo master = new createReferenceOrderInfo();

                master.CallSource = "CRM";
                master.IP = CRM.header.userobj.hostname;
                master.APIUsage_ClientName = CRM.header.client;
                master.APIUsage_LocationName = CRM.header.location;

                master.OrderInfo = GetRefOrderInfo(new ReferenceOrderInfo(), CRM);
                master.OrderInfo.status = CRM.detail.createorderinfo.orderheader.orderstatus;
                master.OrderInfo.clientRefNo2 = CRM.detail.crmincidentno;
                master.OrderInfo.id = CRM.detail.createorderinfo.returninfo.rnrreferenceorderid;

                master.OrderInfo.headerFlexFields = GetFlexFields(CRM.header.HeaderFlexFields);

                master.OrderInfo.shipToDo = new ContactAddressInfo();
                //master.OrderInfo.shipToDo = GetShippingAddress(new ContactAddressInfo(), CRM);
                master.OrderInfo.shipToDo = GetDellShippingAddress(new ContactAddressInfo(), CRM, CRM.header.HeaderFlexFields, CRM.detail.createorderinfo.orderheader.orderstatus);

                master.OrderInfo.billToDo = new ContactAddressInfo();
                master.OrderInfo.billToDo.ContactInfo = new ContactInfo();
                master.OrderInfo.billToDo.AddressInfo = new AddressInfo();

                XmlDocument pdoc = new XmlDocument();
                pdoc.LoadXml(CROMethods.obj2String(master));
                pdoc.Save("C:\\CRMSvc\\xml\\PostWrapper.xml");

                try
                {
                    master.UserName = System.Configuration.ConfigurationManager.AppSettings.GetValues("RnrLogin")[0].ToString();  //"CRMEDI";
                    master.Password = System.Configuration.ConfigurationManager.AppSettings.GetValues("RnrPassword")[0].ToString();  //"CRMEDI";
                    Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, RnR Credentials Configuration settings: ", System.Diagnostics.TraceEventType.Information, master.UserName);
                }
                catch (Exception exc)
                {
                    CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                    CRM.detail.createorderinfo.returninfo.failmessage = "CreateOrder, Get RnR Credentials Configuration settings Error - " + exc.Message;
                    Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, Get RnR Credentials Configuration settings Error.", exc);
                    throw exc;
                }

                XmlDocument adoc = new XmlDocument();
                adoc.LoadXml(CROMethods.obj2String(master));
                adoc.Save("C:\\CRMSvc\\xml\\PreWrapper_SN.xml");

                ReceiptWrapperSoapClient svc = new ReceiptWrapperSoapClient();

                string y = svc.UpdateRO(master, false);
                //string y = svc.UpdateRO(master, true);

                //string filename = "C:\\CRMSvc\\xml\\ReceiptWrapper.xml";

                //using (FileStream fs = File.Create(filename))
                //{
                //    Byte[] info = new UTF8Encoding(true).GetBytes(y);
                //    fs.Write(info, 0, info.Length);
                //}

                if (y == "SUCCESS")
                {
                    CRM.detail.createorderinfo.returninfo.result = "Success";
                    CRM.detail.createorderinfo.returninfo.rnrreferenceorderid = y;
                }
                else
                {
                    CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                    CRM.detail.createorderinfo.returninfo.failmessage = "Wrapper Connect " + y;
                }
            }
            catch (Exception exc)
            {                
                CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                CRM.detail.createorderinfo.returninfo.failmessage = "Wrapper Update " + exc.Message;
            }

            return CRM;
        }

        private static ContactAddressInfo GetDellShippingAddress(ContactAddressInfo CAI, CRM1ClickService CRM, List<FlexField> CRMIn, string status)
        {
            ContactInfo CI = new ContactInfo();
            AddressInfo AI = new AddressInfo();

            CI.abbrv = null;
            CI.contactDesc = null;
            CI.fax = null;
            CI.pager = null;
            CI.title = null;

            CI.email = CRM.detail.contactinfo.contactemail;
            CI.mobilePhone = CRM.detail.contactinfo.contactphone.phoneno;
            CI.name = CRM.detail.contactinfo.contactname;
            CI.primaryPhone = CRM.detail.contactinfo.contactphone.phoneno;

            AI.abbrv = null;
            AI.line4 = null;
            AI.addrName = null;
            AI.state = "No State was found.";

            if (status.ToUpper() == "NEW")
            {
                foreach (FlexField ff in CRMIn)
                {
                    if (ff.name == "collection_address_line_1")
                    {
                        AI.line1 = ff.value;
                    }
                    if (ff.name == "collection_address_line_2")
                    {
                        AI.line2 = ff.value;
                    }
                    if (ff.name == "collection_address_line_3")
                    {
                        AI.line3 = ff.value;
                    }
                    if (ff.name == "collection_city")
                    {
                        AI.city = ff.value;
                    }
                    if (ff.name == "collection_country")
                    {
                        AI.country = ff.value;
                    }
                    if (ff.name == "collection_postal_code")
                    {
                        AI.postalCode = ff.value;
                    }
                }
            }
            else
            {
                foreach (FlexField ff in CRMIn)
                {
                    if (ff.name == "delivery_address_line_1")
                    {
                        AI.line1 = ff.value;
                    }
                    if (ff.name == "delivery_address_line_2")
                    {
                        AI.line2 = ff.value;
                    }
                    if (ff.name == "delivery_address_line_3")
                    {
                        AI.line3 = ff.value;
                    }
                    if (ff.name == "delivery_city")
                    {
                        AI.city = ff.value;
                    }
                    if (ff.name == "delivery_country")
                    {
                        AI.country = ff.value;
                    }
                    if (ff.name == "delivery_postal_code")
                    {
                        AI.postalCode = ff.value;
                    }
                }
            }

            CAI.ContactInfo = CI;
            CAI.AddressInfo = AI;

            return CAI;
        }

        public static Holidays CRMGetDBHolidays(Holidays crm)
        {
            string res = string.Empty;

            List<OracleParameter> oraParams = new List<OracleParameter>
            {               
                new OracleParameter("cCntyCode", crm.countrycode),
                new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output)
            };

            try
            {
                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.GETHOLIDAYS", oraParams.ToArray());
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        HDays hd = new HDays();

                        hd.Holiday = row["BANK_HOLIDAY_DATE"].ToString();

                        crm.HDaysFlexFields.Add(hd);
                    }
                }
            }
            catch (Exception ex)
            {
                crm.resultMessage = "ERR: " + ex.Message.ToString();
            }

            return crm;
        }


        public static CRM1ClickService string2CRM1ClickObj(string xmlIn)
        {
            XmlSerializer ser = new XmlSerializer(typeof(CRM1ClickService));
            StringReader sr = new StringReader(xmlIn);
            CRM1ClickService outObj = (CRM1ClickService)ser.Deserialize(sr);
            return outObj;
        }


        public static string TestOrder(CRM1ClickService CRM)
        {
            string warehouse = "None";
            //doc.Save("C:\\CRMSvc\\FinalXML.xml");
            //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, TestOrder", System.Diagnostics.TraceEventType.Information, obj2String(CRM));

            try
            {
                //int binId = int.Parse(CRM.detail.createorderinfo.orderlinedetail.bin);
                //warehouse = GetWarehouseByBinId(binId);                
                //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, TestOrder, Warehopuse", System.Diagnostics.TraceEventType.Information, warehouse); --AIC

                //ValidateLoaner2(CRM);                
                //string partProdLine = GetProductLineByPartNo(CRM.detail.createorderinfo.orderlinedetail.part_no); --AIC
                //string partBusinessLine = ConvertProdLineToBusiness(partProdLine); --AIC
                //warehouse = GetItemCondition(CRM.detail.serialno);
                //Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, TestOrder, Condition", System.Diagnostics.TraceEventType.Information, warehouse);
                warehouse = "LoggerWork";
                //warehouse = GetName("927", false);
            }
            catch (Exception exc)
            {
                Logger.AddEntry(Logger.EventSource.Process, "CreateOrder, GetWarehouseByBinId Error.", exc);
                throw exc;
                //warehouse = "Error with logger " + exc.Message;
            }

             return warehouse;

        }

        /// <summary>
        /// Searches Warehouse Name by BinId
        /// </summary>
        /// <param name="binId"></param>
        /// <returns></returns>
        public static string GetWarehouseByBinId(int binId)
        {
            Logger.AddEntry(Logger.EventSource.Process, "GetWarehouseByBinId, ConnStr", System.Diagnostics.TraceEventType.Information, ConnString);

            string res = string.Empty;
            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("Bin_id", binId),
                new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output)
            };
            DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.GetWarehouseName", oraParams.ToArray());
            if (ds.Tables[0].Rows.Count > 0)
            {
                res = ds.Tables[0].Rows[0]["warehouse_name"].ToString();
            }
            return res;
        }

        /// <summary>
        /// Get ProductLine by PartNo 
        /// </summary>
        /// <param name="partNo"></param>
        /// <returns>Product Line</returns>
        public static string GetProductLineByPartNo(string partNo)
        {
            //Logger.AddEntry(Logger.EventSource.Process, "GETPRODUCTLINE, ConnStr", System.Diagnostics.TraceEventType.Information, ConnString);

            // EXEC CUELLAAL.GETPRODUCTLINE('LL046EA', :rc);
            string prodLine = string.Empty;
            string partNumber = string.Empty;

            try
            {
                List<OracleParameter> oraParams = new List<OracleParameter>
                {
                    new OracleParameter("i_partNum", partNo),
                    new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output)
                };

                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.GETPRODUCTLINE", oraParams.ToArray());
                if (ds.Tables[0].Rows.Count > 0)
                {
                    prodLine = ds.Tables[0].Rows[0]["ProductLine"].ToString();
                    partNumber = ds.Tables[0].Rows[0]["PART_NO"].ToString();
                }
            }
            catch (Exception exc)
            {
                Logger.AddEntry(Logger.EventSource.Process, "GETPRODUCTLINE", exc);
                throw exc;
            }
            return prodLine;
        }

        private static string ConvertProdLineToBusiness(string productLine)
        {
            string[] consumerPL = { "6J", "6V", "KV", "7S" };
            string[] commercialSMB = { "5U", "6U" };
            string[] commercialCEP = { "7F", "7X", "AN", "TA", "2C", "UU"};
            string bussLine = string.Empty;

            if (consumerPL.Contains(productLine.ToUpper()))
            {
                bussLine = "Consumer";
            }
            else if (commercialSMB.Contains(productLine.ToUpper()))
            {
                bussLine = "Commercial-SMB";
            }
            else if (commercialCEP.Contains(productLine.ToUpper()))
            {
                bussLine = "Commercial-CEP";
            }
            else
            {
                bussLine = productLine;
            }
           
            return bussLine;
        }


        /// <summary>
        /// Searches Warehouse Name by Disposition type
        /// </summary>
        /// <param name="binId"></param>
        /// <returns></returns>
        public static string GetWarehouseName(string dispositionType, string clientName, string contractName, int binId)
        {
            //Logger.AddEntry(Logger.EventSource.Process, "GetWarehouseName, ConnStr", System.Diagnostics.TraceEventType.Information, ConnString);

            string res = string.Empty;
            List<OracleParameter> oraParams = new List<OracleParameter>
            {               
                new OracleParameter("p_dispositionType", dispositionType),
                new OracleParameter("p_clientName", clientName),
                new OracleParameter("p_contractName", contractName),
                new OracleParameter("p_binId", binId),
                new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output)
            };

            try
            {
                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.GETWAREHOUSENAME", oraParams.ToArray());
                if (ds.Tables[0].Rows.Count > 0)
                {
                    res = ds.Tables[0].Rows[0]["warehouse_name"].ToString();
                    Logger.AddEntry(Logger.EventSource.Process, "GetWarehouseName, warehouse_name: " + res, System.Diagnostics.TraceEventType.Information, res);
                }
                else
                {
                    Exception exc = new Exception("No Warehouse Found!");
                    Logger.AddEntry(Logger.EventSource.Process, "GETWAREHOUSENAME", exc);
                    throw exc;
                }
            }
            catch (Exception exc)
            {
                string spParams = string.Format("Input Parameters: p_dispositionType={0}, p_clientName={1}, p_contractName={2}, p_binId={3} ", dispositionType, clientName, contractName, binId);
                Logger.AddEntry(Logger.EventSource.Process, "GETWAREHOUSENAME", spParams, exc);
            }
            return res;
        }


        public static CRM1ClickService GetClientInfo(CRM1ClickService CRMXml, string xmlInput)
        {
            List<OracleParameter> crParams = new List<OracleParameter>();

            crParams.Add(new OracleParameter("SITE", CRMXml.detail.crmsitecode));
            crParams.Add(new OracleParameter("CRMINTERFACE", CRMXml.detail.crminterfacename));

            if (xmlInput.Contains("DispositionType") && xmlInput.Contains("TCC_Location") && xmlInput.Contains("OrderType") && xmlInput.Contains("Geographic"))
            {
                if (xmlInput.Contains("DispositionType"))
                {
                    crParams.Add(new OracleParameter("DISPOSITIONTYPE", CRMXml.detail.dispositiontype));
                }

                if (xmlInput.Contains("TCC_Location"))
                {
                    crParams.Add(new OracleParameter("TCCLOCATION", CRMXml.detail.createorderinfo.orderheader.tcc_location));
                }

                if (xmlInput.Contains("OrderType"))
                {
                    crParams.Add(new OracleParameter("ORDERTYPE", CRMXml.detail.ordertype));
                }

                if (xmlInput.Contains("Geographic"))
                {
                    crParams.Add(new OracleParameter("LocationName", CRMXml.detail.geographic));// .homelocation));
                }
            }
            else
            {
                if (xmlInput.Contains("DispositionType") && xmlInput.Contains("TCC_Location") && xmlInput.Contains("Geographic"))
                {
                    if (xmlInput.Contains("DispositionType"))
                    {
                        crParams.Add(new OracleParameter("DISPOSITIONTYPE", CRMXml.detail.dispositiontype));
                    }

                    if (xmlInput.Contains("Geographic"))
                    {
                        crParams.Add(new OracleParameter("LocationName", CRMXml.detail.geographic));// .homelocation));
                    }

                    if (xmlInput.Contains("TCC_Location"))
                    {
                        crParams.Add(new OracleParameter("TCCLOCATION", CRMXml.detail.createorderinfo.orderheader.tcc_location));
                    }

                }
                else
                {
                    if (xmlInput.Contains("Geographic"))
                    {
                        crParams.Add(new OracleParameter("LocationName", CRMXml.detail.geographic));// .homelocation));
                    }

                    if (xmlInput.Contains("TCC_Location"))
                    {
                        crParams.Add(new OracleParameter("TCCLOCATION", CRMXml.detail.createorderinfo.orderheader.tcc_location));
                    }
                }
            }

            crParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

            try
            {
                // Save output to log file
                Logger.AddEntry(Logger.EventSource.Process, "GetClientInfo, Connection String", System.Diagnostics.TraceEventType.Information, ConnString);     

                //CRMXml.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | GetClientInfo";
                //DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WebApp1.CRMRTN.GetCRMClientInfo", crParams.ToArray());
                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.GetCRMClientInfo", crParams.ToArray());

                CRMXml.header.location = ds.Tables[0].Rows[0]["LOCATION_NAME"].ToString();
                CRMXml.header.client = ds.Tables[0].Rows[0]["CLIENT_NAME"].ToString();
                /*
                if (ds.Tables[0].Columns.Contains("CONTRACT_NAME"))
                {
                    CRMXml.header.contract = ds.Tables[0].Rows[0]["CONTRACT_NAME"].ToString();
                }
                else
                {
                    CRMXml = LogIncidentCRM(CRMXml);
                    if (CRMXml.detail.createorderinfo.returninfo.result != "Success")
                    {
                        return CRMXml;
                    }
                }
                */
                if (ds.Tables[0].Columns.Contains("CONTRACT_NAME"))
                {
                    CRMXml.header.contract = ds.Tables[0].Rows[0]["CONTRACT_NAME"].ToString();
                }
                // All the request should be stored on the CRM_CLIENT_INCIDENT table, if the insert fail we should stop the execution
                CRMXml = LogIncidentCRM(CRMXml);
                if (CRMXml.detail.createorderinfo.returninfo.result != "Success")
                {
                    return CRMXml;
                }

                if (ds.Tables[0].Columns.Contains("BTT"))
                {
                    CRMXml.detail.createorderinfo.orderheader.btt = ds.Tables[0].Rows[0]["BTT"].ToString();
                }

                if (ds.Tables[0].Columns.Contains("PENDING_BIN_ID"))
                {
                    CRMXml.detail.createorderinfo.orderlinedetail.bin = ds.Tables[0].Rows[0]["PENDING_BIN_ID"].ToString();
                }

                CRMXml.detail.createorderinfo.returninfo.result = "Success";
                return CRMXml;
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Process, "GetClientInfo", ex);

                System.Diagnostics.Trace.WriteLine(ex.Message);
                CRMXml.detail.result = "Failure";
                CRMXml.detail.resultmessage = ex.Message + "| GetCRMClientInfo procedure failed";
                CRMXml.detail.createorderinfo.returninfo.result = "Failure";
                CRMXml.detail.createorderinfo.returninfo.failmessage = ex.Message;
                return CRMXml;
            }

        }

        public static CRM1ClickService PartValidate(CRM1ClickService CRM)
        {

            List<OracleParameter> ValidateParams = new List<OracleParameter>();
            ValidateParams.Add(new OracleParameter("partNumberCollection", CRM.detail.createorderinfo.orderlinedetail.part_no));  //463953-001X
            ValidateParams.Add(new OracleParameter("userName", CRM.detail.crmsitecode));
            ValidateParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

            //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | PartValidate - ValidatePartNumbers";                
            DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBADAPTERS1.QUERYADAPTER.ValidatePartNumbers", ValidateParams.ToArray());
            if (ds.Tables[0].Rows.Count > 0)
            {
                CRM.detail.partavailabilityinfo.returninfo.result = "Success";
            }
            else
            {
                //CRM.detail.createorderinfo.returninfo.result = "Success";
                CRM.detail.createorderinfo.returninfo.result = "Failure";
                CRM.detail.createorderinfo.returninfo.failmessage = "The Part Number does not exist - ValidatePartNumbers";
            }
            return CRM;
        }

        public static CRM1ClickService PartValidate2(CRM1ClickService CRM)
        {
            if (CRM.detail.createorderinfo.orderlinedetail.part_no != null)
            {
                List<OracleParameter> ValidateParams = new List<OracleParameter>();
                ValidateParams.Add(new OracleParameter("i_partNum", CRM.detail.createorderinfo.orderlinedetail.part_no));
                ValidateParams.Add(new OracleParameter("i_location", CRM.header.location));
                ValidateParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

                //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | PartValidate - partInventoryCheck";
                // new PS to validate inventory on the part is: CUELLAAL.partInventoryCheck(i_partNum,  i_location,  o_cursor)    
                try
                {
                    DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.partInventoryCheck", ValidateParams.ToArray());
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string quntyOnHand = ds.Tables[0].Rows[0]["quantityOnHand"].ToString();
                        string conditionName = ds.Tables[0].Rows[0]["conditionName"].ToString();
                        CRM.detail.partavailabilityinfo.returninfo.result = "Success";
                        CRM.detail.createorderinfo.returninfo.result = "Success";
                    }
                    else
                    {
                        CRM.detail.createorderinfo.returninfo.result = "Failure";
                        CRM.detail.createorderinfo.returninfo.failmessage = "The Part Number does not exist - partInventoryCheck";
                    }
                }
                catch (Exception exc)
                {
                    CRM.detail.createorderinfo.returninfo.result = "Failure";
                    CRM.detail.createorderinfo.returninfo.failmessage = exc.Message + " - partInventoryCheck";
                }
            }
            else
            {
                string skuPartNum = GetFlexFielsValue(CRM.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number");
                CRM.detail.createorderinfo.orderlinedetail.part_no = skuPartNum;
            }
            return CRM;
        }

        //public static CRM1ClickService PartAvailavableLookupByPartNumWur(CRM1ClickService CRM)
        //{
        //    List<OracleParameter> ValidateParams = new List<OracleParameter>();
        //    ValidateParams.Add(new OracleParameter("i_partNum", CRM.detail.createorderinfo.orderlinedetail.part_no));
        //    ValidateParams.Add(new OracleParameter("i_partDesc", CRM.detail.partavailabilityinfo.dispositioncode.ToUpper()));
        //    ValidateParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

        //    CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | PartValidate";                        
        //    DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "CUELLAAL.bomBySkuLookupWur", ValidateParams.ToArray());
        //    if (ds.Tables[0].Rows.Count > 0)
        //    {
        //        string partNo = ds.Tables[0].Rows[0]["PART_NO"].ToString();
        //        string partDesc = ds.Tables[0].Rows[0]["PART_DESC"].ToString();
        //        string partClass = ds.Tables[0].Rows[0]["CLASS_NAME"].ToString();

        //        CRM.detail.createorderinfo.orderlinedetail.part_no = partNo;
        //        CRM.detail.createorderinfo.returninfo.result = "Success";
        //    }
        //    else
        //    {
        //        CRM.detail.createorderinfo.returninfo.result = "Failure";
        //        CRM.detail.createorderinfo.returninfo.failmessage = "Part Availavable Lookup By Part Num Wur";
        //    }
        //    return CRM;
        //}


        public static CRM1ClickService ProcessOrder(CRM1ClickService CRM)
        {
            if (CRM.detail.shipto != null)
            {
                if (CRM.detail.shipto.ToUpper() == "Customer".ToUpper())
                    CRM.detail.createorderinfo.orderheader.customerinfo.shippingaddress = CRM.detail.contactinfo;


                if (CRM.detail.dispositiontype == "Customer Replacable Unit")
                {
                    // Soft Allocate the part
                    SoftAllocationByPartNum2Pass(CRM);
                    //PartAvailavableLookupByPartNumSecondPass(CRM);

                    // Here we should come after Part has been soft allocated
                    if (CRM.detail.createorderinfo.returninfo.result != "Failure")
                    {
                        CRM = CreateOrder(CRM);
                    }
                }
                else
                {
                    // WUR                
                    // Part availability check
                    //CRM = PartAvailavableLookupByPartNumWur(CRM);

                    // IF loaner is NULL the default value "NO" will be used
                    if (CRM.detail.wholeunitreturn.loaner == null)
                    {
                        CRM.detail.wholeunitreturn.loaner = "NO";
                    }

                    if (CRM.detail.wholeunitreturn.loaner.ToUpper() == "NO")
                    {
                        // Need to chech the part on inventory 
                        CRM = PartValidate2(CRM);

                        // Soft allocate by Core PartNum
                        //SoftAllocationByPartNumWur(CRM);


                        if (CRM.detail.createorderinfo.returninfo.result != "Failure")
                        {
                            CRM = CreateOrder(CRM);
                        }
                        else
                        {
                            return CRM;
                        }
                    }
                    else
                    {
                        // Validate loaner before placing the order
                        //CRM = ValidateLoaner(CRM);
                        CRM = ValidateLoaner2(CRM);
                        if (CRM.detail.createorderinfo.returninfo.result != "Success")
                        {
                            return CRM;
                        }
                        else
                        {
                            CRM = GetClientInfo(CRM, obj2String(CRM));
                            CRM = CreateOrder(CRM);
                        }
                    }
                }
            }
            else
            {
                CRM.detail.createorderinfo.returninfo.result = "Failure";
                CRM.detail.createorderinfo.returninfo.failmessage = "ShipTo cannot be null";
            }
            return CRM;
        }


        /// <summary>
        /// Looks up for available parts. 
        /// </summary>
        /// <param name="crm">CRM1ClickService object</param>
        /// <returns>PartsList</returns>
        public static PartsList PartAvailavableLookupByPartNum(CRM1ClickService crm)
        {
            // Here we will do Parts availability check with 'SoftAllocation' & 'GNOME'           
            ContactAddressInfo cai = new ContactAddressInfo();
            string AddressFound = "TRUE";
            if (crm.detail.shipto != null)
            {                
                cai = GetShippingAddress(new ContactAddressInfo(), crm);                
                if (cai.AddressInfo.line4 != null)
                {
                    if (cai.AddressInfo.line4.ToUpper().StartsWith("ERROR:"))
                    {
                        AddressFound = "FALSE";
                    }
                }
            }
            else
            {
                AddressFound = "FALSE";
                cai.AddressInfo.line4 = "[PartAvailavableLookupByPartNum] - ShipTo can't be null";
            }

            PartsList parts = null;

            if (AddressFound == "TRUE")
            {
                string skuPartNum = GetFlexFielsValue(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number");
                //string skuPartNum = crm.detail.createorderinfo.orderlinedetail.part_no;

                if (!string.IsNullOrEmpty(skuPartNum))
                {
                    List<OracleParameter> icParams = new List<OracleParameter>();
                    icParams.Add(new OracleParameter("i_partNum", skuPartNum));
                    icParams.Add(new OracleParameter("i_partDesc", crm.detail.partavailabilityinfo.dispositioncode));  //"battery" <--  detail.partavailabilityinfo.dispositioncode    
                    icParams.Add(new OracleParameter("i_request_no", crm.detail.crmincidentno));
                    icParams.Add(new OracleParameter("i_btt", string.Empty));
                    icParams.Add(new OracleParameter("i_site", crm.detail.crmsitecode));
                    icParams.Add(new OracleParameter("i_crm_interface", crm.detail.crminterfacename));
                    icParams.Add(new OracleParameter("i_gio_name", crm.detail.geographic));
                    icParams.Add(new OracleParameter("i_quantity", 1));
                    icParams.Add(new OracleParameter("i_crm_incident_no", crm.detail.crmincidentno));
                    icParams.Add(new OracleParameter("i_incident_create_date", crm.detail.incidentcreatedate));
                    icParams.Add(new OracleParameter("i_city", cai.AddressInfo.city));  // "ANKARA"));
                    icParams.Add(new OracleParameter("i_state_province", cai.AddressInfo.state));  // "ANKARA"));
                    icParams.Add(new OracleParameter("i_postal_code", cai.AddressInfo.postalCode));  // "06770"));
                    icParams.Add(new OracleParameter("i_country_code", cai.AddressInfo.country));  // "TR"));
                    icParams.Add(new OracleParameter("i_priority_name", "High"));
                    icParams.Add(new OracleParameter("i_svc_date", DateTime.Now.ToString("MM/dd/yy"))); //"01/01/12"));
                    icParams.Add(new OracleParameter("i_svc_window", "AM1"));
                    icParams.Add(new OracleParameter("i_part_subs", "Whole Line Only"));
                    icParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));


                    try
                    {
                        //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | GetClientInfo";
                        DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.bomBySkuLookup", icParams.ToArray());
                        parts = new PartsList();

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                Part part = new Part()
                                {
                                    ErrorStr = (ds.Tables[0].Rows[i]["O_ERROR_STR"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["O_ERROR_STR"].ToString(),
                                    OriginalPartDesc = (ds.Tables[0].Rows[i]["originalPartDesc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["originalPartDesc"].ToString(),
                                    ProductClass = (ds.Tables[0].Rows[i]["procuctClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctClassName"].ToString(),
                                    ProductSubclass = (ds.Tables[0].Rows[i]["softAllocParams"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["softAllocParams"].ToString(),
                                    RequestLineNo = (ds.Tables[0].Rows[i]["REQUEST_LINE_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["REQUEST_LINE_NO"].ToString(),
                                    PartNo = (ds.Tables[0].Rows[i]["PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["PART_NO"].ToString(),
                                    OwnerName = (ds.Tables[0].Rows[i]["OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["OWNER_NAME"].ToString(),
                                    ConditionName = (ds.Tables[0].Rows[i]["CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["CONDITION_NAME"].ToString(),
                                    Quantity = (ds.Tables[0].Rows[i]["QUANTITY"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["QUANTITY"].ToString(),
                                    WarehouseName = (ds.Tables[0].Rows[i]["WAREHOUSE_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["WAREHOUSE_NAME"].ToString(),
                                    ResrvPartNo = (ds.Tables[0].Rows[i]["RESRV_PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_PART_NO"].ToString(),
                                    ResrvOwnerName = (ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"].ToString(),
                                    ResrvConditionName = (ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"].ToString(),
                                    ResrvPartDesc = (ds.Tables[0].Rows[i]["resrv_part_desc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["resrv_part_desc"].ToString(),
                                    EtaDate = (ds.Tables[0].Rows[i]["ETA_DATE"] == DBNull.Value) ? DateTime.MinValue : DateTime.ParseExact(ds.Tables[0].Rows[i]["ETA_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                                };

                                // parse ETA Date
                                if (Math.Abs(part.EtaDate.Year - DateTime.Now.Year) > 3)
                                {
                                    part.ErrorStr = "No Requested Part found!";
                                }

                                parts.Add(part);
                            }
                        }
                        else
                        {   // nothing found
                            Part part = new Part()
                            {
                                ErrorStr = "No part found"
                            };

                            parts.Add(part);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNum", ex);
                        crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                        crm.detail.partavailabilityinfo.returninfo.failmessage = ex.Message;

                        //throw (ex);              
                    }
                }
            }
            else
            {
                crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                crm.detail.partavailabilityinfo.returninfo.failmessage = cai.AddressInfo.line4;
            }

            return parts;
        }

        public static PartsList PartAvailavableLookupByPartNumSecondPassOld(CRM1ClickService crm)
        {
            // Here we will do Parts availability check with 'SoftAllocation' & 'GNOME'           
            ContactAddressInfo cai = new ContactAddressInfo();
            cai = GetShippingAddress(new ContactAddressInfo(), crm);

            PartsList parts = null;

            //string skuPartNum = GetFlexFielsValue(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number");
            string skuPartNum = string.Empty;
            try
            {
                skuPartNum = crm.detail.createorderinfo.orderlinedetail.part_no;
            }
            catch (Exception exc)
            {
                Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNumSecondPass, skuPartNum error. ", exc);
                crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                crm.detail.partavailabilityinfo.returninfo.failmessage = exc.Message;
                throw exc;
            }

            if (!string.IsNullOrEmpty(skuPartNum))
            {
                List<OracleParameter> icParams = new List<OracleParameter>();
                icParams.Add(new OracleParameter("i_partNum", skuPartNum));  // "436842-141"));            
                icParams.Add(new OracleParameter("i_partDesc", crm.detail.partavailabilityinfo.dispositioncode));  //"battery" <--  detail.partavailabilityinfo.dispositioncode    
                icParams.Add(new OracleParameter("i_request_no", crm.detail.crmincidentno));
                icParams.Add(new OracleParameter("i_btt", string.Empty));
                icParams.Add(new OracleParameter("i_site", crm.detail.crmsitecode));
                icParams.Add(new OracleParameter("i_crm_interface", crm.detail.crminterfacename));
                icParams.Add(new OracleParameter("i_quantity", 1));
                icParams.Add(new OracleParameter("i_crm_incident_no", crm.detail.crmincidentno));
                icParams.Add(new OracleParameter("i_incident_create_date", crm.detail.incidentcreatedate));
                //icParams.Add(new OracleParameter("i_city", crm.detail.contactinfo.contactcity));  // "ANKARA"));
                //icParams.Add(new OracleParameter("i_state_province", crm.detail.contactinfo.contactstate));  // "ANKARA"));
                //icParams.Add(new OracleParameter("i_postal_code", crm.detail.contactinfo.contactpostalcode));  // "06770"));
                //icParams.Add(new OracleParameter("i_country_code", crm.detail.contactinfo.contactcountry));  // "TR"));
                icParams.Add(new OracleParameter("i_city", cai.AddressInfo.city));  // "ANKARA"));
                icParams.Add(new OracleParameter("i_state_province", cai.AddressInfo.state));  // "ANKARA"));
                icParams.Add(new OracleParameter("i_postal_code", cai.AddressInfo.postalCode));  // "06770"));
                icParams.Add(new OracleParameter("i_country_code", cai.AddressInfo.country));  // "TR"));

                icParams.Add(new OracleParameter("i_priority_name", "High"));
                icParams.Add(new OracleParameter("i_svc_date", DateTime.Now.ToString("MM/dd/yy"))); //"01/01/12"));
                icParams.Add(new OracleParameter("i_svc_window", "AM1"));
                icParams.Add(new OracleParameter("i_part_subs", "Whole Line Only"));
                icParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

                //// Test only
                //////execute CUELLAAL.PARTAVAILABILITYLOOKUP('0000001', 'btt','HP','HP-JABILSUPPORT','436842-141',1,'','','ANKARA','ANKARA','06770','TR','High','01/01/12','AM1','Whole Line Only', :rc );
                ////execute CUELLAAL.bomBySkuLookup('436842-141', '','0000001', 'btt','HP','HP-JABILSUPPORT',1,'','','ANKARA','ANKARA','06770','TR','High','01/01/12','AM1','Whole Line Only',  :rc );
                //icParams.Add(new OracleParameter("i_request_no", "0000002"));
                //icParams.Add(new OracleParameter("i_btt", string.Empty));
                //icParams.Add(new OracleParameter("i_site", "HP"));
                //icParams.Add(new OracleParameter("i_crm_interface", "HP-JABILSUPPORT"));
                //icParams.Add(new OracleParameter("i_sku_part_num", "436842-141"));  // "436842-141"));            
                //icParams.Add(new OracleParameter("i_quantity", 1));
                //icParams.Add(new OracleParameter("i_crm_incident_no", ""));
                //icParams.Add(new OracleParameter("i_incident_create_date", ""));
                //icParams.Add(new OracleParameter("i_city", "ANKARA"));
                //icParams.Add(new OracleParameter("i_state_province", "ANKARA"));
                //icParams.Add(new OracleParameter("i_postal_code", "06770"));
                //icParams.Add(new OracleParameter("i_country_code", "oTR"));
                //icParams.Add(new OracleParameter("i_priority_name", "High"));
                //icParams.Add(new OracleParameter("i_svc_date", "01/01/12"));
                //icParams.Add(new OracleParameter("i_svc_window", "AM1"));
                //icParams.Add(new OracleParameter("i_part_subs", "Whole Line Only"));
                //icParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

                try
                {
                    //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | GetClientInfo";
                    DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.bomBySkuLookup", icParams.ToArray());
                    parts = new PartsList();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Part part = new Part()
                            {
                                ErrorStr = (ds.Tables[0].Rows[i]["O_ERROR_STR"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["O_ERROR_STR"].ToString(),
                                OriginalPartDesc = (ds.Tables[0].Rows[i]["originalPartDesc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["originalPartDesc"].ToString(),
                                ProductClass = (ds.Tables[0].Rows[i]["procuctClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctClassName"].ToString(),
                                ProductSubclass = (ds.Tables[0].Rows[i]["procuctSubClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctSubClassName"].ToString(),
                                RequestLineNo = (ds.Tables[0].Rows[i]["REQUEST_LINE_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["REQUEST_LINE_NO"].ToString(),
                                PartNo = (ds.Tables[0].Rows[i]["PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["PART_NO"].ToString(),
                                OwnerName = (ds.Tables[0].Rows[i]["OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["OWNER_NAME"].ToString(),
                                ConditionName = (ds.Tables[0].Rows[i]["CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["CONDITION_NAME"].ToString(),
                                Quantity = (ds.Tables[0].Rows[i]["QUANTITY"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["QUANTITY"].ToString(),
                                WarehouseName = (ds.Tables[0].Rows[i]["WAREHOUSE_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["WAREHOUSE_NAME"].ToString(),
                                ResrvPartNo = (ds.Tables[0].Rows[i]["RESRV_PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_PART_NO"].ToString(),
                                ResrvOwnerName = (ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"].ToString(),
                                ResrvConditionName = (ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"].ToString(),
                                ResrvPartDesc = (ds.Tables[0].Rows[i]["resrv_part_desc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["resrv_part_desc"].ToString(),
                                EtaDate = (ds.Tables[0].Rows[i]["ETA_DATE"] == DBNull.Value) ? DateTime.MinValue : DateTime.ParseExact(ds.Tables[0].Rows[i]["ETA_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                            };

                            // parse ETA Date
                            if (Math.Abs(part.EtaDate.Year - DateTime.Now.Year) > 3)
                            {
                                part.ErrorStr = "No Requested Part found!";
                            }

                            parts.Add(part);
                        }
                    }
                    else
                    {   // nothing found
                        Part part = new Part()
                        {
                            ErrorStr = "No part found"
                        };

                        parts.Add(part);
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNum", ex);
                    crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                    crm.detail.partavailabilityinfo.returninfo.failmessage = ex.Message;  

                    //throw (ex);              
                }
            }
            return parts;
        }

        public static PartsList PartAvailavableLookupByPartNumSecondPass(CRM1ClickService crm)
        {
            // Here we will do Parts availability check with 'SoftAllocation' & 'GNOME'                      
            PartsList parts = null;           
            string[] softAllocParamsArray;             
            string softAllocParametersStr = string.Empty;

            string request_no, i_btt, i_site, i_crm_interface, part_no, geo_name, quantity, request_line_no, i_incident_create_date, i_city, i_state_province, i_postal_code;
            string i_country_code, i_priority_name, i_svc_date, i_svc_window, i_part_subs, owner_name, condition_name, o_error_str, resrv_part_no, eta_date, resrv_condition_name, resrv_owner_name, resrv_part_desc;

            if (string.IsNullOrEmpty(crm.detail.createorderinfo.orderlinedetail.preferredshipwh))
            {
                throw new Exception("PartAvailavableLookupByPartNumSecondPass. Soft Allocation Info is not defined!");
            }

            try
            {
                softAllocParametersStr = crm.detail.createorderinfo.orderlinedetail.preferredshipwh;
                softAllocParamsArray = softAllocParametersStr.Split('|');

                request_no = softAllocParamsArray[0];
                i_btt = softAllocParamsArray[1];
                i_site = softAllocParamsArray[2];
                i_crm_interface = softAllocParamsArray[3];
                part_no = softAllocParamsArray[4];
                geo_name = softAllocParamsArray[5];
                quantity = softAllocParamsArray[6];
                request_line_no = softAllocParamsArray[7];
                i_incident_create_date = softAllocParamsArray[8];
                i_city = softAllocParamsArray[9];
                i_state_province = softAllocParamsArray[10];
                i_postal_code = softAllocParamsArray[11];
                i_country_code = softAllocParamsArray[12];
                i_priority_name = softAllocParamsArray[13];
                i_svc_date = softAllocParamsArray[14];
                i_svc_window = softAllocParamsArray[15];
                i_part_subs = softAllocParamsArray[16];
                owner_name = softAllocParamsArray[17];
                condition_name = softAllocParamsArray[18];
                o_error_str = softAllocParamsArray[19];
                resrv_part_no = softAllocParamsArray[20];
                eta_date = softAllocParamsArray[21];
                resrv_condition_name = softAllocParamsArray[21];
                resrv_owner_name = softAllocParamsArray[23];
                resrv_part_desc = softAllocParamsArray[24];
            }
            catch(Exception exc)
            {
                Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNumSecondPass, error. ", exc);
                crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                crm.detail.partavailabilityinfo.returninfo.failmessage = exc.Message;
                throw exc;
            }


            List<OracleParameter> icParams = new List<OracleParameter>();
            icParams.Add(new OracleParameter("i_request_no", request_no));
            icParams.Add(new OracleParameter("i_btt", i_btt));
            icParams.Add(new OracleParameter("i_site", i_site));
            icParams.Add(new OracleParameter("i_crm_interface", i_crm_interface));
            icParams.Add(new OracleParameter("i_partNum", part_no));
            icParams.Add(new OracleParameter("i_gio_name", geo_name));     
            icParams.Add(new OracleParameter("i_quantity", quantity));
            icParams.Add(new OracleParameter("i_request_line_no", request_line_no));
            icParams.Add(new OracleParameter("i_incident_create_date", i_incident_create_date));
            icParams.Add(new OracleParameter("i_city", i_city));
            icParams.Add(new OracleParameter("i_state_province", i_state_province));
            icParams.Add(new OracleParameter("i_postal_code", i_postal_code));
            icParams.Add(new OracleParameter("i_country_code", i_country_code));
            icParams.Add(new OracleParameter("i_priority_name", i_priority_name));
            icParams.Add(new OracleParameter("i_svc_date", i_svc_date));
            icParams.Add(new OracleParameter("i_svc_window", i_svc_window));
            icParams.Add(new OracleParameter("i_part_subs", i_part_subs));
            icParams.Add(new OracleParameter("i_owner_name", owner_name));
            icParams.Add(new OracleParameter("i_condition_name", condition_name));            
            icParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));


                try
                {
                    //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | GetClientInfo";
                    DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.bomBySkuLookup2Pass", icParams.ToArray());
                    parts = new PartsList();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Part part = new Part()
                            {
                                ErrorStr = (ds.Tables[0].Rows[i]["O_ERROR_STR"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["O_ERROR_STR"].ToString(),
                                //OriginalPartDesc = (ds.Tables[0].Rows[i]["originalPartDesc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["originalPartDesc"].ToString(),
                                //ProductClass = (ds.Tables[0].Rows[i]["procuctClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctClassName"].ToString(),
                                //ProductSubclass = (ds.Tables[0].Rows[i]["procuctSubClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctSubClassName"].ToString(),
                                RequestLineNo = (ds.Tables[0].Rows[i]["request_line_no"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["request_line_no"].ToString(),
                                PartNo = (ds.Tables[0].Rows[i]["PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["PART_NO"].ToString(),
                                OwnerName = (ds.Tables[0].Rows[i]["OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["OWNER_NAME"].ToString(),
                                ConditionName = (ds.Tables[0].Rows[i]["CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["CONDITION_NAME"].ToString(),
                                Quantity = (ds.Tables[0].Rows[i]["QUANTITY"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["QUANTITY"].ToString(),
                                //WarehouseName = (ds.Tables[0].Rows[i]["WAREHOUSE_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["WAREHOUSE_NAME"].ToString(),
                                ResrvPartNo = (ds.Tables[0].Rows[i]["RESRV_PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_PART_NO"].ToString(),
                                ResrvOwnerName = (ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"].ToString(),
                                ResrvConditionName = (ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"].ToString(),
                                ResrvPartDesc = (ds.Tables[0].Rows[i]["resrv_part_desc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["resrv_part_desc"].ToString(),
                                EtaDate = (ds.Tables[0].Rows[i]["ETA_DATE"] == DBNull.Value) ? DateTime.MinValue : DateTime.ParseExact(ds.Tables[0].Rows[i]["ETA_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                            };

                            // parse ETA Date
                            if (Math.Abs(part.EtaDate.Year - DateTime.Now.Year) > 3)
                            {
                                part.ErrorStr = "No Requested Part found!";
                            }

                            parts.Add(part);
                        }
                    }
                    else
                    {   // nothing found
                        Part part = new Part()
                        {
                            ErrorStr = "No part found"
                        };

                        parts.Add(part);
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNum", ex);
                    crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                    crm.detail.partavailabilityinfo.returninfo.failmessage = ex.Message;  
                    //throw (ex);              
                }
            
            return parts;
        }

        public static PartsList PartAvailavableLookupByPartNumWur(CRM1ClickService crm)
        {
            // Here we will do Parts availability check with 'SoftAllocation' & 'GNOME'           
            ContactAddressInfo cai = new ContactAddressInfo();
            cai = GetShippingAddress(new ContactAddressInfo(), crm);

            PartsList parts = null;

            // string skuPartNum = GetFlexFielsValue(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number");
            string skuPartNum = crm.detail.createorderinfo.orderlinedetail.part_no;

            if (!string.IsNullOrEmpty(skuPartNum))
            {
                List<OracleParameter> icParams = new List<OracleParameter>();
                icParams.Add(new OracleParameter("i_locationName", crm.header.location));
                icParams.Add(new OracleParameter("i_pendingBinId", crm.detail.createorderinfo.orderlinedetail.bin));  
                icParams.Add(new OracleParameter("i_partNum", skuPartNum));  
                icParams.Add(new OracleParameter("i_partDesc", crm.detail.partavailabilityinfo.dispositioncode));  //"battery" <--  detail.partavailabilityinfo.dispositioncode    
                icParams.Add(new OracleParameter("i_request_no", crm.detail.crmincidentno));
                icParams.Add(new OracleParameter("i_btt", string.Empty));
                icParams.Add(new OracleParameter("i_site", "JGS ANKARA"));  // crm.detail.crmsitecode));
                icParams.Add(new OracleParameter("i_crm_interface", crm.detail.crminterfacename));
                icParams.Add(new OracleParameter("i_quantity", 1));
                icParams.Add(new OracleParameter("i_crm_incident_no", crm.detail.crmincidentno));
                icParams.Add(new OracleParameter("i_incident_create_date", crm.detail.incidentcreatedate));              
                icParams.Add(new OracleParameter("i_city", cai.AddressInfo.city));  // "ANKARA"));
                icParams.Add(new OracleParameter("i_state_province", cai.AddressInfo.state));  // "ANKARA"));
                icParams.Add(new OracleParameter("i_postal_code", cai.AddressInfo.postalCode));  // "06770"));
                icParams.Add(new OracleParameter("i_country_code", cai.AddressInfo.country));  // "TR"));
                icParams.Add(new OracleParameter("i_priority_name", "High"));
                icParams.Add(new OracleParameter("i_svc_date", DateTime.Now.ToString("MM/dd/yy"))); //"01/01/12"));
                icParams.Add(new OracleParameter("i_svc_window", "AM1"));
                icParams.Add(new OracleParameter("i_part_subs", "Whole Line Only"));
                icParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                                

                try
                {
                    //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | GetClientInfo";
                    DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.bomBySkuLookupWur", icParams.ToArray());
                    parts = new PartsList();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Part part = new Part()
                            {
                                ErrorStr = (ds.Tables[0].Rows[i]["O_ERROR_STR"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["O_ERROR_STR"].ToString(),
                                OriginalPartDesc = (ds.Tables[0].Rows[i]["originalPartDesc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["originalPartDesc"].ToString(),
                                ProductClass = (ds.Tables[0].Rows[i]["procuctClassName"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["procuctClassName"].ToString(),
                                ProductSubclass = (ds.Tables[0].Rows[i]["softAllocParams"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["softAllocParams"].ToString(),
                                RequestLineNo = (ds.Tables[0].Rows[i]["REQUEST_LINE_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["REQUEST_LINE_NO"].ToString(),
                                PartNo = (ds.Tables[0].Rows[i]["PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["PART_NO"].ToString(),
                                OwnerName = (ds.Tables[0].Rows[i]["OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["OWNER_NAME"].ToString(),
                                ConditionName = (ds.Tables[0].Rows[i]["CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["CONDITION_NAME"].ToString(),
                                Quantity = (ds.Tables[0].Rows[i]["QUANTITY"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["QUANTITY"].ToString(),
                                WarehouseName = (ds.Tables[0].Rows[i]["WAREHOUSE_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["WAREHOUSE_NAME"].ToString(),
                                ResrvPartNo = (ds.Tables[0].Rows[i]["RESRV_PART_NO"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_PART_NO"].ToString(),
                                ResrvOwnerName = (ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_OWNER_NAME"].ToString(),
                                ResrvConditionName = (ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["RESRV_CONDITION_NAME"].ToString(),
                                ResrvPartDesc = (ds.Tables[0].Rows[i]["resrv_part_desc"] == DBNull.Value) ? string.Empty : ds.Tables[0].Rows[i]["resrv_part_desc"].ToString(),
                                EtaDate = (ds.Tables[0].Rows[i]["ETA_DATE"] == DBNull.Value) ? DateTime.MinValue : DateTime.ParseExact(ds.Tables[0].Rows[i]["ETA_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                            };

                            // parse ETA Date
                            if (Math.Abs(part.EtaDate.Year - DateTime.Now.Year) > 3)
                            {
                                part.ErrorStr = "No Requested Part found!";
                            }


                            parts.Add(part);
                        }
                    }
                    else
                    {   // nothing found
                        Part part = new Part()
                        {
                            ErrorStr = "No part found"
                        };

                        parts.Add(part);
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookupByPartNumWhr", ex);
                    crm.detail.partavailabilityinfo.returninfo.result = "Failure";
                    crm.detail.partavailabilityinfo.returninfo.failmessage = ex.Message;  

                    //throw (ex);              
                }
            }
            return parts;
        }


        public static CRM1ClickService SoftAllocationByPartNum(CRM1ClickService crm)
        {
            PartsList pl = PartAvailavableLookupByPartNum(crm);
            if ((pl == null) || (pl.Count == 0))
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. No Soft Allocation Information Available.";
            }
            else if (pl.Count > 1)
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. More than one records returned by process.";
            }
            else
            {
                // change the part number to the value received from Soft Alloc procedure
                Part part = pl[0];
                if (string.IsNullOrEmpty(part.ErrorStr))
                {
                    string resrvPartNo = part.ResrvPartNo;
                    //bool flexFieldFound = UpdateFlexFields(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number", resrvPartNo);
                    crm.detail.createorderinfo.orderlinedetail.part_no = resrvPartNo;
                }
            }
           
            return crm;
        }


        public static CRM1ClickService SoftAllocationByPartNum2Pass(CRM1ClickService crm)
        {
            PartsList pl = PartAvailavableLookupByPartNumSecondPass(crm);
            if ((pl == null) || (pl.Count == 0))
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. No Soft Allocation Information Available.";
            }
            else if (pl.Count > 1)
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. More than one records returned by process.";
            }
            else
            {
                // change the part number to the value received from Soft Alloc procedure
                Part part = pl[0];
                if (string.IsNullOrEmpty(part.ErrorStr))
                {
                    string resrvPartNo = part.ResrvPartNo;
                    //bool flexFieldFound = UpdateFlexFields(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number", resrvPartNo);
                    crm.detail.createorderinfo.orderlinedetail.part_no = resrvPartNo;
                    crm.detail.createorderinfo.orderlinedetail.condition = part.ResrvConditionName;
                    //crm.header.client = part.ResrvOwnerName;
                }
            }

            return crm;
        }
        

        public static CRM1ClickService SoftAllocationByPartNumWur(CRM1ClickService crm)
        {
            PartsList pl = PartAvailavableLookupByPartNumWur(crm);
            if ((pl == null) || (pl.Count == 0))
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. No Soft Allocation Information Available.";
            }
            else if (pl.Count > 1)
            {
                crm.detail.createorderinfo.returninfo.result = "Failure";
                crm.detail.createorderinfo.returninfo.failmessage = "Soft Allocation failed for the Part. More than one records returned by process.";
            }
            else
            {
                // change the part number to the value received from Soft Alloc procedure
                Part part = pl[0];
                if (string.IsNullOrEmpty(part.ErrorStr))
                {
                    string resrvPartNo = part.ResrvPartNo;
                    //bool flexFieldFound = UpdateFlexFields(crm.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number", resrvPartNo);
                    crm.detail.createorderinfo.orderlinedetail.part_no = resrvPartNo;
                    crm.detail.createorderinfo.orderlinedetail.condition = part.ResrvConditionName;
                    //crm.header.client = part.ResrvOwnerName;
                }
            }

            return crm;
        }
        
        
        #endregion


        #region "Private CRM OrderCreate Methods //this Region is for Create Order Only"

        private static CRM1ClickService LogIncidentCRM(CRM1ClickService CRMXml)
        {
                       //Calling the Create Incident
            List<OracleParameter> icParams = new List<OracleParameter>();
            icParams.Add(new OracleParameter("LOCATION_NAME", CRMXml.header.location));
            icParams.Add(new OracleParameter("CLIENT_NAME", CRMXml.header.client));
            icParams.Add(new OracleParameter("INCIDENT_NO", CRMXml.detail.crmincidentno));
            icParams.Add(new OracleParameter("CRM_SITE", CRMXml.detail.crmsitecode));
            icParams.Add(new OracleParameter("CRM_INTERFACE", CRMXml.detail.crminterfacename));
            icParams.Add(new OracleParameter("CRM_DISPOSITION_TYPE", CRMXml.detail.dispositiontype));
            icParams.Add(new OracleParameter("CRM_DISPOSITION_CODE", CRMXml.detail.partavailabilityinfo.dispositioncode));
            icParams.Add(new OracleParameter("CRM_PRODUCT_CODE", CRMXml.detail.partavailabilityinfo.product));
            icParams.Add(new OracleParameter("CRM_SERIAL_NO", CRMXml.detail.serialno));
            icParams.Add(new OracleParameter("CRM_WARRANTY_CODE", CRMXml.detail.createorderinfo.orderlinedetail.warranty));
            icParams.Add(new OracleParameter("CRMORDER_TYPE", CRMXml.detail.ordertype));
            icParams.Add(new OracleParameter("CRM_AGENT_NAME", CRMXml.detail.agentname));
            icParams.Add(new OracleParameter("CRM_CONTACT_NAME", CRMXml.detail.contactinfo.contactname));
            icParams.Add(new OracleParameter("CRM_ADDRESS_LINE1", CRMXml.detail.contactinfo.contactaddress1));
            icParams.Add(new OracleParameter("CRM_ADDRESS_LINE2", CRMXml.detail.contactinfo.contactaddress2));
            icParams.Add(new OracleParameter("CRM_ADDRESS_LINE3", CRMXml.detail.contactinfo.contactaddress3));
            icParams.Add(new OracleParameter("CRM_CITY", CRMXml.detail.contactinfo.contactcity));
            icParams.Add(new OracleParameter("CRM_STATE", CRMXml.detail.contactinfo.contactstate));
            icParams.Add(new OracleParameter("CRM_POSTAL_CODE", CRMXml.detail.contactinfo.contactpostalcode));
            icParams.Add(new OracleParameter("CRM_ISOCOUNTRYCODE", CRMXml.detail.contactinfo.contactcountry));
            icParams.Add(new OracleParameter("CRM_MOBILE_PHONE", CRMXml.detail.contactinfo.contactphone.phoneno));
            icParams.Add(new OracleParameter("CRM_PRIMARY_PHONE", CRMXml.detail.contactinfo.contactphone.phoneno));
            icParams.Add(new OracleParameter("CRM_EMAIL", CRMXml.detail.contactinfo.contactemail));

            //Call the StoredProc
            try
            {
                //CRMXml.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | LogIncident";
                ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WebApp1.CRMRTN.LogCRMContactIncident", icParams.ToArray());
                CRMXml.detail.createorderinfo.returninfo.result = "Success";
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Process, "LogIncidentCRM", ex);
                CRMXml.detail.createorderinfo.returninfo.result = "Failure";
                CRMXml.detail.createorderinfo.returninfo.failmessage = ex.Message;
            }

            return CRMXml;

        }

        private static CRM1ClickService ValidateLoaner(CRM1ClickService CRM)
        {

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(obj2String(CRM));
            //doc.Save("C:\\CRMSvc\\xml\\PreLoaner.xml");
            Logger.AddEntry(Logger.EventSource.Process, "ValidateLoaner, PreLoaner.xml", System.Diagnostics.TraceEventType.Information, obj2String(CRM));
  
            
            List<OracleParameter> LoanerParams = new List<OracleParameter>();
            LoanerParams.Add(new OracleParameter("P_CRM_SITE", CRM.detail.crmsitecode));
            LoanerParams.Add(new OracleParameter("P_CRM_INTERFACE", CRM.detail.crminterfacename));
            LoanerParams.Add(new OracleParameter("P_CRM_DISPOSITION_TYPE", CRM.detail.dispositiontype));
            LoanerParams.Add(new OracleParameter("P_CRM_ORDERTYPE", CRM.detail.ordertype));
            LoanerParams.Add(new OracleParameter("P_LOCATION_NAME", CRM.header.location));
            LoanerParams.Add(new OracleParameter("P_CLIENT_NAME", CRM.header.client));
            LoanerParams.Add(new OracleParameter("P_CONTRACT_NAME", CRM.header.contract));
            LoanerParams.Add(new OracleParameter("P_BTT", CRM.detail.createorderinfo.orderheader.btt));
            LoanerParams.Add(new OracleParameter("P_TCC_LOCATION", CRM.detail.createorderinfo.orderheader.tcc_location));
            LoanerParams.Add(new OracleParameter("P_PART_NO", CRM.detail.createorderinfo.orderlinedetail.part_no));
            LoanerParams.Add(new OracleParameter("P_BIN", CRM.detail.createorderinfo.orderlinedetail.bin));
            LoanerParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

            try
            {
                //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | LoanerAvailable";
                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.LoanerAvailable", LoanerParams.ToArray());

                if (ds.Tables[0].Rows.Count > 0)
                {
                    FlexField ns = new FlexField();
                    ns.name = "Defective Part No"; ns.value = CRM.detail.createorderinfo.orderlinedetail.part_no;
                    CRM.header.HeaderFlexFields.Add(ns);
                    FlexField nf = new FlexField();
                    nf.name = "Defective Serial No"; nf.value = CRM.detail.serialno;
                    CRM.header.HeaderFlexFields.Add(nf);
                    CRM.detail.createorderinfo.orderlinedetail.part_no = ds.Tables[0].Rows[0]["PART_NO"].ToString();

                    //CRM.detail.createorderinfo.returninfo.replacepartnumber = ds.Tables[0].Rows[0]["PART_NUMBER"].ToString();
                }
                else
                {
                    CRM.detail.ordertype = "Repair and Return";
                }

            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Process, "ValidateLoaner", ex);

                CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                CRM.detail.createorderinfo.returninfo.failmessage = ex.Message;
                return CRM;
            }

            CRM.detail.createorderinfo.returninfo.result = "Success";
            return CRM;
        }


        /// <summary>
        /// Loaner validation. Uses generic table to validate loaner unit.
        /// For Whole Unit Repair orders.
        /// </summary>
        /// <param name="CRM"></param>
        /// <returns></returns>
        private static CRM1ClickService ValidateLoaner2(CRM1ClickService CRM)
        {
            Logger.AddEntry(Logger.EventSource.Process, "ValidateLoaner. PreLoaner2", System.Diagnostics.TraceEventType.Information, obj2String(CRM));

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(obj2String(CRM));
            //doc.Save("C:\\CRMSvc\\xml\\PreLoaner.xml");
            Logger.AddEntry(Logger.EventSource.Process, "ValidateLoaner2, PreLoaner.xml", System.Diagnostics.TraceEventType.Information, obj2String(CRM));

            string skuPartNum = GetFlexFielsValue(CRM.detail.createorderinfo.orderlinedetail.LineFlexFields, "SKU Part Number");

            List<OracleParameter> LoanerParams = new List<OracleParameter>();
            LoanerParams.Add(new OracleParameter("P_CRM_SITE", CRM.detail.crmsitecode));
            LoanerParams.Add(new OracleParameter("P_CRM_INTERFACE", CRM.detail.crminterfacename));
            LoanerParams.Add(new OracleParameter("P_CRM_DISPOSITION_TYPE", CRM.detail.dispositiontype));
            LoanerParams.Add(new OracleParameter("P_CRM_ORDERTYPE", CRM.detail.ordertype));
            LoanerParams.Add(new OracleParameter("P_LOCATION_NAME", CRM.header.location));
            LoanerParams.Add(new OracleParameter("P_CLIENT_NAME", CRM.header.client));
            LoanerParams.Add(new OracleParameter("P_CONTRACT_NAME", CRM.header.contract));
            LoanerParams.Add(new OracleParameter("P_BTT", CRM.detail.createorderinfo.orderheader.btt));
            LoanerParams.Add(new OracleParameter("P_TCC_LOCATION", CRM.detail.createorderinfo.orderheader.tcc_location));
            LoanerParams.Add(new OracleParameter("P_PART_NO", skuPartNum)); // CRM.detail.createorderinfo.orderlinedetail.part_no));
            LoanerParams.Add(new OracleParameter("P_BIN", CRM.detail.createorderinfo.orderlinedetail.bin));
            LoanerParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));

            try
            {
                //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | LoanerAvailable4";
                DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.LoanerAvailable4", LoanerParams.ToArray());

                if (ds.Tables[0].Rows.Count > 0)
                {
                    FlexField ns = new FlexField();
                    ns.name = "Defective Part No"; ns.value = skuPartNum;// CRM.detail.createorderinfo.orderlinedetail.part_no;
                    CRM.header.HeaderFlexFields.Add(ns);
                    FlexField nf = new FlexField();
                    nf.name = "Defective Serial No"; nf.value = CRM.detail.serialno;
                    CRM.header.HeaderFlexFields.Add(nf);
                    CRM.detail.createorderinfo.orderlinedetail.part_no = ds.Tables[0].Rows[0]["PART_NO"].ToString();

                    //CRM.detail.createorderinfo.returninfo.replacepartnumber = ds.Tables[0].Rows[0]["PART_NUMBER"].ToString();
                    //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | LoanerAvailable4 IN";
                }
                else
                {
                    CRM.detail.ordertype = "Repair and Return";
                    if (string.IsNullOrEmpty(CRM.detail.createorderinfo.orderlinedetail.part_no))
                    {                        
                        CRM.detail.createorderinfo.orderlinedetail.part_no = skuPartNum;
                    }
                    //CRM.detail.partavailabilityinfo.returninfo.failmessage = ConnString + " | LoanerAvailable4 OUT";
                    //CRM.detail.createorderinfo.orderlinedetail.part_no = NULL;
                }

            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Process, "ValidateLoaner", ex);
                CRM.detail.createorderinfo.returninfo.result = "Unsuccessful";
                CRM.detail.createorderinfo.returninfo.failmessage = "[LoanerAvailable4] - " + ex.Message;
                return CRM;
            }

            CRM.detail.createorderinfo.returninfo.result = "Success";
            return CRM;
        }


        private static ReferenceOrderInfo GetRefOrderInfo(ReferenceOrderInfo ROI, CRM1ClickService CRM)
        {
            return ROI;
        }
        
        private static ContactAddressInfo GetShippingAddress(ContactAddressInfo CAI, CRM1ClickService CRM)
        {
            ContactInfo CI = new ContactInfo();
            AddressInfo AI = new AddressInfo();

            switch (CRM.detail.shipto.ToUpper())
            {
                case "CUSTOMER":
                    {
                        CI.abbrv = null; // ASK
                        CI.contactDesc = null; // ASK
                        CI.email = CRM.detail.contactinfo.contactemail; 
                        CI.fax = null;
                        CI.mobilePhone = CRM.detail.contactinfo.contactphone.phoneno;
                        CI.name = CRM.detail.contactinfo.contactname;
                        CI.pager = null;
                        CI.primaryPhone = CRM.detail.contactinfo.contactphone.phoneno;
                        CI.title = null;


                        AI.abbrv = null; // ASK
                        AI.city = CRM.detail.contactinfo.contactcity;

                        AI.country = CRM.detail.contactinfo.contactcountry; 
                        //if (CRM.detail.contactinfo.contactcountry.ToUpper().Contains("TU"))
                        //{
                        //    AI.country = "TR";
                        //}
                        
                        AI.line1 = CRM.detail.contactinfo.contactaddress1;
                        AI.line2 = CRM.detail.contactinfo.contactaddress2;
                        AI.line3 = CRM.detail.contactinfo.contactaddress3;
                        AI.line4 = null;
                        AI.addrName = null; //CRM.detail.contactinfo.contactname;
                        AI.postalCode = CRM.detail.contactinfo.contactpostalcode;
                        AI.state = CRM.detail.contactinfo.contactstate;

                        CAI.ContactInfo = CI;
                        CAI.AddressInfo = AI;
                        break;
                    }
                case "TCC":
                    {
                        DataSet ds = new DataSet();
                        string TCCLoc = CRM.detail.createorderinfo.orderheader.tcc_location;
                        string TradingPartner = CRM.detail.createorderinfo.orderheader.customertradingpartner;
                        string r = GetTCCAddress(TCCLoc, TradingPartner, out ds);

                        if (ds != null)
                        {
                            CI.abbrv = null; // ASK
                            CI.contactDesc = null; // ASK
                            CI.email = CRM.detail.contactinfo.contactemail;
                            CI.fax = null;
                            CI.mobilePhone = CRM.detail.contactinfo.contactphone.phoneno;
                            CI.name = CRM.detail.contactinfo.contactname;
                            CI.pager = null;
                            CI.primaryPhone = CRM.detail.contactinfo.contactphone.phoneno;
                            CI.title = null;


                            AI.abbrv = null;  //ds.Tables[0].Rows[0]["ADDRESS_ABBREV_CODE"].ToString();
                            AI.city = ds.Tables[0].Rows[0]["CITY"].ToString();
                            AI.country = ds.Tables[0].Rows[0]["ISO_COUNTRY_CODE"].ToString();  // Uncomment that, that should work. Updated SP is stored localy on my PC //"TR";  // 
                            AI.line1 = ds.Tables[0].Rows[0]["ADDRESS_LINE1"].ToString();
                            AI.line2 = ds.Tables[0].Rows[0]["ADDRESS_LINE2"].ToString();
                            AI.line3 = ds.Tables[0].Rows[0]["ADDRESS_LINE3"].ToString();
                            AI.line4 = ds.Tables[0].Rows[0]["ADDRESS_LINE4"].ToString();
                            AI.addrName = ds.Tables[0].Rows[0]["ADDRESS_NAME"].ToString();
                            AI.postalCode = ds.Tables[0].Rows[0]["POSTAL_CODE"].ToString();
                            AI.state = ds.Tables[0].Rows[0]["STATE_PROVINCE"].ToString();

                        }
                        else
                        {
                            AI.line4 = r;
                        }

                        CAI.ContactInfo = CI;
                        CAI.AddressInfo = AI;
                        break;
                    }
            }
            return CAI;
        }

        private static string GetTCCAddress(string TCCLoc, string TradingPartner, out DataSet ds)
        {

            string Error = "None";
            List<OracleParameter> crParams = new List<OracleParameter>();
            ds = null;
            try
            {
                if (TCCLoc == null)
                {
                    Error = "ERROR: TCC Location cannot be null";
                }

                if (TradingPartner == null)
                {
                    Error = "ERROR: Trading Partner cannot be null";
                }

                if (Error == "None")
                {
                    crParams.Add(new OracleParameter("P_TCC_LOCATION", TCCLoc.Trim().ToUpper()));
                    crParams.Add(new OracleParameter("P_TRADING_PARTNER", TradingPartner.Trim().ToUpper()));
                    crParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                    ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.CRMRTN.GetTCCAddress", crParams.ToArray());
                    if (ds.Tables[0].Rows.Count < 1)
                    {
                        Error = "ERROR: There were no results returned from the Address Proc";
                        ds = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Process, "GetTCCAddress", ex);

                Error = "ERROR: " + ex.Message + " | Failed to run Address Proc";
                ds = null;
            }

            return Error;


        }

        private static List<ReferenceOrderLine> GetRefOrderLines(CRM1ClickService input)
        {
            List<ReferenceOrderLine> RefOrderLines = new List<ReferenceOrderLine>();
            ReferenceOrderLine nl = new ReferenceOrderLine();

            //// get condition by Serial#
            //nl.condition = GetItemCondition(input.detail.serialno);


            //if (input.detail.dispositiontype == "Whole Unit Repair")
            //{
            //    nl.condition = "Defective";
            //}
            //else
            //{
            //    nl.condition = "Workable";
            //}

            if (!string.IsNullOrEmpty(input.detail.createorderinfo.orderlinedetail.condition))
            {
                nl.condition = input.detail.createorderinfo.orderlinedetail.condition;
            }
            else
            {
                if (input.detail.dispositiontype == "Whole Unit Repair")
                {
                    nl.condition = "Defective";
                }
                else
                {
                    nl.condition = "Workable";
                }
            }

            //nl.condition = "New"; // test only

            //nl.lineFlexFields = GetFlexFields("line", input).ToArray();
            nl.partNo = input.detail.createorderinfo.orderlinedetail.part_no;
            nl.qty = input.detail.createorderinfo.orderlinedetail.quantity;
            //Value passed will be the ReferenceOrderPartList List<> Object from the CRM XML. (Not Written)
            nl.unitPrice = input.detail.createorderinfo.orderlinedetail.unitprice;
            nl.currency = input.detail.createorderinfo.orderlinedetail.currency;
            nl.lineFlexFields = GetFlexFields(input.detail.createorderinfo.orderlinedetail.LineFlexFields);
            nl.partList = GetOrderParts(input);
            nl.warrantyFlag = WarrantyFlag;
            RefOrderLines.Add(nl);

            return RefOrderLines;
        }

        private static CRMObjects.ReceiptWrapper.RDEOrderPart[] GetOrderParts(CRM1ClickService crm)
        {
            List<CRMObjects.ReceiptWrapper.RDEOrderPart> orderParts = new List<RDEOrderPart>();
            orderParts.Add(new RDEOrderPart() { serialNo=crm.detail.serialno });
            return orderParts.ToArray();
        }

        /// <summary>
        /// Get item condition by serial#
        /// </summary>
        /// <param name="serialNo">serial number</param>
        /// <returns>condition name</returns>
        public static string GetItemCondition(string serialNo)
        {
            Logger.AddEntry(Logger.EventSource.Process, "GetItemCondition, ConnStr", System.Diagnostics.TraceEventType.Information, ConnString);

            string res = string.Empty;
            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("serialNo", serialNo),
                new OracleParameter("o_cursor", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output)
            };
            DataSet ds = ODPNETHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "WEBAPP1.GETITEMCONDITION", oraParams.ToArray());
            if (ds.Tables[0].Rows.Count > 0)
            {
                res = ds.Tables[0].Rows[0]["condition_name"].ToString();
            }
            return res;
        }

        private static RDEOrderPart[] GetReferenceOrderPartList(string input)
        {
            List<RDEOrderPart> np = new List<RDEOrderPart>();
            RDEOrderPart part = new RDEOrderPart();
            return np.ToArray();
        }

        private static FlexFields[] GetFlexFields(List<FlexField> CRMIn)
        {
            List<FlexFields> retList = new List<FlexFields>();
            foreach (FlexField ff in CRMIn)
            {
                FlexFields ffs = new FlexFields();
                ffs.Name = ff.name;
                ffs.Value = ff.value;
                retList.Add(ffs);              

                if (ff.name == "Product Warranty Code")
                {                   
                    FlexFields fw = new FlexFields();
                    if (ff.value == "01")
                    {
                        fw.Name = "Warranty";
                        fw.Value = "No";
                        retList.Add(fw);
                        WarrantyFlag = "No";
                    }
                    else
                    {
                        fw.Name = "Warranty";
                        fw.Value = "Yes";
                        retList.Add(fw);
                        WarrantyFlag = "Yes";
                    }
                }
            }
            return retList.ToArray();
        }


        private static string GetFlexFielsValue(List<FlexField> flexFields, string flexFieldName)
        {
            string res = string.Empty;
            try
            {
                IEnumerable<string> str = from ff in flexFields
                                          where string.Equals(ff.name, flexFieldName, StringComparison.OrdinalIgnoreCase)
                                          select ff.value;
                res = str.First();
            }
            catch
            {
                // no flex field found
            }
            return res;
        }


        private static bool UpdateFlexFields(List<FlexField> CRMIn, string flexFieldName, string flexFieldValue)
        {
            bool flexFieldFound = false;
            List<FlexFields> retList = new List<FlexFields>();
            foreach (FlexField ff in CRMIn)
            {
                if (ff.name == flexFieldName)
                {
                    ff.value = flexFieldValue;
                    flexFieldFound = true;
                }
            }
            return flexFieldFound;
        }


        #endregion
    }
}

