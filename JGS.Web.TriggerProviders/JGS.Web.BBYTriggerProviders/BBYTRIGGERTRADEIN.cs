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
    public class BBYTRIGGERTRADEIN : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_BTT","/Receiving/Header/BusinessTransactionType"}              
            ,{"XML_FFTradeIn","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='TradeIn']/Value"}
            ,{"XML_FFDiscrepancia","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Discrepancia']/Value"}  
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}            
		};

        public override string Name { get; set; }

        public BBYTRIGGERTRADEIN()
        {
            this.Name = "BBYTRIGGERTRADEIN";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
                 
            string BTT = string.Empty;
            string Privilege = string.Empty;
            string FFTradeIn = string.Empty;
            string FFDiscrepancia = string.Empty;
            string UserName = string.Empty;      

            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "BTT can not be found.");
            }

            if (BTT.Trim().ToUpper() == "CR-SCRN")
            {

            //-- Get FF TradeIn
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFTradeIn"]))
            {
                FFTradeIn = Functions.ExtractValue(xmlIn, _xPaths["XML_FFTradeIn"]);
            }
            else
            {
                FFTradeIn = "";
            }

            //-- Get FF Discrepancia
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFDiscrepancia"]))
            {
                FFDiscrepancia = Functions.ExtractValue(xmlIn, _xPaths["XML_FFDiscrepancia"]);
            }
            else
            {
                FFDiscrepancia = "";
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
                    if (FFTradeIn.Trim().ToUpper() == "YES")
                    {
                        if (FFDiscrepancia.Trim().ToUpper() != "")
                        {
                            return SetXmlError(returnXml, "Si FF TradeIn es YES, FF Discrepancia debe estar vacío");
                        }
                    }
                    else
                    {
                        if (FFDiscrepancia.Trim().ToUpper() == "")
                        {
                            return SetXmlError(returnXml, "Si FF TradeIn no es YES, FF Discrepancia debe tener valor seleccionado");
                        } 
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

