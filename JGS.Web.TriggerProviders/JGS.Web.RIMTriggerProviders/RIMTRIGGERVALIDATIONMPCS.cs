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
    public class RIMTRIGGERVALIDATIONMPCS : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"} 
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}              
            ,{"XML_ESN_Decimal","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='ESN_Dec']/Value"}
            ,{"XML_ESN_DecimalName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='ESN_Dec']/Name"}
            ,{"XML_Manufacture_date","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Manufacture_date']/Value"}
            ,{"XML_Manufacture_dateName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Manufacture_date']/Name"}  
            ,{"XML_ESN_UID","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='ESN_UID']/Value"}
            ,{"XML_ESN_UIDName","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='ESN_UID']/Name"} 
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"} 
	    };

        public override string Name { get; set; }

        public RIMTRIGGERVALIDATIONMPCS()
        {
            this.Name = "RIMTRIGGERVALIDATIONMPCS";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
                        
            string SN = string.Empty;
            string PN = string.Empty;
            string ExistSN = string.Empty;
            string ESN_Decimal = string.Empty;
            string MAN_DATE = string.Empty;
            string ESN_UID = string.Empty;
            string UserName = string.Empty;           
            
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

            if (PN.Trim().ToUpper() == "PRD-20839-003" || PN.Trim().ToUpper() == "PRD-26669-019" || PN.Trim().ToUpper() == "RDV-52586-008") 
            {
                ExistSN = ExistSNMPCSTable(SN, UserName);

                if (ExistSN != "")
                {
                    
                    //-- Verify if exist FF ESN_Decimal
                    if (Functions.IsNull(xmlIn, _xPaths["XML_ESN_DecimalName"]))
                    {
                        return SetXmlError(returnXml, "FF ESN_Decimal not found");
                    }

                    //-- Verify if exist FF Manufacture_date
                    if (Functions.IsNull(xmlIn, _xPaths["XML_Manufacture_dateName"]))
                    {
                        return SetXmlError(returnXml, "FF Manufacture_date not found");
                    }

                    //-- Verify if exist FF ESN_UID
                    if (Functions.IsNull(xmlIn, _xPaths["XML_ESN_UIDName"]))
                    {
                        return SetXmlError(returnXml, "FF ESN_UID not found");
                    }

                    ESN_Decimal = getesn_decimal(SN, UserName);
                    MAN_DATE = getman_date(SN, UserName);
                    ESN_UID = getesn_uid(SN, UserName);

                    // Fill FF ESN_Decimal 
                    SetXmlFFESN_Decimal(returnXml, ESN_Decimal);
                    
                    // Fill FF Manufacture_date 
                    SetXmlFFManufacture_date(returnXml, MAN_DATE);
                    
                    // Fill FF ESN_UID 
                    SetXmlFFESN_UID(returnXml, ESN_UID);                    

                }
            }            
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

        private void SetXmlFFESN_Decimal(XmlDocument returnXml, String FillXML_ESN_Decimal)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ESN_Decimal"], FillXML_ESN_Decimal);
        }

        private void SetXmlFFManufacture_date(XmlDocument returnXml, String FillXML_Manufacture_date)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_Manufacture_date"], FillXML_Manufacture_date);
        }

        private void SetXmlFFESN_UID(XmlDocument returnXml, String FillXML_ESN_UID)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ESN_UID"], FillXML_ESN_UID);
        }

        //////////////////// Get if sn exist in custom1.mpcs_inbound table /////////////////////
        public string ExistSNMPCSTable(string Number, string User)
        {
            string SNExist = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();            
            myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, Number.Length, ParameterDirection.Input) { Value = Number });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            SNExist = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "ExistSNMPCSTable", myParams);
            
            if (SNExist == null)
            {
                SNExist = "";
            }

            return SNExist;
        }

        //////////////////// Get ESN DECIMAL from in custom1.mpcs_inbound table /////////////////////
        public string getesn_decimal(string SNumber, string User)
        {
            string esndec = string.Empty;

            List<OracleParameter> myParamsesndec;
            myParamsesndec = new List<OracleParameter>();
            myParamsesndec.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNumber.Length, ParameterDirection.Input) { Value = SNumber });
            myParamsesndec.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            esndec = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "getesn_decimal", myParamsesndec);
            
            if (esndec == null)
            {
                esndec = "";
            }

            return esndec;
        }

        //////////////////// Get Manufactured Date from custom1.mpcs_inbound table /////////////////////
        public string getman_date(string SNumber, string User)
        {
            string ManDate = string.Empty;

            List<OracleParameter> myParamsManDate;
            myParamsManDate = new List<OracleParameter>();
            myParamsManDate.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNumber.Length, ParameterDirection.Input) { Value = SNumber });
            myParamsManDate.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            ManDate = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "getman_date", myParamsManDate);
            
            if (ManDate == null)
            {
                ManDate = "";
            }

            return ManDate;
        }

        //////////////////// Get ESN UID from custom1.mpcs_inbound table /////////////////////
        public string getesn_uid(string SNumber, string User)
        {
            string UID = string.Empty;

            List<OracleParameter> myParamsUID;
            myParamsUID = new List<OracleParameter>();
            myParamsUID.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNumber.Length, ParameterDirection.Input) { Value = SNumber });
            myParamsUID.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            UID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMRECEIPT", "getesn_uid", myParamsUID);
            
            if (UID == null)
            {
                UID = "";
            }

            return UID;
        }

       
    }
}
