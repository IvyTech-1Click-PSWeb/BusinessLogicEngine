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
    public class BESTBUYTRIGGERBOUNCE : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}    
            ,{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_CLIENTID","/Trigger/Header/ClientID"}    
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}      
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            // Extraer valor de Flex Field OOWbyCondition
            ,{"XML_OOWbyCondition","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='OOWbyCondition']/Value"}			         
	    };


        public override string Name { get; set; }

        public BESTBUYTRIGGERBOUNCE()
            {
                this.Name = "BESTBUYTRIGGERBOUNCE";
            }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string NumberOfReceipt = string.Empty;
            string UserName = string.Empty;
            string resultCode = string.Empty;
            string ItemBCN = string.Empty;
            string SN = string.Empty;
            string PartNo = string.Empty;
            string WCTimes = string.Empty;
            string Warranty = string.Empty;            
            string FFProcessType = string.Empty;
            string FFBounceLevel = string.Empty;
            string Scrap = string.Empty;
            string NumOfRec = string.Empty;
            string OOWbyCondition = string.Empty;

            int ItemID;
            int LocId;
            int clientId;
            int contractId;
            List<OracleParameter> myParams;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);


            //-- Get Item ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                ItemID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]));
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            //-- Get Loc ID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Location Id can not be found.");
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

            //-- BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                ItemBCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
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
                PartNo = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.|PN no Encontrado");
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
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
            }


            // Get Grade 

            string Grade = string.Empty;

            List<OracleParameter> myParamsGrd;
            myParamsGrd = new List<OracleParameter>();
            myParamsGrd.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
            myParamsGrd.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
            myParamsGrd.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
            myParamsGrd.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "BOUNCE LEVEL".Length, ParameterDirection.Input) { Value = "BOUNCE LEVEL" });
            myParamsGrd.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });            
            Grade = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetProccessFFValue", myParamsGrd);
            
            if (Grade == null) 
            {
                Grade = "";
            }

            // Get Scrap History  

            List<OracleParameter> myParamsScrap;
            myParamsScrap = new List<OracleParameter>();
            myParamsScrap.Add(new OracleParameter("LocName", OracleDbType.Varchar2, "Reynosa".Length, ParameterDirection.Input) { Value = "Reynosa" });
            myParamsScrap.Add(new OracleParameter("ClientName", OracleDbType.Varchar2, "Best Buy".Length, ParameterDirection.Input) { Value = "Best Buy" });
            myParamsScrap.Add(new OracleParameter("ContractName", OracleDbType.Varchar2, "BBY Rapid Exchange".Length, ParameterDirection.Input) { Value = "BBY Rapid Exchange" });
            myParamsScrap.Add(new OracleParameter("OPTName", OracleDbType.Varchar2, "WRP".Length, ParameterDirection.Input) { Value = "WRP" });
            myParamsScrap.Add(new OracleParameter("WCName", OracleDbType.Varchar2, "ERWC".Length, ParameterDirection.Input) { Value = "ERWC" });
            myParamsScrap.Add(new OracleParameter("ConditionName", OracleDbType.Varchar2, "SCRAP".Length, ParameterDirection.Input) { Value = "SCRAP" });
            myParamsScrap.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
            myParamsScrap.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });           
            Scrap = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetScrapHistory", myParamsScrap);

            // Get Warranty Value
            List<OracleParameter> myParamsWarranty;
            myParamsWarranty = new List<OracleParameter>();
            myParamsWarranty.Add(new OracleParameter("BCN", OracleDbType.Varchar2, ItemBCN.Length, ParameterDirection.Input) { Value = ItemBCN });
            myParamsWarranty.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            Warranty = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetWarranty", myParamsWarranty);           

            if (Warranty == null)
            {
                Warranty = "";
            }
            
            //-- Get FF OOWbyCondition Value
            if (!Functions.IsNull(xmlIn, _xPaths["XML_OOWbyCondition"]))
            {
                OOWbyCondition = Functions.ExtractValue(xmlIn, _xPaths["XML_OOWbyCondition"]).Trim();
            }
            else
            {
                OOWbyCondition = "";
            }

            // Universal FF Process_Type 
            List<OracleParameter> myParamsFFProcessType;
            myParamsFFProcessType = new List<OracleParameter>();
            myParamsFFProcessType.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "PROCESS_TYPE".Length, ParameterDirection.Input) { Value = "PROCESS_TYPE" });
            myParamsFFProcessType.Add(new OracleParameter("PN", OracleDbType.Varchar2, PartNo.Length, ParameterDirection.Input) { Value = PartNo });
            myParamsFFProcessType.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            FFProcessType = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "getuniversalffvalue", myParamsFFProcessType);
                        
            if (FFProcessType == null)
            {
                FFProcessType = "";
            }            

            // Get Number of Receipt
            List<OracleParameter> myParamsNumOfRec;
            myParamsNumOfRec = new List<OracleParameter>();
            myParamsNumOfRec.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
            myParamsNumOfRec.Add(new OracleParameter("PN", OracleDbType.Varchar2, PartNo.Length, ParameterDirection.Input) { Value = PartNo });
            myParamsNumOfRec.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            NumOfRec = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "NumberOfReceipt", myParamsNumOfRec);
            
            if (NumOfRec == null)
            {
                NumOfRec = "0";
            }

            // Grade Validation
            if (Grade.ToUpper() == "TRUE BOUNCE") 
            { 
                if (resultCode.ToUpper() != "2X")
                {
                    return SetXmlError(returnXml, "Unidad Grade - Solo puede seleccionar RC 2X para esta unidad");
                }
            }
            // Scrap Validation
            else if (Scrap != null && Scrap != "NULO" && Scrap != "0")
            {
                if (resultCode.ToUpper() != "OEM_SCRA")
                {
                    return SetXmlError(returnXml, "Esta unidad se embarcó como SCRAP, seleccione result code OEM_SCRA");
                }
            }
            // HTC ONE Validation            
            else if (Warranty.ToUpper().Trim() == "YES" && FFProcessType.ToUpper().Trim() == "ONE" && Convert.ToInt32(NumOfRec) == 1)
            {
                if (OOWbyCondition.ToUpper().Trim() == "")
                {
                    return SetXmlError(returnXml, "Favor de llenar FF OOWbyCondition");
                }
                else if (OOWbyCondition.ToUpper().Trim() == "FALSE")
                {
                    if (resultCode.ToUpper() != "HTC_RUR")
                    {
                        return SetXmlError(returnXml, "Unidad HTC One, solo puede seleccionar HTC_RUR"); 
                    }    
                }
                else if (OOWbyCondition.ToUpper().Trim() == "TRUE")
                {
                    if (resultCode.ToUpper() == "HTC_RUR")
                    {
                        return SetXmlError(returnXml, "Unidad no cumple con criterios IW ONE, seleccione otro Result Code");
                    }   
                }
            }
            // NOT select RC for HTC ONE (HTC_RUR)
            else if (resultCode.ToUpper() == "HTC_RUR")
            {
                if (Warranty.ToUpper().Trim() != "YES" ||  FFProcessType.ToUpper().Trim() != "ONE" || Convert.ToInt32(NumOfRec) != 1)
                {
                    return SetXmlError(returnXml, "Unidad no cumple con criterios IW ONE, seleccione otro Result Code");
                }
            }
            else 
            {                

                // Get Manufacture Part No

                string Manufacturer = string.Empty;

                List<OracleParameter> myParamsMan;
                myParamsMan = new List<OracleParameter>();
                myParamsMan.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, PartNo.Length, ParameterDirection.Input) { Value = PartNo });
                myParamsMan.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });                
                Manufacturer = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetManufacture", myParamsMan);

                if (Manufacturer == null || Manufacturer == "NULO")
                {
                    return SetXmlError(returnXml, "Número de parte inválido favor de verificar / Invalid Part No please verify");
                }
                else
                {
                    // Manufacturer Samsung?
                    if (Manufacturer.ToUpper() == "SAMSUNG")
                    {
                        // Ha pasado por el WC Recovery Flash?
                        string WCV = "Recovery Flash";

                        List<OracleParameter> myParamsWC;
                        myParamsWC = new List<OracleParameter>();
                        myParamsWC.Add(new OracleParameter("BCN", OracleDbType.Varchar2, ItemBCN.Length, ParameterDirection.Input) { Value = ItemBCN });
                        myParamsWC.Add(new OracleParameter("WC", OracleDbType.Varchar2, WCV.Length, ParameterDirection.Input) { Value = WCV });
                        myParamsWC.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });                       
                        WCTimes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetWCTimes", myParamsWC);

                        if (Convert.ToInt32(WCTimes) == 1)
                        {

                            // Bounce Trigger
                            List<OracleParameter> myParamsBounce;
                            myParamsBounce = new List<OracleParameter>();
                            myParamsBounce.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                            myParamsBounce.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                            myParamsBounce.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                            myParamsBounce.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "2X".Length, ParameterDirection.Input) { Value = "2X" });
                            myParamsBounce.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            NumberOfReceipt = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetProccessFFValue", myParamsBounce);                           
                                      
                            // Es Warranty
                            if (Warranty.ToUpper() == "YES")
                            {
                                                                
                                // Bounce
                                if (NumberOfReceipt != null)
                                {
                                    if (resultCode.ToUpper() != "2X")
                                    {
                                        return SetXmlError(returnXml, "Unidad Samsung Warranty / Bounce - Solo puede seleccionar RC 2X para esta unidad");
                                    }
                                }
                                else
                                {
                                    // NO es Bounce
                                    if (resultCode.ToUpper() != "RTV")
                                    {
                                        if (OOWbyCondition.ToUpper().Trim() == "") 
                                        {
                                            return SetXmlError(returnXml, "Unidad Samsung Warranty - Debe seleccionar valor para FF OOWbyCondition");
                                        }
                                        else if (OOWbyCondition.ToUpper().Trim() == "TRUE") 
                                        {
                                            return SetXmlError(returnXml, "Unidad Samsung Warranty - Solo puede seleccionar RC RTV para esta unidad");
                                        }                                        
                                    }
                                }

                            }
                            else
                            {
                                // Si NO es Warranty y es Bounce
                                if (NumberOfReceipt != null)
                                {
                                    if (resultCode.ToUpper() != "2X")
                                    {
                                        return SetXmlError(returnXml, "Unidad Samsung Bounce - Solo puede seleccionar RC 2X para esta unidad");
                                    }
                                }
                                else
                                {
                                    // Si NO es Warranty y NO es Bounce
                                    if (resultCode.ToUpper() == "2X" || resultCode.ToUpper() == "RTV")
                                    {
                                        return SetXmlError(returnXml, "No puede seleccionar RC 2X o RTV para esta unidad");
                                    }
                                }
                            }
                        }
                        else 
                        {
                            if (resultCode.ToUpper() == "2X")
                            {
                                return SetXmlError(returnXml, "Unidad con historial anterior en Recovery / NO puede seleccionar RC 2X para esta unidad");
                            }
                        }
                    }
                    else
                    {
                        // Bounce Trigger
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("ItemId", OracleDbType.Int32, ItemID.ToString().Length, ParameterDirection.Input) { Value = ItemID });
                        myParams.Add(new OracleParameter("ClientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                        myParams.Add(new OracleParameter("ContractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                        myParams.Add(new OracleParameter("FFName", OracleDbType.Varchar2, "2X".Length, ParameterDirection.Input) { Value = "2X" });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        NumberOfReceipt = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYBOUNCE", "GetProccessFFValue", myParams);                       

                        if (NumberOfReceipt != null)
                        {
                            if (resultCode.ToUpper() != "2X")
                            {
                                return SetXmlError(returnXml, "Unidad Bounce - Solo puede seleccionar RC 2X para esta unidad");
                            }
                        }
                        else
                        {
                            if (resultCode.ToUpper() == "2X")
                            {
                                return SetXmlError(returnXml, "Unidad NO es Bounce - No puede seleccionar RC 2X para esta unidad");
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


    }

}
