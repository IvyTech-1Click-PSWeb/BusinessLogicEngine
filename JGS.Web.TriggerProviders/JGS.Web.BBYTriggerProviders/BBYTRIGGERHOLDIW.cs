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
    public class BBYTRIGGERHOLDIW : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}            
            //,{"XML_STOCKINGLOC","/Trigger/Detail/HoldRelease/Source/StorageHoldCode"}
            ,{"XML_STGHoldCode","/Trigger/Detail/HoldRelease/Dest/StorageHoldCode"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}              
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}            
	    };

        public override string Name { get; set; }

        public BBYTRIGGERHOLDIW()
        {
            this.Name = "BBYTRIGGERHOLDIW";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            
            string UserName = string.Empty;            
            string BCN = string.Empty;
            string STCKLOC = string.Empty;

             //-- BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
            }

            //-- BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_STGHoldCode"]))
            {
                STCKLOC = Functions.ExtractValue(xmlIn, _xPaths["XML_STGHoldCode"]).Trim();
            }
            else
            {
                STCKLOC = "";
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

            // Start Validations

            if (STCKLOC.Trim().ToUpper() == "AWAITING ENGINEERING")
            {
                // IW?
                string Warranty = GetWarranty(BCN, UserName);

                if (Warranty != null)
                {
                    if (Warranty.Trim().ToUpper() == "YES")
                    {
                        // Factory Warranty?
                        string FW = GetFactoryWarranty(BCN, UserName);

                        if (FW != null) 
                        {
                            if (FW.Trim().ToUpper() == "FW")
                            {
                                // Previous history in hold like "Warranty Issue" ?
                                string History = GetHistoryinHold(BCN, UserName);

                                if (History == null || History == "")
                                {
                                    return SetXmlError(returnXml, "No puede seleccionar ese Storage Hold Code, entregar la unidad al Lider");
                                }
                            }
                        }                        
                    }
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

        public string GetWarranty(string BCN, string User)
        {
            string InWarranty = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();            
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            InWarranty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyholdiw", "getwarranty", myParams);

            return InWarranty;
        }

        public string GetFactoryWarranty(string BCN, string User)
        {
            string FacWarranty = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("FF", OracleDbType.Varchar2, "Labor_Coverage".Length, ParameterDirection.Input) { Value = "Labor_Coverage" });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            FacWarranty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyholdiw", "getfflineval", myParams);

            return FacWarranty;
        }

        public string GetHistoryinHold(string BCN, string User)
        {
            string HoldHist = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("SHC", OracleDbType.Varchar2, "Warranty Issue".Length, ParameterDirection.Input) { Value = "Warranty Issue" });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            HoldHist = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyholdiw", "getshc", myParams);

            return HoldHist;
        }
    }
}
