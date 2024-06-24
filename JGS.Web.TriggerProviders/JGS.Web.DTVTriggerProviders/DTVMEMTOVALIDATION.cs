using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.Web.DTVTriggerProviders;
using JGS.Web.TriggerProviders;

namespace JGS.Web.TriggerProviders
{
    public class DTVMEMTOVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
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
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
		};

        public override string Name { get; set; }


        public DTVMEMTOVALIDATION()  
        {
            this.Name = "DTVMEMTOVALIDATION";
        }
        
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTVMemWildBlueValidation";            
            
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
            string newPart = null;
            string opt;            
            string strResult;
            string SN;
            string routeId;
            string rid;
            string part;
            string itemId;
            string diagCode;
            string Recycled_CAM;            
            
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

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                workcenterId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]));
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTERID can not be found.");
            }
           
            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]);
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
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

            //-- Get FAT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FIXEDASSETTAG"]))
            {
                rid = Functions.ExtractValue(xmlIn, _xPaths["XML_FIXEDASSETTAG"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Fixed Asset Tag could not be found.");
            }

            //-- Get DIAGCODE
            if (!Functions.IsNull(xmlIn, _xPaths["XML_DIAGCODE"]))
            {
                diagCode = Functions.ExtractValue(xmlIn, _xPaths["XML_DIAGCODE"]).Trim();
            }
            else
            {
               // return SetXmlError(returnXml, "Diagnostic Code could not be blank.");
                diagCode = null;
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

                          Functions.DebugOut("----------  Check B2B Variables  -------");
            Functions.DebugOut("GeoName:        " + geo);
            Functions.DebugOut("ClientName:     " + client);
            Functions.DebugOut("ContractName:   " + contract);
            Functions.DebugOut("ResultCode:     " + result);
            Functions.DebugOut("OPT:            " + opt);
            Functions.DebugOut("SN:             " + SN);
            Functions.DebugOut("BCN:            " + bcn);
            Functions.DebugOut("ItemId:         " + itemId);
            Functions.DebugOut("PartNumber:     " + part);
            Functions.DebugOut("workCenterId:   " + workcenterId);
            Functions.DebugOut("WorkCenterName: " + workcenter);
            Functions.DebugOut("--------------------------------");

            /****************************************** LOGIC START UP ***************************************/
            
            if (workcenter.ToUpper().Equals("Pack Out".ToUpper()))
            {
                Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                myParams.Add(new OracleParameter("sn", OracleDbType.Varchar2, SN.ToString().Length, ParameterDirection.Input) { Value = SN });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateTechnicalReturn", myParams);
                if (!string.IsNullOrEmpty(strResult))
                {
                    if (!strResult.ToUpper().Equals("TRUE") && !strResult.ToUpper().Equals("FALSE"))
                    {
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        if (strResult.ToUpper().Equals("TRUE"))
                        {
                            // need to inactive record in technicalReturntable
                            Functions.DebugOut("-----  TECHNICAL RETURN VALIDATION --------> ");
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("rid", OracleDbType.Varchar2, rid.ToString().Length, ParameterDirection.Input) { Value = rid });
                            strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "inactivateTechnicalreturn", myParams);
                            if (!string.IsNullOrEmpty(strResult))
                            {
                                return SetXmlError(returnXml, strResult);
                            }
                        }
                    }
                }
             
                Functions.DebugOut("-----  checkPGAFFRecycledCAM --------> ");
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                myParams.Add(new OracleParameter("LocationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                myParams.Add(new OracleParameter("LanguageInd", OracleDbType.Int32, LanguageInd.ToString().Length, ParameterDirection.Input) { Value = LanguageInd });
                strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "checkPGAFFRecycledCAM", myParams);
                
                if (strResult.IndexOf("[RECYCLED_CAM]").ToString() == "" || strResult.IndexOf("[RECYCLED_CAM]").ToString() == null)
                {
                    return SetXmlError(returnXml, strResult);
                }
                else if (strResult.IndexOf("[RECYCLED_CAM]") >= 0)
                {
                    return SetXmlError(returnXml, strResult);
                }
                else
                {
                    Recycled_CAM = strResult;
                }

                if (Recycled_CAM.ToUpper().Equals("NO"))
                {
                    Functions.DebugOut("-----  getNewPartNo --------> ");
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("part", OracleDbType.Varchar2, part.ToString().Length, ParameterDirection.Input) { Value = part });
                    newPart = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getNewPartNo", myParams);
                    if (string.IsNullOrEmpty(newPart))
                    {
                        if (LanguageInd == 1)
                        {
                            strResult = "Parte " + part + " no esta en la tabla change_part_ref!";
                        }
                        else
                        {
                            strResult = "Part " + part + " is not in the change_part_ref table!";
                        }
                        return SetXmlError(returnXml, strResult);
                    }
                    else
                    {
                        // CALL THE CHANGE PART API
                        string response;
                        Functions.DebugOut("-----  Call Change Part Wrapper --------> ");
                        try
                        {
                            JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper CPobj = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartWrapper();
                            JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo cpi = new JGS.Web.DTVTriggerProviders.ChangePart.ChangePartInfo();
                            cpi.SesCustomerID = "1";
                            cpi.RequestId = "1";
                            cpi.BCN = bcn;
                            cpi.NewPartNo = newPart;
                            cpi.MustBeOnHold = false;
                            cpi.ReleaseIfHold = false;
                            cpi.MustBeTimedIn = false;
                            cpi.TimedInWorkCenterName = " ";
                            cpi.userName = UserName;
                            response = CPobj.PerformChangePart(cpi, false);
                        }
                        catch (Exception ex)
                        {
                            response = ex.ToString();
                        }
                        if (!response.ToUpper().Equals("SUCCESS"))
                        {
                            return SetXmlError(returnXml, "Change Part Error: [" + response + "]");
                        }
                    }
                }
                        
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

        private String getWCFFValue(XmlDocument XmlIn, string flexFieldName)
        {
            XmlNodeList nodes;
            string ffValue = null;
            nodes = XmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes.Item(i).FirstChild.InnerXml.ToUpper().Equals(flexFieldName.ToUpper()))
                {
                    ffValue = nodes.Item(i).LastChild.InnerXml.ToString();
                    break;
                }
            }
            return ffValue;
        }
    }
}
