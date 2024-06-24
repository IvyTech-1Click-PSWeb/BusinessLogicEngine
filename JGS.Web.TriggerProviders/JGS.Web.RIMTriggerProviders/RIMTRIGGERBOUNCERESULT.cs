using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERBOUNCERESULT : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_CLIENTID","/Trigger/Header/ClientID"}
            ,{"XML_CONTRACTID","/Trigger/Header/ContractID"}            
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
	    };

        public override string Name { get; set; }

        public RIMTRIGGERBOUNCERESULT()
        {
            this.Name = "RIMTRIGGERBOUNCERESULT";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            int itemid;
            int clientId;
            int contractId;           
            string username = string.Empty;
            string BounceUnits = string.Empty;
            string Bounce_Result = string.Empty;

            //-- Get ItemID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemid = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
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

            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                username = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }
            
            // ***** Start validations *****

                // Get Value of Flex Field "BOUNCE UNITS" 
                BounceUnits = GetFFValue("BOUNCE UNITS", clientId, contractId, itemid, username);

                // Get Value of Flex Field "BOUNCE_RESULT"
                Bounce_Result = GetFFValueWC("BOUNCE_RESULT", clientId, contractId, "BOUNCE", itemid, username);

                if (BounceUnits == null)
                {
                    BounceUnits = "";
                }

                if (Bounce_Result == null)
                {
                    Bounce_Result = "";
                }

                if (BounceUnits.ToUpper().Trim() == "YES")
                {
                    if (Bounce_Result.ToUpper().Trim() != "VALID" && Bounce_Result.ToUpper().Trim() != "INVALID")
                    {
                        return SetXmlError(returnXml, "Esta unidad es Bounce y no ha sido diagnosticada por el técnico, favor de enviar al area de Bounce");
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

        public string GetFFValue(string FF, int Client, int Contract, int Item, string User)
        {
            string FFResult = string.Empty;

            List<OracleParameter> myParamsFFBounceUnits;
            myParamsFFBounceUnits = new List<OracleParameter>();
            myParamsFFBounceUnits.Add(new OracleParameter("FFName", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParamsFFBounceUnits.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParamsFFBounceUnits.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParamsFFBounceUnits.Add(new OracleParameter("ItemId", OracleDbType.Int32, Item.ToString().Length, ParameterDirection.Input) { Value = Item });
            myParamsFFBounceUnits.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            FFResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBOUNCERESULT", "getffvalue", myParamsFFBounceUnits);            

            return FFResult;
        }

        public string GetFFValueWC(string FF, int Client, int Contract, string WC, int Item, string User)
        {
            string FFResultWC = string.Empty;

            List<OracleParameter> myParamsFFBounce_Result;
            myParamsFFBounce_Result = new List<OracleParameter>();
            myParamsFFBounce_Result.Add(new OracleParameter("FFName", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParamsFFBounce_Result.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParamsFFBounce_Result.Add(new OracleParameter("ContractId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParamsFFBounce_Result.Add(new OracleParameter("WC", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = WC });
            myParamsFFBounce_Result.Add(new OracleParameter("ItemId", OracleDbType.Int32, Item.ToString().Length, ParameterDirection.Input) { Value = Item });
            myParamsFFBounce_Result.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            FFResultWC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBOUNCERESULT", "getffvaluewc", myParamsFFBounce_Result);            

            return FFResultWC;
        }       
    }
}
