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
    public class DTVFFTITOVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_ORDERID","/Trigger/Detail/ItemLevel/OrderID"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
            ,{"XML_DIAGCODE","/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode/Name"}

                //To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES","/Trigger/Detail/FailureAnalysis/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            ,{"XML_FA_COMP_LOC","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentLocation"}


            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_PASSWORD","/Trigger/Header/UserObj/PassWord"}
		};

        public override string Name { get; set; }

        public DTVFFTITOVALIDATION()
        {
            this.Name = "DTVFFTITOVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";    //"CRUZE"; //
            string Package_name = "DTVTITOVALIDATION";

            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string bcn;
            int workcenterId;
            int LocationId;
            string FFTitoValue;
            string SN;
            string part;
            string itemId;
           // string resultcode;
            string defectcode = string.Empty;
            string actioncode = string.Empty;
            string Component = string.Empty;
            string geo;
            string client;
            string notename = string.Empty;
            string pnname = string.Empty;


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

            //-- Get WC Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                workcenterId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTERID can not be found.");
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                bcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
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

            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                part = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.");
            }

            //-- Get ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                itemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item Id cannot be empty.");
            }

            //-- Get ResultCode
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            //{
            //    resultcode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Resultcode cannot be empty.");
            //}

            XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
            foreach (XmlNode xn in xnList)
            {
                defectcode = xn["DefectCodeName"].InnerText;

            }


            XmlNodeList xnListAc = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode");
            foreach (XmlNode xn in xnListAc)
            {
                actioncode = xn["ActionCodeName"].InnerText;

            }


            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_LOC"]))
            {
                Component = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_LOC"]).Trim().ToUpper();
            }


            if (Component == "HDD")
            {
                Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
                {
                    {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
                    , {Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
                };

                //Call the DB to get necessary data from Oracle ///////////////
                queries["GeoName"].ParameterValue = LocationId.ToString();
                queries["ClientName"].ParameterValue = clientId.ToString();

                Functions.GetMultipleDbValues(this.ConnectionString, queries);
                geo = queries["GeoName"].Result;
                client = queries["ClientName"].Result;
                notename = "TLHDD";
                pnname = "PNHDD";
                /****************************************** LOGIC START UP ***************************************/

                Functions.DebugOut("-----  TitoFlexFliedValue --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("partn", OracleDbType.Varchar2, part.Length, ParameterDirection.Input) { Value = part  });
                myParams.Add(new OracleParameter("defectcode", OracleDbType.Varchar2, defectcode.Length, ParameterDirection.Input) { Value = defectcode });
                myParams.Add(new OracleParameter("actioncode", OracleDbType.Varchar2, actioncode.Length, ParameterDirection.Input) { Value = actioncode });
                myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, geo.Length, ParameterDirection.Input) { Value = geo });
                myParams.Add(new OracleParameter("notename", OracleDbType.Varchar2, notename.Length, ParameterDirection.Input) { Value = notename });
                myParams.Add(new OracleParameter("pnname", OracleDbType.Varchar2, pnname.Length, ParameterDirection.Input) { Value = pnname });

                FFTitoValue = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetFFTito", myParams);

                //if (resultcode != "FVT")
                //{
                    if (!string.IsNullOrEmpty(FFTitoValue))
                    {
                        if (FFTitoValue.Length <= 0)
                        {
                            return SetXmlError(returnXml, "Unit requires FF TITO_Auto and have an HDD Defect Code. Use 3250/A009 and send to FVT / Unidad Requiere FF TITO_Auto y tiene un Codigo de Defecto de HDD. Direccionarlo usando 3250/A009 y envie a FVT.");
                        }

                        if (FFTitoValue.Equals("NULL"))
                        {
                            return SetXmlError(returnXml, "Unit requires FF TITO_Auto and have an HDD Defect Code. Use 3250/A009 and send to FVT / Unidad Requiere FF TITO_Auto y tiene un Codigo de Defecto de HDD. Direccionarlo usando 3250/A009 y envie a FVT.");
                        }
                        if (FFTitoValue.Contains("NOT FOUND"))
                        {
                            return SetXmlError(returnXml, "Unit requires FF TITO_Auto and have an HDD Defect Code. Use 3250/A009 and send to FVT / Unidad Requiere FF TITO_Auto y tiene un Codigo de Defecto de HDD. Direccionarlo usando 3250/A009 y envie a FVT.");
                        }

                        if (FFTitoValue.Contains("FAIL"))
                        {
                            return SetXmlError(returnXml, "Unit requires FF TITO_Auto and have an HDD Defect Code. Use 3250/A009 and send to FVT / Unidad Requiere FF TITO_Auto y tiene un Codigo de Defecto de HDD. Direccionarlo usando 3250/A009 y envie a FVT.");
                        }
                         
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Unit requires FF TITO_Auto and have an HDD Defect Code. Use 3250/A009 and send to FVT / Unidad Requiere FF TITO_Auto y tiene un Codigo de Defecto de HDD. Direccionarlo usando 3250/A009 y envie a FVT.");
                    }

               // } //end if resultcode
            } //END IF PART

            // Set Return Code to Success
            SetXmlSuccess(returnXml);
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
        /// Update the various fields in the XmlDocument after all the validation is completed.
        /// </summary>
        private void UpdateFields()
        {
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
