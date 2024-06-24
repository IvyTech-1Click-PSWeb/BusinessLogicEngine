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
    public class xLenovoDictionary : Object
    {
        public static Dictionary<string, string> _locations = new Dictionary<string, string>()
        {
          {"LOU_RTV","LOU-Building 1/LEN_PF_RTVRETURNS/LENOVO"},//
          {"LOU_RTV_IW","LOU-Building 1/LEN_PF_RTVSTORES/IW"},//
          {"LOU_SCRAP","LOU-Building 1/LEN_PF_SCRAP/Default"},//
          {"LOU_WIP_OW","LOU-Building 1/LEN_PF_WIP/OW-WU"},//
          {"LOU_WIP_IW","LOU-Building 1/LEN_PF_WIP/IW-WU"},//
          
          {"BYD_RTV","BYD-Stores_B3/LEN_PF_RTVRETURNS/LENOVO"},//
          {"BYD_RTV_IW","BYD-Stores_B3/LEN_PF_RTVSTORES/IW"},//
          {"BYD_SCRAP","BYD-Stores_B3/LEN_PF_SCRAP/Default"}, //
          {"BYD_WIP_OW","BYD-Stores_B3/LEN_PF_WIP/OW-WU"},
          {"BYD_WIP_IW","BYD-Stores_B3/LEN_PF_WIP/IW-WU"},
          
          {"AME_TO","UMISS-JGS/LEN_PF_TO_LOU/Default"},//
          {"AME_SCRAP","UMISS-JGS/LEN_PF_SCRAP/Default"}, //
          {"AME_RTV","UMISS-JGS/LEN_PF_RTVRETURNS/LENOVO"}
        };

        public static Dictionary<string, string> _invalidRemCodes = new Dictionary<string, string>() 
        {
         {"IW_CND","PFV/PFS/OWS/OWR/OOW/NPR/DOA/SAS"},
         {"OW","PFP/PFC/PFL/PFR/PFV/PFS/NPR/DOA"}
        };

        public static Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{   
             {"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_LOCATIONID","/Trigger/Header/LocationID"} 
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_SN","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_OPTID","/Trigger/Detail/ItemLevel/OrderProcessTypeID"}
            ,{"XML_WORKCENTERID","/Trigger/Detail/ItemLevel/WorkCenterID"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_COMP_FF_VALUE","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/FAFlexFieldList/FlexField[Name= '{ffName}']/Value"}
             ,{"XML_COMP_FF_NAME","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/FAFlexFieldList/FlexField[Name= '{ffName}']"}
             ,{"XML_COMP_FF_Condition","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/Condition"}
     	};

        public static Dictionary<string, string> _xPathsTO = new Dictionary<string, string>()
		{   
            {"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            ,{"XML_LOCATIONID","/Trigger/Header/LocationID"} 
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_OPT","/Trigger/Detail/ItemLevel/OrderProcessType"}
			,{"XML_NOTES","/Trigger/Detail/TimeOut/Notes"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
			,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_WORKORDERID","/Trigger/Detail/ItemLevel/WorkOrderID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
     	};
    }
}
