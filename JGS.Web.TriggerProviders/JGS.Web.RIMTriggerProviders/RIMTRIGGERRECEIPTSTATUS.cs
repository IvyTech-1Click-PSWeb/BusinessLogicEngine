using System;
using System.Collections.Generic;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using System.Xml;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERRECEIPTSTATUS : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
            ,{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
        };
        
        
        public override string Name { get; set; }
                   
        public RIMTRIGGERRECEIPTSTATUS()
        {
            this.Name = "RIMTRIGGERRECEIPTSTATUS";
        }
        
        public override XmlDocument Execute(XmlDocument xmlIn)
       {
                System.Xml.XmlDocument returnXml = xmlIn;

                //Build the trigger code here
                ////////////////////////////// Variables ///////////////////////////////////////////////////
               string SN = string.Empty;
               string GetStatus = string.Empty;
               List<OracleParameter> myParams;
               string BCN = string.Empty;
               string UserName = string.Empty;
               string MathChangePart = string.Empty;
               List<OracleParameter> myParams1;
               List<OracleParameter> myParams2;
               string FlagEnc = string.Empty;
               string resFFMsgID = string.Empty;
               string resFF = string.Empty;
               string SNF = string.Empty;
               string FF = string.Empty;
  
                // Set Return Code to Success
                SetXmlSuccess(returnXml);

                //-- Get Serial Number
                if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
                {
                    SN = '%' + Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
                    SNF = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
                }
                else
                {
                    return SetXmlError(returnXml, "Serial Number can not be found.");
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

                //-- Get User Name
                if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
                {
                    UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
                }
                else
                {
                    return SetXmlError(returnXml, "User Name can not be found.");
                }

             

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

                ////////////////////Get Status of the RO /////////////////////
                FF = "FF_RMAStatus";
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SNF.Length, ParameterDirection.Input) { Value = SNF });//new parameter
                myParams.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
                GetStatus = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams);


                if (GetStatus == null)
                {
                    return SetXmlError(returnXml, "No existe registro de la Orden/Not exist Order records");

                }
                else
                {
                    if (GetStatus != "Receipt Complete")
                    {
                        return SetXmlError(returnXml, "La Reference Order esta" + GetStatus + "favor de cerrarla/Reference Order is " + GetStatus + " please close it");
                    }
                }


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