using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class RIMTRIGGERBOUNCEDISP : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
            ,{"XML_ItemID","/Trigger/Detail/ItemLevel/ItemID"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
            ,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}  
            ,{"XML_BOUNCE_DISPOSITION","/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='Bounce_Disposition']/Value"} 
            ,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_RESULTCODE","/Trigger/Detail/TimeOut/ResultCode"}
            ,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
	    };


        public override string Name { get; set; }

        public RIMTRIGGERBOUNCEDISP()
        {
            this.Name = "RIMTRIGGERBOUNCEDISP";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here
            ////////////////////////////// Variables ///////////////////////////////////////////////////
            string Itemid = string.Empty;
            string UserName = string.Empty;
            List<OracleParameter> myParams;
            string SN;
            string CallBounce = string.Empty;
            string BounceDisp = string.Empty;
            string workcenterName = string.Empty;
            string resultCode = string.Empty;
            List<OracleParameter> myParams1;
            List<OracleParameter> myParams2;
            string FlagEnc = string.Empty;
            string resFFMsgID = string.Empty;
            string resFF = string.Empty;
            string SNF = string.Empty;
            string FF = string.Empty;
            string BCN = string.Empty;


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

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = '%' + Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Serial Number can not be found.");
            }

            if (!Functions.IsNull(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]))
            {
                BounceDisp = Functions.ExtractValue(xmlIn, _xPaths["XML_BOUNCE_DISPOSITION"]).Trim().ToUpper();
            }
            // - Get Work Center Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenterName = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Work Center Name can not be found.");
            }
            //-- Get Result Code
            if (!Functions.IsNull(xmlIn, _xPaths["XML_RESULTCODE"]))
            {
                resultCode = Functions.ExtractValue(xmlIn, _xPaths["XML_RESULTCODE"]);
            }
            else
            {
                return SetXmlError(returnXml, "Result Code could not be found.");
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

            FF = "FF_B2B_FLAG";
            //////////////////// Get the Service Level /////////////////////
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams2.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
            myParams2.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFF = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams2);

            FF = "MsgID";
            //////////////////// Get the Service Level /////////////////////
            myParams1 = new List<OracleParameter>();
            myParams1.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
            myParams1.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });//new parameter
            myParams1.Add(new OracleParameter("FF", OracleDbType.Varchar2, FF.Length, ParameterDirection.Input) { Value = FF });
            myParams1.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });//new parameter
            resFFMsgID = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERDOA", "GETFFVALUE", myParams1);

            if (resFFMsgID != null)
            {
                FlagEnc = getSHA1Hash(resFFMsgID);
            }
            if (resFF == FlagEnc & resFFMsgID != null & resFF != null)
            {
                //////////////////// Bounce Disposition /////////////////////
                myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("SN", OracleDbType.Varchar2, SN.Length, ParameterDirection.Input) { Value = SN });
                myParams.Add(new OracleParameter("BCN", OracleDbType.Varchar2, BCN.Length, ParameterDirection.Input) { Value = BCN });//new parameter
                myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                CallBounce = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMTRIGGERBNC", "GetBNC3", myParams);

                if (CallBounce != null)
                {
                    //Functions.UpdateXml(ref returnXml, _xPaths["XML_BOUNCE_DISPOSITION"], CallBounce);
                    if (BounceDisp == null || BounceDisp == "")
                    {
                        return SetXmlError(returnXml, "Debe llenar el FF Bounce Disposition/Must be fill the FF Bounce Disposition");

                    }
                    else
                    {
                        if (BounceDisp == "VALID")
                        {
                            if (resultCode != "SWAP_RED")
                            {
                                return SetXmlError(returnXml, "Debe seleccionar SWAP_RED como Result Code /You must select SWAP_RED as Result Code");
                            }
                        }

                    }
                }
                if (CallBounce != "BNC3")
                {
                    Functions.UpdateXml(ref returnXml, _xPaths["XML_BOUNCE_DISPOSITION"], "INVALID");

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

        public static string HashCode(string str)
        {
            string rethash = "";
            try
            {

                System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                byte[] combined = encoder.GetBytes(str);
                hash.ComputeHash(combined);
                rethash = Convert.ToBase64String(hash.Hash);
            }
            catch (Exception ex)
            {
                string strerr = "Error in HashCode : " + ex.Message;
            }
            return rethash;
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
