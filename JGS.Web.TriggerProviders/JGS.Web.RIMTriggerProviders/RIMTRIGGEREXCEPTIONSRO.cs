using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using System.Xml;

namespace JGS.Web.TriggerProviders
{
   public class RIMTRIGGEREXCEPTIONSRO : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
           {"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
           ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
           ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}  
           ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
        };

        public override string Name { get; set; }

        public RIMTRIGGEREXCEPTIONSRO()
        {
            this.Name = "RIMTRIGGEREXCEPTIONSRO";
        }
        public override XmlDocument Execute(XmlDocument xmlIn)
        {
            System.Xml.XmlDocument returnXml = xmlIn;

            List<OracleParameter> myParams;
            string BCN = string.Empty;
            string UserName = string.Empty;
            string GetException = string.Empty;

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
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
            


            ////////////////////Get Status of the RO /////////////////////
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.ToString().Length, ParameterDirection.Input) { Value = BCN });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
            GetException = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMEXCEPTIONSRO", "GetExceptions", myParams);


            if (GetException != null)
            {
                return SetXmlError(returnXml, "La RO tiene una excepcion " + GetException + "/ The RO has a exception " + GetException );
                                 
            }

            return returnXml;
        }


        private void SetXmlSuccess(System.Xml.XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }


        private XmlDocument SetXmlError(System.Xml.XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
            Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

    }           

}
