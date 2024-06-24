using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using JGS.DAL;
using System.Data;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class MS_RECEIVING_TRG : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        //////////////////////////////// Schema name for Stored Procs calls ////////////////////////

        //private string Schema_name = "GREENSTM";
        private string Schema_name = "WEBAPP1";
        private string Package_name = "Microsoft_Receiving";

        private int InboundOrder;
        private int LineNo;
        private string UserName = string.Empty;

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{

             {"XML_USERNAME","/Receiving/Header/User/UserName"}
            ,{"XML_ORDERNUMBER","/Receiving/Detail/Order/OrderNumber"}
            ,{"XML_INBOUNDORDER","/Receiving/Detail/Order/InboundOrder"}
            ,{"XML_LINENO","/Receiving/Detail/Order/Lines/Line/LineNo"}
            ,{"XML_SERIALNO","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}
            ,{"XML_RESULTCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}
            ,{"XML_REDEMPTIONCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Redemption Code']/Value"}
            ,{"XML_DISPOSITIONCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Disposition Code']/Value"}
            ,{"XML_100","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '100 - External Tampered Console']/Value"}
            ,{"XML_101","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '101 - Banned Console']/Value"}
            ,{"XML_102","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '102 - Lost Console']/Value"}
            ,{"XML_103","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '103 - Damaged Console']/Value"}
            ,{"XML_107","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '107 - Liquidated Inventory]/Value"}
            ,{"XML_11","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '11 - Category 1 Rapid Response']/Value"}
            ,{"XML_12","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '12 - Category 2 Rapid Response']/Value"}
            ,{"XML_13","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '13 - Category 3 Rapid Response']/Value"}
            ,{"XML_130","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '130 - Paid Tamper Override']/Value"}
            ,{"XML_131","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '131 - ADP Unit']/Value"}
            ,{"XML_140","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '140 - Liquid Damage Flag - LDF']/Value"}
            ,{"XML_141","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '141 - Cracked Screen Flag - CSF']/Value"}
            ,{"XML_200","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '200 - Internal Tampered Console']/Value"}
            ,{"XML_201","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '201 - Severe Physical damage']/Value"}
            ,{"XML_86","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '86 - Xbox Live Beta Console']/Value"}
            ,{"XML_94","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '94 - Early Warning Program SKU']/Value"}
            ,{"XML_99","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '99 - Charitable Account Exchange']/Value"}
            ,{"XML_COMPONENTPARTNO","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'ComponentPartNumber']/Value"}
            ,{"XML_PLATFORM","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'PlatformType']/Value"}
            ,{"XML_PRODUCTFAMILYNAME","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'ProductFamilyName']/Value"}
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
          };

        public override string Name { get; set; }

        public MS_RECEIVING_TRG()
        {
            this.Name = "MS_RECEIVING_TRG";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            string SN = string.Empty;
            string FinalProdFlagId = string.Empty;
            string ShippingInstCode = string.Empty;
            string VendorActCode = string.Empty;
            string Redemption_Code = string.Empty;
            string User_Redemption_Code = string.Empty;
            string Repair_Type_Code = string.Empty;
            string Result_Code = string.Empty;

            int ReferenceOrderNumber;
            int Is_Redemption_Code_By_Trigger;

            //string mswrapperxml = string.Empty;
            string Flagupdate = string.Empty;
            
            bool ProdFlagFound;
            bool ProdFamilyName;
            bool isProdFlagPriorityMatched;
            string ProdFamily = string.Empty;
            string wrappermessage = string.Empty;  

            List<string> ProdFlaglist = new List<string>();
            DataSet ds;

            #region "Validations for Required fields "
            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]);
            }
            else
            {
                return SetXmlError(returnXml, "User Name not found.");
            }

            //-- Get Inbound Order
            if (!Functions.IsNull(xmlIn, _xPaths["XML_INBOUNDORDER"]))
            {
                InboundOrder = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_INBOUNDORDER"]));
            }
            else
            {
                return SetXmlError(returnXml, "Inbound order not found.");
            }

            //-- Get Line Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LINENO"]))
            {
                LineNo = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LINENO"]));
            }
            else
            {
                return SetXmlError(returnXml, "Line number not found.");
            }

            //-- Get Reference Order Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ORDERNUMBER"]))
            {
                ReferenceOrderNumber = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ORDERNUMBER"]));
            }
            else
            {
                return SetXmlError(returnXml, "Reference Order number not found.");
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]);
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number not found.");
            }


            if (!IsSNMatchOrder(SN))
            {
                return SetXmlError(returnXml, "Serial Number not matching with Order.");
            }

            //Check if Shipping instruction code and venddor activity type are available 
            //Unit Testing Begin
            //ReferenceOrderNumber = 1864097445; // Shipping instruction code and venddor activity type
            //Unit Testing End 
            List<OracleParameter> myParamVASI = new List<OracleParameter>();

            myParamVASI.Add(new OracleParameter("p_username", OracleDbType.Varchar2, UserName, ParameterDirection.Input));
            myParamVASI.Add(new OracleParameter("p_ReferenceOrderNumber", OracleDbType.Int32, ReferenceOrderNumber, ParameterDirection.Input));
            myParamVASI.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            ds = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetVendorActCdAndShippingInstr", myParamVASI.ToArray());
            if (ds.Tables[0].Rows.Count == 2)
            //if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {

                    if (ds.Tables[0].Rows[i][0].ToString().ToUpper() == "SHIPPING INSTRUCTION CODE")
                    {
                        ShippingInstCode = ds.Tables[0].Rows[i][1].ToString();
                    }

                    if (ds.Tables[0].Rows[i][0].ToString().ToUpper() == "VENDOR ACTIVITY TYPE")
                    {
                        VendorActCode = ds.Tables[0].Rows[i][1].ToString();
                    }
                }

                if ((ShippingInstCode.Length == 0) || (VendorActCode.Length == 0))
                {
                    return SetXmlError(returnXml, "Either Shipping instruction code or vendor activity type is not available for the reference order number.");
                }
            }
            else
            {
                return SetXmlError(returnXml, "Shipping instruction code and vendor activity type are not available for the reference order number.");
            }

            #endregion

            // Extract the Redemption code entered by operator
            if (!Functions.IsNull(xmlIn, _xPaths["XML_REDEMPTIONCODE"]))
            {
                User_Redemption_Code = Functions.ExtractValue(xmlIn, _xPaths["XML_REDEMPTIONCODE"]);
            }
           
            //Call Microsoft Return WS wrapper
            JGS.Web.MSTriggerProvider.MSW.MicrosoftWrapper ms = new JGS.Web.MSTriggerProvider.MSW.MicrosoftWrapper();     
            XmlDocument xmlMSWrapperOutput = new XmlDocument();
            xmlMSWrapperOutput.LoadXml(ms.GetProductInfo(SN).OuterXml.ToString());
           
            //Unit Test Begin
            //xmlMSWrapperOutput.LoadXml(ms.GetProductInfo("495725311104").OuterXml.ToString());
            //xmlMSWrapperOutput.Load("C:\\Guru\\TestMSWrapperIP.xml");
            //Unit Test End

            //Check if the wrapper result is FAIL
            XmlNodeList Resnodes = xmlMSWrapperOutput.GetElementsByTagName("Result");
            if (Resnodes.Count > 0)
            {
                if (Resnodes.Item(0).InnerText == "FAIL")
                {
                    XmlNodeList errmsgnode = xmlMSWrapperOutput.GetElementsByTagName("Message");
                    wrappermessage = errmsgnode.Item(0).InnerText;
                    return SetXmlError(returnXml, wrappermessage);
                }
                else
                {
                    XmlNodeList wrapperresult = xmlMSWrapperOutput.GetElementsByTagName("Message");
                    wrappermessage = wrapperresult.Item(0).InnerText;
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], wrappermessage);
                }
            }
            // To update the Family Name
            XmlNodeList nodes = xmlMSWrapperOutput.SelectNodes("/ShowItemMasterType");
            ProdFamilyName = false;

            foreach (XmlNode xn in nodes)
            {
                for (int i = 0; i < xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Count; i++)
                {
                    if (!ProdFamilyName)
                    {
                        if ((xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).Name == "Specification") && (xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).Attributes["type"].Value == "Family Name"))
                        {
                            //Assuming there will be only one entry for family name
                            ProdFamily = xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).InnerText;
                            ProdFamilyName = true;
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_PRODUCTFAMILYNAME"], ProdFamily);
                        }
                    }
                    else
                        break;
                }
            }

            // To get the list of ProdFlags
            ProdFlagFound = false;
            foreach (XmlNode xn in nodes)
            {
                for (int i = 0; i < xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Count; i++)
                {
                    if (!ProdFlagFound)
                    {
                        if ((xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).Name == "Classification") && (xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).Attributes["type"].Value == "Flag Codes"))
                        {
                            XmlNodeList NodeFinal = xn.ChildNodes.Item(1).LastChild.LastChild.ChildNodes.Item(i).ChildNodes.Item(0).ChildNodes;
                            if (NodeFinal.Count > 0)
                                ProdFlagFound = true;
                            for (int j = 0; j < NodeFinal.Count; j++)
                            {
                                ProdFlaglist.Add(NodeFinal.Item(j).InnerText);

                            }

                        }
                    }
                    else
                        break;
                }
            }
            
            if (ProdFlaglist.Count == 0)
            {
                FinalProdFlagId = "N/A";
            }
            else if (ProdFlaglist.Count == 1)
            {
                Flagupdate = "XML_" + ProdFlaglist[0].ToString();
                //Updating the Flex field as true
                if (_xPaths.ContainsKey(Flagupdate))
                {
                    Functions.UpdateXml(ref returnXml, _xPaths[Flagupdate], "1");
                    FinalProdFlagId = ProdFlaglist[0].ToString();
                }
                else
                    FinalProdFlagId = "N/A";
            }
                
            else if (ProdFlaglist.Count > 1)
            {
                //Updating all the Flex fields as true
                foreach (string value in ProdFlaglist)
                {
                    Flagupdate = "XML_" + value;
                    if (_xPaths.ContainsKey(Flagupdate))
                        Functions.UpdateXml(ref returnXml, _xPaths[Flagupdate], "1");
                }

                //Get highest priority Prod Flag
               List<OracleParameter> myParamProdFlag = new List<OracleParameter>();

                myParamProdFlag.Add(new OracleParameter("p_username", OracleDbType.Varchar2, UserName, ParameterDirection.Input));
                myParamProdFlag.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));


                try
                {
                    ds = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetProductFlags", myParamProdFlag.ToArray());
                    isProdFlagPriorityMatched = false;
                    FinalProdFlagId = "N/A";
                    //Assuming procedure will return the data sorted by priority in ascending order
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (!isProdFlagPriorityMatched)
                            {
                                foreach (string value in ProdFlaglist)
                                {
                                    if (ds.Tables[0].Rows[i][0].ToString().ToUpper() == value.ToUpper())
                                    {
                                        FinalProdFlagId = ds.Tables[0].Rows[i][0].ToString();
                                        isProdFlagPriorityMatched = true;
                                        break;
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "The product flag priority is not available for the given user name.");

                    }
                }
                catch (Exception ex) { return SetXmlError(returnXml, "GetProductFlags" + ex); }

            }

            //Get Product information
            List<OracleParameter> myParamProdInfo = new List<OracleParameter>();

            myParamProdInfo.Add(new OracleParameter("p_username", OracleDbType.Varchar2, UserName, ParameterDirection.Input));
            myParamProdInfo.Add(new OracleParameter("p_ProdFlagId", OracleDbType.Varchar2, FinalProdFlagId, ParameterDirection.Input));
            myParamProdInfo.Add(new OracleParameter("p_ShippingInstCode", OracleDbType.Varchar2, ShippingInstCode, ParameterDirection.Input));
            myParamProdInfo.Add(new OracleParameter("p_VendorActCode", OracleDbType.Varchar2, VendorActCode, ParameterDirection.Input));
            myParamProdInfo.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            try
            {
                ds = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + "." + "GetProductInfo", myParamProdInfo.ToArray());

                //ds returns IS_REDEMPTION_CODE_BY_TRIGGER, REDEMPTION_CODE, REPAIR_TYPE_CODE, RESULT_CODE 
                Is_Redemption_Code_By_Trigger = Int32.Parse(ds.Tables[0].Rows[0][0].ToString());

                if ((Is_Redemption_Code_By_Trigger == 0) && (User_Redemption_Code.Length == 0))
                {
                    return SetXmlError(returnXml, "Select the desired redemption code.");

                }
                else if ((Is_Redemption_Code_By_Trigger == 0) && (User_Redemption_Code.Length > 0))
                {
                    //Unit Testing Begin 
                    //User_Redemption_Code = "CSD";
                    //Unit Testing End 
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i][1].ToString().ToUpper() == User_Redemption_Code.ToUpper())
                        {
                            Redemption_Code = ds.Tables[0].Rows[i][1].ToString();
                            Repair_Type_Code = ds.Tables[0].Rows[i][2].ToString();
                            Result_Code = ds.Tables[0].Rows[i][3].ToString();
                            break;
                        }

                    }

                    if (Repair_Type_Code == "INVALID")
                        return SetXmlError(returnXml, "The repair type is invalid for the given redemption code.");

                    Functions.UpdateXml(ref returnXml, _xPaths["XML_DISPOSITIONCODE"], Repair_Type_Code);
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], Result_Code);
                }
                else if (Is_Redemption_Code_By_Trigger == 1)
                // We are expecting only one record
                {
                    Redemption_Code = ds.Tables[0].Rows[0][1].ToString();
                    Repair_Type_Code = ds.Tables[0].Rows[0][2].ToString();
                    Result_Code = ds.Tables[0].Rows[0][3].ToString();

                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REDEMPTIONCODE"], Redemption_Code);
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_DISPOSITIONCODE"], Repair_Type_Code);
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], Result_Code);
                }

            }
            catch (Exception ex) { return SetXmlError(returnXml, "GetProductInfo " + ex); }



            //// Set Return Code to Success
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

        private bool IsSNMatchOrder(string SN)
        {
            bool isSNMatch;
            List<OracleParameter> myParams;

            //Unit Testing Start
            //Unit Testing Values LineNo = 1, INboundorder = 349644
            //InboundOrder = 349644;
            //LineNo = 1;
            //Unit Testing End 

            Functions.DebugOut("-----  GetInboundOrderSN --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            myParams.Add(new OracleParameter("p_InboundOrderNumber", OracleDbType.Int32, InboundOrder.ToString().Length, ParameterDirection.Input) { Value = InboundOrder });
            myParams.Add(new OracleParameter("p_LineNumber", OracleDbType.Int32, LineNo.ToString().Length, ParameterDirection.Input) { Value = LineNo });
            myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

            try
            {
                DataTable dtSNForOrder = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, Schema_name+"." + Package_name + "."+"GetInboundOrderSN", myParams.ToArray());
                isSNMatch = false;
                //Unit Tsting Start
                //SN = "MY01C0624101313T0KL6";
                //Unit Testing End 
                if (dtSNForOrder.Rows.Count >= 1)
                {

                    for (int i = 0; i + 1 <= dtSNForOrder.Rows.Count; i++)
                    {
                        if (dtSNForOrder.Rows[i][0].ToString().ToUpper().Equals(SN.ToUpper()))
                        {
                            isSNMatch = true;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex) { return false; }


            return isSNMatch;

        }
    }
}
