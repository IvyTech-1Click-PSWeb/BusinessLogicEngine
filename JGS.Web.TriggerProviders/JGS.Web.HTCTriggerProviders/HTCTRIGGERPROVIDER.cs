using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class HTCTRIGGERPROVIDER : JGS.Web.TriggerProviders.TriggerProviderBase
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

        public HTCTRIGGERPROVIDER()
        {
            this.Name = "HTCTRIGGERPROVIDER";
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
            string valor = string.Empty ;
            string resultCode;
            string SN;
            string lastitembcn;
            bool  DefectCode;
            bool  Actioncode;
            int LocationId;
            int clientId;
            int contractId;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

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
                partNumber = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part Number could not be found.");
            }


            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
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

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
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
          
          
            // Check if any Defect is Set.
            bool DefectCodeExists = false;
            bool ActionCodeExists = false;
            DefectCode = false;
            Actioncode = false;

            XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
            foreach (XmlNode xn in xnList)
            {
                string DefectCodeName = xn["DefectCodeName"].InnerText;
                if ( DefectCodeName == "F100" )
                {
                    DefectCode = true;
                }
                if (!string.IsNullOrEmpty(DefectCodeName))
                {
                    DefectCodeExists = true;
                }
                else
                    DefectCodeExists = false;

            } //end for each

            if (DefectCode == true)
            {
                Actioncode = false;

                // Buscar Action Code P003
                XmlNodeList xnListAc = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode");  //quite /FAFlexFieldList
                foreach (XmlNode xn in xnListAc)
                {
                    string ActionCodeName = xn["ActionCodeName"].InnerText;
                    if (ActionCodeName == "P003")
                    {
                        Actioncode = true;
                    }
                    if (!string.IsNullOrEmpty(ActionCodeName))
                    {
                        ActionCodeExists = true;
                    }
                    else
                        ActionCodeExists = false;

                } //end for each
            }

           
            if (resultCode.ToUpper() == "UNDERFIL" || resultCode.ToUpper() == "PASS")
            {

                //////////////////// Parameters List /////////////////////
                List<OracleParameter> myParams;
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("Pn", OracleDbType.Varchar2, partNumber.Length, ParameterDirection.Input) { Value = partNumber });
                myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientId.ToString().Length, ParameterDirection.Input) { Value = clientId });
                myParams.Add(new OracleParameter("contractId", OracleDbType.Int32, contractId.ToString().Length, ParameterDirection.Input) { Value = contractId });
                myParams.Add(new OracleParameter("LocationId", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                string res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "LoadValidPartNumUnderfill", myParams);
                                                                                
                valor = res.Substring (0,1);
                if (valor == "P")
                {
                    if (resultCode.ToUpper() == "UNDERFIL")
                    { 
                        return SetXmlError(returnXml, "Num Serie Requiere Enviar a DIAGNOSTIC, Seleccione PASS.");
                    }
                }
                else
                {
                    //Verifica si es Repeat Return               
                    myParams = new List<OracleParameter>();
                    myParams.Add(new OracleParameter("nserie", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                    myParams.Add(new OracleParameter("rs", OracleDbType.Varchar2, resultCode.Length, ParameterDirection.Input) { Value = resultCode  });
                    res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "UnitRepReG1", myParams);
                                                                                
                    valor = res.Substring(0, 1);

                    if (valor == "P")
                    {
                       // Es un Repeat Return.
                        //Validar si paso x wc underfill en el BCN pasado
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("nserie", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                        res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "WcInLastBcn", myParams);
                        
                        valor = res.Substring(0, 1);
                        if (valor == "P")
                        {
                            // Si paso enviar a diagnosis
                            if (resultCode.ToUpper() == "UNDERFIL")
                            { 
                                return SetXmlError(returnXml, "BCN anterior tuvo Underfill Enviar a DIAGNOSTIC,seleccione PASS.");
                            }
                        }
                        else if (valor != "I")
                        {
                            //No paso x wc underfill en el bcn pasado verificar si se cambio el MB    
                            lastitembcn = res;

                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("itmBCN", OracleDbType.Varchar2, lastitembcn.Length, ParameterDirection.Input) { Value = lastitembcn });
                            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "changeMB", myParams);
                            
                            valor = res.Substring(0, 1);

                            if (valor == "P")
                            {
                                //SI se cambio el MB
                                //Validar fecha de manufactura
                                myParams = new List<OracleParameter>();
                                myParams.Add(new OracleParameter("nserie", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                                res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "ValidDateReceiveBCN", myParams);
                                
                                valor = res.Substring(0, 1);
                                if (valor == "P")
                                {
                                    //significa que tengo que valida fecha manufactura si es > jun-05-09 enviar a diagnosis
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("nserie", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                                    res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "ValidManufactureDate", myParams);
                                    
                                    valor = res.Substring(0, 1);
                                    if (valor == "P")
                                    {
                                        //enviar a diagnosis 
                                        if (resultCode.ToUpper() == "UNDERFIL")
                                        {
                                            return SetXmlError(returnXml, "Fecha > Jun-5-09 Dic.Enviar a Diagnosis,Seleccione PASS");
                                        }
                                    }
                                    else
                                    { 
                                        //enviar a underfill y captura DEFECT code y Action code
                                        if (resultCode.ToUpper() == "PASS")
                                        {
                                            return SetXmlError(returnXml, "Requiere Enviar a Underfil.Seleccione UNDERFIL");
                                        }
                                        else
                                        {
                                            //Verificar que haya capturado Defect code y Action Code 
                                            if (DefectCode == true)
                                            {
                                                if (Actioncode == false)
                                                {
                                                    return SetXmlError(returnXml, "Requiere Capturar ActionCode P003");
                                                }
                                            }
                                            else
                                            {
                                                return SetXmlError(returnXml, "Requiere Capturar DefectCode F100");
                                            } // end if validacion captura defec code

                                        } //end if validacion result code seleccionado

                                    }  //end if validacion fecha manufactura
                                }
                                else
                                { 
                                    //enviar a diagnosis significa que la fecha de recibido del BCN fue >  14-DIC-09
                                    if (resultCode.ToUpper() == "UNDERFIL")
                                    {
                                        return SetXmlError(returnXml, "Fecha BCN Recibido > Dic-14-09 Dic.Enviar a Diagnosis,Seleccione PASS");
                                    } 
                                } //end if validacion fecha recibido BCN
                            }
                            else
                            {
                                if (resultCode.ToUpper() == "UNDERFIL")
                                {
                                    return SetXmlError(returnXml, "NO se cambio el MB enviar a DIAGNOSIS.Seleccione PASS");
                                }
                            }  //end if si cambio moderboard
                        }
                        else
                        {
                            return SetXmlError(returnXml, "ERROR al extraer el Item BCN");
                        }  //end if si en el anterior BCN tuvo WC Underfill
                    }
                    else
                    {
                        // sino es repeat return validar fecha manufactura si es > jun-05-09 enviar a diagnosis
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("nserie", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                        res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "HTC_ANDROI", "ValidManufactureDate", myParams);
                        
                        valor = res.Substring(0, 1);
                        if (valor == "P")
                        {
                            //enviar a diagnosis 
                            if (resultCode.ToUpper() == "UNDERFIL")
                            {
                                return SetXmlError(returnXml, "Fecha > Jun-5-09 Dic.Enviar a Diagnosis,Seleccione PASS");
                            }
                        }
                        else
                        {
                            //enviar a underfill y captura DEFECT code y Action code
                            if (resultCode.ToUpper() == "PASS")
                            {
                                return SetXmlError(returnXml, "Requiere Enviar a Underfil.Seleccione UNDERFIL");
                            }
                            else
                            {
                                //Verificar que haya capturado Defect code y Action Code 
                                if (DefectCode == true)
                                {
                                    if (Actioncode == false)
                                    {
                                        return SetXmlError(returnXml, "Requiere Capturar ActionCode P003");
                                    }
                                }
                                else
                                {
                                    return SetXmlError(returnXml, "Requiere Capturar DefectCode F100");
                                } // end if validacion captura defec code

                            } //end if validacion result code seleccionado

                        }  //end if validacion fecha manufactura

                    } //end if validacion si es Repeat Return

                } //end if validacion num de partes 

            }  //end if resultcode underfill or pass
            
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
