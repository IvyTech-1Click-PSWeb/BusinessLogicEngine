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
   public class DTVBLINDRECEIPTADD1 : JGS.Web.TriggerProviders.TriggerProviderBase
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
            ,{"XML_PARTNO","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_NOTES","/Receiving/Detail/Order/Lines/Line/Notes"}
            ,{"XML_REVISIONLEVEL","/Receiving/Detail/Order/Lines/Line/RevisionLevel"}
            ,{"XML_WAREHOUSE","/Receiving/Detail/Order/Warehouse"} 
            ,{"XML_RESULTCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}
            ,{"XML_WARRANTY","/Receiving/Detail/Order/Lines/Line/Items/Item/Warranty"}
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
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
			,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
            ,{"XML_COMP_FF_VALUE","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name= '{ffName}']/Value"}

        
		};

       public override string Name { get; set; }

       public DTVBLINDRECEIPTADD1()
        {
            this.Name = "DTVBLINDRECEIPTADD1";
        }

       //public static int WeekNumber(DateTime date)
       //{
       //    GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
       //    return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
       //}

       public static int getManuYear( String yearCode) {
		int yearCd = 2004;
		int i = "4567890123".IndexOf( yearCode );
		if ( i == -1 ) {
            return -1;
		}
		int j = 0;
		while ( j <= i ) {
			if ( j == i ) {
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

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string geo;
            string client;
            string bcn;
            string contract;
            string workcenter;
            int workcenterId;
            int LocationId;
            string result;
            int LanguageInd = 0; //0 English, 1 Espanol            
            string UserName;
            string Password;
            string opt;
            string strResult;
            string SN;
            string routeId;
            string rid;
            string part;
            string partid;
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
            string result_code;

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
                warehouse= Functions.ExtractValue(xmlIn, _xPaths["XML_WAREHOUSE"]);
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
            else
            {
                return SetXmlError(returnXml, "Part No can not be found.");
            }

            //Extract Flex Field values
             StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
             XmlNodeList recList = xmlIn.SelectNodes("/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField");

             foreach (XmlNode recComp in recList)
             {
                 //string ffName = null;
                 //string diagnosticCode = null;
                 //string camid = null;
                 //string primaryReturnCondition = null;
                 //string diagnosticstatus = null;
                 //string strErr;
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

            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "UserName could not be found.");
            }
            //-- Get Password
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_PASSWORD"]))
            //{
            //    Password = Functions.ExtractValue(xmlIn, _xPaths["XML_PASSWORD"]).Trim();
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Password could not be found.");
            //}
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
            #region "RID/FAT Validation"
            //Validation for RID/FAT
            int returnValue;
            string returnSNValue;
           List<OracleParameter> myParamFunc = new List<OracleParameter>();

           myParamFunc.Add(new OracleParameter(":INPUTVALUE", OracleDbType.Varchar2, rid, ParameterDirection.Input));
           returnValue = Convert.ToInt32(Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "Mod10Validation", myParamFunc));//ODPNETHelper.ExecuteNonQuery(this.ConnectionString, 

            if(returnValue.Equals(-999))
            {
                return SetXmlError(returnXml, "RID/Flowtag should be numerical");
            }
            if (returnValue.Equals(2))
            {
                return SetXmlError(returnXml, "RID/Flowtag failed Mod 10 validation");
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
            if (camidreq != null)   //Checking the table to see if the column is populated
            {
                if (camidreq != "N") //Check to see the value of camidreq field in the table
                {
                    if (camid == null) //if camid in the xml is null
                    {
                        return SetXmlError(returnXml, "Cam ID is required and cannot be null");
                    }
                }
            }
            else
            {
                return SetXmlError(returnXml, "Please Update the table.  Cam Id column is null.");
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

            if (part.StartsWith("H25") && RecPSH25!= null)
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
            myParamDate.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName, ParameterDirection.Input));
            returnDateValue =  Convert.ToDateTime(Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "GetManufDate", myParamDate));

            Functions.UpdateXml(ref returnXml, _xPaths["XML_MANUFDATE"], Convert.ToString(returnDateValue));
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MANUFCODE"], manufCode);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_REVISIONLEVEL"], ecoRev);
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

    }
}
