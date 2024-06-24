using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using Oracle.DataAccess.Types;
using JGS.DAL;
//using System.Web.Configuration;
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders
{
    public class HTCTRIGGERVMIMOBO : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            //FA Fields
			,{"XML_FA_FLEXFIELDS","/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/FAFlexFieldList"}
            ,{"XML_MEID DEC VMI","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/FAFlexFieldList/FlexField[Name='MEID DEC VMI']/Value"} 
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

        public HTCTRIGGERVMIMOBO()
        {
            this.Name = "HTCTRIGGERVMIMOBO";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            //string valor = string.Empty;
            string NumComp = string.Empty;
            string DataEntry_RC = string.Empty;
            string CalSerLev = string.Empty;
            string Trigger = string.Empty;
            int ItemID;
            string SN;
            string UserName;
            string BCN;
            string WC;
            string OPT;
            string RC;
            string PartNo = string.Empty;
            int clientId;
            int contractId;
            int WCid;
            int LocationID;
            string InvalidComp = string.Empty;
            string Msg = string.Empty;
            string FFName;
            string FFValue;
            string IMEI = string.Empty;
            string IMEI2 = string.Empty;
            string SERIAL = string.Empty;
            string MSG2 = string.Empty;
            string PART = string.Empty;
            string Parte = string.Empty;
            string DefectCodeName = string.Empty;
            string ActionCodeName = string.Empty;
            string VALOR = string.Empty;
            string VALORDEC = string.Empty;
            string VALORHEX = string.Empty;
            string VALORDECVMI = string.Empty;
            string VALORHEXVMI = string.Empty;
            int i = 0;
            // Set Return Code to Success
            SetXmlSuccess(returnXml);
            //-- Get Location ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }
            //-- Get Item ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                PartNo = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number can not be found.");
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

            //-- Get OPT Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "OPT can not be found.");
            }



            //-- Get WC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                WC = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "WC can not be found.");
            }

            //-- Get WCID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WCid = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WC ID can not be found.");
            }

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                Trigger = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
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


            


            if (Trigger.ToUpper() == "FAILUREANALYSIS")
            {
                FFName = "MEID_HEX";
                List<OracleParameter> myParams2;
                myParams2 = new List<OracleParameter>();
                myParams2.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams2.Add(new OracleParameter("ContractId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams2.Add(new OracleParameter("FFNAME", OracleDbType.Varchar2, FFName.ToString().Length, ParameterDirection.Input) { Value = FFName });
                myParams2.Add(new OracleParameter("LocationId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = LocationID});
                myParams2.Add(new OracleParameter("ITEMID", OracleDbType.Varchar2, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                VALOR = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCVMI", "GETRETFAFF", myParams2);
                if ((VALOR != "OK") && (VALOR.ToUpper().Length==14))
                {
                    VALORHEX = VALOR.ToUpper();
                }
                else
                {
                    Msg = "El Valor MEID HEX VMI es longuitud incorrecto o nulo!";
                    return SetXmlError(returnXml, Msg);
                }
                FFName = "MEID_DEC";
                List<OracleParameter> myParams;
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams.Add(new OracleParameter("FFNAME", OracleDbType.Varchar2, FFName.ToString().Length, ParameterDirection.Input) { Value = FFName });
                myParams.Add(new OracleParameter("LocationId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                myParams.Add(new OracleParameter("ITEMID", OracleDbType.Varchar2, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                VALOR = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCVMI", "GETRETFAFF", myParams);
                if ((VALOR != "OK") && (VALOR.ToUpper().Length == 18))
                {
                    VALORDEC = VALOR.ToUpper();
                }
                else
                {
                    Msg = "El Valor MEID DEC es de longuitud incorrecta!";
                    return SetXmlError(returnXml, Msg);
                }

                XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/FAFlexFieldList/FlexField");

                foreach (XmlNode xn in xnList)
                {
                    FFName = xn["Name"].InnerText;
                    if (FFName == "MEID HEX VMI")
                    {
                        FFValue = xn["Value"].InnerText;
                        VALORHEXVMI = FFValue.ToUpper();
                        if (VALORHEXVMI.ToString().Length!=14)
                        {
                            Msg = "El Valor MEID HEX VMI es de longuitud incorrecta";
                            return SetXmlError(returnXml, Msg);
                        }
                        if (VALORHEX == VALORHEXVMI)
                        {
                            return SetXmlError(returnXml, "El valor de MEID HEX es igual con el que se recibió, Favor de Corregir");
                        }
                    }
                }
                List<OracleParameter> myParams1;
                myParams1 = new List<OracleParameter>();
                myParams1.Add(new OracleParameter("MEID_HEX_IN", OracleDbType.Varchar2, VALORHEXVMI.Length, ParameterDirection.Input) { Value = VALORHEXVMI });
                myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                VALOR = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCVMI", "MEIDHEXTODEC", myParams1);
                if (VALOR != "OK")
                {
                    VALORDECVMI = VALOR;
                }
                else
                {
                  Msg = "El Valor MEID DEC es incorrecto o nulo o no es de la longitud!";
                  return SetXmlError(returnXml, Msg);
                }
                Functions.UpdateXml(ref returnXml, _xPaths["XML_MEID DEC VMI"], VALORDECVMI);
            }
            return returnXml;
        }

        /// <summary>
        /// Set the Result to EXECUTION_ERROR and the Message to the specified message
        /// </summary>
        /// <param name="returnXml">The XmlDocument to update</param>
        /// <param name="message">The error message to set</param>
        /// <returns>The modified XmlDocument</returns>
        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        /// <summary>
        /// Set Return XML to Success before validation begin.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }

    }
}
