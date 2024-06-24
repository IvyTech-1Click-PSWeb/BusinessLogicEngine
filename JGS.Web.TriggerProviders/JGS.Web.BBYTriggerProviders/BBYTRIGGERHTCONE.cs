using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERHTCONE : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}              
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}    
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}  
            ,{"XML_RC","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}  
            ,{"XML_FFReceipt_Flag","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Receipt_Flag']/Value"}
            ,{"XML_FFReceipt_FlagName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Receipt_Flag']/Name"}            
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}            
		};

        public override string Name { get; set; }

        public BBYTRIGGERHTCONE()
        {
            this.Name = "BBYTRIGGERHTCONE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string UserName = string.Empty;
            string SN = string.Empty;            
            string PN = string.Empty;
            string BTT = string.Empty;
            string Privilege = string.Empty;            
            string FFProcessType = string.Empty;
            string FFReceipt_Flag = string.Empty;
            string LastOwner = string.Empty;
            string LastCondition = string.Empty;
            string PreviousHistory = string.Empty;

            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "BTT can not be found.");
            }

            if (BTT.Trim().ToUpper() == "CR-IWE")
            {
            
            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }            

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PN"]))
            {
                PN = Functions.ExtractValue(xmlIn, _xPaths["XML_PN"]);
            }
            else
            {
                return SetXmlError(returnXml, "Part Number can not be found.");
            }            

            //-- Verify if exist FF Receipt_Flag
            if (Functions.IsNull(xmlIn, _xPaths["XML_FFReceipt_FlagName"]))
            {
                return SetXmlError(returnXml, "FF Receipt_Flag not found, you need added in template");
            }            

            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }

            
                // Get User Privilege
                Privilege = GetPrivilege("RECEIPT", UserName);

                if (Privilege.ToUpper() != "RECEIPT")
                {
                    // Get Process Type Value
                    FFProcessType = GetFFProcessType(PN, UserName);

                    // Get Last Owner
                    LastOwner = GetLastOwner(SN, UserName);

                    if (FFProcessType.ToUpper().Trim() == "ONE")
                    {
                        // Get if SN has history
                        PreviousHistory = GetPreviousHistory(SN, UserName);

                        if (Int32.Parse(PreviousHistory) > 0)
                        {
                            if (LastOwner.Trim().ToUpper() == "BESTBUY")
                            {
                                LastCondition = GetLastCondition(SN, UserName);

                                if (LastCondition.Trim().ToUpper() == "DEFECTIVE")
                                {
                                    SetXmlFFReceipt_Flag(returnXml, "One Repaired");

                                    SetXmlRC(returnXml, "RUR");
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "No puede recibir la unidad, revise historial previo");
                                }
                            }
                            else
                            {
                                return SetXmlError(returnXml, "No puede recibir la unidad, revise historial previo");
                            }
                        }
                        else
                        {
                            return SetXmlError(returnXml, "No puede recibir la unidad, revise historial previo");
                        }
                    }
                    else if (FFProcessType.ToUpper().Trim() == "OWR")
                    {
                        if (LastOwner.Trim().ToUpper() == "HTC")
                        {
                            SetXmlFFReceipt_Flag(returnXml, "HTC Rejected");

                            SetXmlRC(returnXml, "Process Unit");
                        }
                        else
                        {
                            return SetXmlError(returnXml, "No puede recibir la unidad, revise historial previo");
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "No puede recibir la unidad con este Template");
                    }

                }
            }

            // Set Return Code to Success            
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

        private void SetXmlFFReceipt_Flag(XmlDocument returnXml, String Result)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFReceipt_Flag"], Result);
        }

        private void SetXmlRC(XmlDocument returnXml, String Result)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RC"], Result);
        }

        ////////////////////// Get Previous History  /////////////////////    
        public string GetPreviousHistory(string SerialNo, string User)
        {
            string Times = string.Empty;

            List<OracleParameter> myParams1;
            myParams1 = new List<OracleParameter>();
            myParams1.Add(new OracleParameter("SN", OracleDbType.Varchar2, SerialNo.Length, ParameterDirection.Input) { Value = SerialNo });
            myParams1.Add(new OracleParameter("User", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            Times = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "getPreviousHistory", myParams1);
                        
            if (Times == null)
            {
                Times = "0";
            }

            return Times;
        }

        ////////////////////// Get Last Owner Value /////////////////////    
        public string GetLastOwner(string SerialNo, string User)
        {
            string Owner = string.Empty;

            List<OracleParameter> myParamsLastOwner;
                myParamsLastOwner = new List<OracleParameter>();
                myParamsLastOwner.Add(new OracleParameter("SN", OracleDbType.Varchar2, SerialNo.Length, ParameterDirection.Input) { Value = SerialNo });
                myParamsLastOwner.Add(new OracleParameter("User", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
                Owner = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "getLastOwner", myParamsLastOwner);
                
                if (Owner == null)
                {
                    Owner = "";
                }

            return Owner;
        }

        ////////////////////// Get Universal FF Process_Type  /////////////////////    
        public string GetFFProcessType(string PartNum, string User)
        {
            string PT = string.Empty;

            List<OracleParameter> myParamsFFProcessType;
            myParamsFFProcessType = new List<OracleParameter>();
            myParamsFFProcessType.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "PROCESS_TYPE".Length, ParameterDirection.Input) { Value = "PROCESS_TYPE" });
            myParamsFFProcessType.Add(new OracleParameter("PartNum", OracleDbType.Varchar2, PartNum.Length, ParameterDirection.Input) { Value = PartNum });
            myParamsFFProcessType.Add(new OracleParameter("User", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            PT = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "getuniversalffvalue", myParamsFFProcessType);
            
            if (PT == null)
            {
                PT = "";
            }

            return PT;
        }

        ////////////////////// Get Last Condition Value  /////////////////////    
        public string GetLastCondition(string SerialNum, string User)
        {
            string Cond = string.Empty;

            List<OracleParameter> myParamsLastCondition;
            myParamsLastCondition = new List<OracleParameter>();
            myParamsLastCondition.Add(new OracleParameter("SerialNum", OracleDbType.Varchar2, SerialNum.Length, ParameterDirection.Input) { Value = SerialNum });
            myParamsLastCondition.Add(new OracleParameter("User", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            Cond = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "getLastCondition", myParamsLastCondition);
            
            if (Cond == null)
            {
                Cond = "";
            }

            return Cond;
        }

        //////////////////////  Check if User has RECEIPT privileges /////////////////////    
        public string GetPrivilege(string Value, string User)
        {
            string Priv = string.Empty;

            List<OracleParameter> myParams2;
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("Value", OracleDbType.Varchar2, Value.Length, ParameterDirection.Input) { Value = Value });
            myParams2.Add(new OracleParameter("User", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User.ToUpper() });
            Priv = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetPriv", myParams2);

            // If variable Privilege is null, fill value with ""
            if (Priv == null)
            {
                Priv = "";
            }

            return Priv;
        }
    }
}

