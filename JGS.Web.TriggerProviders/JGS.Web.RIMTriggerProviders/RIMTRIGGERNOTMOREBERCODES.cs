using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{

    public class RIMTRIGGERNOTMOREBERCODES : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_FA_DEFECTCODES","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode"}
        
	    };

         public override string Name { get; set; }

         public RIMTRIGGERNOTMOREBERCODES()
        {
            this.Name = "RIMTRIGGERNOTMOREBERCODES";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string UserName = string.Empty;
             string BCN = string.Empty;
            List<OracleParameter> myParams;
            string FA_Defect = string.Empty;
            string BerCodes = string.Empty;
            string[] DefCode = null;
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

              //-- Get BCN
             if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
             {
                 BCN = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
             }
             else
             {
                 return SetXmlError(returnXml, "BCN can not be found.");
             }

             if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_DEFECTCODES"]))
             {
                 FA_Defect = Functions.ExtractValue(xmlIn, _xPaths["XML_FA_DEFECTCODES"]).Trim().ToUpper();
          
                 XmlNodeList xnList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                 DefCode = new string[xnList.Count];

                 foreach (XmlNode xn in xnList)
                 {
                     DefCode[i] = xn["DefectCodeName"].InnerText;
                     i = i + 1;

                 }
             }

             if (FA_Defect != null & FA_Defect.Contains("F0002"))
             {
                 for (x = 0; x < i; x++)
                 {
                     //////////////////// Not more than two BER's codes /////////////////////
                     myParams = new List<OracleParameter>();
                     myParams.Add(new OracleParameter("BNC", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });
                     myParams.Add(new OracleParameter("FA_Defect", OracleDbType.Varchar2, DefCode[x].Length, ParameterDirection.Input) { Value = DefCode[x] });
                     myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                     BerCodes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERBERCODES", "GetBERCodes", myParams);

                     if (BerCodes != null)
                     {
                         if (BerCodes == "true")
                         {
                             return SetXmlError(returnXml, "No Debe agregar mas de un codigo BER/Not to add more than one BER code");
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
