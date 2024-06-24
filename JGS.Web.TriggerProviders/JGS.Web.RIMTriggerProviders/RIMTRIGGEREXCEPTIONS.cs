using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using System.Xml;


namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGEREXCEPTIONS : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
           ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
           ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
           ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
           ,{"XML_LOCATIONID","/Trigger/Header/LocationID"}
           ,{"XML_CLIENTID","/Trigger/Header/ClientID"}
           ,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
           ,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode"}
		   ,{"XML_FA_ACTIONCODES","/Trigger/Detail/FailureAnalysis/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
           ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
           ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
           ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
           ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}  
           ,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
           ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
        };


        public override string Name { get; set; }

        public RIMTRIGGEREXCEPTIONS()
        {
            this.Name = "RIMTRIGGEREXCEPTIONS";
        }

        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            System.Xml.XmlDocument returnXml = xmlIn;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string ItemId = string.Empty;
            string FFBounce = string.Empty;
            string ResulCode = string.Empty;
            string GetStatus = string.Empty;
            string UserName = string.Empty;
            string resultCode = string.Empty;
            string SerialHistory;
            string[] SerHistory;
            string TRXID = string.Empty;
            string ActualWC = string.Empty;
            string NextWC = string.Empty;
            string WC = string.Empty;
            string EXCEPTION_DESC = string.Empty;
            string CountTRX = string.Empty;
            string Function = string.Empty;
            int i;
            int LocationId;
            int clientId;
            string OrderProcessType = string.Empty;
            string DefCod = string.Empty;
            string ActCod = string.Empty;
            string Comp = string.Empty;
            string FAChanges = string.Empty;
            string TriggerType = string.Empty;
            string LastTRX = string.Empty;
            List<OracleParameter> myParams;
            List<OracleParameter> myParams1;
            List<OracleParameter> myParams2;
            string FlagEnc = string.Empty;
            string resFFMsgID = string.Empty;
            string resFF = string.Empty;
            string SNF = string.Empty;
            string FF = string.Empty;
            string BCN = string.Empty;
            


            ////////////////////////////// Get XML Values ///////////////////////////////////////////////////           
            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Item ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "ItemId can not be found.");
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

            //-- Get Work Center
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                ActualWC = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "WorkCenter can not be found.");
            }
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
     
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
            //-- Get OrderProcessType
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OrderProcessType = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "OPT could not be found.");
            }

            //-- Get Defect Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_DEFECTCODES"]))
            {
                DefCod = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODES"]).Trim();
            }
               
            //-- Get Action Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_ACTIONCODES"]))
            {
                ActCod = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_ACTIONCODES"]).Trim();
            }
     
            //-- Get Component
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_PN"]))
            {
                Comp = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_PN"]).Trim();
            }

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                TriggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Trigger Type can not be found.");
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                   SNF = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
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

            FF = "FF_B2B_FLAG";
            //////////////////// Get the Service Level /////////////////////
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams2.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNF.Length, ParameterDirection.Input) { Value = SNF });//new parameter
            myParams2.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFF = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams2);

            FF = "MsgID";
            //////////////////// Get the Service Level /////////////////////
            myParams1 = new List<OracleParameter>();
            myParams1.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams1.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNF.Length, ParameterDirection.Input) { Value = SNF });//new parameter
            myParams1.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFFMsgID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams1);

            if (resFFMsgID != null)
            {
                FlagEnc = getSHA1Hash(resFFMsgID);
            }
            if (resFF == FlagEnc & resFFMsgID != null & resFF != null)
            {

                //if (TriggerType == "FAILUREANALYSIS")
                //{
                    ///////////////////////////Start the validation///////////////////////
                    //////////////////// Get the Last TRXID /////////////////////
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, ItemId.Length, ParameterDirection.Input) { Value = ItemId });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    LastTRX = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", "GetLastTRXID", myParams);

                    if (LastTRX != null)
                    {
                        //////////////////// Get the Exception /////////////////////
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("TRXID", OracleDbType.Varchar2, LastTRX.Length, ParameterDirection.Input) { Value = LastTRX });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        EXCEPTION_DESC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", "CheckIfExistEXCEPTION", myParams);

                        if (EXCEPTION_DESC != null)
                        {
                            return SetXmlError(returnXml, "This Item has an Exception: " + EXCEPTION_DESC + " please return to the last WC.");
                        }
                    }
                //}
                
                //if (TriggerType == "TIMEOUT")
                //{
 

                //    TRXID = null;
                //    CountTRX = GetTRXIDFunctions(ItemId, UserName, TRXID, "CountTRXIDs");

                //    for (i = 0; i < Convert.ToInt16(CountTRX); i++)
                //    {
                //        if (i == 0)
                //        {
                //            Function = "GetTRXID";
                //            TRXID = null;
                //        }
                //        else
                //        {
                //            Function = "GetNEXTTRXID";
                //        }

                //        SerialHistory = GetTRXIDFunctions(ItemId, UserName, TRXID, Function);

                //        if (SerialHistory != null )
                //        {
                //            SerHistory = SerialHistory.Split('|');
                //            TRXID = SerHistory[0];
                //            WC = SerHistory[1];
                //        }
                //        else
                //        {
                //            TRXID = null;
                //            WC = null;
                //        }
 

                //        if (TRXID != null)
                //        {

                //            //////////////////// Get the Exception /////////////////////
                //            myParams = new List<OracleParameter>();
                //            myParams.Add(new OracleParameter("TRXID", OracleDbType.Varchar2, TRXID.Length, ParameterDirection.Input) { Value = TRXID });
                //            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //            EXCEPTION_DESC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", "CheckIfExistEXCEPTION", myParams);

                //            if (EXCEPTION_DESC != null)
                //            {
                //                //////////////////// Get the Next WC /////////////////////
                //                myParams = new List<OracleParameter>();
                //                myParams.Add(new OracleParameter("WCID", OracleDbType.Varchar2, TRXID.Length, ParameterDirection.Input) { Value = TRXID });
                //                myParams.Add(new OracleParameter("LocationID", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                //                myParams.Add(new OracleParameter("ClientID", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                //                myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, OrderProcessType.Length, ParameterDirection.Input) { Value = OrderProcessType });
                //                myParams.Add(new OracleParameter("resultCode", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode });
                //                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //                NextWC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", "GetNextWC", myParams);


                //                if (NextWC != null)
                //                {
                //                    return SetXmlError(returnXml, "Destination WC not found.");
                //                }
                //                else
                //                {
                //                    if (NextWC != WC)
                //                    {
                //                        //////////////////// Get the FA Changes /////////////////////
                //                        myParams = new List<OracleParameter>();
                //                        myParams.Add(new OracleParameter("DefCod", OracleDbType.Varchar2, DefCod.Length, ParameterDirection.Input) { Value = DefCod });
                //                        myParams.Add(new OracleParameter("ActCod", OracleDbType.Varchar2, ActCod.Length, ParameterDirection.Input) { Value = ActCod });
                //                        myParams.Add(new OracleParameter("Comp", OracleDbType.Varchar2, Comp.Length, ParameterDirection.Input) { Value = Comp });
                //                        myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, ItemId.Length, ParameterDirection.Input) { Value = ItemId });
                //                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //                        FAChanges = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", "CheckFAValues", myParams);

                //                        if (FAChanges == "false")
                //                        {
                //                            return SetXmlError(returnXml, "This Item must be send to " + WC + ".");
                //                        } //if


                //                    }//if                            
                //                }//else         
                //            } //if
                //        }//if
                //    }  //for  
                //}     



            }//Flag            
            
            return returnXml;

        }

        private string GetTRXIDFunctions(string Iid, string User, string TRXID, string Function)
        {
            List<OracleParameter> myParams;
            string Result = string.Empty;
        
            //////////////////// Get the TRXID and WC /////////////////////
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("ItemId", OracleDbType.Varchar2, Iid.Length, ParameterDirection.Input) { Value = Iid });
            if (TRXID != null) 
            {
                myParams.Add(new OracleParameter("TNextRXID", OracleDbType.Varchar2, TRXID.Length, ParameterDirection.Input) { Value = TRXID }); 
            }
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            Result = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONS", Function, myParams);

            return Result;

        }
      

        private void SetXmlSuccess(System.Xml.XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }


        private XmlDocument SetXmlError(System.Xml.XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        public string getSHA1Hash(string input)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider SHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] data = SHA1Hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(input));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            int i = 0;
            for (i = 0; i <= data.Length - 1; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }   
    }
}
