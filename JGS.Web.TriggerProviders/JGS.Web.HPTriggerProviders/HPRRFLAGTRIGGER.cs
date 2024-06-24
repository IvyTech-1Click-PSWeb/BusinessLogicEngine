using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using System.Web;
using JGS.DAL;
using JGS.Web.TriggerProviders;

namespace JGS.Web.TriggerProviders
{
    public class HPRRFLAGTRIGGER : TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			 {"XML_LOCATIONID","/Trigger/Header/LocationID"} 
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_NOTES","/Trigger/Detail/TimeOut/Notes"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_WORKORDERID","/Trigger/Detail/ItemLevel/WorkOrderID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
		};

        public override string Name { get; set; }

        public HPRRFLAGTRIGGER()
        {
            this.Name = "HPRRFLAGTRIGGER";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "HPLAPTOPWURTO";

           
            
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            int workOrderId;
            string workcenter;
            int LocationId;
            string result;
            string UserName;
            string opt;
            string strResult;
            int itemId;
            string notes;
            string memorySizes;
            string hddSize;
            string osFF;
            

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

           
            //-- Get WC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]);
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
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
            //-- Get WorkOrder
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKORDERID"]))
            {
                workOrderId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKORDERID"]));
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
            else
            {
                return SetXmlError(returnXml, "OPT cannot be empty.");
            }

            //-- Get Notes
            if(!Functions.IsNull(xmlIn,_xPaths["XML_NOTES"]))
            {
                 notes = Functions.ExtractValue(xmlIn,_xPaths["XML_NOTES"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Notes cannot be empty.");
            }
            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]));
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

            if (workcenter.ToUpper().Equals("WUR_REPAIR"))
            {
                if (!result.ToUpper().Equals("PARTHOLD") && !result.ToUpper().Equals("BILLABLE") && !result.ToUpper().Equals("NFF_HOLD")
                      && !result.ToUpper().Equals("CUSTHOLD") && !result.ToUpper().Equals("ENG_HOLD") && !result.ToUpper().Equals("DEBUG"))
                {
                    //////////////////// Parameters List /////////////////////
                    List<OracleParameter> myParams;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("optId", OracleDbType.Varchar2, opt.Length, ParameterDirection.Input) { Value = opt });
                    myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("locId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "PRIORITYRC", myParams);

                    if (!string.IsNullOrEmpty(strResult))
                    {
                        if (strResult.IndexOf("ERROR") > 0)
                        {
                            return SetXmlError(returnXml, "In HPLAPTOPWURTO.PRIORITYRC: " + strResult);
                        }
                        else if (!strResult.Equals(result))
                        {
                            return SetXmlError(returnXml, "Please select the result code : " + strResult);
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPLAPTOPWURTO.PRIORITYRC]");
                    }



                    memorySizes = getWCFFValue(xmlIn, "Memory Sizes");
                    hddSize = getWCFFValue(xmlIn, "HDD Sizes");
                    osFF = hddSize = getWCFFValue(xmlIn, "Operating System");

                    strResult = null;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("itemId", OracleDbType.Int32, itemId.ToString().Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("osValue", OracleDbType.Varchar2, osFF.ToString().Length, ParameterDirection.Input) { Value = osFF });
                    myParams.Add(new OracleParameter("hddSize", OracleDbType.Varchar2, hddSize.ToString().Length, ParameterDirection.Input) { Value = hddSize });
                    myParams.Add(new OracleParameter("memorySizes", OracleDbType.Varchar2, memorySizes.ToString().Length, ParameterDirection.Input) { Value = memorySizes });
                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.ToString().Length, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "RRFLAGLOGIC", myParams);

                    if (!string.IsNullOrEmpty(strResult))
                    {
                        if (strResult.IndexOf("ERROR") > 0)
                        {
                            return SetXmlError(returnXml, "Notes field cannot be null!");
                        }
                        else
                        {
                            if (!Functions.IsNull(xmlIn, _xPaths["XML_WC_FF_NAME"].Replace("{FLEXFIELDNAME}", "RR Flag")))
                            {
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_WC_FF_VALUE"].Replace("{FLEXFIELDNAME}", "RR Flag"), strResult);
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Work center flex field \"RR Flag\" cannot be found for update.");
                            }

                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPLAPTOPWURTO.RRFLAGLOGIC]");
                    }
                } 
             }
             string smsTemplateDesc = null;
             if (workcenter.ToUpper().Equals("AWAITING_NFF_HOLD"))
             {                                
                //sms validation
                if (result.ToUpper().Equals("NFF_HOLD"))
                {
                    smsTemplateDesc = "HP - MEMPHIS - UNIT TIMED-OUT: AWAITING_NFF_HOLD";
                    strResult = null;
                    DataSet dataSetVar = new DataSet();
                    OracleParameter[] myParam = new OracleParameter[8];
                    myParam[0] = new OracleParameter("locId", OracleDbType.Int32, LocationId, ParameterDirection.Input);
                    myParam[1] = new OracleParameter("clientId", OracleDbType.Int32, clientId, ParameterDirection.Input);
                    myParam[2] = new OracleParameter("contractId", OracleDbType.Int32, contractId, ParameterDirection.Input);
                    myParam[3] = new OracleParameter("workOrderId", OracleDbType.Int32, workOrderId, ParameterDirection.Input);
                    myParam[4] = new OracleParameter("itemId", OracleDbType.Int32, itemId, ParameterDirection.Input);
                    myParam[5] = new OracleParameter("tempDesc", OracleDbType.Varchar2, smsTemplateDesc, ParameterDirection.Input);
                    myParam[6] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                    myParam[7] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                    dataSetVar = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".SMSVAL", myParam);

                    if (dataSetVar.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow DR in dataSetVar.Tables[0].Rows)
                        {
                            try
                            {
                                strResult = DR["STRMSG"].ToString();
                                if (!string.IsNullOrEmpty(strResult))
                                {
                                    return SetXmlError(returnXml, strResult);
                                }
                            }
                            catch (Exception ex) { return SetXmlError(returnXml, ex.ToString()); }
                        } //data row
                    } //matrix table count  
                }//sms val
             }
             if (workcenter.ToUpper().Equals("AWAITING_PARTS_HOLD"))
             {                                
                 //sms validation
                 if (result.ToUpper().Equals("PARTHOLD"))
                 {
                     smsTemplateDesc = "HP - MEMPHIS - UNIT TIMED-OUT: AWAITING_PARTS_HOLD";
                     strResult = null;
                     DataSet dataSetVar = new DataSet();
                     OracleParameter[] myParam = new OracleParameter[8];
                     myParam[0] = new OracleParameter("locId", OracleDbType.Int32, LocationId, ParameterDirection.Input);
                     myParam[1] = new OracleParameter("clientId", OracleDbType.Int32, clientId, ParameterDirection.Input);
                     myParam[2] = new OracleParameter("contractId", OracleDbType.Int32, contractId, ParameterDirection.Input);
                     myParam[3] = new OracleParameter("workOrderId", OracleDbType.Int32, workOrderId, ParameterDirection.Input);
                     myParam[4] = new OracleParameter("itemId", OracleDbType.Int32, itemId, ParameterDirection.Input);
                     myParam[5] = new OracleParameter("tempDesc", OracleDbType.Varchar2, smsTemplateDesc, ParameterDirection.Input);
                     myParam[6] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                     myParam[7] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                     dataSetVar = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".SMSVAL", myParam);

                     if (dataSetVar.Tables[0].Rows.Count > 0)
                     {
                         foreach (DataRow DR in dataSetVar.Tables[0].Rows)
                         {
                             try
                             {
                                 strResult = DR["STRMSG"].ToString();
                                 if (!string.IsNullOrEmpty(strResult))
                                 {
                                     return SetXmlError(returnXml, strResult);
                                 }
                             }
                             catch (Exception ex) { return SetXmlError(returnXml, ex.ToString()); }
                         } //data row
                     } //matrix table count  
                 }//sms val
             }
            SetXmlSuccess(returnXml);
            return returnXml;
        }

        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

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
        
    }
}
