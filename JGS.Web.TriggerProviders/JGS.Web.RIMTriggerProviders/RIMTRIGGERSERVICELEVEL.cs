using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using System.Security.Cryptography;


namespace JGS.Web.TriggerProviders
{
  
    public class RIMTRIGGERSERVICELEVEL : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_SERVICE_LEVEL","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='SERVICE_LEVEL']/Value"}
            ,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
            ,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
    		,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_CLIENTID","/Trigger/Header/ClientID"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
        
	    };


        public override string Name { get; set; }

        public RIMTRIGGERSERVICELEVEL()
        {
            this.Name = "RIMTRIGGERSERVICELEVEL";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string Itemid = string.Empty;
            string UserName = string.Empty;
            int LocationId;
            List<OracleParameter> myParams;
            List<OracleParameter> myParams1;
            List<OracleParameter> myParams2;
            string workcenterName;
            string resultCode;
            string Service_Level = string.Empty;
            string BCN = string.Empty;
            string WCId;
            int clientId;
            int contractId;
            string res = string.Empty;
            string resDC = string.Empty;
            string resFF = string.Empty;
            string OrderProcessType = string.Empty;
            string DEFCODE = string.Empty;
            string FlagEnc = string.Empty;
            string SN = string.Empty;
            string FF = string.Empty;
            string qty = string.Empty;
            string GetStatus = string.Empty;
            string SL = string.Empty;
            string resFFMsgID = string.Empty;

 
            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
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

            //-- Get ItemID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                Itemid = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
            }

            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
            }
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERVICE_LEVEL"]))
            {
                Service_Level = Functions.ExtractValue(xmlIn, _xPaths["XML_SERVICE_LEVEL"]).Trim().ToUpper();
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

            //-- Get WCID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WCId = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTERID can not be found.");
            }

            //-- Get OPTID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OrderProcessType = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Order Process Type can not be found.");
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





            if (workcenterName == "Debug-Rew" & resultCode == "SwapAzul")
            {
               // Functions.UpdateXml(ref returnXml, _xPaths["XML_SERVICE_LEVEL"], "INSP");
                res = "INSP";
            }

            else
            {
                SL = null;
                //////////////////// Get the Service Level /////////////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParams.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams.Add(new OracleParameter("CONTRACTID", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams.Add(new OracleParameter("ACTUALWCNAME", OracleDbType.Varchar2, workcenterName.Length, ParameterDirection.Input) { Value = workcenterName });//new parameter
                myParams.Add(new OracleParameter("WCId", OracleDbType.Varchar2, WCId.Length, ParameterDirection.Input) { Value = WCId });//new parameter
                myParams.Add(new OracleParameter("ACTUALRC", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode });//new parameter
                myParams.Add(new OracleParameter("Itemid", OracleDbType.Varchar2, Itemid.Length, ParameterDirection.Input) { Value = Itemid });//new parameter
                myParams.Add(new OracleParameter("OrderProcessType", OracleDbType.Varchar2, OrderProcessType.Length, ParameterDirection.Input) { Value = OrderProcessType });//new parameter
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
                res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams);
 
            }

  
            if (workcenterName == "Debug-Rew" )
                {
                    if (resultCode == "SWAP_RED")
                    {
                        DEFCODE = "F0005_D020_01";

                        //////////////////// Get the Service Level /////////////////////
                        myParams1 = new List<OracleParameter>();
                        myParams1.Add(new OracleParameter("Itemid", OracleDbType.Varchar2, Itemid.Length, ParameterDirection.Input) { Value = Itemid });//new parameter
                        myParams1.Add(new OracleParameter("DEFCODE", OracleDbType.Varchar2, DEFCODE.Length, ParameterDirection.Input) { Value = DEFCODE });//new parameter
                        myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        resDC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDEFCODE", "GETDEFCODE", myParams1);

                        if (resDC != null)
                        {
                            if (res != "0")
                            {
                                if (Service_Level != "0")
                                {
                                    return SetXmlError(returnXml, "Debe seleccionar un Repair Level 0 /Must be select a Repair Level 0");
                                }
                                res = "0";
                            }

                        }
                        else
                        {
                            if (res != "3B")
                            {
                                if (Service_Level != "3B")
                                {
                                    return SetXmlError(returnXml, "Debe seleccionar un Repair Level 3B /Must be select a Repair Level 3B");
                                }
                                res = "3B";
                            }
                        }
                    }                 
                } //WC Debug---

                if (workcenterName == "BER")
                {
                    if (resultCode == "BER-UR")
                    {
                        if (res != "0")
                        {
                            if (Service_Level != "0")
                            {
                                return SetXmlError(returnXml, "Debe seleccionar Service Level 0 / Must select Service Level 0");
                            }
                            //////////////////// Get que Qyt of components /////////////////////
                            myParams2 = new List<OracleParameter>();
                            myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            qty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSTRIGGERBERUR", "GETQTYOFCOMPONENTS", myParams2);

                            if (qty != null)
                            {
                                if (Convert.ToInt16(qty) > 0)
                                {
                                    return SetXmlError(returnXml, "Debe remover componentes para este Result code/Please remove the components for this Result Code");
                                }
                            }
                            
                            res = "0";
                        }//Res Armando
                        else
                        {
                            //////////////////// Get que Qyt of components /////////////////////
                            myParams2 = new List<OracleParameter>();
                            myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            qty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSTRIGGERBERUR", "GETQTYOFCOMPONENTS", myParams2);

                            if (qty != null)
                            {
                                if (Convert.ToInt16(qty) > 0)
                                {
                                    return SetXmlError(returnXml, "Debe remover componentes para este Result code/Please remove the components for this Result Code");
                                }
                            }
                        }

                        //checar que lleve al menos un codigo BER
                    }//BER-UR
                }//WC BER---

                //DOA--


                FF = "FF_B2B_FLAG";

                //////////////////// Get the Service Level /////////////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
                myParams.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
                resFF = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams);

                FF = "MsgID";

                //////////////////// Get the Service Level /////////////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
                myParams.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
                resFFMsgID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams);


                if (resFFMsgID != null)
                {
                    FlagEnc = getSHA1Hash(resFFMsgID);
                }
                if (resFF == FlagEnc & resFFMsgID != null & resFF != null)
                {
                    FF = "FF_OReason";

                    ////////////////////Get Status of the RO /////////////////////
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.ToString().Length, ParameterDirection.Input) { Value = BCN });
                    myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    GetStatus = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams);


                    if (GetStatus != null)
                    {
                        if (GetStatus == "Dead On Arrival")
                        {                            
                            if (res == "0" || res == "1")
                            {
                                SL = "DOA1";
                            }
                            if (res == "2")
                            {
                                SL = "DOA2";
                            }
                            if (res == "3" || res == "4" || res == "3B" || res == "2PLS")
                            {
                                SL = "DOA3";
                            }

                            res = SL;
                        }

                    }
                    //DOA
                }//Filter                


                    if (res != null)
                    {
                        Functions.UpdateXml(ref returnXml, _xPaths["XML_SERVICE_LEVEL"], res);
                    }
                    else
                    {
                        if (Service_Level == null || Service_Level == "")
                        {
                            return SetXmlError(returnXml, "Debe seleccionar un Repair Level/Must be select a Repair Level");
                        }
                        //else
                        //{
                        //    return SetXmlError(returnXml, "Service Level Could not be Calculate|Nivel de Servicio no puede ser calculado");
                        //}                       
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
