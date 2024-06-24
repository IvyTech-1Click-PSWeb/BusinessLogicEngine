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
    public class BBYTRIGGERSTOPSHIP : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Receiving/Header/LocationID"}
			,{"XML_CLIENTID","/Receiving/Header/ClientID"}
            ,{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}  
            ,{"XML_COND","/Receiving/Detail/Order/Lines/Line/Items/Item/Condition"}  
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}    
           ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}
           ,{"XML_CLIENTREF1","/Receiving/Detail/Order/ClientRef1"}  
            ,{"XML_CLIENTREF2","/Receiving/Detail/Order/ClientRef2"}  
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}



		};

        public override string Name { get; set; }

        public BBYTRIGGERSTOPSHIP()
        {
            this.Name = "BBYTRIGGERSTOPSHIP";
        }
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int LocationId;
            int clientId;
            int contractID;

            string UserName = string.Empty;
            string SN = string.Empty;
            string FAT = string.Empty;
            string Cond = string.Empty;
            string Privilege = string.Empty;
            string SNinVal = string.Empty;
            string PN = string.Empty;
            string ClientRef1 = string.Empty;
            string ClientRef2 = string.Empty;
            string Cadena = string.Empty;
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

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
            }
            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]);
            }
            else
            {
                SN = "";
            }

            //-- Get FixedAssetTag
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FAT"]))
            {
                FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FAT"]);
            }
            else
            {
                FAT = "";
            }
            //-- Get Condition
            if (!Functions.IsNull(xmlIn, _xPaths["XML_COND"]))
            {
                Cond = Functions.ExtractValue(xmlIn, _xPaths["XML_COND"]);
            }
            else
            {
                Cond = "";
            }

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PN"]))
            {
                PN = Functions.ExtractValue(xmlIn, _xPaths["XML_PN"]);
            }
            else
            {
                PN = "";
            }
            //-- Get Client Ref 1
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTREF1"]))
            {
                ClientRef1 = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTREF1"]);
            }
            else
            {
                ClientRef1 = "";
            }
            //-- Get Client Ref 2
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTREF2"]))
            {
                ClientRef2 = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTREF2"]);
            }
            else
            {
                ClientRef2 = "";
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
            ////////////////////// Check if User has RECEIPT privileges /////////////////////
            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LOCATIONID", OracleDbType.Varchar2, LocationId, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("SERIAL", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN.ToUpper() });
            myParams.Add(new OracleParameter("FAT", OracleDbType.Varchar2, FAT.Length, ParameterDirection.Input) { Value = FAT.ToUpper() });
            myParams.Add(new OracleParameter("COND", OracleDbType.Varchar2, Cond.Length, ParameterDirection.Input) { Value = Cond.ToUpper() });
            myParams.Add(new OracleParameter("PARTNO", OracleDbType.Varchar2, PN.Length, ParameterDirection.Input) { Value = PN.ToUpper() });
            myParams.Add(new OracleParameter("CONTRACT", OracleDbType.Varchar2, contractID, ParameterDirection.Input) { Value = contractID });
            myParams.Add(new OracleParameter("CLIEREF1", OracleDbType.Varchar2, ClientRef1.Length, ParameterDirection.Input) { Value = ClientRef1.ToUpper() });
            myParams.Add(new OracleParameter("CLIEREF2", OracleDbType.Varchar2, ClientRef2.Length, ParameterDirection.Input) { Value = ClientRef1.ToUpper() });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
            Cadena = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSTOPSHIP", "STOPSHIP", myParams);
            string Msg;
            if ((Cadena != "0") && (Cadena != null))
            {
                int encuentra = 0;
                if ((Cadena != "0") && (Cadena != "1"))
                {
                    Cadena = Cadena.ToUpper();
                    string Busca = string.Empty;

                    XmlNodeList xnList = xmlIn.SelectNodes("/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField");
                    foreach (XmlNode xn in xnList)
                    {
                        if (xn["Name"].InnerText.ToUpper() != string.Empty || xn["Name"].InnerText.ToUpper() != null)
                        {
                            if ((xn["Value"].InnerText.ToUpper() != string.Empty) || (xn["Value"].InnerText.ToUpper() != null))
                            {
                                Busca = xn["Name"].InnerText.ToUpper() + "," + xn["Value"].InnerText.ToUpper();
                            }
                            encuentra = Cadena.IndexOf(Busca);
                            if (encuentra > 0)
                                break;
                        }
                    }
                }
                if ((encuentra > 0) || (Cadena == "1"))
                {
                    List<OracleParameter> myParams2;
                    myParams2 = new List<OracleParameter>();
                    myParams2.Add(new OracleParameter("LOCATIONID", OracleDbType.Varchar2, LocationId, ParameterDirection.Input) { Value = LocationId });
                    myParams2.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId, ParameterDirection.Input) { Value = clientId });
                    myParams2.Add(new OracleParameter("SERIAL", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN.ToUpper() });
                    myParams2.Add(new OracleParameter("FAT", OracleDbType.Varchar2, FAT.Length, ParameterDirection.Input) { Value = FAT.ToUpper() });
                    myParams2.Add(new OracleParameter("COND", OracleDbType.Varchar2, Cond.Length, ParameterDirection.Input) { Value = Cond.ToUpper() });
                    myParams2.Add(new OracleParameter("PARTNO", OracleDbType.Varchar2, PN.Length, ParameterDirection.Input) { Value = PN.ToUpper() });
                    myParams2.Add(new OracleParameter("CONTRACT", OracleDbType.Varchar2, contractID, ParameterDirection.Input) { Value = contractID });
                    myParams2.Add(new OracleParameter("CLIEREF1", OracleDbType.Varchar2, ClientRef1.Length, ParameterDirection.Input) { Value = ClientRef1.ToUpper() });
                    myParams2.Add(new OracleParameter("CLIEREF2", OracleDbType.Varchar2, ClientRef2.Length, ParameterDirection.Input) { Value = ClientRef1.ToUpper() });
                    myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
                    Cadena = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSTOPSHIP", "STOPSHIPREASON", myParams2);

                    return SetXmlError(returnXml, "Trigger Error: STOP SHIP RULE FOUND:" + Cadena);
                }
            }
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

        //public string ResultinQA(int Loc, int Client, int Contract, string Val, string User)
        //{
        //    string ValResult = string.Empty;

        //    List<OracleParameter> myParams;
        //    myParams = new List<OracleParameter>();
        //    myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
        //    myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
        //    myParams.Add(new OracleParameter("ContracId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
        //    myParams.Add(new OracleParameter("SnFat", OracleDbType.Varchar2, Val.Length, ParameterDirection.Input) { Value = Val.ToUpper().Trim() });
        //    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
        //    ValResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetQaSnFat", myParams);

        //    return ValResult;
        //}

    }
}

