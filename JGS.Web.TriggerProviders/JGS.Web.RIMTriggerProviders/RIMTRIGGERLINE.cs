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
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERLINE : JGS.Web.TriggerProviders.TriggerProviderBase
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
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_PNCHANGEPART","/Trigger/Detail/ChangePart/NewPartNo"}
            ,{"XML_SNCHANGEPART","/Trigger/Detail/ChangePart/NewSerialNo"}
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

        public RIMTRIGGERLINE()
        {
            this.Name = "RIMTRIGGERLINE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string ChangePartDone = string.Empty;
            string CalSerLev = string.Empty;
            string Trigger = string.Empty;
            string SN = string.Empty;
            string UserName = string.Empty;
            string BCN = string.Empty;
            string WC = string.Empty;
            string OPT = string.Empty;
            string RC = string.Empty;
            string PartNo = string.Empty;
            string InvalidComp = string.Empty;
            string ChangePart = string.Empty;
            string SNChangePart = string.Empty;
            string MathChangePart = string.Empty;
            string FFName = string.Empty;
            string FFLineValue = string.Empty;
            string ValorFF = string.Empty;
            int ItemID;
            int clientId;
            int contractId;
            int LocationID;
            int OPTId;
            int WCId;
            string PartId = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Loc ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Location Id can not be found.");
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


            //-- Get OPT Id 
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPTID"]))
            {
                OPTId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_OPTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "OPT Id can not be found.");
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

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                RC = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            }
            else
            {
                RC = "";
                //return SetXmlError(returnXml, "Result Code can not be found.");
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

            //-- Get WC Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WCId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WC can not be found.");
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

            //-- Get Change Part No
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PNCHANGEPART"]))
            {
                ChangePart = Functions.ExtractValue(xmlIn, _xPaths["XML_PNCHANGEPART"]).Trim().ToUpper();
            }
            else
            {
                ChangePart = "";
                //return SetXmlError(returnXml, "Part Number Change Part can not be found.");
            }

            //-- Get Change Part SN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SNCHANGEPART"]))
            {
                SNChangePart = Functions.ExtractValue(xmlIn, _xPaths["XML_SNCHANGEPART"]).Trim().ToUpper();
            }
            else
            {
                SNChangePart = "";
                //                return SetXmlError(returnXml, "Serial Number Change Part can not be found.");
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

            // Validaciones para Change Part

            FFName = "LINE_NO";
            // Validaciones para Time out Failure Analysis
            if (Trigger.ToUpper() == "TIMEOUT")
            {

                    XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
                    foreach (XmlNode xn in xnList)
                    {
                        if (xn["Name"].InnerText.ToUpper() == "LINE_NO")
                            if (xn["Value"].InnerText != String.Empty && xn["Value"].InnerText != null)
                            {
                                ValorFF = xn["Value"].InnerText;
                            }
                    }
                    if (ValorFF != string.Empty && ValorFF.ToString().Length>1)
                    {
                        // Validar si en Xelus existe un change part
                        // en caso de no existir enviar error
                        List<OracleParameter> myParams;
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("PARTNO", OracleDbType.Varchar2, PartNo.ToString().Length, ParameterDirection.Input) { Value = PartNo });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        PartId = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSHTCDIAGNOSIS", "GETPARTID", myParams);
                        List<OracleParameter> myParams2;
                        myParams2 = new List<OracleParameter>();
                        myParams2.Add(new OracleParameter("FFNAME", OracleDbType.Varchar2, FFName.ToString().Length, ParameterDirection.Input) { Value = FFName });
                        myParams2.Add(new OracleParameter("LOCATIONID", OracleDbType.Varchar2, LocationID.ToString().Length, ParameterDirection.Input) { Value = LocationID });
                        myParams2.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                        myParams2.Add(new OracleParameter("PARTID", OracleDbType.Varchar2, PartId.ToString().Length, ParameterDirection.Input) { Value = PartId });
                        myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        FFLineValue = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMFFLINE", "GETFFPGA", myParams2);
                        if (FFLineValue == null)
                        {
                            return SetXmlError(returnXml, "Trigger Error: Este FF " + FFName + " no tiene valor");
                        }
                        int Busca =FFLineValue.IndexOf(ValorFF);
                        if (Busca<0)
                        {
                            return SetXmlError(returnXml, "Trigger Error: El valor que ingreso en el FF " + FFName + " no es igual que el de nivel PGA debe ser " + FFLineValue);
                        }
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Trigger Error: Debe capturar valor en FF or el valor no esta en el Nivel" + FFName );
                    }

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

        /// <summary>
        /// Set Return XML to change Result code according validation.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlResultCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "DIAGNOSIS");
        }

        /// <summary>
        /// Set Return XML to assing defect code according validation.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlDefectCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_DEFECTCODES"], "F100");

        }

    }
}
