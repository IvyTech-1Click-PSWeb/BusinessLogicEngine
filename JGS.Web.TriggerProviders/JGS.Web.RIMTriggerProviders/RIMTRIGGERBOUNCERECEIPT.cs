using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.Web.TriggerProviders;
using System.Resources;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERBOUNCERECEIPT : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Receiving/Header/LocationID"}
			,{"XML_CLIENTID","/Receiving/Header/ClientID"}			
            ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}		
            ,{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}         
            ,{"XML_RMA","/Receiving/Detail/Order/ClientRef1"}
            ,{"XML_BounceUnits","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Bounce Units']/Value"}
            ,{"XML_BounceUnitsName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Bounce Units']/Name"}
            ,{"XML_DaysBounce","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Days Bounce']/Value"}
            ,{"XML_DaysBounceName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Days Bounce']/Name"}           
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"} 
	    };

        public override string Name { get; set; }

        public RIMTRIGGERBOUNCERECEIPT()
        {
            this.Name = "RIMTRIGGERBOUNCERECEIPT";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            int LocationId;
            int clientId;
            int contractId;            
            string SN = string.Empty;
            string Days = string.Empty;
            string UserName = string.Empty;
            string FF_OReason = string.Empty;
            string RMA = string.Empty;

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

            //-- Get RMA
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RMA"]))
            {
                RMA = Functions.ExtractValue(xmlIn, _xPaths["XML_RMA"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "RMA can not be found.");
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

            // ***** Start validations *****

            string BounceUnit = "NO";

            //  Get days of last ERWC and actual date (SN)
            Days = Bounce(LocationId, clientId, contractId, SN, UserName);

            // Value 0 if unit hasn't been received before.
            if (Days == null)
            {
                Days = "0";
            }

            //  Get value of FF_OReason (Reference Order in Header Level)
            FF_OReason = GetROHeaderFF("FF_OREASON", LocationId, clientId, contractId, RMA, UserName);
            
            // Value empty if unit hasn't value in Flex Field FF_OReason.
            if (FF_OReason == null)
            {
                FF_OReason = "";
            }             

            // If days are between 10 and 90 fill FF 2X
            if (Int32.Parse(Days) >= 10 && Int32.Parse(Days) <= 90)
            {
                // Validate Flex Field OReason Value                 
                if (FF_OReason != "CARRIER REJECTION" && FF_OReason != "RETURN TO STOCK" && FF_OReason != "NPI TEST" && FF_OReason != "DOA CREDIT" && FF_OReason != "OOW AUTHORIZED REFUB NTF UNITS" && FF_OReason != "PM - OOW AUTHORIZED REFUB NTF UNITS")
                {
                    BounceUnit = "YES";                         
                }                   
            }

            //-- Verify if exist FF Bounce Units
            if (Functions.IsNull(xmlIn, _xPaths["XML_BounceUnitsName"]))
            {
                return SetXmlError(returnXml, "FF Bounce Units not found");
            }

            // Fill FF "BounceUnits" with "YES" or "NO" value
            SetXmlFFBounceUnits(returnXml, BounceUnit);

            //-- Verify if exist FF Days Bounce
            if (Functions.IsNull(xmlIn, _xPaths["XML_DaysBounceName"]))
            {
                return SetXmlError(returnXml, "FF Days Bounce not found");
            }  
            
            // Fill FF "DaysBounce" with Days value
            SetXmlFFDaysBounce(returnXml, Days);

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

        private void SetXmlFFBounceUnits(XmlDocument returnXml, String FillXML_BounceUnits)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_BounceUnits"], FillXML_BounceUnits);
        }

        private void SetXmlFFDaysBounce(XmlDocument returnXml, String FillXML_DaysBounce)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_DaysBounce"], FillXML_DaysBounce);
        }

        //////////////////// Get difference in days between last ERWC and actual date  /////////////////////
        public string Bounce(int Loc, int Client, int Contract, string Number, string User)
        {
            string TotalDays = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });            
            myParams.Add(new OracleParameter("ERWC", OracleDbType.Varchar2, "Perform Exit Routing".Length, ParameterDirection.Input) { Value = "Perform Exit Routing" });
            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            TotalDays = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "LastShippingDays", myParams);            

            return TotalDays;
        }

        //////////////////// Get value of FF_OReason (Reference Order in Header Level)  /////////////////////
        public string GetROHeaderFF(string FF, int Loc, int Client, int Contract, string RMA, string User)
        {
            string FFValue = string.Empty;

            List<OracleParameter> myParams2;
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("FF_OReason", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams2.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams2.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams2.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams2.Add(new OracleParameter("RMA", OracleDbType.Varchar2, RMA.Length, ParameterDirection.Input) { Value = RMA });
            myParams2.Add(new OracleParameter("ROStatus", OracleDbType.Int32, "2".Length, ParameterDirection.Input) { Value = 2 });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            FFValue = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "FFROHeader", myParams2);
            
            return FFValue;
        }
    }
}
