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
    public class DTVBLINDRECEIPTSAVE :  TriggerProviderBase
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
            ,{"XML_BCN","/Receiving/Detail/Order/Lines/Line/Items/Item/BCN"}
            ,{"XML_FIXEDASSETTAG","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}
            ,{"XML_REASONCODE","/Receiving/Detail/Order/Lines/Line/Items/Item/ReasonCode"}
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

        public DTVBLINDRECEIPTSAVE()
        {
            this.Name = "DTVBLINDRECEIPTSAVE";
        }
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string Schema_name = "WEBAPP1";
            string Package_name = "DTV_BR_PROCESS";


            //////////////////// Parameters List /////////////////////
           List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int clientId;
            int contractId;
            string geo;
            string client;
            string bcn = null;
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
            string returncondition =null;
            string result_code = null;
            string reason_code = null;

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
            string resultvalue;
            DataSet dsCD;  //Dataset for cross dock
            string crossdock;
            string resultcode;
            string TechRetInd;
            string actionValue = null;

            int intECOReq;

            DataSet dsTR; //Dataset for Tech Ret
            string clientref1 = null;
            string btt;
            string diagCode;
            DateTime rmaDate;
            DateTime activationDate;
            int daysacttorma = 0;
            string rmacompcode;
            string complainCode;
            //string rmalineitem;
            string rmareasondesc;
            string rmaproblemdesc;
            string username;
            string notes;
            string bounceInd = "NO";
            string bounceSite = null;
            int timesRet = 0;
            int daysShiptoRMA;
            string ResultCode = null;
            string frequentFlyer = null;
            string rmalineitem = null;
            string rma = null;


          //   DateTime returnDateValue;

            ////-- Get Location Id
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            //{
            //    LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Geography Id can not be found.");
            //}

            ////-- Get Client Id
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            //{
            //    clientId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]));
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Client Id can not be found.");
            //}

            ////-- Get Contract Id
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            //{
            //    contractId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Contract Id can not be found.");
            //}



            ////-- Get Warehouse
            //if (!Functions.IsNull(xmlIn, _xPaths["XML_WAREHOUSE"]))
            //{
            //    warehouse= Functions.ExtractValue(xmlIn, _xPaths["XML_WAREHOUSE"]);
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Warehouse can not be found.");
            //}
            ////-- Get Part
            // if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            //{
            //    part = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]);
            //}
            //else
            //{
            //    return SetXmlError(returnXml, "Part No can not be found.");
            //}
       
            ///////////////////////////////////////////////////////////////////////

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
            //--Get BCN
             if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
             {
                 bcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]);
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

            //--Get UserName
             if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
             {
                 UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
             }
             else
             {
                 return SetXmlError(returnXml, "UserName could not be found.");
             }
             //--Get complain code
             if (!Functions.IsNull(xmlIn, _xPaths["XML_COMPLAINTCODE"]))
             {
                 complainCode = Functions.ExtractValue(xmlIn, _xPaths["XML_COMPLAINTCODE"]);
             }
             else
             {
                 return SetXmlError(returnXml, "complain code must not be as null at this point.");
             }
             //--Get Times Returned
             if (!Functions.IsNull(xmlIn, _xPaths["XML_TIMESRETURNED"]))
             {
                 timesRet = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_TIMESRETURNED"]));
             }
             //--Get days between activation and rma creation
             if (!Functions.IsNull(xmlIn, _xPaths["XML_DAYSACTTORMA"]))
             {
                 daysacttorma = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_DAYSACTTORMA"]));
             }
             //--Get notes
             if (!Functions.IsNull(xmlIn, _xPaths["XML_NOTES"]))
             {
                 notes = Functions.ExtractValue(xmlIn, _xPaths["XML_NOTES"]);
             }
             //-- Get Frequent Flyer Flex field
             if (!Functions.IsNull(xmlIn, _xPaths["XML_FREQUENTFLYER"]))
             {
                 frequentFlyer = Functions.ExtractValue(xmlIn, _xPaths["XML_FREQUENTFLYER"]);
             }
             else
             {
                 return SetXmlError(returnXml, "Frequent Flyer FlexField can't be null at this point.");
             }
             //-- Get Bounce Site
             if (!Functions.IsNull(xmlIn, _xPaths["XML_BOUNCESITE"]))
             {
                 bounceSite = Functions.ExtractValue(xmlIn, _xPaths["XML_BOUNCESITE"]);
             }
             //else
             //{
             //    return SetXmlError(returnXml, "Bounce Site can't be null at this point.");
             //}

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
                     if (ffName.Equals("RMA_LINE_ITM"))
                     {
                         if (recComp["Value"] != null)
                         {
                            rmalineitem = recComp["Value"].InnerText;
                         }
                     }

                 }
                 catch (Exception ex)
                 {
                    strErr = "ERROR: " + ex.Message;
                    // return false;
                 }

             }

             if (rmalineitem != null)
             {
                 rma = rmalineitem.ToString();
             }
             else if (clientref1 != null)
             {
                 rma = clientref1.ToString();
             }
             else
             {
                 rma = bcn.ToString();
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
                   myParamsAuto[4] =  new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

                 //TODO Change schema name
                  DataSet dsAutoScrapResult= ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WEBAPP1.DTV_BR_PROCESS.getresultfromscraphisotry", myParamsAuto.ToArray());
                  if (dsAutoScrapResult.Tables[0].Rows.Count > 0)
                  {
                      DataRow drAuto = dsAutoScrapResult.Tables[0].Rows[0];
                      actionValue = drAuto["action"].ToString();
                      if (actionValue.ToUpper().Equals("SCRAP"))
                      {
                          Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], actionValue);
                          Functions.UpdateXml(ref returnXml, _xPaths["XML_REASONNOTES"], actionValue);
                          return returnXml;
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
                      if(part.ToUpper().Equals("MDR1R0-01"))
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
                          if (!primaryReturnCondition.ToUpper().Equals("SRC800")||(camid!=null && camid.Trim().Length>0))
                          {
                              Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC800");
                              return returnXml;
                          }
                          else if(!returncondition.ToUpper().Equals("SRC850")||camid==null||camid.Trim().Length==0)
                          {
                              Functions.UpdateXml(ref returnXml, _xPaths["XML_PRYRTNCOND"], "SRC850");
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

              OracleParameter[] myParamsECO = new OracleParameter[9];
              myParamsECO[0] = new OracleParameter("GEO", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[0].Value = geo;
              myParamsECO[1] = new OracleParameter("CLIENT", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[1].Value = client;
              myParamsECO[2] = new OracleParameter("CONTRACT", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[2].Value = contract;
              myParamsECO[3] = new OracleParameter("SERIALNO", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[3].Value = SN;
              myParamsECO[4] = new OracleParameter("PARTNO", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[4].Value = part;
              myParamsECO[5] = new OracleParameter("REASONCODE", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[5].Value = reason_code;
              myParamsECO[6] = new OracleParameter("USERNAME", OracleDbType.Varchar2, ParameterDirection.Input);
              myParamsECO[6].Value = UserName;
              myParamsECO[7] = new OracleParameter("P_ECOREQ", OracleDbType.Int32, ParameterDirection.Output);
              myParamsECO[8] = new OracleParameter("P_ECORES", OracleDbType.Varchar2, ParameterDirection.Output);


              //TODO Change the schema name
              //TODO In the package, change the table schema name from yuping to custom1

              ODPNETHelper.ExecuteScalar(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVJGSVALIDATIONS.ECOValidationAtReceipt", myParamsECO);
              string strECOReq = Convert.ToString(myParamsECO[7].Value);
              string strECORes = myParamsECO[8].Value.ToString();
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

            
            
            
            /****************************************************************************************/

                      
            #region "BOUNCE VALIDATION"
            Functions.DebugOut("-----  Bounce Validation Section --------> ");
            /*Unit is consider as bounce if meets below criterias*/
            //1.If complain code is different to S015
            if(!complainCode.ToUpper().Equals("S015"))
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
                        myParams.Add(new OracleParameter("PN", OracleDbType.Varchar2, client.ToString().Length, ParameterDirection.Input) { Value = part });
                        myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("RIDNum", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = rid });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
                            try
                            {
                                daysShiptoRMA = Int32.Parse(Functions.DbFetch(this.ConnectionString, "Custom1", "DTVBounceTechRet", "ValidateBouncebyShipDate", myParams));
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
                            strResult = Functions.DbFetch(this.ConnectionString, "Custom1", "DTVBounceTechRet", "GetBounceSite", myParams);
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
                        myParams.Add(new OracleParameter("ReasonCode", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username });
                        myParams.Add(new OracleParameter("strResult", OracleDbType.Varchar2, ParameterDirection.Output));
                        ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVBounceTechRet.UpdateBounceTechRet", myParams.ToArray());
                        strResult = myParams[4].Value.ToString();
                        if (strResult.Equals("SUCCESS"))
                        {
                            return SetXmlError(returnXml, "UpdateBounceTechRet "+strResult);
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
                       DataSet dsCapBySN = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "WERBAPP1.DTV_BR_PROCESS.capturebysn", myParamsSN.ToArray());
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
                //RA Change - Added if statement
                if (bounceSite!=null)
                {
                    Functions.DebugOut("-----  GET BOUNCE SITE INFO --------> ");
                    try
                    {
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("Geo", OracleDbType.Varchar2, geo.ToString().Length, ParameterDirection.Input) { Value = geo });
                        myParams.Add(new OracleParameter("Serial", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                        myParams.Add(new OracleParameter("RID", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                        myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                        myParams.Add(new OracleParameter("BounceInd", OracleDbType.Varchar2, bounceInd.ToString().Length, ParameterDirection.Input) { Value = bounceInd });
                        myParams.Add(new OracleParameter("BounceSite", OracleDbType.Varchar2, bounceSite.Length, ParameterDirection.Input) { Value = bounceSite });
                        myParams.Add(new OracleParameter("ReasonCode", OracleDbType.Varchar2, complainCode.ToString().Length, ParameterDirection.Input) { Value = complainCode });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, username.ToString().Length, ParameterDirection.Input) { Value = username });
                        myParams.Add(new OracleParameter("StrResult", OracleDbType.Varchar2, ParameterDirection.Output));
                        myParams.Add(new OracleParameter("ResultCode", OracleDbType.Varchar2, ParameterDirection.Output));
                        myParams.Add(new OracleParameter("PCNNum", OracleDbType.Varchar2, ParameterDirection.Output));
                        myParams.Add(new OracleParameter("InstResult", OracleDbType.Varchar2, ParameterDirection.Output));

                        ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, "CUSTOM1.DTVCAPTURE.CAPTUREMain", myParams.ToArray());
                        strResult = myParams[8].Value.ToString();
                        ResultCode = myParams[9].Value.ToString();
                        string PCNNumber = myParams[10].Value.ToString();
                        string PCNInst = myParams[11].Value.ToString();
                        if (strResult.Equals("TRUE"))
                        {
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], ResultCode);
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_ENGSAMPPCN"], PCNNumber);
                            Functions.UpdateXml(ref returnXml, _xPaths["XML_NOTES"], PCNInst);
                        }
                    }
                    catch (Exception ex) { return SetXmlError(returnXml, "Er CAPTUREMain " + ex); }
                }
            }

            #endregion

            #region "CapturebyFrequentFlyer"
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
              myParamsIns.Add(new OracleParameter("p_rma", OracleDbType.Varchar2, rma.ToString().Length, ParameterDirection.Input) { Value = rma });
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
                  myParamsIns.Add(new OracleParameter("p_rma", OracleDbType.Varchar2, rma.ToString().Length, ParameterDirection.Input) { Value = rma });
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


            // Set Return Code to Success
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