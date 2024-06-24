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
using System.Globalization;


namespace JGS.Web.TriggerProviders
{
    public class DTVBLINDRECEIPT : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
              {"XML_LOCATIONID","/Receiving/Header/LocationID"}
            ,{"XML_CLIENTID","/Receiving/Header/ClientID"}
            ,{"XML_CONTRACTID","/Receiving/Header/ContractID"}
            ,{"XML_OPT","/Receiving/Header/OrderProcessType"}
            ,{"XML_OPTID","/Receiving/Header/OrderProcessTypeID"}
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}
            ,{"XML_CLIENTREF1","/Receiving/Detail/Order/ClientRef1"}
            ,{"XML_TRACKINGNO","/Receiving/Detail/Order/TrackingNo"}
            ,{"XML_PARTNO","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_NOTES","/Receiving/Detail/Order/Lines/Line/Notes"}
            ,{"XML_REVISIONLEVEL","/Receiving/Detail/Order/Lines/Line/RevisionLevel"}
            ,{"XML_WAREHOUSE","/Receiving/Detail/Order/Warehouse"} 
            ,{"XML_RESULTCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}
            ,{"XML_WARRANTY","/Receiving/Detail/Order/Lines/Line/Items/Item/Warranty"}
            ,{"XML_SERIALNO","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}
            ,{"XML_FIXEDASSETTAG","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}
            ,{"XML_REASONNOTES","/Receiving/Detail/Order/Lines/Line/Items/Item/ReasonNotes"}
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
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
			,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
            ,{"XML_COMP_FF_VALUE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '{ffName}']/Value"}

        
		};

        public override string Name { get; set; }

        public DTVBLINDRECEIPT()
        {
            this.Name = "DTVBLINDRECEIPT";
        }

        //public static int WeekNumber(DateTime date)
        //{
        //    GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
        //    return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        //}

        public static int getManuYear(String yearCode)
        {
            int yearCd = 2004;
            int i = "4567890123".IndexOf(yearCode);
            if (i == -1)
            {
                return -1;
            }
            int j = 0;
            while (j <= i)
            {
                if (j == i)
                {
                    yearCd += i;
                }
                j++;
            }

            return yearCd;
        }

        public virtual GregorianCalendar date { get; set; }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            // TODO Change schema name
            //DB Call
            string Schema_name = "WEBAPP1";
            string Package_name = "DTV_BR_PROCESS";


            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            #region "Variables"
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string geo;
            string client;
            string contract;
            int LocationId;
            string result;
            int LanguageInd = 0; //0 English, 1 Espanol            
            string opt;
            string btt;
            string strResult;
            string SN;
            string routeId;
            string rid;
            string part=null;
            string warehouse;
            DataSet ds;
            DataSet dsscraphistory;

            string contractname;
            string model;
            string snlength;
            string ridreq;
            string maclength;
            string camidreq;
            string returncondition;
            string result_code="Process Unit";

            string ffName = null;
            string diagnosticCode = null;
            string camid = null;
            string primaryReturnCondition = null;
            string diagnosticstatus = null;
            string strErr;
            string manufCode;
            string manufYear;
            string ecoRev;
            string RecPSH25 = null;
            string ClientRef1 = null;
            string TrackingNo = null;
            DateTime rmaDate;
            DateTime activationDate;
            int daysacttorma=0;
            string rmacompcode;
            string complainCode;
            string rmalineitem=null;
            string rmareasondesc;
            string rmaproblemdesc;
            string username;
            string notes;
            Int32 timesRet=0;
            Int32 dayslastShipped = 0;
            Int32 maxCapturedays;
            DataTable dtResult;
            DataSet dsCD;  //Dataset for cross dock
            string crossdock;
            string resultcode;
            string TechRetInd;
            string actionValue = null;
            DataSet dsTR; //Dataset for Tech Ret
            string rma = null;
            double daysShiptoRMA=0;
            string bounceInd = "NO";
            string bounceSite = null;
            string ResultCode = null;
            string frequentFlyer = null;
            //   DateTime returnDateValue;

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

            ///////////////////////////////////////////////////////////////////////////
            //-- Get Warehouse
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WAREHOUSE"]))
            {
                warehouse = Functions.ExtractValue(xmlIn, _xPaths["XML_WAREHOUSE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Warehouse can not be found.");
            }
            //-- Get Part
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                part = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]);
            }
            //else
            //{
            //    return SetXmlError(returnXml, "Part No can not be found.");
            //}
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTREF1"]))
            {
                ClientRef1 = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTREF1"]);
            }
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRACKINGNO"]))
            {
                TrackingNo = Functions.ExtractValue(xmlIn, _xPaths["XML_TRACKINGNO"]);
            }
            //Extract Flex Field values
            XmlNodeList recList = xmlIn.SelectNodes("/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField");

            foreach (XmlNode recComp in recList)
            {
                ffName = recComp["Name"].InnerText;
                try
                {
                    if (ffName.Equals("CAM ID Number"))
                    {
                        if (recComp["Value"] != null)
                        {
                            camid = recComp["Value"].InnerText;
                        }
                    }
                    if (ffName.Equals("Primary Return Condition Code"))
                    {
                        if (recComp["Value"] != null)
                        {
                            primaryReturnCondition = recComp["Value"].InnerText;
                        }
                    }
                    if (ffName.Equals("Diag Code"))
                    {
                        if (recComp["Value"] != null)
                        {
                            diagnosticCode = recComp["Value"].InnerText;
                        }
                    }
                    if (ffName.Equals("Diag Status"))
                    {
                        if (recComp["Value"] != null)
                        {
                            diagnosticstatus = recComp["Value"].InnerText;
                        }
                    }
                    if (ffName.Equals("Rec_PS_H25"))
                    {
                        if (recComp["Value"] != null)
                        {
                            RecPSH25 = recComp["Value"].InnerText;
                        }
                    }
                }
                catch (Exception ex)
                {
                    strErr = "ERROR: " + ex.Message;
                    // return false;
                }

            }



            Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
            {
                {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
              , {Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
              , {Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
                       };

            //Call the DB to get necessary data from Oracle ///////////////
            queries["GeoName"].ParameterValue = LocationId.ToString();
            queries["ClientName"].ParameterValue = clientId.ToString();
            queries["ContractName"].ParameterValue = contractId.ToString();

            Functions.GetMultipleDbValues(this.ConnectionString, queries);
            geo = queries["GeoName"].Result;
            client = queries["ClientName"].Result;
            contract = queries["ContractName"].Result;


            if (String.IsNullOrEmpty(geo))
            {
                Functions.DebugOut("Geography Name can not be found.");
                return SetXmlError(returnXml, "Geography Name can not be found.");
            }
            if (String.IsNullOrEmpty(client))
            {
                Functions.DebugOut("Client Name can not be found.");
                return SetXmlError(returnXml, "Client Name can not be found.");
            }
            if (String.IsNullOrEmpty(contract))
            {
                Functions.DebugOut("Contract Name can not be found.");
                return SetXmlError(returnXml, "Contract Name can not be found.");
            }

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                result = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
            }

            //-- Get Order Process Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                opt = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT cannot be empty.");
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

            //-- Get FAT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            {
                rid = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Fixed Asset Tag could not be found.");
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

            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                username = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "UserName could not be found.");
            }
            if (string.IsNullOrEmpty(opt) || opt.Length != 0)
            {
                if (opt.Length > 5)
                {
                    return SetXmlError(returnXml, "Order Process Type more than 5 Characters");
                }
            }
            else
            {
                return SetXmlError(returnXml, "Order Process Type is Blank!");
            }
#endregion

            /************************************************** SET LANGUAGE INDICATOR **************************************************/

            Functions.DebugOut("-----  SET LANGUAGE INDICATOR --------> ");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("locationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
            myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParams.Add(new OracleParameter("processName", OracleDbType.Varchar2, "DTVBLINDRECEIPT".ToString().Length, ParameterDirection.Input) { Value = "DTVBLINDRECEIPT" });
            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetLanguage", myParams);
            if (!string.IsNullOrEmpty(strResult))
            {
                LanguageInd = Int32.Parse(strResult);
            }
            else
            {
                LanguageInd = 0;
            }

            /****************************************************************************************************************************/
            #region "ClientRef1 and Tracking No Validation"
            /*Validate Client Ref 1*/
            if (!string.IsNullOrEmpty(ClientRef1))
            {
                if (string.Equals(ClientRef1.Substring(0, 2).ToUpper(), "HB"))
                {
                    if (ClientRef1.Length != 10)
                    {
                        if (LanguageInd == 1) { return SetXmlError(returnXml, "Client Ref No1 debe ser de 10 digitos para HSP o Retailer Return"); }
                        else { return SetXmlError(returnXml, "HSP or Retailer Return the length of Client Ref No 1 must be 10!"); }
                    }
                    if (ClientRef1.Substring(2, 3).ToUpper().Equals("O"))
                    {
                        if (LanguageInd == 1) { return SetXmlError(returnXml, "3rd digito de RMA debe ser numerico 0!"); }
                        else { return SetXmlError(returnXml, "3rd digit of RMA must be numeric 0!"); }
                    }
                }
            }
            /*Validate Tracking No Value*/
            if (!string.IsNullOrEmpty(TrackingNo))
            {
                int j;
                string ch;
                int l = TrackingNo.Trim().Length;
                for (j = 0; j < l; j++)
                {
                    ch = TrackingNo.Substring(0, 1);
                    if (ch.IndexOf("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ") > 0)
                    {
                        if (LanguageInd == 1) { return SetXmlError(returnXml, "Caracter special " + ch + " es invalido en Tracking no"); }
                        else { return SetXmlError(returnXml, "Invalid special character " + ch + " in Tracking number"); }
                    }

                }
            }
            #endregion
            
            #region "ADD Number 1/User input validation"
            #region "RID/FAT Validation"
            //Validation for RID/FAT
            string returnValue;
            string returnSNValue;
            List<OracleParameter> myParamFunc = new List<OracleParameter>();

            myParamFunc.Add(new OracleParameter(":INPUTVALUE", OracleDbType.Varchar2, rid, ParameterDirection.Input));
            returnValue = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "Mod10Validation", myParamFunc);//ODPNETHelper.ExecuteNonQuery(this.ConnectionString, 

            if (!string.IsNullOrEmpty(returnValue))
            {
                if (returnValue.Equals("FALSE"))
                {
                    if (LanguageInd == 1) { return SetXmlError(returnXml, "RID debe ser numerico o fallo validacion Mod10"); }
                    else { return SetXmlError(returnXml, "RID should be numerical or RID failed Mod 10 validation"); }
                }
            }
            /*Validate RID Unique*/
            List<OracleParameter> myParamRid = new List<OracleParameter>();

            myParamRid.Add(new OracleParameter(":RID", OracleDbType.Varchar2, rid, ParameterDirection.Input));
            returnValue = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ValidateFATUnique", myParamRid);//ODPNETHelper.ExecuteNonQuery(this.ConnectionString, 

            if (returnValue.Equals("TRUE"))
            {
                if (LanguageInd == 1) { return SetXmlError(returnXml, "FixedAssetTag " + rid + " ya existe en el inventario"); }
                else { return SetXmlError(returnXml, "FixedAssetTag " + rid + " exits in current inventory!"); }
            }
            #endregion

            #region "SerialNumberValidation"
            //VALIDATE SERIAL NUMBER
            List<OracleParameter> myParamSN = new List<OracleParameter>();

            myParamSN.Add(new OracleParameter("P_SN", OracleDbType.Varchar2, SN, ParameterDirection.Input));
            myParamSN.Add(new OracleParameter("P_PARTNO", OracleDbType.Varchar2, part, ParameterDirection.Input));
            returnSNValue = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "dtv_sn_validation", myParamSN);

            if (returnSNValue != null)
            {
                return SetXmlError(returnXml, returnSNValue);
            }

            ////GET THE DATASET FROM DTV_BR_PROCESS BASED ON CONTRACT AND MODEL

            List<OracleParameter> myParam = new List<OracleParameter>();


            myParam.Add(new OracleParameter(":P_CONTRACT", OracleDbType.Varchar2, contract, ParameterDirection.Input));
            myParam.Add(new OracleParameter(":P_MODEL", OracleDbType.Varchar2, part, ParameterDirection.Input));
            myParam.Add(new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output));

            //TODO Change the schema name
            //TODO In the package, change the table schema name from yuping to custom1
            ds = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.dtv_rec_by_contract_model", myParam.ToArray());
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                contractname = dr["contract"].ToString();
                model = dr["model"].ToString();
                snlength = dr["length_sn"].ToString();
                ridreq = dr["rid"].ToString();
                maclength = dr["length_mac1"].ToString();
                camidreq = dr["cam_id"].ToString();
                returncondition = dr["return_condition"].ToString();
                result_code = dr["result_code"].ToString();
            }
            else
            {
                //TODO Include the schema name in the error message
                return SetXmlError(returnXml, "Entry not found in DTV_REC_CONTRACT_BY_MODEL");
            }
            #endregion

            #region "CamID Validation"
            //Validate CAM ID
            if (!string.IsNullOrEmpty(camid))
            {
                if (camid == rid)
                {
                    if (LanguageInd == 1) { return SetXmlError(returnXml, "RID y CAM ID no pueden ser iguales"); }
                    else { return SetXmlError(returnXml, "RID and CAM ID number can not be the same!"); }
                }
                if (!string.Equals(primaryReturnCondition, "SRC100"))
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC100");
                }
            }
            else
            {
                if (!string.Equals(primaryReturnCondition, "SRC600"))
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC600");
                }
            }


            #endregion
            #region "ScrapHistoryValidation"

            //VALIDATE SCRAP HISTORY TABLE - CHECK TO SEE IF THE UNIT EXISTS IN SCRAP HISTORY

            List<OracleParameter> myParamscrap = new List<OracleParameter>();


            myParamscrap.Add(new OracleParameter(":P_SN", OracleDbType.Varchar2, SN, ParameterDirection.Input));
            myParamscrap.Add(new OracleParameter(":P_CLIENT", OracleDbType.Varchar2, client, ParameterDirection.Input));
            myParamscrap.Add(new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output));

            //TODO Change the schema name
            //TODO In the package, change the table schema name from yuping to custom1
            dsscraphistory = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.dtv_scrap_history", myParamscrap.ToArray());

            if (dsscraphistory.Tables[0].Rows.Count > 0)
            {
                return SetXmlError(returnXml, "Unit found in scrap history table.  Receiving cannot be performed.");
            }
            #endregion
            #region "REC_PS_H25"

            if (part.StartsWith("H25"))
            {

                if (!RecPSH25.ToUpper().Equals("NO PSU"))
                {
                    if (RecPSH25.Length != 14)
                    {
                        return SetXmlError(returnXml, "REC_PS_H25 must be 14 digits long for H25 model");
                    }
                    else //check the 3rd and 4th digit to be 10 or 13
                    {
                        if (!RecPSH25.Substring(2, 2).Equals("10") && !RecPSH25.Substring(2, 2).Equals("13"))
                        {
                            return SetXmlError(returnXml, "The 3rd and 4th character must be either 10 or 13 on REC_PS_H25 flex field");
                        }
                    }

                }
                else if (RecPSH25.ToUpper().Equals("NO PSU"))
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], "The unit came with No PSU");
                }

                else if (part.StartsWith("H25") && RecPSH25 == null)
                {
                    return SetXmlError(returnXml, "REC_PS_H25 Flex Field cannot be null for H25 models");
                }

            }
            #endregion

            #region "ManuCode/Date/ECO Rev"
            DateTime returnDateValue;

            manufCode = part.Substring(part.IndexOf("-") + 1, 3);
            manufYear = SN.Substring(5, 3);
            ecoRev = SN.Substring(4, 1);

            List<OracleParameter> myParamDate = new List<OracleParameter>();

            myParamDate.Add(new OracleParameter("MfYearCode", OracleDbType.Varchar2, manufYear, ParameterDirection.Input));
            myParamDate.Add(new OracleParameter("UserName", OracleDbType.Varchar2, username, ParameterDirection.Input));
            returnDateValue = Convert.ToDateTime(Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetManufDate", myParamDate));

            Functions.UpdateXml(ref returnXml, _xPaths["XML_MANUFDATE"], returnDateValue.ToString("MM-dd-yyyy"));
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MANUFCODE"], manufCode);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_REVISIONLEVEL"], ecoRev);
            #endregion
            #region "In-Transit Validation"
            List<OracleParameter> myParamIntransit = new List<OracleParameter>();
            myParamIntransit.Add(new OracleParameter(":SN", OracleDbType.Varchar2, SN, ParameterDirection.Input));
            returnValue = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "CheckIntransitStatusbySN", myParamIntransit);

            if (returnValue.Equals("TRUE"))
            {
                if (LanguageInd == 1)
                {
                    return SetXmlError(returnXml, "Numero de seria " + SN + "Esta en Transito, no se puede recibir");
                }
                else
                {
                    return SetXmlError(returnXml, "Serial number " + SN + " is in Intransit cannot be receive!");
                }
            }

            #endregion

            #region "auto set from plant ff"
            if (string.Equals(geo.ToUpper(), "JGS MEMPHIS"))
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_FROMPLANT"], "RRR1-MEMPHIS");
            }
            else if (string.Equals(geo.ToUpper(), "CHIHUAHUA"))
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_FROMPLANT"], "RRR3-CUU");
            }
            #endregion
            #endregion

            #region "ADD number 2/Customer Validation"
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
                            myParams.Add(new OracleParameter("c_cursor", OracleDbType.RefCursor, ParameterDirection.Output));
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
                                    if (string.IsNullOrEmpty(ClientRef1) || (ClientRef1.Substring(0, 2).ToUpper().Equals("HB") && rmalineitem.Substring(0, 2).Equals("1-")) || !ClientRef1.Substring(0, 2).ToUpper().Equals("HB"))
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
                                                    
                                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RMADATE"], rmaDate.ToString("MM-dd-yyyy"));
                                                    //Validate if activation date is greater than 1990
                                                    if (!string.IsNullOrEmpty(activationDate.ToString()) && Int32.Parse(activationDate.Year.ToString()) >= 1990)
                                                    {
                                                        Functions.UpdateXml(ref returnXml, _xPaths["XML_ACTIVATIONDATE"], activationDate.ToString("MM-dd-yyyy"));
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
                                                if (!string.IsNullOrEmpty(ClientRef1))
                                                {
                                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], ClientRef1 + ", NM - Not Matched complain rec");
                                                }
                                                else { Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], "NM - Not Matched complain rec"); }
                                            }

                                        }
                                        catch (Exception ex) { return SetXmlError(returnXml, "Er CheckRepTechRet " + ex); }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(ClientRef1))
                                        {   //Validate if clientref1 start with HB
                                            if (ClientRef1.Substring(0, 2).ToUpper().Equals("HB"))
                                            {
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "HSP");
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], ClientRef1 + ", HSP - Installer Return");
                                            }
                                            else
                                            {
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], ClientRef1 + ", NM - Not Matched complain rec");
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
                        if (!string.IsNullOrEmpty(ClientRef1))
                        {
                            if (ClientRef1.Substring(0, 2).ToUpper().Equals("HB"))
                            {
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "HSP");
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], ClientRef1 + ", HSP - Installer Return");
                            }
                            else
                            {
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_COMPLAINTCODE"], "NM");
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], ClientRef1 + ", NM - Not Matched complain rec");
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
            
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FREQUENTFLYER"], "No");
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
            myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
            try
            {
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getTimeReturned", myParams);
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
                            if (!string.IsNullOrEmpty(strResult) && !string.Equals(strResult, "-999"))
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
                                if (!string.IsNullOrEmpty(strResult) && (strResult.Length < 4))
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
                                            if (!string.IsNullOrEmpty(strResult) && (strResult.Length <= 4))
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

            

            #endregion

            #region "SAVE"
            if (rmalineitem != null)
            {
                rma = rmalineitem.ToString();
            }
            else if (ClientRef1 != null)
            {
                rma = ClientRef1.ToString();
            }

            # region "Auto Scrap"
            Functions.DebugOut("-----Auto Scrap------>");
            try
            {

                OracleParameter[] myParamsAuto = new OracleParameter[5];
                myParamsAuto[0] = new OracleParameter("GEO", OracleDbType.Varchar2, ParameterDirection.Input);
                myParamsAuto[0].Value = geo;
                myParamsAuto[1] = new OracleParameter("CLIENT", OracleDbType.Varchar2, ParameterDirection.Input);
                myParamsAuto[1].Value = client;
                myParamsAuto[2] = new OracleParameter("CONTRACT", OracleDbType.Varchar2, ParameterDirection.Input);
                myParamsAuto[2].Value = contract;
                myParamsAuto[3] = new OracleParameter("pn", OracleDbType.Varchar2, ParameterDirection.Input);
                myParamsAuto[3].Value = part;
                myParamsAuto[4] = new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

                //TODO Change schema name
                DataSet dsAutoScrapResult = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.getresultfromscraphistory", myParamsAuto.ToArray());
                if (dsAutoScrapResult.Tables[0].Rows.Count > 0)
                {
                    DataRow drAuto = dsAutoScrapResult.Tables[0].Rows[0];
                    actionValue = drAuto["action"].ToString();
                    if (actionValue.ToUpper().Equals("SCRAP"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], actionValue);
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], actionValue);
                    }
                }
                else //check if part number is WB9000T as it is autoscrap part
                {
                    if (part.ToUpper().Equals("WB9000T"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "TO_FT");
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "TO_FT");
                        return returnXml;
                    }
                    if (part.ToUpper().Equals("MDR1R0-01"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "Process Unit");
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "Process Unit");
                        return returnXml;
                    }
                }
                if (!(actionValue == null || actionValue.Trim().Length == 0))
                {
                    if (actionValue.ToUpper().Equals("SCRAP"))
                    {
                        if (!primaryReturnCondition.ToUpper().Equals("SRC800") || (camid != null && camid.Trim().Length > 0))
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC800");
                            SetXmlSuccess(returnXml);
                            return returnXml;
                        }
                        //else if (!returncondition.ToUpper().Equals("SRC850") || camid == null || camid.Trim().Length == 0)
                        else if (camid == null || camid.Trim().Length == 0)
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC850");
                            SetXmlSuccess(returnXml);
                            return returnXml;
                        }
                    }

                }
                //Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], "Auto Scrap Unit");
                //Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], "Auto Scrap Unit");
                //return returnXml;
            }
            catch (Exception ex)
            {
                return SetXmlError(returnXml, "Err. on Auto Scrap " + ex);
            }
            # endregion
            # region "Cross Dock/Tech Return"
            Functions.DebugOut("-----  CROSS DOCK --------> ");
            try
            {
                List<OracleParameter> myParamCrossDock = new List<OracleParameter>();

                myParamCrossDock.Add(new OracleParameter(":PART", OracleDbType.Varchar2, part, ParameterDirection.Input));
                myParamCrossDock.Add(new OracleParameter(":GEO", OracleDbType.Varchar2, geo, ParameterDirection.Input));
                myParamCrossDock.Add(new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output));

                //TODO Change the schema name

                dsCD = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.getcrossdockresultbypartgeo", myParamCrossDock.ToArray());
                if (dsCD.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsCD.Tables[0].Rows[0];
                    crossdock = dr["cross_dock"].ToString();
                    resultcode = dr["result"].ToString();

                    if (crossdock.Equals("Y"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], resultcode);
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], resultcode);
                    }
                    else if (crossdock.Equals("N"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "Process Unit");
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "Process Unit");
                    }

                    string newresult = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
                    string newreasonnotes = Functions.ExtractValue(xmlIn, _xPaths["XML_REASONNOTES"]).Trim();


                }

            }
            catch (Exception ex)
            {
                return SetXmlError(returnXml, "Err. on CrossDock " + ex);
            }
            # endregion
            # region "TECH RETURN"

            Functions.DebugOut("-----  TECH RETURN --------> ");
            if (!complainCode.ToUpper().Equals("S015")) //Unit is tech return 
            {
                try
                {
                    result = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
                    List<OracleParameter> myParamTechRet = new List<OracleParameter>();

                    myParamTechRet.Add(new OracleParameter(":PART", OracleDbType.Varchar2, part, ParameterDirection.Input));
                    myParamTechRet.Add(new OracleParameter(":GEO", OracleDbType.Varchar2, geo, ParameterDirection.Input));
                    myParamTechRet.Add(new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output));

                    //TODO Change the schema name

                    dsTR = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.getTechRetIndicatorByPartGeo", myParamTechRet.ToArray());
                    if (dsTR.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsTR.Tables[0].Rows[0];
                        TechRetInd = dr["tech_ret_cross"].ToString();
                        resultcode = dr["result"].ToString();

                        /*Check if result code in xml is not capture. Check if tech ret cross ind in table Y  and result is rey_process
                         * or cuu_process and if satisfied, update the xml with new result code.  If tech ret cross ind is N and if
                           result is not Rey_process or Cuu_process (i.e, process_unit), update the xml with new 
                         * result code "Technical_return"*/
                        if (result.ToUpper() != "CAPTURE")
                        {
                            if (TechRetInd.ToString().Equals("Y"))
                            {
                                if (resultcode.ToUpper().Equals("REY_PROCESS"))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "REY_Tech_Ret");
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "REY_Tech_Ret");
                                }
                                else if (resultcode.ToUpper().Equals("CUU_PROCESS"))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "CUU_Tech_Ret");
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "CUU_Tech_Ret");
                                }
                            }
                            else if (TechRetInd.ToString().Equals("N"))
                            {
                                if (resultcode.ToUpper().Equals("REY_PROCESS"))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "REY_Tech_Ret");
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "REY_Tech_Ret");
                                }
                                else if (resultcode.ToUpper().Equals("CUU_PROCESS"))
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "CUU_Tech_Ret");
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "CUU_Tech_Ret");
                                }
                                else
                                {
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "Technical_Return");
                                    Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], "Technical_Return");
                                }

                            }

                        }

                    }
                }

                catch (Exception ex)
                {
                    return SetXmlError(returnXml, "Err. on TechReturn " + ex);
                }
            }
            # endregion


            # region "ECO Validation"
            Functions.DebugOut("----- ECO VALIDATION --------> ");
            try
            {
                string strECOReq = null;
                string strECORes = null;
                List<OracleParameter> myParamsECO = new List<OracleParameter>();
                myParamsECO.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.Length, ParameterDirection.Input) { Value = geo });
                myParamsECO.Add(new OracleParameter("Client", OracleDbType.Varchar2, client.Length, ParameterDirection.Input) { Value = client });
                myParamsECO.Add(new OracleParameter("Contract", OracleDbType.Varchar2, contract.Length, ParameterDirection.Input) { Value = contract });
                myParamsECO.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParamsECO.Add(new OracleParameter("partNo", OracleDbType.Varchar2, part.Length, ParameterDirection.Input) { Value = part });
                myParamsECO.Add(new OracleParameter("ReasonCode", OracleDbType.Varchar2, complainCode.Length, ParameterDirection.Input) { Value = complainCode });
                myParamsECO.Add(new OracleParameter("userName", OracleDbType.Varchar2, username.Length, ParameterDirection.Input) { Value = username });
                myParamsECO.Add(new OracleParameter("ECOreq", OracleDbType.Int32, 100, strECOReq, ParameterDirection.Output));
                myParamsECO.Add(new OracleParameter("ECOResult", OracleDbType.Varchar2, 100, strECORes, ParameterDirection.Output));


                //TODO Change the schema name
                //TODO In the package, change the table schema name from yuping to custom1

                ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVJGSVALIDATIONS.ECOValidationAtReceipt", myParamsECO.ToArray());
                strECOReq = myParamsECO[7].Value.ToString();
                strECORes = myParamsECO[8].Value.ToString();
                //   result_code = null;
                //-- Get result code
                if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                {
                    result_code = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
                }

                if (result_code != null)
                {
                    if (strECOReq.Equals("0"))
                    {
                        if (geo.ToString() == "JGS Memphis" && strECORes.ToString() == "ECO Capture")
                        {//update the xml result code with strECORes
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else if (result_code.ToString() == "REY_Process")
                        {
                            strECORes = "REY_ECO";
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else if (result_code.ToString() == "CUU_Process")
                        {
                            strECORes = "CUU_ECO";
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else if (result_code.ToString() == "REY_Tech_Ret")
                        {
                            strECORes = "REY_ECO_Tech_Ret";
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else if (result_code.ToString() == "CUU_Tech_Ret")
                        {
                            strECORes = "CUU_ECO_Tech_Ret";
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else if (result_code.ToString() == "Technical_Return")
                        {
                            strECORes = "Tech_Ret_ECO";
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                        else
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], strECORes);
                        }
                    }
                }

            }
            //}
            catch (Exception ex)
            {
                return SetXmlError(returnXml, "Err. on ECO " + ex);
            }
            #endregion

            #region "BOUNCE VALIDATION"
            Functions.DebugOut("-----  Bounce Validation Section --------> ");
            /*Unit is consider as bounce if meets below criterias*/
            //1.If complain code is different to S015
            if (!complainCode.ToUpper().Equals("S015"))
            {
                //2.If Times Returned >= 2
                if (timesRet >= 2)
                {
                    /* 3. if RMA date - Activation Date <= 30 days */
                    if (daysacttorma > 0)
                    {
                        if (daysacttorma <= 30)
                        {
                            /*Unit is Tech Return*/
                            bounceInd = "YES";
                        }
                    }
                    else
                    {
                        /*4. daysactivation as null, RMA date - Shipped Date <= 90 days */
                        Functions.DebugOut("-----  FUNCTION VALIDATEBOUNCEBYSHIPDATE --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("PN", OracleDbType.Varchar2, part.Length, ParameterDirection.Input) { Value = part });
                        myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("RIDNum", OracleDbType.Varchar2, rid.Length, ParameterDirection.Input) { Value = rid });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, username.Length, ParameterDirection.Input) { Value = username });
                        try
                        {
                            strResult = Functions.DbFetch(this.ConnectionString, "CUSTOM1", "DTVBounceTechRet", "ValidateBouncebyShipDate", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                daysShiptoRMA = double.Parse(strResult);
                                daysShiptoRMA = Math.Round(daysShiptoRMA);
                            }
                            if (daysShiptoRMA > 0 && daysShiptoRMA <= 90)
                            {
                                bounceInd = "YES";
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_DAYSSHIPTORMA"], daysShiptoRMA.ToString());

                            }
                        }
                        catch (Exception ex) { return SetXmlError(returnXml, "ValidateBouncebyShipDate " + ex); }
                    }
                    if (bounceInd.Equals("YES"))
                    {
                        /*Unit is consider as Bounce Get site name that caused this bounce*/
                        Functions.DebugOut("-----  GET BOUNCE SITE INFO --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("v_client_name", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                        myParams.Add(new OracleParameter("v_serial_no", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
                        try
                        {
                            strResult = Functions.DbFetch(this.ConnectionString, "CUSTOM1", "DTVBounceTechRet", "GetBounceSite", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                bounceSite = strResult;
                                Functions.UpdateXml(ref returnXml, _xPaths["XML_BOUNCESITE"], bounceSite);
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Error on get Bounce Site ");
                            }
                        }
                        catch (Exception ex) { return SetXmlError(returnXml, "Er DTVBounceTechRet " + ex); }
                        /*Update Bounce_Ind on custom1.Technical_return table*/
                        Functions.DebugOut("-----  UpdateBounceTechRet Information --------> ");
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = client });
                        myParams.Add(new OracleParameter("RIDNo", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("ReasonCode", OracleDbType.Varchar2, complainCode.ToString().Length, ParameterDirection.Input) { Value = complainCode });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
                        myParams.Add(new OracleParameter("strResult", OracleDbType.Varchar2, 100, strResult, ParameterDirection.Output));
                        ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVBounceTechRet.UpdateBounceTechRet", myParams.ToArray());
                        strResult = myParams[4].Value.ToString();
                        if (!strResult.Equals("SUCCESS"))
                        {
                            return SetXmlError(returnXml, "UpdateBounceTechRet " + strResult);
                        }

                    }
                }
            }

            #endregion

            #region "CapturebySN"
            Functions.DebugOut("-----  CAPTURE BY SN --------> ");


            OracleParameter[] myParamsSN = new OracleParameter[6];
            myParamsSN[0] = new OracleParameter("GEO", OracleDbType.Varchar2, ParameterDirection.Input);
            myParamsSN[0].Value = geo;
            myParamsSN[1] = new OracleParameter("CLIENT", OracleDbType.Varchar2, ParameterDirection.Input);
            myParamsSN[1].Value = client;
            myParamsSN[2] = new OracleParameter("CONTRACT", OracleDbType.Varchar2, ParameterDirection.Input);
            myParamsSN[2].Value = contract;
            myParamsSN[3] = new OracleParameter("sn", OracleDbType.Varchar2, ParameterDirection.Input);
            myParamsSN[3].Value = SN;
            myParamsSN[4] = new OracleParameter("rid", OracleDbType.Varchar2, ParameterDirection.Input);
            myParamsSN[4].Value = rid;
            myParamsSN[5] = new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            //TODO Change schema name

            try
            {
                DataSet dsCapBySN = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.capturebysn", myParamsSN.ToArray());
                if (dsCapBySN.Tables.Count > 0)
                {
                    if (dsCapBySN.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsCapBySN.Tables[0].Rows[0];
                        string capturenotes = dr["notes"].ToString();
                        //resultcode = dr["result"].ToString();
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "Capture");
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], capturenotes);
                        return returnXml;

                    }
                }
            }
            catch (Exception ex) { return SetXmlError(returnXml, "CaptureBYSN " + ex); }

            #endregion

            #region "CapturebyPCN"
            //-- Get ResultCode
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                ResultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code can not be found.");
            }
            if (!ResultCode.ToUpper().Equals("CAPTURE"))
            {
                Functions.DebugOut("-----  GET BOUNCE SITE INFO --------> ");
                try
                {
                    string PCNNumber = null;
                    string PCNInst = null;
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                    myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                    myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                    myParams.Add(new OracleParameter("BounceInd", OracleDbType.Varchar2, bounceInd.ToString().Length, ParameterDirection.Input) { Value = bounceInd });
                    myParams.Add(new OracleParameter("BounceSite", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bounceSite });
                    myParams.Add(new OracleParameter("ReasonCode", OracleDbType.Varchar2, complainCode.ToString().Length, ParameterDirection.Input) { Value = complainCode });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, username.ToString().Length, ParameterDirection.Input) { Value = username });
                    myParams.Add(new OracleParameter("StrResult", OracleDbType.Varchar2, 100, strResult, ParameterDirection.Output));
                    myParams.Add(new OracleParameter("ResultCode", OracleDbType.Varchar2, 100, ResultCode, ParameterDirection.Output));
                    myParams.Add(new OracleParameter("PCNNum", OracleDbType.Varchar2, 100, PCNNumber, ParameterDirection.Output));
                    myParams.Add(new OracleParameter("InstResult", OracleDbType.Varchar2, 100, PCNInst, ParameterDirection.Output));

                    ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVCAPTURE.CAPTUREMain", myParams.ToArray());
                    strResult = myParams[8].Value.ToString();
                    ResultCode = myParams[9].Value.ToString();
                    PCNNumber = myParams[10].Value.ToString();
                    PCNInst = myParams[11].Value.ToString();
                    if (strResult.Equals("TRUE"))
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], ResultCode);
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_ENGSAMPPCN"], PCNNumber);
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], PCNInst);
                    }
                }
                catch (Exception ex) { return SetXmlError(returnXml, "Er CAPTUREMain " + ex); }
            }

            #endregion

            #region "CapturebyFrequentFlyer"

            //-- Get Frequent Flyer Flex field
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FREQUENTFLYER"]))
            {
                frequentFlyer = Functions.ExtractValue(xmlIn, _xPaths["XML_FREQUENTFLYER"]);
            }
            if (!ResultCode.ToUpper().Equals("CAPTURE"))
            {
                if (frequentFlyer.ToUpper().Equals("YES"))
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "Capture");
                }
            }

            #endregion
            #region "Insert Values into Technical Return Table"
            Functions.DebugOut("----- INSERT INTO TECHNICAL RETURN --------> ");
            try
            {
                //OracleParameter[] myParamsIns = new OracleParameter[5];
                List<OracleParameter> myParamsIns = new List<OracleParameter>();
                myParamsIns.Add(new OracleParameter("p_sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                myParamsIns.Add(new OracleParameter("p_rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParamsIns.Add(new OracleParameter("p_rma", OracleDbType.Varchar2, ParameterDirection.Input) { Value = rma });
                myParamsIns.Add(new OracleParameter("p_reason", OracleDbType.Varchar2, complainCode.ToString().Length, ParameterDirection.Input) { Value = complainCode });
                myParamsIns.Add(new OracleParameter("p_user", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "BlindReceipt" });
                string returnVal = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "insertintotechreturn", myParamsIns);
            }
            catch (Exception ex)
            {
                return SetXmlError(returnXml, "Er Insert Tech Return " + ex);
            }
            #endregion
            #region "Insert Values into DTV_CUST_COMP_DATA Table"
            Functions.DebugOut("----- INSERT INTO DTV_CUST_COMP_DATA --------> ");
            if (complainCode.ToUpper().Equals("NM") || complainCode.ToUpper().Equals("HSP"))
            {
                string reasonDesc;
                if (complainCode.ToUpper().Equals("NM"))
                {
                    reasonDesc = "Unknown Return Reason";
                }
                else
                {
                    reasonDesc = "Installer Return";
                }

                try
                {
                    //OracleParameter[] myParamsIns = new OracleParameter[5];
                    List<OracleParameter> myParamsIns = new List<OracleParameter>();
                    myParamsIns.Add(new OracleParameter("p_sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                    myParamsIns.Add(new OracleParameter("p_rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                    myParamsIns.Add(new OracleParameter("p_rma", OracleDbType.Varchar2, ParameterDirection.Input) { Value = rma });
                    myParamsIns.Add(new OracleParameter("p_reason", OracleDbType.Varchar2, complainCode.ToString().Length, ParameterDirection.Input) { Value = complainCode });
                    myParamsIns.Add(new OracleParameter("p_reasonDesc", OracleDbType.Varchar2, reasonDesc.ToString().Length, ParameterDirection.Input) { Value = reasonDesc });
                    myParamsIns.Add(new OracleParameter("p_user", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "BlindReceipt" });
                    string returnVal = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "insertintocustcomplaint", myParamsIns);
                }
                catch (Exception ex)
                {
                    return SetXmlError(returnXml, "Er Insert Cust Complaint " + ex);
                }
            }
            #endregion


            #endregion

            //// Set Return Code to Success
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
