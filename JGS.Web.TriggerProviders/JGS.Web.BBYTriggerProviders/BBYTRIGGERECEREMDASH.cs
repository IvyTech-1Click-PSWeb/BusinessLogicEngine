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
    public class BBYTRIGGERECEREMDASH : JGS.Web.TriggerProviders.TriggerProviderBase 
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_FLEXFIELBBYRGM","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BBY_RGM']/Value"}
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"} 
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
           
		};


            public override string Name { get; set; }

            public BBYTRIGGERECEREMDASH()
            {
                this.Name = "BBYTRIGGERECEREMDASH";
            }

            public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
            {
                XmlDocument returnXml = xmlIn;

                //Build the trigger code here


                ////////////////////////////// Variables ///////////////////////////////////////////////////


                string FFbby_rgm = string.Empty;
                string BTT = string.Empty;

                //-- Get BTT
                if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
                {
                    BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
                }
                else
                {
                    return SetXmlError(returnXml, "BTT can not be found.");
                }

                if (BTT.Trim().ToUpper() == "CR")
                {
                    //-- Flex Field BBY_RGM

                    if (!Functions.IsNull(xmlIn, _xPaths["XML_FLEXFIELBBYRGM"]))
                    {
                        FFbby_rgm = Functions.ExtractValue(xmlIn, _xPaths["XML_FLEXFIELBBYRGM"]);
                        FFbby_rgm = FFbby_rgm.Replace("-", "");
                    }
                    else
                    {
                        FFbby_rgm = "";
                    }

                    SetXmlFFBBYRGM(returnXml, FFbby_rgm);
                    
                }

                // Set Return Code to Success
                SetXmlSuccess(returnXml);
                return returnXml;

            }

            private void SetXmlSuccess(XmlDocument returnXml)
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
            }


            private void SetXmlFFBBYRGM(XmlDocument returnXml, String FFDiaValue)
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_FLEXFIELBBYRGM"], FFDiaValue);
            }

            private XmlDocument SetXmlError(XmlDocument returnXml, string message)
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
                Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
                Functions.DebugOut(message);
                return returnXml;
            }
    }
}
