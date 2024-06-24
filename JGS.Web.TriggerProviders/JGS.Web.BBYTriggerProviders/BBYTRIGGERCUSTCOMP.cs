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
    public class BBYTRIGGERCUSTCOMP : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
		
             {"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
            ,{"XML_ResultCode","/Receiving/Detail/Order/Lines/Line/Items/Item/ResultCode"}
            ,{"XML_FLEXFIELBBYRGM","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='BBY_RGM']/Value"}
            ,{"XML_FLEXFIELCCC","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='CCC']/Value"}
            ,{"XML_NOTE","/Receiving/Detail/Order/Lines/Line/Items/Item/Notes"}  
		};

        public override string Name { get; set; }

        public BBYTRIGGERCUSTCOMP()
        {
            this.Name = "BBYTRIGGERCUSTCOMP";
        }
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
                       

         
            string PN = string.Empty;
            string UserName = string.Empty;
            string OEM = string.Empty;
            string BBYRGM = string.Empty;
            string notes = string.Empty;
           
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
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FLEXFIELBBYRGM"]))
            {
                BBYRGM = Functions.ExtractValue(xmlIn, _xPaths["XML_FLEXFIELBBYRGM"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "FF BBYRGM Code can not be found.");
           }


           
           //Validation of the Serial Number
            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("RGM", OracleDbType.Varchar2, BBYRGM.Length, ParameterDirection.Input) { Value = BBYRGM });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            notes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "CUSTCOMPRGM", myParams);

            if (string.IsNullOrEmpty(notes))
            {
                notes = "NULL";
            }


            SetXmlResult(returnXml, notes);

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


        // ADD INFORMATION 
        private void SetXmlResult(XmlDocument returnXml, String Notes1)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FLEXFIELCCC"], Notes1);
        }


    }
}
