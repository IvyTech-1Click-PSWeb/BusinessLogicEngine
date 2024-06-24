using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;



namespace JGS.Web.TriggerProviders
{
    public class xPathDictionary : Object
    {
        public static Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			//
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
			,{"XML_FA_ACTIONCODES", "/Trigger/Detail/FailureAnalisys/FAFlexFieldList/DefectCodeList/DefectCode/ActionCodeList/ActionCode/FAFlexFieldList"}
            ,{"XML_FA_DEFECTCODE_NAME_SEARCH","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
                 "DefectCode[DefectCodeName='{DEFECTCODE}']"}
            ,{"XML_FA_DEFECTCODE_FLEX_FIELD_VAL","/Trigger/Detail/FailureAnalysis/DefectCodeList/" +
                 "DefectCode[DefectCodeName='{DEFECTCODE}']/FAFlexFieldList/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            
             /////////////////////// Change Part /////////////////////////////
            ,{"XML_CP_NEW_PART_NUM",  "/Trigger/Detail/ChangePart/NewPartNo"}
            ,{"XML_CP_NEW_SERIAL_NUM","/Trigger/Detail/ChangePart/NewSerialNo"}
            ,{"XML_CP_NEW_REVISION_LEVEL","/Trigger/Detail/ChangePart/NewRevisionLevel"}
            ,{"XML_CP_NEW_FIXED_ASSET_TAG","/Trigger/Detail/ChangePart/NewFixedAssetTag"}
            //////////////////// Item Level Flex Field Value ////////////////////////
            ,{"XML_ITEMLEVEL_FLEX_FIELD","/Trigger/Detail/ItemLevel/ItemFlexField/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_ITEM_LEVEL_FF_VALUE", "/Trigger/Detail/ItemLevel/ItemFlexField/FlexField[Name='{FLEXFIELDNAME}']/Value"}
              ////// WorkCenter Flex Fields ///////
             ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
             ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
                        
		};
    }
}