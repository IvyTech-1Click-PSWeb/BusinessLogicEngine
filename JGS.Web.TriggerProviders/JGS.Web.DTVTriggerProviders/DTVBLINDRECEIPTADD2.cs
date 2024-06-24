using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using System.Web;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Linq;
using JGS.DAL;

namespace JGS.Web.TriggerProviders
{
    public class DTVBLINDRECEIPTADD2 : TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
            {"XML_LOCATIONID","/Receiving/Header/LocationID"}
            ,{"XML_LOCATIONNAME","/Receiving/Header/LocationName"}
            ,{"XML_CLIENTID","/Receiving/Header/ClientID"}
            ,{"XML_CLIENTNAME","/Receiving/Header/ClientName"}
            ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}
            ,{"XML_CONTRACTNAME","/Receiving/Header/ContractName"}
            ,{"XML_OPT","/Receiving/Header/OrderProcessType"}
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}
            ,{"XML_CLIENTREF1","/Receiving/Detail/Order/ClientRef1"}
            ,{"XML_PARTNO","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_SERIALNO","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}
            ,{"XML_FIXEDASSETTAG","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}
            ,{"XML_CAMID","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='CAM ID Number']/Value"}
            ,{"XML_PRYRTNCOND","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Primary Return Condition Code']/Value"}
            ,{"XML_DIAGCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Diag Code']/Value"}
            ,{"XML_DIAGSTATUS","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Diag Status']/Value"}
            ,{"XML_FROMPLANT","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='From Plant']/Value"}
            ,{"XML_BOUNCESITE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BOUNCE_SITE']/Value"}
            ,{"XML_COMPLAINTCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Complaint Code']/Value"}
            ,{"XML_DAYSLASTSHIPPED","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Days Last Shipped']/Value"}
            ,{"XML_DAYSACTTORMA","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Days_Activation_to_RMA']/Value"}
            ,{"XML_DAYSSHIPTORMA","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Days_Shipped_to_RMA']/Value"}
            ,{"XML_FREQUENTFLYER","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='Frequent_Flyer']/Value"}
            ,{"XML_MANUFCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Manufacturer Code']/Value"}
            ,{"XML_MANUFDATE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Manufacturer Date']/Value"}
            ,{"XML_TIMESRETURNED","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Times_Returned']/Value"}
            ,{"XML_RECPSH25","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Rec_PS_H25']/Value"}
            ,{"XML_RMALINEITM","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'RMA_LINE_ITM']/Value"}
            ,{"XML_ENGSAMPPCN","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Eng_Samp_PCN']/Value"}
            ,{"XML_RMADATE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'RMA Date']/Value"}
            ,{"XML_ACTIVATIONDATE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= 'Activation Date']/Value"}
            ,{"XML_NOTES","/Receiving/Detail/Order/Lines/Line/Notes"}
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
			,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
		};
        public override string Name { get; set; }

        public DTVBLINDRECEIPTADD2()
        {
            this.Name = "DTVBLINDRECEIPTADD2";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTV_BR_PROCESS";

            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string geo;
            string client;
            string contract;
            int LocationId;
            string clientref1 = null;
            string result;
            int LanguageInd = 0; //0 English, 1 Espanol 
            string opt;
            string btt;
            string SN;
            string rid;
            string part;
            string diagCode;
            string camid;
            string strResult;
            DateTime rmaDate;
            DateTime activationDate;
            int daysacttorma;
            string rmacompcode;
            string complainCode;
            string rmalineitem;
            string rmareasondesc;
            string rmaproblemdesc;
            string username;
            string notes;

            DataTable dtResult;

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
            }
            //-- Get Location Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONNAME"]))
            {
                geo = Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONNAME"]);
            }
            else
            {
                return SetXmlError(returnXml, "Geography Name can not be found.");
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
            //-- Get Client Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTNAME"]))
            {
                client = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTNAME"]);
            }
            else
            {
                return SetXmlError(returnXml, "Client Name can not be found.");
            }
            //-- Get Contract Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTNAME"]))
            {
                contract = Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTNAME"]);
            }
            else
            {
                return SetXmlError(returnXml, "Contract Name can not be found.");
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
            //-- Get OPT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                opt = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]);
            }
            else
            {
                return SetXmlError(returnXml, "Order Process Type can not be found.");
            }
            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                btt = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "Business transaction type can not be found.");
            }
            //-- Get ClientRef1
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTREF1"]))
            {
                clientref1 = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTREF1"]);
            }
            //-- Get UserName
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                username = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]);
            }
            else
            {
                return SetXmlError(returnXml, "UserName can not be found.");
            }
            //-- Get Partno
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                part = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]);
            }
            else
            {
                return SetXmlError(returnXml, "Part no can not be found.");
            }
            //-- Get serialno
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]);
            }
            else
            {
                return SetXmlError(returnXml, "Serial No. can not be found.");
            }
            //-- Get FixedAssettag
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            {
                rid = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]);
            }
            else
            {
                return SetXmlError(returnXml, "Fixed asset tag can not be found.");
            }

            /************************************************** SET LANGUAGE INDICATOR **************************************************

            Functions.DebugOut("-----  SET LANGUAGE INDICATOR --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("routeId", OracleDbType.Int32, routeId.ToString().Length, ParameterDirection.Input) { Value = routeId });
            myParams.Add(new OracleParameter("workCenterId", OracleDbType.Int32, workcenterId.ToString().Length, ParameterDirection.Input) { Value = workcenterId });
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVHDDSWAPVAL".ToString().Length, ParameterDirection.Input) { Value = "DTVHDDSWAPVAL" });
            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetLanguage", myParams);
            if (!string.IsNullOrEmpty(strResult))
            {
                LanguageInd = Int32.Parse(strResult);
            }
            else
            {
                LanguageInd = 0;
            }

            ****************************************************************************************************************************/
            #region "TechReturnValidation"
            Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
            try
            {

                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "isTechRet", myParams);
                if (!strResult.ToUpper().Equals("TRUE") && !strResult.ToUpper().Equals("FALSE"))
                {
                    return SetXmlError(returnXml, "Er on TecRet Val " + strResult);
                }
                else
                {
                    //Unit is tech return 
                    if (strResult.ToUpper().Equals("TRUE"))
                    {
                        try
                        {
                            //Get customer complain information
                            Functions.DebugOut("-----  GET CUST COMPLAIN DATA --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                            myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                            myParams.Add(new OracleParameter("c_cursor", OracleDbType.RefCursor,ParameterDirection.Output));
                            dtResult = ODPNETHelper.ExecuteDataTable(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.getCustCompData", myParams.ToArray());
                            if (dtResult.Rows.Count > 0)
                            {
                                rmaDate = DateTime.Parse(dtResult.Rows[0]["RMA_DATE"].ToString());
                                rmacompcode = dtResult.Rows[0]["RMA_REASON_CODE"].ToString();
                                rmareasondesc = dtResult.Rows[0]["RMA_REASON_DESCRIPTION"].ToString();
                                rmaproblemdesc = dtResult.Rows[0]["PROBLEM_DESCRIPTION"].ToString();
                                rmalineitem = dtResult.Rows[0]["RMA_LINEITEM"].ToString();
                                activationDate = DateTime.Parse(dtResult.Rows[0]["ACTIVATION_DATE"].ToString());
                                /*Call Function to set notes*/
                                notes = setNotesValue(rmaDate.ToString(), rmacompcode, rmareasondesc, rmaproblemdesc, rmalineitem);
                                if (!string.IsNullOrEmpty(notes))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], notes);
                                }
                                
                                if (!string.IsNullOrEmpty(rmacompcode) && !string.IsNullOrEmpty(rmalineitem))
                                {
                                    //validate if clientrefno1 is null or different that HB only if it start with HB, rmalineitem must be 1-.
                                    if (string.IsNullOrEmpty(clientref1) || (clientref1.Substring(0, 2).ToUpper().Equals("HB") && rmalineitem.Substring(0, 2).Equals("1-")) || !clientref1.Substring(0, 2).ToUpper().Equals("HB"))
                                    {
                                        //Validate Repeat tech return info
                                        try
                                        {
                                            Functions.DebugOut("-----  CHECK REPEATED TECHNICAL RETURN INFO DATA --------> ");
                                            myParams = new List<OracleParameter>();
                                            myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                                            myParams.Add(new OracleParameter("FixedAssetTag", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                                            myParams.Add(new OracleParameter("RMANo", OracleDbType.Varchar2, rmalineitem.Length, ParameterDirection.Input) { Value = rmalineitem });
                                            myParams.Add(new OracleParameter("RMACompCode", OracleDbType.Varchar2, rmacompcode.Length, ParameterDirection.Input) { Value = rmacompcode });
                                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, username.Length, ParameterDirection.Input) { Value = username });
                                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "CheckRepTechRet", myParams);
                                            if (strResult.ToUpper().Equals("FALSE"))
                                            {

                                                //Tech Return info is not repeated ready to set flex field with this information
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], rmacompcode);
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_RMALINEITM"], rmalineitem);
                                                if (!string.IsNullOrEmpty(rmaDate.ToString()))
                                                {
                                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RMADATE"], rmaDate.ToString());
                                                    //Validate if activation date is greater than 1990
                                                    if (!string.IsNullOrEmpty(activationDate.ToString()) && Int32.Parse(activationDate.Year.ToString()) >= 1990)
                                                    {
                                                        Functions.UpdateXml(ref returnXml, _xPaths["XML_ACTIVATIONDATE"], rmacompcode);
                                                        //Get and set Days_Activation_to_RMA
                                                        Functions.DebugOut("-----  GET Days_Activation_to_RMA --------> ");
                                                        myParams = new List<OracleParameter>();
                                                        myParams.Add(new OracleParameter("FixedAssetTag", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                                                        myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                                                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, username.Length, ParameterDirection.Input) { Value = username });
                                                        try
                                                        {
                                                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getDaysActivationToRMA", myParams);
                                                            if (!strResult.Substring(0, 2).Equals("ER"))
                                                            {
                                                                daysacttorma = Int32.Parse(strResult);
                                                                if (daysacttorma != 0)
                                                                {
                                                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_DAYSACTTORMA"], daysacttorma.ToString());
                                                                }
                                                            }
                                                            else { return SetXmlError(returnXml, "Er DB Days_Activation_to_RMA " + strResult); }
                                                        }
                                                        catch (Exception ex) { return SetXmlError(returnXml, "Er Days_Activation_to_RMA " + ex); }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //set NM (Not Matched) complain code
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                                if (!string.IsNullOrEmpty(clientref1))
                                                {
                                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], clientref1 + ", NM - Not Matched complain rec");
                                                }
                                                else { Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"],"NM - Not Matched complain rec"); }
                                            }

                                        }
                                        catch (Exception ex) { return SetXmlError(returnXml, "Er CheckRepTechRet " + ex); }
                                    }
                                    else
                                    {   
                                        if (!string.IsNullOrEmpty(clientref1))
                                        {   //Validate if clientref1 start with HB
                                            if (clientref1.Substring(0, 2).ToUpper().Equals("HB"))
                                            {
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "HSP");
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], clientref1 + ", HSP - Installer Return");
                                            }
                                            else
                                            {
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], clientref1 + ", NM - Not Matched complain rec");
                                            }
                                        }
                                        else
                                        {
                                            //set NM (Not Matched) complain code
                                            Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                            Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], "NM - Not Matched complain rec");
                                        }
                                    }
                                }
                                else
                                {
                                    //go to set NM (Not Matched) complain code
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                }


                            }
                            else
                            {
                                //go to set NM (Not Matched) complain code
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                            }
                        }
                        catch (Exception ex) { return SetXmlError(returnXml, "Er getCustCompData " + ex); }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(clientref1))
                        {
                            if (clientref1.Substring(0, 2).ToUpper().Equals("HB"))
                            {
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "HSP");
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], clientref1 + ", HSP - Installer Return");
                            }
                            else { Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM"); 
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], clientref1 + ", NM - Not Matched complain rec");
                            }
                        }
                        else
                        {
                            //set NM (Not Matched) complain code
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], "NM - Not Matched complain rec");
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                return SetXmlError(returnXml, "Er on isTechRet " + ex);
            }

            //-- Get ComplainCode
            if (!Functions.IsNull(xmlIn, _xPaths["XML_COMPLAINTCODE"]))
            {
                complainCode = Functions.ExtractValue(xmlIn, _xPaths["XML_COMPLAINTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "After CustCompVal ComplainCode can not be null");
            }

            #endregion

            #region "Frequent Flyer"
            /*Unit is FF if 
                1.Complain code must be different than S015 and NM
                2.Times returned >= Max Return (PGA Flex field)
                3.DaysSinceLastShipped <=  maxCaptureDays(PGA Flex field */
            
           /*Get times Returned */
            Int32 timesRet;
            Int32 dayslastShipped =0;
            Int32 maxCapturedays;
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FREQUENTFLYER"], "No");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
            myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username});
            try
            {
                strResult= Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getTimeReturned", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    timesRet = Int32.Parse(strResult);
                    if (timesRet > 0)
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_TIMESRETURNED"], timesRet.ToString());
                        /*Get DaysSinceLastShipped*/
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("V_CLIENT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = client });
                        myParams.Add(new OracleParameter("V_ORDER_PROCESS_TYPE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "OSTKV" });
                        myParams.Add(new OracleParameter("V_ORDER_PROCESS_TYPE_CODE2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "OSTK" });
                        myParams.Add(new OracleParameter("V_SERIAL_NO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = SN });
                        try
                        {
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GET_DAYS_LAST_SHIPPED", myParams); 
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                dayslastShipped = Int32.Parse(strResult);
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_DAYSLASTSHIPPED"], dayslastShipped.ToString());
                            }
                        }
                        catch (Exception ex) { return SetXmlError(returnXml, "Er DaysSinceLastShipped " + ex); }
                        if (!complainCode.Equals("NM") && !complainCode.Equals("S015"))
                        {
                        
                        /*Get Max Return PGA Flex field*/
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.Length, ParameterDirection.Input) { Value = geo });
                        myParams.Add(new OracleParameter("Part", OracleDbType.Varchar2, part.Length, ParameterDirection.Input) { Value = part });
                        myParams.Add(new OracleParameter("PGAFlexField", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "MAX_RETURN" });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = username });
                        try
                        {
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GETPGAflexfield", myParams);
                            if (!string.IsNullOrEmpty(strResult)&& (strResult.Length < 4))
                            {
                                int maxret = Int32.Parse(strResult);
                                if (timesRet >= maxret)
                                {
                                    /*Get MAX_CAPTURE_DAYS PGA Flex field*/
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.Length, ParameterDirection.Input) { Value = geo });
                                    myParams.Add(new OracleParameter("Part", OracleDbType.Varchar2, part.Length, ParameterDirection.Input) { Value = part });
                                    myParams.Add(new OracleParameter("PGAFlexField", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "MAX_CAPTURE_DAYS" });
                                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = username });
                                    try
                                    {
                                        strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GETPGAflexfield", myParams);
                                        if (!string.IsNullOrEmpty(strResult)&&(strResult.Length <= 4))
                                        {
                                            maxCapturedays = Int32.Parse(strResult);
                                            //Validate if unit is frequent flyer
                                            if (maxCapturedays >= dayslastShipped)
                                            {
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_FREQUENTFLYER"], "Yes");
                                            }
                                        }
                                        else { return SetXmlError(returnXml, "Er on MAX_CAPTURE_DAYS PGA ff " + strResult); }
                                    }
                                    catch (Exception ex) { return SetXmlError(returnXml, "Er GETPGAflexfield MaxCaptureDays " + ex); }
                                }

                            }
                            else { return SetXmlError(returnXml, "Er on MaxReturn PGA ff is null or greater than 4 char " + strResult); }
                    }
                    catch (Exception ex) { return SetXmlError(returnXml, "Er GETPGAflexfield MaxReturn " + ex); }
                    }   
                    }
                }
            }
            catch (Exception ex) { return SetXmlError(returnXml, "Er getTimeReturned " + ex); }

            
                
        

            #endregion

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
        /// Set Return XML to Success before validation begin.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }
        private string setNotesValue(string rmaDate, string rmaReasonCd, string rmaReasonDesc, string probDesc, string rmaLineNo)
        {
            string complainNotes = null;
            if (!string.IsNullOrEmpty(rmaDate))
            {
                complainNotes = rmaDate;
            }
            if (!string.IsNullOrEmpty(rmaReasonCd))
            {
                if (!string.IsNullOrEmpty(complainNotes))
                {
                    complainNotes = complainNotes + ". " + rmaReasonCd;
                }
                else { complainNotes = rmaReasonCd; }
            }
            if (!string.IsNullOrEmpty(rmaReasonDesc))
            {
                if (!string.IsNullOrEmpty(complainNotes))
                {
                    complainNotes = complainNotes + ". " + rmaReasonDesc;
                }
                else { complainNotes = rmaReasonDesc; }

            }
            if (!string.IsNullOrEmpty(probDesc))
            {
                if (!string.IsNullOrEmpty(complainNotes))
                {
                    complainNotes = complainNotes + ". " + probDesc;
                }
                else { complainNotes = probDesc; }

            }
            if (!string.IsNullOrEmpty(rmaLineNo))
            {
                if (!string.IsNullOrEmpty(complainNotes))
                {
                    complainNotes = complainNotes + ". " + rmaLineNo;
                }
                else { complainNotes = rmaLineNo; }
            }
            return complainNotes;
        }
    }
}