using System;
using System.Collections.Generic;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERVMITO : JGS.Web.TriggerProviders.TriggerProviderBase
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
            //WC Fields
            ,{"XML_COM_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='NFF_Confirmation']/Value"}
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

        public BBYTRIGGERVMITO()
        {
            this.Name = "BBYTRIGGERVMITO";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
           
            string ItemId = string.Empty;
            string Username = string.Empty;
            string Comp = string.Empty;
            string result = string.Empty;
            string ff = string.Empty;


           
          

            //-- Get user
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USER"]))
            {
                Username = Functions.ExtractValue(xmlIn, _xPaths["XML_USER"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.|Usuario no Encontrado");
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
            //-- result code

            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                result = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.|Result Code no Encontrado");
            }

            //XML_COM_FF_VALUE

            if (!Functions.IsNull(xmlIn, _xPaths["XML_COM_FF_VALUE"]))
            {
                ff = Functions.ExtractValue(xmlIn, _xPaths["XML_COM_FF_VALUE"]).Trim();
            }
           


            // validations 

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("ItemID", OracleDbType.Varchar2, ItemId.Length, ParameterDirection.Input) { Value = ItemId });
            myParams.Add(new OracleParameter("Username", OracleDbType.Varchar2, Username.Length, ParameterDirection.Input) { Value = Username });
            Comp = Functions.DbFetch(this.ConnectionString, "WEBAPP1",  "JGRBBYVMICOM", "GET_COMP", myParams);
            //Comp = Functions.DbFetch("user id=ALVAREZJ5;data source=RNRDEV;password=ALVAREZJ5", "ALVAREZJ5", "JGRBBYVMICOM", "GET_COMP", myParams);

            if ((result.ToUpper().Trim() == "PASS") && (string.IsNullOrEmpty(Comp))&& (string.IsNullOrEmpty(ff)) )
            {
                return SetXmlError(returnXml, "Seleccione los componentes a reemplazar  o confirme que la unidad es NFF");
            }
            else if ((result.ToUpper().Trim() == "PASS") && (!string.IsNullOrEmpty(Comp)) && (!string.IsNullOrEmpty(ff)))
            {
                 return SetXmlError(returnXml,"Esta confirmando como NFF una unidad con componentes cargados");
            }

            
          
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

        private void SetXmlSuccess(XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }

    }
}
