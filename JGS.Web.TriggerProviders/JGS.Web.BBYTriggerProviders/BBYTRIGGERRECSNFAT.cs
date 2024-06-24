using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;
using System.Text.RegularExpressions;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERRECSNFAT : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			 {"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"}  
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"}  
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}
            ,{"XML_FFFATLEG","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='FF_FAT_LEG']/Value"}
            ,{"XML_FFSNLEG","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='FF_SN_LEG']/Value"}
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}
		};

        public override string Name { get; set; }

        public BBYTRIGGERRECSNFAT()
        {
            this.Name = "BBYTRIGGERRECSNFAT";
        }
        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            
            int digits_SN ;
		    int digits_FAT;
            bool SN_Flag;
            bool FAT_Flag;

            string SN = string.Empty;
            string FF_SN_LEG = string.Empty;
            string SN_Characters = string.Empty;
            string SN_Type = string.Empty;
            string FAT = string.Empty;
            string BTT = string.Empty;
            string FF_FAT_LEG = string.Empty;
            string FAT_Characters = string.Empty;
            string FAT_Type = string.Empty;
            string PN = string.Empty;
            string Privilege = string.Empty;
            string UserName = string.Empty;
       

           // User Name
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "User Name can not be found.");
            }
            // Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Serial No can not be found.");
            }
           // Fixed Asset Tag 
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FAT"]))
            {
                FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FAT"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "Fixed Asset Tag can not be found.");
            }

            //-- Get BTT
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BTT"]))
            {
                BTT = Functions.ExtractValue(xmlIn, _xPaths["XML_BTT"]);
            }
            else
            {
                return SetXmlError(returnXml, "BTT can not be found.");
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

            if (BTT.Trim().ToUpper() == "CR" || BTT.Trim().ToUpper() == "CR-IWE")
            {
          
            //////////////////// Check if User has RECEIPT privileges /////////////////////
            List<OracleParameter> myParams2;
            myParams2 = new List<OracleParameter>();
            myParams2.Add(new OracleParameter("Value", OracleDbType.Varchar2, "RECEIPT".Length, ParameterDirection.Input) { Value = "RECEIPT" });
            myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
            Privilege = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetPriv", myParams2);

            if (Privilege == null)
            {
                Privilege = "";
            }

            if (Privilege.ToUpper() != "RECEIPT")
            {
           // Validations
                //Validation of the Serial Number
                FF_SN_LEG = ValSn(PN, UserName);
                
                //Validation of the Fixed Asset Tag 
                FF_FAT_LEG = ValFAT(PN, UserName);

                if (FF_SN_LEG != null || FF_FAT_LEG != null)
                {
                    char[] sim = {','};
                    string[] srtFF_SN_LEG = FF_SN_LEG.Split(sim);
                    string[] srtFF_FAT_LEG = FF_FAT_LEG.Split(sim);
                    // convert the first value in the array into an integer because always the first value
                    // will be
                    // an integer 
                    digits_SN = int.Parse(srtFF_SN_LEG[0]);
                    digits_FAT = int.Parse(srtFF_FAT_LEG[0]);
                    // saved the second value of the array into another string. This variable indicates which
					// kind
                    // of serial and FAT is alphanumeric or numeric.
                    SN_Characters =srtFF_SN_LEG[1];
                    FAT_Characters = srtFF_FAT_LEG[1];
                    // saved the third value of the array into another string. This variable indicates which
                    // kind
                    // of serial and FAT is MEID,IMEI, etc.
                    SN_Type = srtFF_SN_LEG[2];
                    FAT_Type = srtFF_FAT_LEG[2];
                    //////////////////// validate the correct length of the SN and FAT
                    if (SN.Length != digits_SN)
                    {
                        return SetXmlError(returnXml, "El Serial Number debe ser de " + digits_SN + " digitos verifique el formato del SN");
                    }

                    if(FAT.Length != digits_FAT)
                    {
                         return SetXmlError(returnXml,"El Fixed Asset Tag debe ser de " + digits_FAT
                             + " digitos verifique el formato del Fixed Asset Tag");
                    }


                    // Validate if the format of the SN is Alphanumeric or numeric

               
                   
                    SN_Flag = SN_Characters.Equals("AlphaNumeric", StringComparison.OrdinalIgnoreCase);

                    if (SN_Flag.Equals(true))
                     {
                         Regex alf = new Regex("[A-Za-z]");
                         Match s = alf.Match(SN);
                         if (string.IsNullOrEmpty(s.Value))
                         {
                             return SetXmlError(returnXml, "El Formato del Serial Number debe contener numeros y letras, verifique el formato del Serial Number ");
                         }
                        
                     }
                     else 
                     {
                         Regex alf = new Regex("[0-9]");
                         Match s = alf.Match(SN);
                         if (string.IsNullOrEmpty(s.Value))
                         {
                             return SetXmlError(returnXml, "El Formato del Serial Number debe contener solo numeros, verifique el formato del Serial Number ");
                         }
                     }


                    
                    // Validate if the format of the SN is Alphanumeric or numeric




                    FAT_Flag = FAT_Characters.Equals("AlphaNumeric", StringComparison.OrdinalIgnoreCase);

                    if (FAT_Flag.Equals(true))
                    {
                        Regex alf = new Regex("[A-Za-z]");
                        Match s = alf.Match(FAT);
                        if (string.IsNullOrEmpty(s.Value))
                        {
                            return SetXmlError(returnXml, "El Formato del Fixed Asset Tag debe contener numeros y letras, verifique el formato del Fixed Asset Tag ");
                        }

                    }
                    else
                    {
                        Regex alf = new Regex("[0-9]");
                        Match s = alf.Match(FAT);
                        if (string.IsNullOrEmpty(s.Value))
                        {
                            return SetXmlError(returnXml, "El Formato del Fixed Asset Tag debe contener solo numeros, verifique el formato del Fixed Asset Tag ");
                        }
                    }





                }
                else 
                {
                    return SetXmlError(returnXml, " El Part Number no tiene capturados el FF_SN_LEG y el FF_FAT_LEG, estos FF deben de llevar valores validos ");

                }

                // Functions and XML
                SetXmlFF_FAT(returnXml, FAT_Type);
                SetXmlFF_SN(returnXml, SN_Type);             
                
            }
        }
            
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

        public string ValSn (string Pn1, string User)
        {
            string ValSNRes = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("PartNo",OracleDbType.Varchar2,Pn1.Length,ParameterDirection.Input){Value = Pn1});
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) {Value = User});
            ValSNRes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "VALSNBYFF", myParams);


            return ValSNRes;

        }

        public string ValFAT (string Pn2, string User)
        {
            string ValFATRes = string.Empty;

            List<OracleParameter> myParams;
            myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("PartNo",OracleDbType.Varchar2,Pn2.Length,ParameterDirection.Input){Value = Pn2});
            myParams.Add(new OracleParameter("UserName", OracleDbType.Varchar2, User.Length, ParameterDirection.Input) {Value = User});
            ValFATRes = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "VALFATBYFF", myParams);

            return ValFATRes;

        }

        private void SetXmlFF_FAT(XmlDocument returnXml, String FATtrimupper)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFFATLEG"], FATtrimupper);
        }
        // ADD INFORMATION 
        private void SetXmlFF_SN(XmlDocument returnXml, String SNtrimupper)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_FFSNLEG"], SNtrimupper);
        }

        
    }
}
