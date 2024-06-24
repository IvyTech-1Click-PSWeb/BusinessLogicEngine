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
    public class BBYTRIGGERHTCONERUR : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Receiving/Header/LocationID"}
			,{"XML_CLIENTID","/Receiving/Header/ClientID"}
            ,{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}  
            ,{"XML_RC","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}  
            ,{"XML_CONDITION","/Receiving/Detail/Order/Lines/Line/Condition"}  
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}    
           ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}
           ,{"XML_CLIENTREF1","/Receiving/Detail/Order/ClientRef1"}  
            ,{"XML_CLIENTREF2","/Receiving/Detail/Order/ClientRef2"}  
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}



		};

        public override string Name { get; set; }

        public BBYTRIGGERHTCONERUR()
        {
            this.Name = "BBYTRIGGERHTCONERUR";
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
            string Privilege = string.Empty;
            string SNinVal = string.Empty;
            string PN = string.Empty;
            string ClientRef1 = string.Empty;
            string ClientRef2 = string.Empty;
            string RC = string.Empty;
            string Condition = string.Empty;
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
            //-- Get ResultCode
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RC"]))
            {
                RC = Functions.ExtractValue(xmlIn, _xPaths["XML_RC"]);
            }
            else
            {
                RC = "";
            }
            //-- Get Condition
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONDITION"]))
            {
                Condition = Functions.ExtractValue(xmlIn, _xPaths["XML_CONDITION"]);
            }
            else
            {
                Condition = "";
            }
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTREF1"]))
            {
                ClientRef1 = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTREF1"]);
            }
            else
            {
                ClientRef1 = "";
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

            if (RC.ToUpper() == "QUALITY")
            {
                if (Condition.ToUpper()!="REPAIRED")
                {
                    return SetXmlError(returnXml, "Trigger Error: Unidad sin reparar, no puede enviar a QA");
                }
                //if (Condition.ToUpper() == "DEFECTIVE")
                //{
                //    return SetXmlError(returnXml, "Trigger Error: Unidad sin reparar, no puede enviar a QA");
                //}

            }
            if (RC.ToUpper() == "HTC_RUR")
            {
                if (Condition.ToUpper() == "REPAIRED")
                {
                    return SetXmlError(returnXml, "Trigger Error: Unidad Reparada, seleccione el Result Code 'Quality'");
                }
            }



            ////////////////////// Check if User has RECEIPT privileges /////////////////////
            //List<OracleParameter> myParams2;
            //myParams2 = new List<OracleParameter>();
            //myParams2.Add(new OracleParameter("Value", OracleDbType.Varchar2, "RECEIPT".Length, ParameterDirection.Input) { Value = "RECEIPT" });
            //myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
            //Privilege = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetPriv", myParams2);

            //if (Privilege == null)
            //{
            //    Privilege = "";
            //}

            //if (Privilege.ToUpper() != "RECEIPT")
            //{
            //    SNinVal = ResultinQA(LocationId, clientId, contractID, SN, UserName);
            //    if (SNinVal != null)
            //    {
            //        return SetXmlError(returnXml, "Trigger Error: Unidad reportada por cliente, favor de entregar a QA para ponerse en cuarentena");
            //    }

            //    if (FAT != "")
            //    {
            //        SNinVal = ResultinQA(LocationId, clientId, contractID, FAT, UserName);

            //        if (SNinVal != null)
            //        {
            //            return SetXmlError(returnXml, "Trigger Error: Unidad reportada por cliente, favor de entregar a QA para ponerse en cuarentena");
            //        }

            //    }

            //}




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

        public string ResultinQA(int Loc, int Client, int Contract, string Val, string User)
        {
            string ValResult = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, Loc.ToString().Length, ParameterDirection.Input) { Value = Loc });
            myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, Client.ToString().Length, ParameterDirection.Input) { Value = Client });
            myParams.Add(new OracleParameter("ContracId", OracleDbType.Int32, Contract.ToString().Length, ParameterDirection.Input) { Value = Contract });
            myParams.Add(new OracleParameter("SnFat", OracleDbType.Varchar2, Val.Length, ParameterDirection.Input) { Value = Val.ToUpper().Trim() });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            ValResult = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetQaSnFat", myParams);

            return ValResult;
        }

    }
}

