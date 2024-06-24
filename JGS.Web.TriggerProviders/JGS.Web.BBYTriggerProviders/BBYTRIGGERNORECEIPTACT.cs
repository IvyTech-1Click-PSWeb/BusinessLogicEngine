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
    public class BBYTRIGGERNORECEIPTACT : JGS.Web.TriggerProviders.TriggerProviderBase 
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Receiving/Header/LocationID"}
			,{"XML_CLIENTID","/Receiving/Header/ClientID"}			
            ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}	
            ,{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}  
            ,{"XML_FFRGM","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BBY_RGM']/Value"}
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}  
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
		};

        public override string Name { get; set; }

        public BBYTRIGGERNORECEIPTACT()
        {
            this.Name = "BBYTRIGGERNORECEIPTACT";
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
            string RGM = string.Empty;
            string SNinWIP = string.Empty;
            string Privilege = string.Empty;

            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "BTT can not be found.");
            }

            if (BTT.Trim().ToUpper() == "CR-IWE" || BTT.Trim().ToUpper() == "CR" || BTT.Trim().ToUpper() == "CR-SCRN")
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
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim().ToUpper(); ;
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

            //-- Flex Field BBY_RGM
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFRGM"]))
            {
                RGM = Functions.ExtractValue(xmlIn, _xPaths["XML_FFRGM"]).Trim().ToUpper();
            }
            else
            {
                RGM = "";
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

                    // Verify if SN exist in SN WIP
                    SNinWIP = UnitinWip(LocationId, clientId, SN, UserName);

                    // If exist show trigger error
                    if (SNinWIP != null)
                    {
                        return SetXmlError(returnXml, "Trigger Error: El serial number que ingresó está actualmente activo en el WIP, no puede ser utilizado para el cambio. Verifique nuevamente el serial.");
                    }

                    // Verify if SN exist in FAT in WIP
                    SNinWIP = FATinWip(LocationId, clientId, SN, UserName);

                    // If exist show trigger error
                    if (SNinWIP != null)
                    {
                        return SetXmlError(returnXml, "Trigger Error: El serial number que ingresó está actualmente activo en el WIP, en FAT " +  SNinWIP + " no puede ser utilizado para el cambio. Verifique nuevamente el serial.");
                    }                    

                    // Verify if FAT is not empty
                    if (FAT != "")
                    {
                        // Verify if FAT exist in FAT in WIP
                        SNinWIP = FATinWip(LocationId, clientId, FAT, UserName);

                        // If exist show trigger error
                        if (SNinWIP != null)
                        {
                            return SetXmlError(returnXml, "Trigger Error: El FAT que ingresó está actualmente activo en el WIP, no puede ser utilizado para el cambio. Verifique nuevamente el FAT.");
                        }

                        // Verify if FAT exist in SN WIP
                        SNinWIP = UnitinWip(LocationId, clientId, FAT, UserName);

                        // If exist show trigger error
                        if (SNinWIP != null)
                        {
                            return SetXmlError(returnXml, "Trigger Error: El FAT que ingresó está actualmente activo en el WIP, en SN " +  SNinWIP + " no puede ser utilizado para el cambio. Verifique nuevamente el FAT.");
                        }
                    }

                    if (RGM != "")
                    {
                        // Verify if FF BBY_RGM exist in SN in WIP
                        SNinWIP = FFinWip(LocationId, clientId, contractId, "BBY_RGM", RGM, UserName);

                        // If exist show trigger error
                        if (SNinWIP != null)
                        {
                            return SetXmlError(returnXml, "Trigger Error: El FF BBY_RGM que ingresó está actualmente activo en el WIP, en SN " + SNinWIP + " , no puede ser utilizado para el cambio. Verifique nuevamente el FF BBY_RGM.");
                        }  
                    }

            // Update SN (trim and toupper)
            SetXmlSN(returnXml, SN);

            // Update FAT (trim and toupper)
            SetXmlFAT(returnXml, FAT);

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

        private void SetXmlSN(XmlDocument returnXml, String SNtrimupper)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_SN"], SNtrimupper);
        }

        private void SetXmlFAT(XmlDocument returnXml, String FATtrimupper)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FAT"], FATtrimupper);
        }

        public string UnitinWip(int Loc, int Client, string Number, string User)
        {
            string ResultinWip = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("Inactive", OracleDbType.Int32, 0.ToString().Length, ParameterDirection.Input) { Value = 0 });
            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            ResultinWip = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "ExistSNinWIP", myParams);
            
            return ResultinWip;
        }

        public string FATinWip(int Loc, int Client, string FAT, string User)
        {
            string ResultinWip = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("Inactive", OracleDbType.Int32, 0.ToString().Length, ParameterDirection.Input) { Value = 0 });
            myParams.Add(new OracleParameter("FAT", OracleDbType.Varchar2, FAT.Length, ParameterDirection.Input) { Value = FAT });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            ResultinWip = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "ExistFATinWIP", myParams);
            
            return ResultinWip;
        }

        public string FFinWip(int Loc, int Client, int Contract, string FFName, string FFValue,  string User)
        {
            string FlexFinWip = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams.Add(new OracleParameter("Owner", OracleDbType.Varchar2, "BestBuy".Length, ParameterDirection.Input) { Value = "BestBuy" });
            myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, FFName.Length, ParameterDirection.Input) { Value = FFName });
            myParams.Add(new OracleParameter("FFValue", OracleDbType.Varchar2, FFValue.Length, ParameterDirection.Input) { Value = FFValue.ToUpper().Trim() });
            myParams.Add(new OracleParameter("Inactive", OracleDbType.Int32, 0.ToString().Length, ParameterDirection.Input) { Value = 0 });            
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            FlexFinWip = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "ExistRecFFinWip", myParams);           

            return FlexFinWip;
        }
    }
}
