using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.Web.TriggerProviders;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERLCMVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
    		,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}  
            ,{"XML_FA_COMP","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList"}
            ,{"XML_PASSWORD","/Trigger/Header/UserObj/PassWord"}
            ,{"XML_ROLENAME","/Trigger/Header/UserObj/RoleName"}

	    };


        public override string Name { get; set; }

        public BBYTRIGGERLCMVALIDATION()
        {
            this.Name = "BBYTRIGGERLCMVALIDATION";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            List<OracleParameter> myParams;
            string UserName = string.Empty;
            string LocationId = string.Empty;
            string BCN = string.Empty;
            string FACOMP = string.Empty;
            string res = string.Empty;
            string[] Comp = null;
            int i = 0;
            int x;
            int c;
            string EmployeeType = string.Empty;

            string FA_COMP_PNList = string.Empty;

            string[] Comps;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
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

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "BCN can not be found.");
            }

            //-- Get COmponents
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_PN"]))
            {
                FACOMP = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_PN"]).Trim().ToUpper();
            }

                //-- Get Each COmponents
                if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP"]))
                {
                    FA_COMP_PNList = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP"]).Trim().ToUpper();

                    XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component");
                    Comp = new string[xnList.Count];

                    foreach (XmlNode xn in xnList)
                    {
                        Comp[i] = xn["ComponentPartNo"].InnerText;
                        i = i + 1;

                    }
                }

                if (i > 0)
                {
                    for (x = 0; x < i; x++)
                    {
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                        myParams.Add(new OracleParameter("FACOMP", OracleDbType.Varchar2, Comp[x].Length, ParameterDirection.Input) { Value = Comp[x] });//new parameter
                        myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.Length, ParameterDirection.Input) { Value = LocationId });//new parameter
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
                        res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "RIMTRIGGERLCDVALIDATION", "GETLCDVALIDATION", myParams);

                        if (res == "true")
                        {
                            SetXmlRole(returnXml, "Glb_Engineering");
                            return SetXmlError(returnXml, "El componente " + FACOMP + ", excedió el numero de veces que se puede agregar/The component " + FACOMP + ", exceeded the number of times you can add ");
                        }
                        else
                        {

                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("FACOMP", OracleDbType.Varchar2, FACOMP.Length, ParameterDirection.Input) { Value = FACOMP });//new parameter
                            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                            //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
                            res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGRBBYLCMVALIDATION", "CheckAlternative", myParams);
                            
                            if (res != null)
                            {
                                Comps  = res.Split(',');

                                for (c = 0; c <= Comps.Count()-1; c++)
                                {
                                    FACOMP = Comps[c];
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                                    myParams.Add(new OracleParameter("FACOMP", OracleDbType.Varchar2, Comps[c].Length, ParameterDirection.Input) { Value = Comps[c] });//new parameter
                                    myParams.Add(new OracleParameter("LocationId", OracleDbType.Varchar2, LocationId.Length, ParameterDirection.Input) { Value = LocationId });//new parameter
                                    myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    //res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "CalSerLev", myParams); old function
                                    res = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "RIMTRIGGERLCDVALIDATION", "GETLCDVALIDATION", myParams);

                                    if (res == "true")
                                    {
                                        SetXmlRole(returnXml, "Glb_Engineering");
                                        return SetXmlError(returnXml, "El componente " + FACOMP + ", excedió el numero de veces que se puede agregar/The component " + FACOMP + ", exceeded the number of times you can add ");
                                    }

                                    
                                }
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

        private XmlDocument SetXmlRole(XmlDocument returnXml, string message)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_ROLENAME"], message);
            Functions.DebugOut(message);
            return returnXml;
        }

        private XmlDocument AddRoleNameNode(XmlDocument XmlIn, string NodeName, string NodeValue)
        {
           
            XmlDocument returnXmlUpdate = XmlIn;
            XmlNode oldCd;
            XmlElement root = returnXmlUpdate.DocumentElement;

            oldCd = root.SelectSingleNode("/Trigger/Header/UserObj");
            oldCd.InnerXml = "<RoleName>" + "Glb_Engineer" + "</RoleName>";

            XmlIn = returnXmlUpdate;

            return XmlIn;
        }


    }
}
