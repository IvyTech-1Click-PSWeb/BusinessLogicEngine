using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JGS.Web.TriggerProviders
{
    class xGDIDictionary:Object
    {
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
            ,{"XML_SN","/Trigger/Detail/ItemLevel/SerialNumber"} 
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"} 
            ,{"XML_PN","/Trigger/Detail/ItemLevel/PartNo"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_PWD","/Trigger/Header/UserObj/PassWord"}
            ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
     	};
    }
}