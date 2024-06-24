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
    public class BBYTRIGGERFILLFF : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}            
            ,{"XML_FFRSP_COMMENTS","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='RSP_COMMENTS']/Value"}			         
            ,{"XML_FFRSP_TIMESTAMP","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='RSP_TIMESTAMP']/Value"}			         
            ,{"XML_FFAPPROVED","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='APPROVED']/Value"}			         
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}              
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}            
	    };

        public override string Name { get; set; }

        public BBYTRIGGERFILLFF()
        {
            this.Name = "BBYTRIGGERFILLFF";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////

            string UserName = string.Empty;
            string BCN = string.Empty;
            string SO = string.Empty;
            
            //-- BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Item BCN could not be found.|BCN no Encontrado");
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

            // Start Validations

            // Get SO
            SO = GetSO(BCN, UserName);

            if (SO != null && SO != "NULO") 
            {
                // Get rsp_timestamp
                string rsp_timestamp = getrsp_timestamp(SO, UserName);

                // Get approved
                string approved = getapproved(SO, UserName);

                // Get rsp_comments
                string rsp_comments = getrsp_comments(SO, UserName);
                
                // Update RSP_TIMESTAMP
                SetXml_FFRSP_TIMESTAMP(returnXml, rsp_timestamp);

                // Update approved
                SetXml_FFAPPROVED(returnXml, approved);

                // Update RSP_COMMENTS
                SetXml_FFRSP_COMMENTS(returnXml, rsp_comments);

            }            

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

        private void SetXml_FFRSP_TIMESTAMP(XmlDocument returnXml, String rsp_timestampupd)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFRSP_TIMESTAMP"], rsp_timestampupd);
        }

        private void SetXml_FFAPPROVED(XmlDocument returnXml, String FFAPPROVED)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFAPPROVED"], FFAPPROVED);
        }

        private void SetXml_FFRSP_COMMENTS(XmlDocument returnXml, String FFRSP_COMMENTS)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFRSP_COMMENTS"], FFRSP_COMMENTS);
        }

        public string GetSO(string BCNin, string Userin)
        {
            string InSO = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCNin.Length, ParameterDirection.Input) { Value = BCNin });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Userin.Length, ParameterDirection.Input) { Value = Userin });
            InSO = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyfillff", "getso", myParams);

            return InSO;
        }

        public string getrsp_timestamp(string SOin, string Userin)
        {
            string rsp_timestampin = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, SOin.Length, ParameterDirection.Input) { Value = SOin });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Userin.Length, ParameterDirection.Input) { Value = Userin });
            rsp_timestampin = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyfillff", "getrsp_timestamp", myParams);

            return rsp_timestampin;
        }

        public string getapproved(string SOin, string Userin)
        {
            string approvedin = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, SOin.Length, ParameterDirection.Input) { Value = SOin });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Userin.Length, ParameterDirection.Input) { Value = Userin });
            approvedin = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyfillff", "getapproved", myParams);

            return approvedin;
        }

        public string getrsp_comments(string SOin, string Userin)
        {
            string rsp_commentsin = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, SOin.Length, ParameterDirection.Input) { Value = SOin });
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, Userin.Length, ParameterDirection.Input) { Value = Userin });
            rsp_commentsin = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "jgsbbyfillff", "getrsp_comments", myParams);

            return rsp_commentsin;
        }
    }
}

