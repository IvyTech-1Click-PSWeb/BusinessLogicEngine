using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using JGS.DAL;

namespace JGS.Web.TriggerProviders
{
    public class GDI_TO_ASSEMBLY_TRG:TriggerProviderBase
    {
        public override string Name { get; set; }

        public GDI_TO_ASSEMBLY_TRG()
        {
            this.Name = "GDI_TO_ASSEMBLY_TRG";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string Schema_name = "WEBAPP1";
            string Package_name = "GDI_TIMEOUT_TRG";
            string triggerType;
            string workcenter;
            int workOrder;
            string serialNo;
            string bcn;
            string partNo;
            string opt;
            string resultCode;
            int itemId;
            string UserName;
            string pwd;
            string response;

            /**********************Getting values from xml*******************/
            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_TRIGGERTYPE"]))
                triggerType = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_TRIGGERTYPE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Trigger type can not be found.");
            //-- Get WC
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_WORKCENTER"]))
                workcenter = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_WORKCENTER"]);
            else
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
            //-- Get OPT
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_OPT"]))
                opt = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_OPT"]);
            else
                return SetXmlError(returnXml, "OPT can not be found.");
            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_SN"]))
                 serialNo = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_SN"]);
            else
                return SetXmlError(returnXml, "serialNo can not be found.");
            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_PN"]))
                partNo = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_PN"]);
            else
                return SetXmlError(returnXml, "partNo can not be found.");
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_RESULTCODE"]))
                resultCode = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_RESULTCODE"]);
            else
                return SetXmlError(returnXml, "ResultCode can not be found.");
            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_ItemID"]))
                itemId = Int32.Parse(Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_ItemID"]));
            else
                return SetXmlError(returnXml, "ItemId can not be found.");
            //-- Get BCN
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_BCN"]))
                bcn = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_BCN"]);
            else
                return SetXmlError(returnXml, "BCN can not be found.");
            //-- Get WorkOrderId
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_WORKORDERID"]))
                workOrder = Int32.Parse(Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_WORKORDERID"]));
            else
                return SetXmlError(returnXml, "workOrder can not be found.");
            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_USERNAME"]))
                UserName = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_USERNAME"]).Trim();
            else
                return SetXmlError(returnXml, "UserName could not be found.");
            //-- Get PWD
            if (!Functions.IsNull(xmlIn, xGDIDictionary._xPathsTO["XML_PWD"]))
                pwd = Functions.ExtractValue(xmlIn, xGDIDictionary._xPathsTO["XML_PWD"]).Trim();
            else
                return SetXmlError(returnXml, "Pwd could not be found.");

            if (triggerType.ToUpper() == "TIMEOUT")
            {
                String strResult = null;
                List<OracleParameter> myParams;

                if (workcenter.ToUpper() == "ASSEMBLE")
                {
                    #region"issueComp"

                    string ffName;
                    string ffValue;
                    string issuedPartNo;
                    string compList = null;
                    string confSList = null;
                    List<string> issuedComp = new List<string>();
                    List<string> confString = new List<string>();
                    DataSet spDataSet = new DataSet();
                    OracleParameter[] myParameters = new OracleParameter[4];
                    myParameters[0] = new OracleParameter("workorder", OracleDbType.Int32, workOrder, ParameterDirection.Input);
                    myParameters[1] = new OracleParameter("partno", OracleDbType.Varchar2, partNo, ParameterDirection.Input);
                    myParameters[2] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                    myParameters[3] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                    spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".getConfString", myParameters);

                    if (spDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow DR in spDataSet.Tables[0].Rows)
                        {
                            try
                            {
                                ffName = DR["FFNAME"].ToString();
                                ffValue = DR["FFVALUE"].ToString();
                                if (!string.IsNullOrEmpty(ffName) && !string.IsNullOrEmpty(ffValue))
                                {
                                    if (ffName.ToUpper().Equals("ORDER_CREATE_ERROR"))
                                    {
                                        if (!ffValue.ToUpper().Equals("X-"))
                                        {
                                            return SetXmlError(returnXml, "Reference Order Line has an error: " + ffValue);
                                        }
                                    }
                                    else
                                    {
                                        if (!ffValue.ToUpper().Equals("X-"))
                                        {
                                            confString.Add(ffValue);
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

                    spDataSet = new DataSet();
                    myParameters = new OracleParameter[3];
                    myParameters[0] = new OracleParameter("itemId", OracleDbType.Int32, itemId, ParameterDirection.Input);
                    myParameters[1] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                    myParameters[2] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                    spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".getIssuedComp", myParameters);

                    if (spDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow DR in spDataSet.Tables[0].Rows)
                        {
                            try
                            {
                                issuedPartNo = DR["ISSUEDCOMP"].ToString();
                                if (!string.IsNullOrEmpty(issuedPartNo))
                                {
                                    issuedComp.Add(issuedPartNo);
                                }
                            }
                            catch (Exception ex)
                            {
                                return SetXmlError(returnXml, ex.ToString());
                            }
                        }
                    }

                    if (confString.Count > 0)
                    {
                        if (issuedComp.Count > 0)
                        {
                            if (confString.Count == issuedComp.Count)
                            {
                                confString.RemoveAll(x => issuedComp.Contains(x));
                                if (confString.Count > 0)
                                {
                                    foreach (string confS in confString)
                                    {
                                        if (compList == null) compList = confS;
                                        else compList = compList + ", "+ confS;
                                    }
                                    return SetXmlError(returnXml, "You must issue the following component(s): " + compList);
                                }
                            }
                            else if (issuedComp.Count > confString.Count)
                            {
                                issuedComp.RemoveAll(x => confString.Contains(x));
                                if (issuedComp.Count > 0)
                                {
                                    foreach (string issuedC in issuedComp)
                                    {
                                        if (compList == null) compList = issuedC;
                                        else compList = compList + ", "+ issuedC;
                                    }
                                    return SetXmlError(returnXml, "You must remove the following components they do not belong to this unit: " + compList);
                                }
                            }
                            else if (confString.Count > issuedComp.Count)
                            {
                                confString.RemoveAll(x => issuedComp.Contains(x));
                                if (confString.Count > 0)
                                {
                                    foreach (string confS in confString)
                                    {
                                        if (compList == null) compList = confS;
                                        else compList = compList +", "+ confS;
                                    }
                                    return SetXmlError(returnXml, "You must issue the following component(s): " + compList);
                                }
                            }
                        }
                        else
                        {
                            foreach (string confS in confString)
                            {
                                if (confSList == null) confSList = confS;
                                else confSList = confSList +", "+ confS;
                            }
                            return SetXmlError(returnXml, "You must issue the following components: " + confSList);
                        }

                    }
                    else
                    {
                        return SetXmlError(returnXml, "Problems to retrive confString values");
                    }
                    #endregion

                    #region "changeSN"
                    if (serialNo.StartsWith("GDI"))
                    {
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "changeSNLogic", myParams);
                        if (!string.IsNullOrEmpty(strResult))
                        {
                            if (strResult.StartsWith("ERROR"))
                            {
                                return SetXmlError(returnXml, strResult);
                            }
                            else
                            {
                                /*ChangePart API*/
                                response = string.Empty;
                                try
                                {
                                    JGS.Web.GDITriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.GDITriggerProviders.ChangePart.ChangePartWrapper();
                                    JGS.Web.GDITriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.GDITriggerProviders.ChangePart.ChangePartInfo();
                                    cpi.SesCustomerID = "1";
                                    cpi.RequestId = "1";
                                    cpi.BCN = bcn;
                                    //cpi.NewPartNo = newPart;
                                    cpi.NewSerialNo = strResult;
                                    cpi.MustBeOnHold = false;
                                    cpi.ReleaseIfHold = false;
                                    cpi.MustBeTimedIn = false;
                                    cpi.TimedInWorkCenterName = " ";
                                    cpi.userName = UserName;
                                    cpi.Password = pwd;
                                    response = CPobj.PerformChangePart(cpi, false);
                                }
                                catch (Exception ex)
                                {
                                    response = ex.ToString();
                                }
                                if (!response.ToUpper().Equals("SUCCESS"))
                                {
                                    strResult = null;
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "rollbackSN", myParams);
                                    if (!string.IsNullOrEmpty(strResult))
                                    {
                                        if (strResult.StartsWith("ERROR"))
                                        {
                                            return SetXmlError(returnXml, strResult);
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                                        }
                                    }
                                }
                                //call changeSN API
                            }
                        }
                        else
                        {
                            return SetXmlError(returnXml, "Problems to retrieve data from DB [GDI_WUR_TIMEOUT.changeSNLogic]");
                        }
                    }
                    #endregion
                }

                if (workcenter.ToUpper() == "PACK OUT")
                {
                    #region "erwcRC"
                    strResult = null;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("workOrder", OracleDbType.Int32, workOrder.ToString().Length, ParameterDirection.Input) { Value = workOrder });
                    myParams.Add(new OracleParameter("partNo", OracleDbType.Varchar2, partNo.Length, ParameterDirection.Input) { Value = partNo });
                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "erwcRC", myParams);
                    if (!string.IsNullOrEmpty(strResult))
                    {
                        if (strResult.StartsWith("ERROR"))
                        {
                            return SetXmlError(returnXml, strResult);
                        }
                        else
                        {
                            if (!strResult.Equals(resultCode.ToUpper()))
                            {
                                XmlNode resultCNode = returnXml.SelectSingleNode(xGDIDictionary._xPathsTO["XML_RESULTCODE"]);
                                resultCNode.InnerText = strResult;
                            }
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Problems to retrieve data from DB [GDI_WUR_TIMEOUT.erwcRC]");
                    }

                  #endregion
                }
               }
            SetXmlSuccess(returnXml);
            return returnXml;
        }//execution
        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, xGDIDictionary._xPathsTO["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, xGDIDictionary._xPathsTO["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, xGDIDictionary._xPathsTO["XML_RESULT"], EXECUTION_OK);
        }
    }
}
