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
    public class DTVHDDSWAPWRPVAL : JGS.Web.TriggerProviders.TriggerProviderBase
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
                //To get defect codes
			,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode"}
			,{"XML_FA_ACTIONCODES","/Trigger/Detail/FailureAnalysis/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            //
            //,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            ,{"XML_FA_COMP_LOC","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentLocation"}

            ,{"XML_ORDERID","/Trigger/Detail/ItemLevel/OrderID"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
            ,{"XML_DIAGCODE","/Trigger/Detail/TimeOut/DiagnosticCodeList/DiagnosticCode/Name"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_PASSWORD","/Trigger/Header/UserObj/PassWord"}
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}
		};

        public override string Name { get; set; }

        public DTVHDDSWAPWRPVAL()
        {
            this.Name = "DTVHDDSWAPWRPVAL";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            /*Update on 7/10/2012 by Mayra I. Adding validation of TITO Automation test before replace a HDD
              two condition needs to meet to allow the change 
              1. WC that failed the unit used TITO Automation program (fill an specific flex field TITO_Auto
              2. Defect that captured that wc put an Hdd swap defect based on defect by part number in custom1.validate_result_by_defect*/
            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTVHDDSwapVal";

            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string bcn;
            int workcenterId;
            int LocationId;
            string routeId;
            string LooperValue;
            string SN;
            string part;
            string itemId;
            string geo;
            string client;
            string notename = "TLHDD";
            string defectcode = string.Empty;
            string TITOVal = string.Empty;
            string actioncode = string.Empty;
            string Component = string.Empty;
            string compname = "COMP";

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

            //-- Get Order Process Type ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPTID"]))
            {
                routeId = Functions.ExtractValue(xmlIn, _xPaths["XML_OPTID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT ID cannot be empty.");
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
            XmlNodeList xnListNewcomp = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component");
            foreach (XmlNode xncomp in xnListNewcomp)
            {
                Component = xncomp["ComponentLocation"].InnerText;
                if ((Component == "HDD" || Component == "CH06800" || Component == "10215" || Component == "0070" || Component == "AY1") && actioncode == "A012")
                {
                    /************************** TITO Auto Val Trigger***************************************/
                    Functions.DebugOut("----- ValTITOAutoTest Function --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                    myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, itemId.Length, ParameterDirection.Input) { Value = itemId });

                    TITOVal = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ValTITOAutoTest", myParams);

                    if (!string.IsNullOrEmpty(TITOVal))
                    {
                        if (!string.Equals(TITOVal, "TRUE"))
                        {
                            return SetXmlError(returnXml, TITOVal);
                        }
                    }
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


                    /****************************************** LOGIC START UP ***************************************/
                    Functions.DebugOut("-----  Looper HDD --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, itemId.Length, ParameterDirection.Input) { Value = itemId });
                    myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("routeId", OracleDbType.Int32, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
                    myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
                    myParams.Add(new OracleParameter("defectcode", OracleDbType.Varchar2, defectcode.Length, ParameterDirection.Input) { Value = defectcode });
                    myParams.Add(new OracleParameter("actioncode", OracleDbType.Varchar2, actioncode.Length, ParameterDirection.Input) { Value = actioncode });
                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, geo.Length, ParameterDirection.Input) { Value = geo });
                    myParams.Add(new OracleParameter("notename", OracleDbType.Varchar2, notename.Length, ParameterDirection.Input) { Value = notename });
                    myParams.Add(new OracleParameter("compname", OracleDbType.Varchar2, compname.Length, ParameterDirection.Input) { Value = compname });

                    LooperValue = Functions.DbFetch(this.ConnectionString, Schema_name, "DTVLOOPERHD", "GetLooper", myParams);

                    if (!string.IsNullOrEmpty(LooperValue))
                    {
                        if (LooperValue.Length <= 0)
                        {
                            //return SetXmlError(returnXml, "This Unit already had an HDD replaced, please contact your supervisor to get approval for this replacement/ Esta Unidad actualmente se le cambio un HDD, favor verificar con supervisor para autorizacion del cambio");
                            return SetXmlError(returnXml, LooperValue);
                        }

                        if (LooperValue.Equals("NULL"))
                        {
                            //return SetXmlError(returnXml, "This Unit already had an HDD replaced, please contact your supervisor to get approval for this replacement/ Esta Unidad actualmente se le cambio un HDD, favor verificar con supervisor para autorizacion del cambio");
                            return SetXmlError(returnXml, LooperValue);
                        }

                        string valor = string.Empty;
                        valor = LooperValue.Substring(0, 1);

                        if (valor == "E" || valor == "T")
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], "Glb_Engineering");
                            return SetXmlError(returnXml, LooperValue);
                        }

                    }
                    else
                    {
                        // return SetXmlError(returnXml, "This Unit already had an HDD replaced, please contact your supervisor to get approval for this replacement/ Esta Unidad actualmente se le cambio un HDD, favor verificar con supervisor para autorizacion del cambio");
                        return SetXmlError(returnXml, LooperValue);
                    }

                }  // end if component
            }

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


        private XmlDocument SetXmlRole(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLNAME"], message);
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

