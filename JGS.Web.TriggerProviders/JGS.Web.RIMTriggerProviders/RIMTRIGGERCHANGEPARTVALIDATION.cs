using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.DAL;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERCHANGEPARTVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase   
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_PARTNO","/Trigger/Detail/ItemLevel/PartNo"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}  
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
       };


        public override string Name { get; set; }

        public RIMTRIGGERCHANGEPARTVALIDATION()
        {
            this.Name = "RIMTRIGGERCHANGEPARTVALIDATION";
        }


        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
           string SN = string.Empty;
           string ItemID = string.Empty;
           string PartNumber = string.Empty;
           string UserName = string.Empty;
           string PartNo = string.Empty;
           List<OracleParameter> myParams1;
           List<OracleParameter> myParams2;
           string FlagEnc = string.Empty;
           string resFFMsgID = string.Empty;
           string resFF = string.Empty;
           string SNF = string.Empty;
           string FF = string.Empty;
           string BCN = string.Empty;

           //-- Get Serial Number
           if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
           {
               SN = "%" + Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
               SNF = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
           }
           else
           {
               return SetXmlError(returnXml, "Serial Number can not be found.");
           }

           //-- Get Item ID
           if (!Functions.IsNull(xmlIn, _xPaths["XML_ItemID"]))
           {
               ItemID = Functions.ExtractValue(xmlIn, _xPaths["XML_ItemID"]).Trim().ToUpper(); 

           }
           else
           {
               return SetXmlError(returnXml, "ItemID can not be found.");
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

           //-- Get Part Number
           if (!Functions.IsNull(xmlIn, _xPaths["XML_PARTNO"]))
           {
               PartNo = Functions.ExtractValue(xmlIn, _xPaths["XML_PARTNO"]).Trim();
           }
           else
           {
               return SetXmlError(returnXml, "Part Number can not be found.");
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
           resFFMsgID = null;

            FF = "FF_B2B_FLAG";
            //////////////////// Get the Service Level /////////////////////
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams2.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNF.Length, ParameterDirection.Input) { Value = SNF });//new parameter
            myParams2.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFF = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams2);

            FF = "MsgID";
            //////////////////// Get the Service Level /////////////////////
            myParams1 = new List<OracleParameter>();
            myParams1.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams1.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNF.Length, ParameterDirection.Input) { Value = SNF });//new parameter
            myParams1.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFFMsgID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams1);

            if (resFFMsgID != null)
            {
                FlagEnc = getSHA1Hash(resFFMsgID);
            }
            if (resFF == FlagEnc & resFFMsgID != null & resFF != null)
            {
               List<OracleParameter> myParams;
               myParams = new List<OracleParameter>();
               myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
               myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
               myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
               PartNumber = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMCHANGEPARTVALIDATION", "GetPartChangeNumber", myParams);

               if (PartNumber != null)
               {
                   if (PartNumber.ToUpper() != PartNo.ToUpper())
                   {
                       return SetXmlError(returnXml, "Error: Favor de hacer el cambio de numero de parte: " + PartNumber + "/ This unit don't have Change Part: " + PartNumber );
                   }
               }
               else
               {
                   return SetXmlError(returnXml, "Error: Favor de hacer el cambio de numero de parte " + PartNumber + " / This unit don't have Change Part: " + PartNumber );

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

        private void SetXmlSuccess(System.Xml.XmlDocument returnXml)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
        }

        public string getSHA1Hash(string input)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider SHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] data = SHA1Hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(input));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            int i = 0;
            for (i = 0; i <= data.Length - 1; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }    



    }
}
