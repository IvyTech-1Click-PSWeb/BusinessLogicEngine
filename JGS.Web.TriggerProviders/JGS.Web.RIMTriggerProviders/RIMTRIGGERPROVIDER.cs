using System;
using System.Collections.Generic;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders

{
    public class RIMTRIGGERPROVIDER : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}    
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USER","/Trigger/Header/UserObj/UserName"}
            ,{"XML_USERPWD","/Trigger/Header/UserObj/PassWord"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_FIXEDASSETTAG","/Trigger/Detail/ItemLevel/FixedAssetTag"}
			,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}            
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

        public RIMTRIGGERPROVIDER()
        {
            this.Name = "RIMTRIGGERPROVIDER";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string errMsg = string.Empty;
            string partNumber;
            string workcenterName;
            string pn = string.Empty;
            string valor = string.Empty;
            string resultCode;
            string SN;
            string Username = string.Empty;
            string usrpwd = string.Empty;
            string TriggerType = string.Empty;
            string ActionCodeName=string.Empty;
            string DefectCodeName=string.Empty;
            string ComponentPn = string.Empty;
            string ItemId = string.Empty;
            string KitMandatory = string.Empty;
            string ItemBcn = string.Empty;
            string OPT = string.Empty;
            string RecRC = string.Empty;
            string res = string.Empty;
            List<OracleParameter> myParams;
           
            string LocationId=string.Empty;
            string clientId=string.Empty;
            string contractId=string.Empty;
            string WcId = string.Empty;
            string Kitassigned = string.Empty;
            bool KitOk=false;
            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.|Numero de Serie no Encontrado");
            }

            //-- Get user
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USER"]))
            {
                Username = Functions.ExtractValue(xmlIn, _xPaths["XML_USER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.|Usuario no Encontrado");
            }

            //-- Get pwd
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERPWD"]))
            {
                usrpwd = Functions.ExtractValue(xmlIn, _xPaths["XML_USERPWD"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Pwd can not be found.|Pwd No Encontrado");
            }


            //-- Get opt
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OPT"]))
            {
                OPT = Functions.ExtractValue(xmlIn, _xPaths["XML_OPT"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "OPT can not be found.|OPT no Encontrado");
            }


            

            //-- Get PartNumber
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
            {
                partNumber = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.|PN no Encontrado");
            }


            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.| Work Center Name no Encontrado");
            }
            // - Get Work Center id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTERID"]))
            {
                WcId = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTERID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center id can not be found.| Work Center id no Encontrado");
            }

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.|Id de Geography no Encontrado");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientId = Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.|Id de Cliente no Encontrado");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]);
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.|Id de Contrato No Encontrado");
            }           

            //-- TriggerType
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                TriggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Trigger type could not be found.|Tipo de Trigger no Encontrado");
            }

            //-- ItemId
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemId = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item Id could not be found.|Item ID no Encontrado");
            }

            //-- ItemBcn
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                ItemBcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
            }


            if (TriggerType == "TIMEOUT")
            {

                if (workcenterName == "Data_Entry")
                {
                    //-- Get Result Code
                    if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
                    {
                        resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
                    }
                    else
                    {
                        return SetXmlError(returnXml, "Result Code could not be found.|Result Code No Encontrado");
                    }

                    //--trigger to set ServiceLevel
                    //--get WcFlexField Service Level
                    XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/TimeOut/WCFlexFields/FlexField");
                    foreach (XmlNode xn in xnList)
                    {
                        if (xn["Name"].InnerText == "SERVICE_LEVEL")
                        {
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("bcn", OracleDbType.Varchar2, ItemBcn.Length, ParameterDirection.Input) { Value = ItemBcn });
                            myParams.Add(new OracleParameter("CLIENTID", OracleDbType.Varchar2, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                            myParams.Add(new OracleParameter("CONTRACTID", OracleDbType.Varchar2, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                            myParams.Add(new OracleParameter("ACTUALWCNAME", OracleDbType.Varchar2, workcenterName.Length, ParameterDirection.Input) { Value = workcenterName });//new parameter
                            myParams.Add(new OracleParameter("ACTUALWCID", OracleDbType.Varchar2, WcId.Length, ParameterDirection.Input) { Value = WcId });//new parameter
                            myParams.Add(new OracleParameter("ACTUALRC", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode });//new parameter
                            myParams.Add(new OracleParameter("ITEMID", OracleDbType.Varchar2, ItemId.Length, ParameterDirection.Input) { Value = ItemId });//new parameter
                            myParams.Add(new OracleParameter("ACTUAL_OPT", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });//new parameter
                            myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                            
                            
                            //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
                            
                            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLevWithActualWc", myParams);
                            if (!string.IsNullOrEmpty(res))
                            {
                                xn["Value"].InnerText = res;
                            }
                            else
                            {
                                return SetXmlError(returnXml, "Service Level Could not be Calculate|Nivel de Servicio no puede ser calculado");
                            }

                        }
                        //--get WcFlexField Bounce Disposition
                        //if (xn["Name"].InnerText == "Bounce_Disposition")
                        //{
                        //    myParams = new List<OracleParameter>();
                        //    myParams.Add(new OracleParameter("itemid", OracleDbType.Varchar2, ItemId.Length, ParameterDirection.Input) { Value = ItemId });
                        //    myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, OPT.ToString().Length, ParameterDirection.Input) { Value = OPT });
                        //    myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });

                        //    res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetBounceDispDebug", myParams);
                        //    if (!string.IsNullOrEmpty(res))
                        //    {
                        //        xn["Value"].InnerText = res;
                        //    }
                        //    else
                        //    {
                        //        myParams = new List<OracleParameter>();
                        //        myParams.Add(new OracleParameter("SerialNo", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });                                
                        //        myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                        //        res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetBounceDisp", myParams);
                        //        if (!string.IsNullOrEmpty(res))
                        //        {
                        //            xn["Value"].InnerText = res;
                        //        }
                        //        else
                        //        {
                        //            return SetXmlError(returnXml, "Bounce Disposition  Flex Field Could Not be Found!!!!");
                        //        }
                                
                        //    }

                        //}
                        ////--get WcFlexField LoopCount
                        //if (xn["Name"].InnerText == "Loop_Count")
                        //{
                        //    myParams = new List<OracleParameter>();
                        //    myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, ItemBcn.Length, ParameterDirection.Input) { Value = ItemBcn });
                        //    myParams.Add(new OracleParameter("LocationID", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                        //    myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });

                        //    res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetLoop", myParams);
                        //    if (!string.IsNullOrEmpty(res))
                        //    {
                        //        xn["Value"].InnerText = res;
                        //    }
                        //    else
                        //    {
                        //        return SetXmlError(returnXml, "Loop Count Could not be Calculate");
                        //    }

                        //}

                    }   
                }                                             

            }
            if (TriggerType == "FAILUREANALYSIS")
            {
                //get if kit already assigned            
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, ItemBcn.Length, ParameterDirection.Input) { Value = ItemBcn });
                myParams.Add(new OracleParameter("LocationID", OracleDbType.Varchar2, LocationId.Length, ParameterDirection.Input) { Value = LocationId });
                myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                RecRC = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetReceiptRC", myParams);

                if ((workcenterName == "VMI") && (OPT=="WRP"))
                {
                    //--Get Kit Mandatory
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("Partno", OracleDbType.Varchar2, partNumber.Length, ParameterDirection.Input) { Value = partNumber });
                    myParams.Add(new OracleParameter("OPT", OracleDbType.Varchar2, OPT.Length, ParameterDirection.Input) { Value = OPT });
                    myParams.Add(new OracleParameter("ContractID", OracleDbType.Varchar2, contractId.Length, ParameterDirection.Input) { Value = contractId });
                    myParams.Add(new OracleParameter("ClientID", OracleDbType.Varchar2, clientId.Length, ParameterDirection.Input) { Value = clientId });
                    myParams.Add(new OracleParameter("LocationID", OracleDbType.Varchar2, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                    myParams.Add(new OracleParameter("WCNAME", OracleDbType.Varchar2, workcenterName.ToString().Length, ParameterDirection.Input) { Value = workcenterName });
                    myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                    KitMandatory = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetKitNo", myParams);
                    if (!string.IsNullOrEmpty(KitMandatory))
                    {
                        //get if kit already assigned            
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, ItemBcn.Length, ParameterDirection.Input) { Value = ItemBcn });
                        myParams.Add(new OracleParameter("KIT", OracleDbType.Varchar2, KitMandatory.Length, ParameterDirection.Input) { Value = KitMandatory });
                        myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                        Kitassigned = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "GetIfExistTheKit", myParams);
                    }

                    //--get DefectCode 
                    XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                    if (xnList.Count == 0)
                    {
                        return SetXmlError(returnXml, "No Defect Code Found|Codigos de Defecto No Encontrados");
                    }
                    foreach (XmlNode xn in xnList)
                    {
                        if (!string.IsNullOrEmpty(xn["DefectCodeName"].InnerText))
                        {
                            DefectCodeName = xn["DefectCodeName"].InnerText;
                            XmlNodeList xnListAction = xn.SelectNodes("ActionCodeList/ActionCode");
                            foreach (XmlNode xnAction in xnListAction)
                            {
                                if (!string.IsNullOrEmpty(xnAction["ActionCodeName"].InnerText))
                                {
                                    ActionCodeName = xnAction["ActionCodeName"].InnerText;
                                    XmlNodeList xnListComponent = xnAction.SelectNodes("ComponentCodeList/NewList/Component");
                                    foreach (XmlNode xnComponent in xnListComponent)
                                    {
                                        if (!string.IsNullOrEmpty(xnComponent["ComponentPartNo"].InnerText))
                                        {
                                            ComponentPn = xnComponent["ComponentPartNo"].InnerText;

                                            //--trigger to verify if component has been assign more than 3 times
                                            myParams = new List<OracleParameter>();
                                            myParams.Add(new OracleParameter("Component", OracleDbType.Varchar2, ComponentPn.Length, ParameterDirection.Input) { Value = ComponentPn });
                                            myParams.Add(new OracleParameter("itemid", OracleDbType.Varchar2, ItemId.ToString().Length, ParameterDirection.Input) { Value = ItemId });
                                            myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                                            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "ThreeXComponent", myParams);
                                            if (res == "TRUE")
                                            {                                                
                                                return SetXmlError(returnXml, "Component: " + ComponentPn + ", Has been Assign more than three times|Component" + ComponentPn + "Ha Sido Asignado Mas de Tres Ocaciones");
                                            }
                                            //--trigger to verify if Defect and action already assing to item
                                            myParams = new List<OracleParameter>();
                                            myParams.Add(new OracleParameter("defectcodename", OracleDbType.Varchar2, DefectCodeName.Length, ParameterDirection.Input) { Value = DefectCodeName });
                                            myParams.Add(new OracleParameter("actioncodename", OracleDbType.Varchar2, ActionCodeName.ToString().Length, ParameterDirection.Input) { Value = ActionCodeName });
                                            myParams.Add(new OracleParameter("Component", OracleDbType.Varchar2, ComponentPn.ToString().Length, ParameterDirection.Input) { Value = ComponentPn });
                                            myParams.Add(new OracleParameter("itemid", OracleDbType.Varchar2, ItemId.ToString().Length, ParameterDirection.Input) { Value = ItemId });
                                            myParams.Add(new OracleParameter("p_username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
                                            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "VerifyExistsDefActComp", myParams);
                                            if (res == "TRUE")
                                            {
                                                return SetXmlError(returnXml, "Combination Defect: " + DefectCodeName + ", Action: " + ActionCodeName + ", Component: " + ComponentPn + ", Already assigned|Combinacion Defecto: " + DefectCodeName + ", Accion: " + ActionCodeName + ", Componente: " + ComponentPn + ", ya Ha Sido Asignada");
                                            }
                                            if ((!string.IsNullOrEmpty(KitMandatory)) && (RecRC != "Return Customer"))
                                            {
                                                if (ComponentPn == KitMandatory)
                                                {
                                                    if (ComponentPn == KitMandatory)
                                                    {                                                                                                              
                                                        KitOk = true;                                                       
                                                    }

                                                }
                                            }

                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Component Not Found.|Componente no Encontrado");
                                        }
                                    }
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "Action Code Not Found.|Codigo de Accion no Encontrado");
                                }
                            }


                        }
                        else
                        {
                            return SetXmlError(returnXml, "Defect Code Not Found.|Codigo de Defecto no Encontrado");
                        }
                    }

                    //Validate of all componente to be assign if kit mandatory was assigned

                    if ((!string.IsNullOrEmpty(KitMandatory)) && (RecRC != "Return Customer"))
                    {
                        if (KitOk == false)
                        {
                            if (string.IsNullOrEmpty(Kitassigned))
                            {
                                return SetXmlError(returnXml, "Need to assign kit: " + KitMandatory + "|Necesita asignar kit: " + KitMandatory);
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Kitassigned))
                            {
                                return SetXmlError(returnXml, "kit: " + KitMandatory + " Already Assigned|kit: " + KitMandatory + " ya Asignado");
                            }
                        }
                    }


                }
            }
                

            return returnXml;
        }

        private XmlDocument SetXmlError(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }

        private void SetXmlResultCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULTCODE"], "DIAGNOSIS");
        }

        private void SetXmlDefectCode(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_DEFECTCODES"], "F100");

        }

    }
}
