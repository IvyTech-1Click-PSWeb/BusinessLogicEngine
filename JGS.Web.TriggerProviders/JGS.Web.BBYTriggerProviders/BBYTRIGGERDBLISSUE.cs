using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using Oracle.DataAccess.Types;
using JGS.DAL;
using JGS.WebUI;
using System.Collections;
using System.Diagnostics;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERDBLISSUE : JGS.Web.TriggerProviders.TriggerProviderBase 
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
            {"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}			
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
       };

        public override string Name { get; set; }

        public BBYTRIGGERDBLISSUE()
        {
            this.Name = "BBYTRIGGERDBLISSUE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string UserName = string.Empty;
            string BCN = string.Empty;
            string Comp = string.Empty;
            string DblIssue = string.Empty;
            string FAComponent = string.Empty;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

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

            // Validaciones

            //-- Get Component
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_PN"]))
            {
                FAComponent = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_PN"]).Trim().ToUpper();
            }
            else
            {
                FAComponent = "";
            }

            if (FAComponent != "") 
            {
                List<OracleParameter> myParams;
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                //Comp = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYDBLISSUE", "GetComp", myParams);
                //Comp = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYDBLISSUE", "GetComp", myParams);
                Comp = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYDBLISSUE", "GetComp", myParams);

                if (Comp != null)
                {
                    List<OracleParameter> myParams2;
                    myParams2 = new List<OracleParameter>();
                    myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
                    //DblIssue = Functions.DbFetch("user id=NUNEZA4;data source=RNRDEV;password=NUNEZA4", "NunezA4", "JGSBBYDBLISSUE", "GetPrivDblIssue", myParams2);
                    //DblIssue = Functions.DbFetch("user id=WebAppAMS;data source=RNRSTG;password=CKL1@$12!%", "WEBAPP1", "JGSBBYDBLISSUE", "GetPrivDblIssue", myParams2);
                    DblIssue = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYDBLISSUE", "GetPrivDblIssue", myParams2);

                    if (DblIssue == null) 
                    {
                        DblIssue = "";
                    }

                    if (DblIssue.ToUpper() != "VALID")
                    {                        
                        if ((Comp.Contains(FAComponent)) == true)
                        {
                            return SetXmlError(returnXml, "El componente " + FAComponent + " ya ha sido agregado al menos una vez");
                        }
                    }
                }  
            }
            

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

    }
}
