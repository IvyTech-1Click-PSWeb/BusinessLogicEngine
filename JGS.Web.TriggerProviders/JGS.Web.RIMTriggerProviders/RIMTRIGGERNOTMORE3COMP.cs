using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
 
   public class RIMTRIGGERNOTMORE3COMP : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            ,{"XML_FA_COMP","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList"}

 	    };

       public override string Name { get; set; }

       public RIMTRIGGERNOTMORE3COMP()
        {
            this.Name = "RIMTRIGGERNOTMORE3COMP";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string Itemid = string.Empty;
            string UserName = string.Empty;
            List<OracleParameter> myParams;
            string FA_COMP_PN = string.Empty;
            string FA_COMP_PNList = string.Empty;
            string Call3Comp = string.Empty; 
            string[] Comp = null;
            int i = 0;
            int x;

            // Set Return Code to Success
            SetXmlSuccess(returnXml);


            //-- Get User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }

            //-- Get ItemID
            if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
            {
                Itemid = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "ItemID can not be found.");
            }

            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_PN"]))
            {
                FA_COMP_PN = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_COMP_PN"]).Trim().ToUpper();
            }
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
                        //////////////////// No more than 3 components /////////////////////
                        myParams = new List<OracleParameter>();
                        myParams.Add(new OracleParameter("FA_COMP_PN", OracleDbType.Varchar2, Comp[x].Length, ParameterDirection.Input) { Value = Comp[x] });
                        myParams.Add(new OracleParameter("itemid", OracleDbType.Varchar2, Itemid.ToString().Length, ParameterDirection.Input) { Value = Itemid });
                        myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                        Call3Comp = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "ThreeXComponent", myParams);
                        
                        if (Call3Comp == "TRUE")
                        {
                            return SetXmlError(returnXml, "No se puede agregar por tercera vez el componente " + FA_COMP_PN + "/ You can't add more than 3 times the component " + FA_COMP_PN);

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
