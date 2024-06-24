using System;
using System.Collections.Generic;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERSTOPSHIPPROCESS : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}    
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USER","/Trigger/Header/UserObj/UserName"}
            ,{"XML_USERPWD","/Trigger/Header/UserObj/PassWord"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            //,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}            
			//FA Fields
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
			//To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalisys/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES", 
				 "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
         ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']"}
         ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
             "DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
		};

        public override string Name { get; set; }

        public BBYTRIGGERSTOPSHIPPROCESS()
        {
            this.Name = "BBYTRIGGERSTOPSHIPPROCESS";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            string partNumber;
            string workcenterName;
            string pn = string.Empty;
            string valor = string.Empty;
            string resultCode;
            string SN;
            string Username = string.Empty;
            string usrpwd = string.Empty;
            string TriggerType = string.Empty;
            string ActionCodeName = string.Empty;
            string DefectCodeName = string.Empty;
            string ComponentPn = string.Empty;
            string ItemId = string.Empty;
            string KitMandatory = string.Empty;
            string ItemBcn = string.Empty;
            string OPT = string.Empty;
            string RecRC = string.Empty;
            string res = string.Empty;
            string v_FFBounce = string.Empty;
            string LocationId = string.Empty;
            string clientId = string.Empty;
            string contractId = string.Empty;
            string WcId = string.Empty;
            string Cadena = string.Empty;
            string FAT = null;
            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.|Numero de Serie no Encontrado");
            }

            //-- Get user
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USER"]))
            {
                Username = Functions.ExtractValue(xmlIn, _xPaths["XML_USER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.|Usuario no Encontrado");
            }

            //-- Get pwd
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERPWD"]))
            {
                usrpwd = Functions.ExtractValue(xmlIn, _xPaths["XML_USERPWD"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Pwd can not be found.|Pwd No Encontrado");
            }


            //-- Get opt
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT can not be found.|OPT no Encontrado");
            }




            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                partNumber = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.|PN no Encontrado");
            }


            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.| Work Center Name no Encontrado");
            }
            // - Get Work Center id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WcId = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center id can not be found.| Work Center id no Encontrado");
            }

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.|Id de Geography no Encontrado");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientId = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.|Id de Cliente no Encontrado");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.|Id de Contrato No Encontrado");
            }

            //-- TriggerType
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                TriggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Trigger type could not be found.|Tipo de Trigger no Encontrado");
            }

            //-- ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item Id could not be found.|Item ID no Encontrado");
            }

            //-- ItemBcn
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                ItemBcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
            }

            //if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            //{
            //    FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]).Trim();
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
            //}

            if (TriggerType.ToUpper() == "TIMEOUT")
            {               
            // Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.|Result Code No Encontrado");
            }

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("LOCATIONID", OracleDbType.Varchar2, LocationId, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("SERIAL", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN.ToUpper() });
            myParams.Add(new OracleParameter("FAT", OracleDbType.Varchar2, FAT, ParameterDirection.Input) { Value = FAT });
            myParams.Add(new OracleParameter("PARTNO", OracleDbType.Varchar2, partNumber.Length, ParameterDirection.Input) { Value = partNumber.ToUpper() });
            myParams.Add(new OracleParameter("CONTRACT", OracleDbType.Varchar2, contractId, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username.ToUpper() });
            Cadena = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSTOPSHIP", "STOPSHIPPROCESS", myParams);
            string Msg;
            if ((Cadena != "0") && (Cadena != null))
            {
                  int encuentra = 0;
                if ((Cadena != "0") && (Cadena != "1"))
                {
                Cadena = Cadena.ToUpper();
                string Busca = string.Empty;
                XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
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
                    myParams2.Add(new OracleParameter("FAT", OracleDbType.Varchar2, FAT, ParameterDirection.Input) { Value = FAT });
                    myParams2.Add(new OracleParameter("PARTNO", OracleDbType.Varchar2, partNumber.Length, ParameterDirection.Input) { Value = partNumber.ToUpper() });
                    myParams2.Add(new OracleParameter("CONTRACT", OracleDbType.Varchar2, contractId, ParameterDirection.Input) { Value = contractId });
                    myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username.ToUpper() });
                    Cadena = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYSTOPSHIP", "STOPSHIPREASONPROCESS", myParams2);
                    return SetXmlError(returnXml, "Trigger Error: STOP SHIP RULE FOUND:" + Cadena);
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

        private void SetXmlResultCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "DIAGNOSIS");
        }

        private void SetXmlDefectCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_DEFECTCODES"], "F100");

        }

    }
}
