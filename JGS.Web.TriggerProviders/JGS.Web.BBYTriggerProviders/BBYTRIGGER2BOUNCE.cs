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
    public class BBYTRIGGER2BOUNCE : JGS.Web.TriggerProviders.TriggerProviderBase 
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Receiving/Header/LocationID"}
			,{"XML_CLIENTID","/Receiving/Header/ClientID"}			
            ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}		
            ,{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}  
            ,{"XML_NUMOFREC","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Number Of Receipts']/Value"}
            ,{"XML_NUMOFRECName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Number Of Receipts']/Name"}
            ,{"XML_BOUNCECOUNT","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BOUNCE COUNT']/Value"}
            ,{"XML_BOUNCECOUNTName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BOUNCE COUNT']/Name"}
            ,{"XML_BounceLevel","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Bounce Level']/Value"}
            ,{"XML_BounceLevelName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Bounce Level']/Name"}
            ,{"XML_2X","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='2x']/Value"}
            ,{"XML_2XName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='2x']/Name"}
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}            
		};

        public override string Name { get; set; }

        public BBYTRIGGER2BOUNCE()
        {
            this.Name = "BBYTRIGGER2BOUNCE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int LocationId;
            int clientId;
            int contractId;

            string UserName = string.Empty;
            string SN = string.Empty;
            string FAT = string.Empty;
            string BTT = string.Empty;
            string RecTimes = string.Empty;
            string Privilege = string.Empty;
            string Days = string.Empty;

            bool X = false;
            bool A = false;

            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "BTT can not be found.");
            }  

            if (BTT.Trim().ToUpper() == "CR")
            {

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

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }

            //-- Get FixedAssetTag
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FAT"]))
            {
                FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FAT"]).Trim().ToUpper();
            }
            else
            {
                FAT = "";
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

                //////////////////// Check if User has RECEIPT privileges /////////////////////
                List<OracleParameter> myParams2;
                myParams2 = new List<OracleParameter>();
                myParams2.Add(new OracleParameter("Value", OracleDbType.Varchar2, "RECEIPT".Length, ParameterDirection.Input) { Value = "RECEIPT" });
                myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
                Privilege = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetPriv", myParams2);

                // If variable Privilege is null, fill value with ""
                if (Privilege == null)
                {
                    Privilege = "";
                }

                if (Privilege.ToUpper() != "RECEIPT")
                {

                    //-- Verify if exist FF Number of Receipt 
                    if (Functions.IsNull(xmlIn, _xPaths["XML_NUMOFRECName"]))
                    {
                        return SetXmlError(returnXml, "FF Number of Receipt not found, you need added in template");
                    } 

                    // Get SN Receiving Times
                    RecTimes = ReceivingTimes( contractId, clientId, SN, UserName);

                    // Value 0 if unit hasn't been received before.
                    if (RecTimes == null)
                    {
                        RecTimes = "0";
                    }

                    // Update NumberofReceipt FF
                    SetXmlFFNumofReceipt(returnXml, RecTimes);

                    if (Int32.Parse(RecTimes) > 1)
                    {

                        //  Get days of last ERWC and actual date (SN)
                        Days = BounceFF2X(LocationId, contractId, clientId, SN, UserName);

                        // Value 0 if unit hasn't been received before.
                        if (Days == null)
                        {
                            Days = "0";
                        }

                        // If days are between 10 and 90 fill FF 2X
                        if (Int32.Parse(Days) >= 10 && Int32.Parse(Days) <= 90)
                        {
                            //-- Verify if exist FF 2X
                            if (Functions.IsNull(xmlIn, _xPaths["XML_2XName"]))
                            {
                                return SetXmlError(returnXml, "FF 2X not found, you need add it in template");
                            }

                            X = true;

                            // Fill FF 2X with "2X" value
                            SetXmlFF2X(returnXml, "2X");
                        }
                    }
                    else
                    {
                        // Check if SN is exist in custom1.client_escalation_sn table
                        Days = GetGradeA(clientId, contractId, LocationId, SN, UserName);

                        if (Days != null)
                        {
                            // If days are between 10 and 90 fill FF 2X
                            if (Int32.Parse(Days) >= 10 && Int32.Parse(Days) <= 90)
                            {
                                //-- Verify if exist FF 2X
                                if (Functions.IsNull(xmlIn, _xPaths["XML_2XName"]))
                                {
                                    return SetXmlError(returnXml, "FF 2X not found, you need add it in template");
                                }

                                A = true;

                                // Fill FF 2X with "2X" value
                                SetXmlFF2X(returnXml, "2x Grade A");
                            }
                        }
                    }

                    if (FAT != "")
                    {
                        // Get FAT Receiving Times
                        RecTimes = ReceivingTimes( contractId, clientId, FAT, UserName);

                        // Value 0 if unit hasn't been received before.
                        if (RecTimes == null)
                        {
                            RecTimes = "0";
                        }

                        if (Int32.Parse(RecTimes) > 1)
                        {
                            if (X == false)
                            {
                                //  Get days of last ERWC and actual date (FAT)
                                Days = BounceFF2X(LocationId, contractId, clientId, FAT, UserName);

                                // Value 0 if unit hasn't been received before.
                                if (Days == null)
                                {
                                    Days = "0";
                                }

                                // If days are between 20 and 90 fill FF 2X
                                if (Int32.Parse(Days) >= 10 && Int32.Parse(Days) <= 90)
                                {
                                    //-- Verify if exist FF 2X
                                    if (Functions.IsNull(xmlIn, _xPaths["XML_2XName"]))
                                    {
                                        return SetXmlError(returnXml, "FF 2X not found, you need add it in template");
                                    }

                                    // Fill FF 2X with "2X" value
                                    SetXmlFF2X(returnXml, "2X");
                                }
                            }
                        }
                        else
                        {
                            if (A == false)
                            {
                                // Check if SN is exist in custom1.client_escalation_sn table
                                Days = GetGradeA(clientId, contractId, LocationId, FAT, UserName);

                                if (Days != null)
                                {
                                    // If days are between 20 and 90 fill FF 2X
                                    if (Int32.Parse(Days) >= 10 && Int32.Parse(Days) <= 90)
                                    {
                                        //-- Verify if exist FF 2X
                                        if (Functions.IsNull(xmlIn, _xPaths["XML_2XName"]))
                                        {
                                            return SetXmlError(returnXml, "FF 2X not found, you need add it in template");
                                        }

                                        // Fill FF 2X with "2X" value
                                        SetXmlFF2X(returnXml, "2x Grade A");
                                    }
                                }
                            }
                        }
                    }

                    if (Days == null)
                    {
                        Days = "0";
                    }

                    //-- Verify if exist FF BOUNCE COUNT
                    if (Functions.IsNull(xmlIn, _xPaths["XML_BOUNCECOUNTName"]))
                    {
                        return SetXmlError(returnXml, "FF BOUNCE COUNT not found, you need add it in template");
                    }

                    // Fill FF 2X with "2X" value
                    SetXmlFFBounceCount(returnXml, Days);

                    //-- Verify if exist FF Bounce Level
                    if (Functions.IsNull(xmlIn, _xPaths["XML_BounceLevelName"]))
                    {
                        return SetXmlError(returnXml, "FF Bounce Level not found, you need add it in template");
                    }

                    // Fill FF Bounce Level 
                    if (Int32.Parse(Days) > 90)
                    {
                        // Fill FF Bounce Level with "OOW" value
                        SetXmlFFXML_BounceLevel(returnXml, "OOW");
                    }
                    else
                    {
                        // Fill FF Bounce Level with "True Bounce" value
                        SetXmlFFXML_BounceLevel(returnXml, "True Bounce");
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

        private void SetXmlFFNumofReceipt(XmlDocument returnXml, String NumofRec)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_NUMOFREC"], NumofRec);
        }

        private void SetXmlFF2X(XmlDocument returnXml, String Fill2X)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_2X"], Fill2X);
        }

        private void SetXmlFFBounceCount(XmlDocument returnXml, String FillBounceCount)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_BOUNCECOUNT"], FillBounceCount);
        }

        private void SetXmlFFXML_BounceLevel(XmlDocument returnXml, String FillXML_BounceLevel)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_BounceLevel"], FillXML_BounceLevel);
        }

        ////////////////////// Get Receiving Times  /////////////////////    
        public string ReceivingTimes( int Contract, int Client, string Number, string User)
        {            
            string Times = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, "1244".Length, ParameterDirection.Input) { Value = "1244" });
            myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("ShiLou", OracleDbType.Varchar2, "Ship Outbound Unit".Length, ParameterDirection.Input) { Value = "Ship Outbound Unit" });
            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            Times = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "ReceivingTimes", myParams);

            return Times;
        }

        //////////////////// Get difference in days between last ERWC and actual date  /////////////////////
        public string BounceFF2X(int Loc, int Contract, int Client, string Number, string User)
        {
            string TotalDays = string.Empty;
            
            List<OracleParameter> myParams3;
            myParams3 = new List<OracleParameter>();
            myParams3.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams3.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams3.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams3.Add(new OracleParameter("ERWC", OracleDbType.Varchar2, "Perform Exit Routing".Length, ParameterDirection.Input) { Value = "Perform Exit Routing" });
            myParams3.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams3.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            TotalDays = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "LastShippingDays", myParams3);

            return TotalDays;
        }

        //////////////////// Get Grade A  /////////////////////
        public string GetGradeA(int Client, int Contract, int Loc, string Number, string User)
        {
            string TotalDays = string.Empty;

            List<OracleParameter> myParams2;
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams2.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams2.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });            
            myParams2.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            TotalDays = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetGrade", myParams2);

            return TotalDays;
        }
        

    }
}
