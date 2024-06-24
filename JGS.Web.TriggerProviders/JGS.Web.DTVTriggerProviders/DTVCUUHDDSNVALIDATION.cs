using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Data;
using System.Web;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Linq;

namespace JGS.Web.TriggerProviders
{
    public class DTVCUUHDDSNVALIDATION : JGS.Web.TriggerProviders.TriggerProviderBase
    {
        public override string Name { get; set; }

        public DTVCUUHDDSNVALIDATION()
        {
            this.Name = "DTVCUUHDDSNVALIDATION";
        }

        public Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_LOCATIONID","/Trigger/Header/LocationID"}
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_SERIALNO","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_BCN","/Trigger/Detail/ItemLevel/BCN"}
            ,{"XML_FA_COMPONENT_LISTS","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList"}
            ,{"XML_FA_COMPONENT_NEW","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component"}
            ,{"XML_FA_COMPONENT_DEFECTIVE","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/DefectiveList/Component"}
            ,{"XML_FA_COMP_LOC","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentLocation"}
            ,{"XML_FA_COMP_PN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentPartNo"}
            ,{"XML_FA_COMP_SN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ComponentSerialNo"}
            ,{"XML_FA_COMP_MANUF","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/Manufacturer"}
            ,{"XML_FA_COMP_MPN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/NewList/Component/ManuFacturerPart"}
            ,{"XML_FA_COMP_DEF_SN","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode/ActionCodeList/ActionCode/ComponentCodeList/DefectiveList/Component/ComponentSerialNo"}

		};

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            //////////////////////////////// Schema name for Stored Procs calls ////////////////////////
            string Schema_name = "WEBAPP1";
            string Package_name = "DTVCuuHddSnValidation";

            // Uses the DB table: DTVHDDSNCHARVALIDATION
            // This allows for dynamic lists of validation characters
            // Calls to the package where actual validation occurs

            
            //////////////////// Parameters List /////////////////////
            List<OracleParameter> myParams;

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            int HddListPos = 0;
            int LocationId = 0;
            int clientId = 0;
            int contractId = 0;
            string bcn = string.Empty;
            string SN = string.Empty;
            string itemId = string.Empty;
            string FA_COMP_LOC = string.Empty;
            string FA_COMP_PN = string.Empty;
            string FA_COMP_SN = string.Empty;
            string FA_COMP_MANUF = string.Empty;
            string FA_COMP_MPN = string.Empty;
            string FA_COMP_DEF_SN = string.Empty;
            string FA_COMP_DEF_MANUF = string.Empty;
            string strComp_Manuf = string.Empty;


            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                LocationId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]));
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractId = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }

            //-- Get BCN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_BCN"]))
            {
                bcn = Functions.ExtractValue(xmlIn, _xPaths["XML_BCN"]).Trim().ToUpper();
            }

            //-- Get Serial Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SERIALNO"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SERIALNO"]).Trim().ToUpper();
            }

            //-- Get the Component information for the HDD Comp_LOC Only
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_LOC"]))
            {
                // Search through the Newlist components to find the entries for the HDD Component Location
                XmlNodeList xcmpList = xmlIn.SelectNodes(_xPaths["XML_FA_COMPONENT_NEW"]);
                foreach (XmlNode xCmp in xcmpList)
                {
                    if (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD")
                    {
                        //-- Get Component Location
                        FA_COMP_LOC = xCmp["ComponentLocation"].InnerText.Trim().ToUpper();

                        //-- Get Component Part Number
                        FA_COMP_PN = xCmp["ComponentPartNo"].InnerText.Trim().ToUpper();
                        
                        //-- Get Component Serial Number
                        FA_COMP_SN = xCmp["ComponentSerialNo"].InnerText.Trim().ToUpper();
                        
                        //-- Get Component Manufacturer name
                        FA_COMP_MANUF = xCmp["Manufacturer"].InnerText.Trim().ToUpper();
                        
                        //-- Get Component Manuf Part Number
                        FA_COMP_MPN = xCmp["ManuFacturerPart"].InnerText.Trim().ToUpper();

                        break;

                    }   //if (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD")

                    //Next Node Position
                    HddListPos += 1;

                }   //foreach (XmlNode xCmp in xcmpList)

                try
                {
                    // Get the DefectiveList HDD Component Location SN
                    xcmpList = xmlIn.SelectNodes(_xPaths["XML_FA_COMPONENT_DEFECTIVE"]);

                    FA_COMP_DEF_SN = xcmpList[HddListPos]["ComponentSerialNo"].InnerText.ToUpper().Trim().Replace(".", "").Trim();
                    FA_COMP_DEF_MANUF = xcmpList[HddListPos]["Manufacturer"].InnerText.ToUpper().Trim().Replace(".", "").Trim();
                }
                catch { }

                xcmpList = null;

                /****************************************************************************************************************************/

                if (!string.IsNullOrEmpty(FA_COMP_PN))
                {
                    ///////////////////////// Display values for debug /////////////////////////////
                    Functions.DebugOut("----------  Check Variables  -------");
                    Functions.DebugOut("LocationId:  " + LocationId);
                    Functions.DebugOut("clientId:    " + clientId);
                    Functions.DebugOut("contractId:  " + contractId);
                    Functions.DebugOut("SN:          " + SN);
                    Functions.DebugOut("BCN:         " + bcn);
                    Functions.DebugOut("ItemId:      " + itemId);
                    Functions.DebugOut("Comp_Loc:    " + FA_COMP_LOC);
                    Functions.DebugOut("Comp_PN:     " + FA_COMP_PN);
                    Functions.DebugOut("Comp_SN:     " + FA_COMP_SN);
                    Functions.DebugOut("Comp_MANUF:  " + FA_COMP_MANUF);
                    Functions.DebugOut("Comp_MPN:    " + FA_COMP_MPN);
                    Functions.DebugOut("--------------------------------");

                    /****************************************** LOGIC START UP ***************************************/

                    if (!string.IsNullOrEmpty(FA_COMP_MANUF))
                    {
                        strComp_Manuf = FA_COMP_MANUF;
                    }

                    if (string.IsNullOrEmpty(strComp_Manuf))
                    {
                        Functions.DebugOut("-----  getComponentManufName --------> ");
                        try
                        {
                            myParams = new List<OracleParameter>();
                            myParams.Add(new OracleParameter("iComponentPart_NO", OracleDbType.Varchar2, FA_COMP_PN.Length, ParameterDirection.Input) { Value = FA_COMP_PN });
                            myParams.Add(new OracleParameter("iGeoID", OracleDbType.Int32, LocationId.ToString().Length, ParameterDirection.Input) { Value = LocationId });
                            strComp_Manuf = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getComponentManufName", myParams);
                            myParams = null;
                        }
                        catch { }
                    }

                    ///////////////////////// Display values for debug /////////////////////////////
                    Functions.DebugOut("Comp_MANUF:  " + strComp_Manuf);
                    Functions.DebugOut("--------------------------------");


                    #region "Hard Drive Serial Number Valdiation"

                    Functions.DebugOut("-----  validate_HDD_SN --------> ");

                    if (!string.IsNullOrEmpty(strComp_Manuf))
                    {
                        string strReturnMsg = "The HDD Serial Number contains special characters, Please Rescan";

                        try
                        {
                            FA_COMP_SN = FA_COMP_SN.Replace(".", "").Trim();

                            //Check for non-AlphNumeric
                            for (int i = 0; i < FA_COMP_SN.Length; i++)
                            {
                                char _Char = FA_COMP_SN[i];

                                if (!char.IsLetterOrDigit(_Char))
                                    return SetXmlError(returnXml, strReturnMsg, HddListPos);
                            }
                        }
                        catch { }

                        strReturnMsg = "The HDD Serial Number is not Valid, Please Rescan";

                        if (strComp_Manuf.Equals("SEAGATE"))
                        {
                            //HDD Serial Numbers logic for Seagate HDD
                            //Length is 8 characters
                            if (!ValidateSNLength(strComp_Manuf, FA_COMP_SN, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                            //Fist digit is always a Number
                            if (!ValidateSNCharsExist(strComp_Manuf, FA_COMP_SN, 0, 1, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                            //Combinations of characters in the 2nd and 3rd position are
                            //in the table DTVHDDSNCHARVALIDATION, supporting more possible combinations
                            if (!ValidateSNCharsExist(strComp_Manuf, FA_COMP_SN, 1, 2, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                            //And for the 4-8 digits could be numeric or alphanumeric.
                            //Tested above

                            //Not allowed letters I, O, U
                            if (!ValidateSNCharsNotExist(strComp_Manuf, FA_COMP_SN, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                        }
                        else if (strComp_Manuf.Equals("WSTRNDGTL") || strComp_Manuf.Equals("WESTERN DIGITAL"))
                        {
                            //For Western Digital HDD

                            //Length is 12 characters
                            if (!ValidateSNLength(strComp_Manuf, FA_COMP_SN, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                            //First digit is always W
                            if (!ValidateSNCharsExist(strComp_Manuf, FA_COMP_SN, 0, 1, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                            //Rest of the digits could be letters or numbers
                            //Tested above

                            //Not allowed letters are B, G, I, O, Q
                            if (!ValidateSNCharsNotExist(strComp_Manuf, FA_COMP_SN, Schema_name, Package_name))
                                return SetXmlError(returnXml, strReturnMsg, HddListPos);

                        }
                    }   //if (!string.IsNullOrEmpty(strComp_Manuf))

                    UpdateFields(returnXml, FA_COMP_SN, HddListPos);
                    UpdateDefSNField(returnXml, FA_COMP_DEF_SN, HddListPos);

                }   //if (!string.IsNullOrEmpty(FA_COMP_PN))
            }   //if (!Functions.IsNull(xmlIn, _xPaths["XML_FA_COMP_LOC"]))

            #endregion

            // Set Return Code to Success
            SetXmlSuccess(returnXml);

            return returnXml;
        }

        /// <summary>
        /// Test if a character is a number
        /// </summary>
        /// <param name="s">Character to Test if it is Numeric</param>
        /// <returns>True if the character is Numeric</returns>
        private bool IsNumeric(string s)
        {
            int n;
            return Int32.TryParse(s, out n);
        }

        /// <summary>
        /// Validate the SN Length
        /// </summary>
        /// <param name="iManuf">Manufacturer Name</param>
        /// <param name="iSN">Serial Number to validate</param>
        /// <param name="Schema_name">DB Schema Name</param>
        /// <param name="Package_name">DB Package Name</param>
        /// <returns>True if the Length is defined in the db table DTVHDDSNCHARVALIDATION</returns>
        private bool ValidateSNLength(string iManuf, string iSN, string Schema_name, string Package_name)
        {
            Functions.DebugOut("-----  ValidateSNLength  --------> ");
            try
            {
                List<OracleParameter> myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("iManuf", OracleDbType.Varchar2, iManuf.Length, ParameterDirection.Input) { Value = iManuf });
                myParams.Add(new OracleParameter("iSN", OracleDbType.Varchar2, iSN.Length, ParameterDirection.Input) { Value = iSN });
                string strValidate = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ValidateSNLength", myParams);

                myParams = null;

                if (strValidate.ToUpper() == "OK")
                    return true;
                else
                    return false;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Test Character(s) at a specific position
        /// </summary>
        /// <param name="iManuf">Manufacturer Name</param>
        /// <param name="iSN">Serial Number to validate</param>
        /// <param name="iPOS">Starting Position to validate</param>
        /// <param name="NumOfChars">Number of Characters to validate</param>
        /// <param name="Schema_name">DB Schema Name</param>
        /// <param name="Package_name">DB Package Name</param>
        /// <returns>True if the First Character is defined in the db table DTVHDDSNCHARVALIDATION</returns>
        private bool ValidateSNCharsExist(string iManuf, string iSN, Int32 iPOS, Int32 NumOfChars, string Schema_name, string Package_name)
        {
            Functions.DebugOut("-----  ValidateSNCharsExist --------> ");
            try
            {
                List<OracleParameter> myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("iManuf", OracleDbType.Varchar2, iManuf.Length, ParameterDirection.Input) { Value = iManuf });
                myParams.Add(new OracleParameter("iSN_Chars", OracleDbType.Varchar2, NumOfChars, ParameterDirection.Input) { Value = iSN.Substring(iPOS, NumOfChars) });
                myParams.Add(new OracleParameter("iSN_POS", OracleDbType.Int32, (iPOS+1).ToString().Length, ParameterDirection.Input) { Value = iPOS+1 });
                string strValidate = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ValidateSNCharsExist", myParams);

                myParams = null;

                if (strValidate.ToUpper() == "OK")
                    return true;
                else
                    return false;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate specific characters do not exists in the SN
        /// </summary>
        /// <param name="iManuf">Manufacturer Name</param>
        /// <param name="iSN">Serial Number to validate</param>
        /// <param name="Schema_name">DB Schema Name</param>
        /// <param name="Package_name">DB Package Name</param>
        /// <returns>True if the Length is defined in the db table DTVHDDSNCHARVALIDATION</returns>
        private bool ValidateSNCharsNotExist(string iManuf, string iSN, string Schema_name, string Package_name)
        {
            Functions.DebugOut("-----  ValidateSNCharsNotExist --------> ");
            try
            {
                List<OracleParameter> myParams = new List<OracleParameter>();
                myParams.Add(new OracleParameter("iManuf", OracleDbType.Varchar2, iManuf.Length, ParameterDirection.Input) { Value = iManuf });
                myParams.Add(new OracleParameter("iSN", OracleDbType.Varchar2, iSN.Length, ParameterDirection.Input) { Value = iSN });
                string strValidate = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "ValidateSNCharsNotExist", myParams);

                myParams = null;
                
                if (strValidate.ToUpper() == "OK")
                    return true;
                else
                    return false;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set the Result to EXECUTION_ERROR and the Message to the specified message
        /// </summary>
        /// <param name="returnXml">The XmlDocument to update</param>
        /// <param name="message">The error message to set</param>
        /// <returns>The modified XmlDocument</returns>
        private XmlDocument SetXmlError(XmlDocument returnXml, string message, int HddListPos)
        {
            try
            {
                UpdateFields(returnXml, string.Empty, HddListPos);
                Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_ERROR);
                Functions.UpdateXml(ref returnXml, _xPaths["XML_MESSAGE"], message);
                Functions.DebugOut(message);
            }
            catch { }

            return returnXml;
        }

        /// <summary>
        /// Set Return XML to Success before validation begin.
        /// </summary>
        /// <param name="returnXml"></param>
        /// <returns></returns>
        private void SetXmlSuccess(XmlDocument returnXml)
        {
            try
            {
                Functions.UpdateXml(ref returnXml, _xPaths["XML_RESULT"], EXECUTION_OK);
            }
            catch { }
        }

        /// <summary>
        /// Update the various fields in the XmlDocument after all the validation is completed.
        /// </summary>
        private void UpdateFields(XmlDocument returnXml, string strSN, int HddListPos)
        {
            try
            {
                // When the FA Component SN Validation Fails, clear the FA Component SN field
                //Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_COMP_SN"], strSN);

                int ListPos = -1;

                // Search through the Newlist components to find the entries for the HDD Component Location
                XmlNodeList xcmpList = returnXml.SelectNodes(_xPaths["XML_FA_COMPONENT_NEW"]);
                foreach (XmlNode xCmp in xcmpList)
                {
                    ListPos += 1;

                    if ((ListPos == HddListPos) && (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD"))
                    {
                        //-- Update the Component Serial Number
                        xCmp["ComponentSerialNo"].InnerText = strSN;

                        break;

                    }   //if (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD")
                }   //foreach (XmlNode xCmp in xcmpList)

            }
            catch { }
        }

        private void UpdateDefSNField(XmlDocument returnXml, string strSN, int HddListPos)
        {
            if (!string.IsNullOrEmpty(strSN))
            {
                try
                {
                    // When the FA Component SN Validation Fails, clear the FA Component SN field
                    //Functions.UpdateXml(ref returnXml, _xPaths["XML_FA_COMP_SN"], strSN);

                    int ListPos = -1;

                    // Search through the Newlist components to find the entries for the HDD Component Location
                    XmlNodeList xcmpList = returnXml.SelectNodes(_xPaths["XML_FA_COMPONENT_DEFECTIVE"]);
                    foreach (XmlNode xCmp in xcmpList)
                    {
                        ListPos += 1;

                        if ((ListPos == HddListPos) && (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD"))
                        {
                            //-- Update the Component Serial Number
                            xCmp["ComponentSerialNo"].InnerText = strSN;

                            break;

                        }   //if (xCmp["ComponentLocation"].InnerText.Trim().ToUpper() == "HDD")
                    }   //foreach (XmlNode xCmp in xcmpList)

                }
                catch { }
            }
        }

    }
}
