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
    public class RIMTRIGGERCHANGEPART : JGS.Web.TriggerProviders.TriggerProviderBase    
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

        public RIMTRIGGERCHANGEPART()
        {
            this.Name = "RIMTRIGGERCHANGEPART";
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

            int ItemID;
            int clientId;
            int contractId;
            int LocId;
            int OPTId;
            int WCId;

            string FF = string.Empty;
            string resFF = string.Empty;
            string FlagEnc = string.Empty;
            string resFFMsgID = string.Empty;       

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Loc ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
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
            if (Trigger.ToUpper() == "CHANGEPART")
            {
                
                // Validando WC Ope_IN
                if (WC.ToUpper() == "OPE_IN")
                {
                    if (OPT.ToUpper() == "WRP")
                    {

                     FF = "FF_B2B_FLAG";
                    //////////////////// Get the Service Level /////////////////////
                    List<OracleParameter> myParams2;
                    myParams2 = new List<OracleParameter>();
                    myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                    myParams2.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
                    myParams2.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                    myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
                    resFF = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams2);

                    FF = "MsgID";
                    //////////////////// Get the Service Level /////////////////////
                    List<OracleParameter> myParams1;
                    myParams1 = new List<OracleParameter>();
                    myParams1.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                    myParams1.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
                    myParams1.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                    myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
                    resFFMsgID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams1);

                    if (resFFMsgID != null)
                    {
                        FlagEnc = getSHA1Hash(resFFMsgID);
                    }
                    if (resFF == FlagEnc & resFFMsgID != null & resFF != null)
                    {

                        string FF_RepPRD = string.Empty;

                        // Validar que el número de parte coincida con el del flexfield FF_RepPRD...

                        List<OracleParameter> myParams;
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                        myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                        myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                        myParams.Add(new OracleParameter("FF_RepPRD", OracleDbType.Varchar2, ChangePart.Length, ParameterDirection.Input) { Value = "FF_RepPRD" });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //FF_RepPRD = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMCHANGEPART", "getff_repprd", myParams);
                        //FF_RepPRD = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMCHANGEPART", "getff_repprd", myParams);
                        FF_RepPRD = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMCHANGEPART", "getff_repprd", myParams);

                        if (FF_RepPRD == null || FF_RepPRD == "NULO")
                        {
                            return SetXmlError(returnXml, "Trigger Error: FF_RepPRD null");
                        }
                        else
                        {
                            if (FF_RepPRD.ToUpper() != ChangePart.ToUpper())
                            {
                                return SetXmlError(returnXml, "Trigger Error: Part Number " + ChangePart + " no corresponde con FF_RepPRD " + FF_RepPRD);
                            }
                        }
                    }                    
                 }
               }
                else
                {
                    // WC LABEL o ZAUD
                    if (WC.ToUpper() == "LABEL" || WC.ToUpper() == "ZAUD")
                    {
                        // Solo estas rutas pueden hacer change part
                        if (OPT.ToUpper() == "ZWRP" || OPT.ToUpper() == "ZRICK")
                        {
                            // Si se intenta cambiar numero de parte
                            if (ChangePart.ToUpper() != "")
                            {
                                // El numero de parte debe iniciar con PRD
                                //if (ChangePart.Substring(0, 3).ToUpper() != "PRD")
                                //{
                                //    return SetXmlError(returnXml, "Trigger Error: El número de parte debe iniciar con PRD.");
                                //}

                                // El numero de parte debe de contener 13 caracteres
                                //if (ChangePart.Length != 13)
                                //{
                                //    return SetXmlError(returnXml, "Trigger Error: El número de parte debe contener 13 dígitos.");
                                //}

                                // Solo la ruta ZWRP puede hacer cambio de numero de serie
                                if (OPT.ToUpper() != "ZWRP" && SNChangePart != "")
                                {
                                    return SetXmlError(returnXml, "Trigger Error: Change Serial number solo puede ser realizado en la ruta ZWRP, la ruta actual es " + OPT + ".");
                                }

                                // Debe ingresarse New SN para la ruta ZWRP si se cambio Numero de parte
                                //if (OPT.ToUpper() == "ZWRP" && SNChangePart == "")
                                //{
                                //    return SetXmlError(returnXml, "Trigger Error: Debe ingresar un número de serie en la sección de Serial Number.");
                                //}
                                //else 
                                if (OPT.ToUpper() == "ZWRP" && SNChangePart != "")
                                {

                                    // Validar que el número de parte coincida con el de la tabla...

                                    List<OracleParameter> myParams;
                                    myParams = new List<OracleParameter>();                                    
                                    myParams.Add(new OracleParameter("OldPN", OracleDbType.Varchar2, PartNo.ToString().Length, ParameterDirection.Input) { Value = PartNo });
                                    myParams.Add(new OracleParameter("NewPN", OracleDbType.Varchar2, ChangePart.Length, ParameterDirection.Input) { Value = ChangePart });
                                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    //MathChangePart = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMCHANGEPART", "ChangePartRef", myParams);
                                    //MathChangePart = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMCHANGEPART", "ChangePartRef", myParams);
                                    MathChangePart = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMCHANGEPART", "ChangePartRef", myParams);

                                    if (MathChangePart == null)
                                    {
                                        return SetXmlError(returnXml, "Trigger Error: No es posible cambiar el Part Number " + ChangePart + " no corresponde con el No. de " + PartNo);
                                    }

                                    // Validaciones de nuevo número de serie
                                    if (SNChangePart.Substring(0, 2).ToUpper() == "35")
                                    {
                                        if (SNChangePart.Length != 15)
                                        {
                                            return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 35, este debe de contener 15 dígitos.");
                                        }
                                    }
                                    else if (SNChangePart.Substring(0, 2).ToUpper() == "A0")
                                    {
                                        if (SNChangePart.Length != 14)
                                        {
                                            return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con A0, este debe de contener 14 dígitos.");
                                        }
                                    }
                                    else if (SNChangePart.Substring(0, 2).ToUpper() == "07")
                                    {
                                        if (SNChangePart.Length != 11)
                                        {
                                            return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 07, este debe de contener 11 dígitos.");
                                        }
                                    }
                                    else if (SNChangePart.Substring(0, 2).ToUpper() == "26")
                                    {
                                        if (SNChangePart.Length != 18)
                                        {
                                            return SetXmlError(returnXml, "Trigger Error: El número de serie comienza con 26, este debe de contener 18 dígitos.");
                                        }
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Trigger Error: Ingresó un número de serie no válido. Los números de serie deben de empezar con 35, 07, A0 o 26.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Si la ruta es diferente a ZWRP o ZRICK
                            return SetXmlError(returnXml, "Trigger Error: No se puede realizar Change Part en la ruta " + OPT + ". Este proceso solo puede ser realizado en la ruta ZWRP y en la ruta ZRICK.");
                        }
                    }
                    else
                    {
                        // Si WC es diferente de LABEL o ZAUD regresa error
                        return SetXmlError(returnXml, "Trigger Error- No se puede realizar Change Part en el WC de " + WC + ", este proceso solo es valido para los WC de ZAUD y LABEL.");
                    }  
                }
            }

            // Validaciones para Time out Failure Analysis
            if (Trigger.ToUpper() == "FAILUREANALYSIS" || Trigger.ToUpper() == "TIMEOUT")
            {
                if (PartNo.Substring(0, 3).ToUpper() == "ASY")
                {
                    if (WC.ToUpper().Trim() == "LABEL")
                    {
                        if (RC.ToUpper().Trim() == "PASS" || RC.ToUpper().Trim() == "GSM/CDMA" || RC.ToUpper().Trim() == "CSC_PASS")
                        {
                            // Validar si en Xelus existe un change part
                            // en caso de no existir enviar error

                            List<OracleParameter> myParams;
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                            myParams.Add(new OracleParameter("WCName", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = WC });
                            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = LocId });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            //ChangePartDone = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);
                            //ChangePartDone = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);
                            ChangePartDone = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);

                            if (ChangePartDone == null)
                            {
                                return SetXmlError(returnXml, "Trigger Error: Para este result code " + RC + " debe realizar change part");
                            }
                        }
                    }
                    else if (WC.ToUpper().Trim() == "ZAUD")
                    {
                        if (RC.ToUpper().Trim() == "REPAIRED" )
                        {
                            // Validar si en Xelus existe un change part
                            // en caso de no existir enviar error
                                                        
                            List<OracleParameter> myParams;
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                            myParams.Add(new OracleParameter("WCName", OracleDbType.Varchar2, WC.Length, ParameterDirection.Input) { Value = WC });
                            myParams.Add(new OracleParameter("LocId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = LocId });
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            //ChangePartDone = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);
                            //ChangePartDone = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);
                            ChangePartDone = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMCHANGEPART", "AlreadyChangePart", myParams);

                            if (ChangePartDone == null)
                            {
                                return SetXmlError(returnXml, "Trigger Error: Para este result code " + RC + " debe realizar change part");
                            }

                        }
                    }
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
