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
    public class BBYTRIGGEROEMRIM : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
		
             {"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
            ,{"XML_ResultCode","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}

		};

        public override string Name { get; set; }

        public BBYTRIGGEROEMRIM()
        {
            this.Name = "BBYTRIGGEROEMRIM";
        }
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////

            

         
            string PN = string.Empty;
            string UserName = string.Empty;
            string OEM = string.Empty;
            string Result = string.Empty;
           
            // User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }
           
            // Part No 
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PN"]))
            {
                PN = Functions.ExtractValue(xmlIn, _xPaths["XML_PN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Part No can not be found.");
            }
            // result code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ResultCode"]))
            {
               Result = Functions.ExtractValue(xmlIn, _xPaths["XML_ResultCode"]).Trim();
            }
            else
            {
               return SetXmlError(returnXml, "Result Code can not be found.");
           }


           
           //Validation of the Serial Number

            OEM = ValOem(PN, UserName);
            if (OEM == "RIM")
             {
                 Result = "RTV";
             }
 //         else 
  //          {
  //             Result = RES;
//      }




            // Functions and XML


            SetXmlResult(returnXml, Result);

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

        public string ValOem(string Pn1, string User)
        {
            string ValOEMRes = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, Pn1.Length, ParameterDirection.Input) { Value = Pn1 });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) { Value = User });
            ValOEMRes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "OEMRIM", myParams);
            


            return ValOEMRes;

        }



        // ADD INFORMATION 
        private void SetXmlResult(XmlDocument returnXml, String SNtrimupper)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ResultCode"], SNtrimupper);
        }


    }
}
