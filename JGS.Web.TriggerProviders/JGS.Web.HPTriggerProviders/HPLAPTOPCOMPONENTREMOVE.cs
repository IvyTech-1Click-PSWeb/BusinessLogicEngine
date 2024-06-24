using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;
using System.Web;
using JGS.Web.TriggerProviders;
using JGS.Web.HPTriggerProviders;
using System.Text.RegularExpressions;
using JGS.DAL;
using System.Web.Configuration;

namespace JGS.Web.TriggerProviders
{
    public class HPLAPTOPCOMPONENTREMOVE : TriggerProviderBase
    {
        bool warranty =false;
        string resultC = null;
        string manuDateStr = null;
        string warrantyPeriodStr = null;
        string condition = null;
        string productClass = null;
        string productSubClass = null;

        private Dictionary <string, string> _xPaths = new Dictionary<string, string>()
		{   
             {"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
			,{"XML_LOCATIONID","/Trigger/Header/LocationID"} 
			,{"XML_CLIENTID","/Trigger/Header/ClientID"}
			,{"XML_CONTRACTID","/Trigger/Header/ContractID"}
			,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
			,{"XML_SN","/Trigger/Detail/ItemLevel/SerialNumber"}
			,{"XML_WORKCENTER","/Trigger/Detail/ItemLevel/WorkCenter"}
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_WC_FF_VALUE", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']/Value"}
            ,{"XML_WC_FF_NAME", "/Trigger/Detail/TimeOut/WCFlexFields/FlexField[Name='{FLEXFIELDNAME}']"}
            ,{"XML_COMP_FF_VALUE","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/FAFlexFieldList/FlexField[Name= '{ffName}']/Value"}
             ,{"XML_COMP_FF_NAME","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/FAFlexFieldList/FlexField[Name= '{ffName}']"}
             ,{"XML_COMP_FF_Condition","/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode[DefectCodeName='{DEFECTCODE}']"+
                            "/ActionCodeList/ActionCode[ActionCodeName= '{ACTIONCODE}']"+
                            "/ComponentCodeList/DefectiveList/Component[ComponentPartNo='{compPN}']"+
                            "/Condition"}
     	};

        public override string Name { get; set; }

        public HPLAPTOPCOMPONENTREMOVE()
        {
            this.Name = "HPLAPTOPCOMPONENTREMOVE";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            string Schema_name = "WEBAPP1";
            string Package_name = "HPWURComponentsFA";

            /**********declaring variables*******************/

            int locationID;
            int clientID;
            int contractID;
            string locationName;
            string clientName;
            string contractName;
            string wholeUnitSn;
            string workcenter;
            string triggerType;
            string newpath;
            string UserName;


            /**********************Getting values from xml*******************/
            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
            {
                locationID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Geography Id can not be found.");
            }

            //-- Get Client Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CLIENTID"]))
            {
                clientID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CLIENTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Client Id can not be found.");
            }

            //-- Get Contract Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_CONTRACTID"]))
            {
                contractID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_CONTRACTID"]));
            }
            else
            {
                return SetXmlError(returnXml, "Contract Id can not be found.");
            }

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
            {
                triggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Trigger type can not be found.");
            }

            //-- Get Unit serial number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                wholeUnitSn = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "Unit Serial Number can not be found.");
            }

            //-- Get WC
            if (!Functions.IsNull(xmlIn, _xPaths["XML_WORKCENTER"]))
            {
                workcenter = Functions.ExtractValue(xmlIn, _xPaths["XML_WORKCENTER"]);
            }
            else
            {
                return SetXmlError(returnXml, "WORKCENTER can not be found.");
            }
            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
            {
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            }
            else
            {
                return SetXmlError(returnXml, "UserName could not be found.");
            }
            if (workcenter.ToUpper().Equals("WUR_REPAIR"))
            {
                if (triggerType.ToUpper() == "FAILUREANALYSIS")
                {
                    /********************Getting geo,client & contract****************************/

                    Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
                {
                  {Name="GeoName", new OracleQuickQuery("INVENTORY1","GEO_LOCATION","UPPER(LOCATION_NAME)","GeoName","LOCATION_ID = {PARAMETER}")}
                 ,{Name="ClientName", new OracleQuickQuery("TP2","CLIENT","UPPER(CLIENT_NAME)","ClientName","CLIENT_ID = {PARAMETER}")}
                 ,{Name="ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
                };

                    //Call the DB to get necessary data from Oracle 
                    queries["GeoName"].ParameterValue = locationID.ToString();
                    queries["ClientName"].ParameterValue = clientID.ToString();
                    queries["ContractName"].ParameterValue = contractID.ToString();
                    Functions.GetMultipleDbValues(this.ConnectionString, queries);
                    locationName = queries["GeoName"].Result;
                    clientName = queries["ClientName"].Result;
                    contractName = queries["ContractName"].Result;

                    if (String.IsNullOrEmpty(locationName))
                    {
                        Functions.DebugOut("Geography Name can not be found.");
                        return SetXmlError(returnXml, "Geography Name can not be found.");
                    }
                    if (String.IsNullOrEmpty(clientName))
                    {
                        Functions.DebugOut("Client Name can not be found.");
                        return SetXmlError(returnXml, "Client Name can not be found.");
                    }
                    if (String.IsNullOrEmpty(contractName))
                    {
                        Functions.DebugOut("Contract Name can not be found.");
                        return SetXmlError(returnXml, "Contract Name can not be found.");
                    }


                    /*********************Loop through component defective parts*************************************/
                    StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
                    XmlNodeList defList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                    foreach (XmlNode defectCode in defList)
                    {
                       
                            string defect = defectCode["DefectCodeName"].InnerText;
                            XmlNodeList actList = defectCode.SelectNodes("ActionCodeList/ActionCode"); //LastChild;//  .SelectNodes("/ActionCodeList");
                           
                            foreach (XmlNode actionCode in actList)
                            {
                                string action = actionCode["ActionCodeName"].InnerText;
                                XmlNodeList compList = actionCode.SelectNodes("ComponentCodeList/DefectiveList/Component");
                                foreach (XmlNode defComp in compList)
                                {
                                    string compPN = defComp["ComponentPartNo"].InnerText;
                                    string owner = defComp["Owner"].InnerText;
                                    string CompCond = defComp["Condition"].InnerText;
                                    string sbi = defComp["SerializedBulkIndicator"].InnerText;

                                 if (sbi.Equals("BCN", ignoreCase))
                                 {
                                    string serialNo = defComp["ComponentSerialNo"].InnerText;
                                    string ffName = null;
                                    string dateSiteCode = null;
                                    this.resultC = "Out of Warranty";
                                    this.condition = "SCRAP-NoValue";
                                    this.warranty = false;
                                    string damageCode = null;
                                    string CTOEMSN = null;
                                    string strResult = null;
                                   

                                    XmlNodeList faFF = defComp.SelectNodes("FAFlexFieldList/FlexField");
                                    foreach (XmlNode ff in faFF)
                                    {
                                        ffName = ff["Name"].InnerText;
                                        if (ffName.Equals("Datesitecode"))
                                        {
                                            dateSiteCode = ff["Value"].InnerText;
                                        }
                                        else if (ffName.Equals("DamageCode"))
                                        {
                                            damageCode = ff["Value"].InnerText;
                                        }
                                        else if (ffName.Equals("CTOEMSerial"))
                                        {
                                            CTOEMSN = ff["Value"].InnerText;
                                        }

                                    }

                                    if (CompCond.Equals("Defective In Warranty", ignoreCase))
                                    { 
                                        this.resultC = "In Warranty";
                                        this.condition = CompCond;
                                    }
                                    else if (CompCond.Equals("Defective", ignoreCase))
                                    {
                                        this.resultC = "PAYG OOW with Demand";
                                        this.condition = CompCond;
                                    };

                                    List<OracleParameter> myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                    myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
                                    myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
                                    myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                    myParams.Add(new OracleParameter("owner", OracleDbType.Varchar2, owner.Length, ParameterDirection.Input) { Value = owner });
                                    myParams.Add(new OracleParameter("ffvalue", OracleDbType.Varchar2, dateSiteCode.Length, ParameterDirection.Input) { Value = dateSiteCode });
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    strResult = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "validateFFValueByProgramVendor", myParams);

                                    if (!string.IsNullOrEmpty(strResult))
                                    {
                                        if (strResult.StartsWith("ERR:")==true)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove function validateFFValueByProgramVendor - " + strResult);
                                        }

                                    }

                                    string supplier = null;
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                    myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
                                    myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
                                    myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    supplier = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getVendorByPart", myParams);

                                    if (!string.IsNullOrEmpty(supplier))
                                    {
                                        if (supplier.IndexOf("ERR") > 0)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove function getVendorByPart - " + supplier);
                                        }
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.getVendorByPart]");
                                    }


                                    string returnable = null;
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                    myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
                                    myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
                                    myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    returnable = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "checkifUnitReturnable", myParams);

                                    if (!string.IsNullOrEmpty(returnable))
                                    {
                                        if (returnable.IndexOf("ERR") > 0)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove function checkifUnitReturnable - " + returnable);
                                        }
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.checkifUnitReturnable]");
                                    }

                                    if (returnable.Equals("N", ignoreCase))
                                    {
                                        this.resultC = "Non-Returnable";
                                        this.condition = "SCRAP-NoValue";
                                    }
                                    else if (returnable.Equals("Y", ignoreCase))
                                    {
                                        this.resultC = "Out of Warranty";
                                        this.condition = "SCRAP-NoValue";
                                    }

                                    //Paygo val
                                    string paygo = null;
                                    if (!this.resultC.Equals("Non-Returnable", ignoreCase))
                                    {
                                        myParams = new List<OracleParameter>();
                                        myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                        myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
                                        myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
                                        myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                        paygo = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "paygoValidation", myParams);

                                        if (!string.IsNullOrEmpty(paygo))
                                        {
                                            if (paygo.IndexOf("ERR") > 0)
                                            {
                                                return SetXmlError(returnXml, "In HPLaptopComponentRemove function paygoValidation - " + paygo);
                                            }
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.paygoValidation]");
                                        }

                                        if (paygo.Equals("N", ignoreCase))
                                        {
                                            this.resultC = "Out of Warranty";
                                            this.condition = "SCRAP-NoValue";
                                        }
                                        else if (paygo.Equals("Y", ignoreCase))
                                        {
                                            this.resultC = "PAYG OOW with Demand";
                                            this.condition = "Defective";
                                        }
                                    }

                                    //CID val
                                    if (!this.resultC.Equals("Non-Returnable", ignoreCase) && !this.resultC.Equals("PAYG OOW with Demand", ignoreCase))
                                    {
                                        if (string.IsNullOrEmpty(damageCode))
                                        {
                                            return SetXmlError(returnXml, "Couldn't find FA flex field \"DamageCode\"");
                                        }

                                        if (!damageCode.Equals("No Customer Abuse", ignoreCase))
                                        {
                                            this.resultC = "CID";
                                            this.condition = "SCRAP-NoValue";
                                        }
                                        else
                                        {
                                            this.resultC = "Out of Warranty";
                                            this.condition = "SCRAP-NoValue";
                                        }

                                    }

                                    //get platform
                                    string platform = null;
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("fftyepname", OracleDbType.Varchar2, "PART".Length, ParameterDirection.Input) { Value = "PART" });
                                    myParams.Add(new OracleParameter("ffName", OracleDbType.Varchar2, "PLATFORM NAME".Length, ParameterDirection.Input) { Value = "PLATFORM NAME" });
                                    myParams.Add(new OracleParameter("owner", OracleDbType.Varchar2, owner.Length, ParameterDirection.Input) { Value = owner });
                                    myParams.Add(new OracleParameter("condition", OracleDbType.Varchar2, "Workable".Length, ParameterDirection.Input) { Value = "Workable" });
                                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                    myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    platform = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getUniversalFFValue", myParams);

                                    if (!string.IsNullOrEmpty(platform))
                                    {
                                        if (platform.IndexOf("ERR") > 0)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove function getUniversalFFValue - " + platform);
                                        }
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.getUniversalFFValue] - PLATFORM NAME");
                                    }

                                    //get vendor
                                    string vendor = null;
                                    myParams = new List<OracleParameter>();
                                    myParams.Add(new OracleParameter("fftyepname", OracleDbType.Varchar2, "PART".Length, ParameterDirection.Input) { Value = "PART" });
                                    myParams.Add(new OracleParameter("ffName", OracleDbType.Varchar2, "VENDOR NAME".Length, ParameterDirection.Input) { Value = "VENDOR NAME" });
                                    myParams.Add(new OracleParameter("owner", OracleDbType.Varchar2, owner.Length, ParameterDirection.Input) { Value = owner });
                                    myParams.Add(new OracleParameter("condition", OracleDbType.Varchar2, "Workable".Length, ParameterDirection.Input) { Value = "Workable" });
                                    myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
                                    myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                    myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                    vendor = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "getUniversalFFValue", myParams);

                                    if (!string.IsNullOrEmpty(vendor))
                                    {
                                        if (vendor.IndexOf("ERR") > 0)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove function getUniversalFFValue - " + vendor);
                                        }
                                    }
                                    else
                                    {
                                        return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.getUniversalFFValue] - VENDOR NAME");
                                    }

                                    if (supplier.Equals("ODM", ignoreCase) && platform == null)
                                    {
                                        return SetXmlError(returnXml, "Part Group Felx field Platform Name is blank!");
                                    }
                                    else if (vendor == null)
                                    {
                                        return SetXmlError(returnXml, "Part Group Felx field Vendor Name is blank!");
                                    }

                                    if (!this.resultC.Equals("Non-Returnable", ignoreCase) && !this.resultC.Equals("CID", ignoreCase) && !this.resultC.Equals("PAYG OOW with Demand", ignoreCase))
                                    {

                                        if (supplier.Equals("OEM", ignoreCase))
                                        {
                                            DataSet snVal = new DataSet();
                                            string snValErr = null;
                                            OracleParameter[] myParam = new OracleParameter[9];
                                            myParam[0] = new OracleParameter("locationName", OracleDbType.Varchar2, locationName, ParameterDirection.Input);
                                            myParam[1] = new OracleParameter("clientName", OracleDbType.Varchar2, clientName, ParameterDirection.Input);
                                            myParam[2] = new OracleParameter("contractName", OracleDbType.Varchar2, contractName, ParameterDirection.Input);
                                            myParam[3] = new OracleParameter("vendor", OracleDbType.Varchar2, vendor, ParameterDirection.Input);
                                            myParam[4] = new OracleParameter("supplier", OracleDbType.Varchar2, supplier, ParameterDirection.Input);
                                            myParam[5] = new OracleParameter("partno", OracleDbType.Varchar2, compPN, ParameterDirection.Input);
                                            myParam[6] = new OracleParameter("serialNo", OracleDbType.Varchar2, serialNo, ParameterDirection.Input);
                                            myParam[7] = new OracleParameter("userName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                                            myParam[8] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                                            snVal = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name+ ".HPWURComponentsFA.validateSNLength", myParam);

                                            if (snVal.Tables[0].Rows.Count > 0)
                                            {
                                                foreach (DataRow DR in snVal.Tables[0].Rows)
                                                {
                                                    try
                                                    {
                                                        this.productClass = DR["PCLASS"].ToString();
                                                        this.productSubClass = DR["PSUBCLASS"].ToString();
                                                        snValErr = DR["FOUNDMATCH"].ToString();
                                                        if (!string.IsNullOrEmpty(snValErr))
                                                        {
                                                            if (snValErr.StartsWith("ERR") == true)
                                                            {
                                                                return SetXmlError(returnXml, "In HPLaptopComponentRemove function validateSNLength - " + snValErr);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.validateSNLength]");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        return SetXmlError(returnXml, ex.ToString());
                                                    }
                                                }
                                            }
                                        }// OEM
                                        ///Warranty Validation
                                        HP_ManufactureDate manufactureDate = null;
                                        HP_Warranty hpWarranty = null;
                                        string err = null;
                                        string fieldName1 = null;
                                        string fieldPattern1 = null;
                                        string fieldPattern2 = null;
                                        string fieldPattern3 = null;
                                        string fieldPattern4 = null;
                                        int warrantyPeriod = 0;

                                        if (dateSiteCode == null)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove - Flex field dateSiteCode cannot be blank! ");
                                        }
                                        if (damageCode == null)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove - Flex field Damage Code cannot be blank!");
                                        }
                                        if (CTOEMSN == null)
                                        {
                                            return SetXmlError(returnXml, "In HPLaptopComponentRemove - Flex field CT/OEM SN cannot be blank!");
                                        }

                                        if (returnable.Equals("Y", ignoreCase) && (paygo == null || paygo.Equals("N", ignoreCase))
                                             && damageCode.Equals("NO CUSTOMER ABUSE", ignoreCase))
                                        {
                                            if (supplier.Equals("ODM", ignoreCase))
                                            {
                                                // validate warranty by Datesitecode 
                                                if (!wholeUnitSn.Equals("XXXXXXXXXX", ignoreCase) && !wholeUnitSn.Equals("XXXXXXXXXXXX", ignoreCase))
                                                {
                                                    manufactureDate = new HP_ManufactureDate(this.ConnectionString, wholeUnitSn, locationName, clientName, contractName, supplier, platform, this.productSubClass, vendor, UserName);
                                                    if (manufactureDate != null)
                                                    {
                                                        if (manufactureDate.getSQLErrorMessage() != null)
                                                        {
                                                            return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                        }
                                                        err = validateWarrantSetFFResult(manufactureDate, hpWarranty, warrantyPeriod);
                                                        if (err != null)
                                                        {
                                                            return SetXmlError(returnXml, err);
                                                        }
                                                        //warr period
                                                    }
                                                }
                                                // validate warranty by Datesitecode 
                                                if (manufactureDate.getSQLErrorMessage() == null &&
                                                     (manufactureDate == null || manufactureDate.getDate() == null ||
                                                       this.warranty == false || hpWarranty.getInWarranty() == false))
                                                {
                                                    if (!dateSiteCode.Equals("XXXXXX", ignoreCase))
                                                    {
                                                        manufactureDate = new HP_ManufactureDate(this.ConnectionString, dateSiteCode, locationName, clientName, contractName, supplier, platform, this.productSubClass, vendor, UserName);
                                                        if (manufactureDate != null)
                                                        {
                                                            if (manufactureDate.getSQLErrorMessage() != null)
                                                            {
                                                                return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                            }
                                                            err = validateWarrantSetFFResult(manufactureDate, hpWarranty, warrantyPeriod);
                                                            if (err != null)
                                                            {
                                                                return SetXmlError(returnXml, err);
                                                            }
                                                            //warr period
                                                        }
                                                    }
                                                }
                                            }//ODM
                                            else //OEM
                                            {
                                                string snDatesitecodePattern = null;
                                                // validate warranty by product class and CT/OEM Serial
                                                manufactureDate = new HP_ManufactureDate(this.ConnectionString, CTOEMSN, locationName, clientName, contractName, supplier, this.productClass, this.productSubClass, vendor, UserName);
                                                if (manufactureDate != null)
                                                {
                                                    if (manufactureDate.getSQLErrorMessage() != null)
                                                    {
                                                        return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                    }
                                                    else if (manufactureDate.getFieldName1() != null)
                                                    {
                                                        fieldName1 = manufactureDate.getFieldName1();
                                                        if (!fieldName1.Equals("DATE CODE", ignoreCase))
                                                        {
                                                            if (manufactureDate != null)
                                                            {
                                                                if (manufactureDate.getSQLErrorMessage() != null)
                                                                {
                                                                    return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                                }
                                                                err = validateWarrantSetFFResult(manufactureDate, hpWarranty, warrantyPeriod);
                                                                if (err != null)
                                                                {
                                                                    return SetXmlError(returnXml, err);
                                                                }
                                                            }
                                                            else if (!manufactureDate.getManualCalculation())
                                                            {
                                                                fieldPattern1 = manufactureDate.getFieldPattern1();
                                                                fieldPattern2 = manufactureDate.getFieldPattern2();
                                                                fieldPattern3 = manufactureDate.getFieldPattern3();
                                                                fieldPattern4 = manufactureDate.getFieldPattern4();

                                                                if (fieldPattern1 != null)
                                                                {
                                                                    snDatesitecodePattern = "X{" + fieldPattern1.Length + "}";
                                                                }
                                                                if (fieldPattern2 != null)
                                                                {
                                                                    snDatesitecodePattern += "|X{" + fieldPattern2.Length + "}";
                                                                }
                                                                if (fieldPattern3 != null)
                                                                {
                                                                    snDatesitecodePattern += "|X{" + fieldPattern3.Length + "}";
                                                                }
                                                                if (fieldPattern4 != null)
                                                                {
                                                                    snDatesitecodePattern += "|X{" + fieldPattern4 + "}";
                                                                }

                                                                if (snDatesitecodePattern != null)
                                                                {
                                                                    Regex regx = new Regex(snDatesitecodePattern, RegexOptions.None);
                                                                    Match matchStr = regx.Match(CTOEMSN.ToUpper());
                                                                    if (!matchStr.Success)
                                                                    {
                                                                        if (fieldPattern1 != null
                                                                                && fieldPattern2 == null
                                                                                && fieldPattern3 == null
                                                                                && fieldPattern4 == null
                                                                                && CTOEMSN.Length != fieldPattern1.Length)
                                                                        {
                                                                            return SetXmlError(returnXml, "CT/OEM Serial MUST be " + fieldPattern1.Length + " Character " + manufactureDate.getFieldName1());
                                                                        }
                                                                        else if (fieldPattern1 != null
                                                                                && fieldPattern2 != null
                                                                                && fieldPattern3 == null
                                                                                && fieldPattern4 == null
                                                                                && CTOEMSN.Length != fieldPattern1.Length
                                                                                && CTOEMSN.Length != fieldPattern2.Length)
                                                                        {
                                                                            return SetXmlError(returnXml, "CT/OEM Serial MUST be " + fieldPattern1.Length + " or " + fieldPattern2.Length + " Character " + manufactureDate.getFieldName1());
                                                                        }
                                                                        else if (fieldPattern1 != null
                                                                              && fieldPattern2 != null
                                                                              && fieldPattern3 != null
                                                                              && fieldPattern4 == null
                                                                              && CTOEMSN.Length != fieldPattern1.Length
                                                                              && CTOEMSN.Length != fieldPattern2.Length
                                                                              && CTOEMSN.Length != fieldPattern3.Length)
                                                                        {
                                                                            return SetXmlError(returnXml, "CT/OEM Serial MUST be " + fieldPattern1.Length + " or " + fieldPattern2.Length + " or " + fieldPattern3.Length + " Character " + manufactureDate.getFieldName1());
                                                                        }
                                                                        else if (fieldPattern1 != null
                                                                              && fieldPattern2 != null
                                                                              && fieldPattern3 != null
                                                                              && fieldPattern4 != null
                                                                              && CTOEMSN.Length != fieldPattern1.Length
                                                                              && CTOEMSN.Length != fieldPattern2.Length
                                                                              && CTOEMSN.Length != fieldPattern3.Length
                                                                              && CTOEMSN.Length != fieldPattern4.Length)
                                                                        {
                                                                            return SetXmlError(returnXml, "CT/OEM Serial MUST be " + fieldPattern1.Length
                                                                                     + " or " + fieldPattern2.Length + " or "
                                                                                     + fieldPattern3.Length + " or "
                                                                                     + fieldPattern4.Length + " Character "
                                                                                     + manufactureDate.getFieldName1());
                                                                        }
                                                                    }
                                                                }//snDatesitecodePattern
                                                            }
                                                        }
                                                        else
                                                        {
                                                            manufactureDate = new HP_ManufactureDate(this.ConnectionString, dateSiteCode, locationName, clientName, contractName, supplier, this.productClass, this.productSubClass, vendor, UserName);
                                                            if (manufactureDate != null)
                                                            {
                                                                if (manufactureDate.getSQLErrorMessage() != null)
                                                                {
                                                                    return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                                }
                                                                else if (manufactureDate.getDate() != null)
                                                                {
                                                                    if (manufactureDate.getSQLErrorMessage() != null)
                                                                    {
                                                                        return SetXmlError(returnXml, manufactureDate.getSQLErrorMessage());
                                                                    }
                                                                    err = validateWarrantSetFFResult(manufactureDate, hpWarranty, warrantyPeriod);
                                                                    if (err != null)
                                                                    {
                                                                        return SetXmlError(returnXml, err);
                                                                    }
                                                                }
                                                                else if (!manufactureDate.getManualCalculation() && manufactureDate.getFieldPattern1() != null)
                                                                {
                                                                    fieldPattern1 = manufactureDate.getFieldPattern1();
                                                                    snDatesitecodePattern = "X{" + fieldPattern1.Length + "}";
                                                                    Regex regx = new Regex(snDatesitecodePattern, RegexOptions.None);
                                                                    Match matchStr = regx.Match(dateSiteCode.ToUpper());

                                                                    if (!matchStr.Success && dateSiteCode.Length != fieldPattern1.Length)
                                                                    {
                                                                        return SetXmlError(returnXml, "Datesitecode MUST be " + fieldPattern1.Length + " Character " + manufactureDate.getFieldName1());
                                                                    }
                                                                }
                                                            }
                                                        }


                                                    }
                                                }
                                            }//OEM
                                        }

                                    } //returnable-paygo-damagecode
                                    
                                    // OOW with Demand, added on 09/24/12 by SL.
                                    if (this.resultC.Equals("Out of Warranty",ignoreCase))
                                    {
                                        string partOnDemand = null;
                                        myParams = new List<OracleParameter>();
                                        myParams.Add(new OracleParameter("partno", OracleDbType.Varchar2, compPN.Length, ParameterDirection.Input) { Value = compPN });
                                        myParams.Add(new OracleParameter("vendor", OracleDbType.Varchar2, vendor.Length, ParameterDirection.Input) { Value = vendor });
                                        myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                                        partOnDemand = Functions.DbFetch(this.ConnectionString, Schema_name, Package_name, "oowDemandCheck", myParams);

                                        if (!string.IsNullOrEmpty(partOnDemand))
                                        {
                                            if (partOnDemand.IndexOf("error") > 0)
                                            {
                                                return SetXmlError(returnXml, "In HPLaptopComponentRemove function oowDemandCheck - " + partOnDemand);
                                            }

                                            if (partOnDemand.Equals("YES",ignoreCase))
                                            {
                                                this.resultC = "OOW with Demand";
                                                this.condition = "Defective";
                                            }
                                        }
                                        else
                                        {
                                            return SetXmlError(returnXml, "Problems to retrieve data from DB [HPWURComponentsFA.oowDemandCheck]");
                                        }
                                    }

                                    //Insert FFs
                                    XmlNode resultCode = null;
                                    XmlNode unitWarrantyPeriod = null;
                                    XmlNode manufacturerDate = null;
                                    XmlNode warranty = null;
                                    XmlNodeList faFFClone= defComp.SelectNodes("FAFlexFieldList/FlexField");
                                    XmlNode singleNode = defComp.SelectSingleNode("FAFlexFieldList");

                                    foreach (XmlNode ff in faFFClone)
                                    {
                                        if (!String.IsNullOrEmpty(this.resultC))
                                        {
                                            resultCode = ff.Clone();
                                            resultCode["Name"].InnerText = "ResultCode";
                                            resultCode["Value"].InnerText = this.resultC;
                                            singleNode.AppendChild(resultCode);
                                        }
                                        if (!String.IsNullOrEmpty(this.warrantyPeriodStr))
                                        {
                                            unitWarrantyPeriod = ff.Clone();
                                            unitWarrantyPeriod["Name"].InnerText = "UnitWarrantyPeriod";
                                            unitWarrantyPeriod["Value"].InnerText = this.warrantyPeriodStr;
                                            singleNode.AppendChild(unitWarrantyPeriod);
                                        }
                                        if (!String.IsNullOrEmpty(this.manuDateStr))
                                        {
                                            manufacturerDate = ff.Clone();
                                            manufacturerDate["Name"].InnerText = "ManufacturerDate";
                                            manufacturerDate["Value"].InnerText = this.manuDateStr;
                                            singleNode.AppendChild(manufacturerDate);
                                        }
                                        if (this.warranty == true || this.warranty == false)
                                        {
                                            warranty = ff.Clone();
                                            warranty["Name"].InnerText = "Warranty";
                                            warranty["Value"].InnerText = this.warranty.ToString();
                                            singleNode.AppendChild(warranty);
                                        }
                                        
                                        break;
                                    }

                                   //updating the contidion
                                    if (!string.IsNullOrEmpty(this.condition))
                                    {
                                        defComp["Condition"].InnerText = this.condition;
                                    }
                                   //changing the destination location for the removed comp
                                    XmlNodeList sourceList = defComp.SelectNodes("Source");
                                    foreach (XmlNode source in sourceList)
                                   {
                                       string stkl = source["StockingLoc"].InnerText;
                                       if (!string.IsNullOrEmpty(stkl))
                                         source["StockingLoc"].InnerText = "HP_RTV_WUR";
                                       else
                                         return SetXmlError(returnXml, "Couldn't find source location, please check the workcenter configuration");
                                   }
                                  }//bulk indicator
                                }//component
                            } // foreach action
                       
                    } // foreach defect

                    /*Loop through New Components*/

                    XmlNodeList newList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectCodeList/DefectCode");
                    foreach (XmlNode defectCode in newList)
                    {

                        string defect = defectCode["DefectCodeName"].InnerText;
                        XmlNodeList actList = defectCode.SelectNodes("ActionCodeList/ActionCode"); 

                        foreach (XmlNode actionCode in actList)
                        {
                            string action = actionCode["ActionCodeName"].InnerText;
                            XmlNodeList compList = actionCode.SelectNodes("ComponentCodeList/NewList/Component");
                            foreach (XmlNode newComp in compList)
                            {
                                string sbi = newComp["SerializedBulkIndicator"].InnerText;

                                if (sbi.Equals("NONBCN", ignoreCase))
                                {
                                    string compPN = newComp["ComponentPartNo"].InnerText;
                                    string qtyStr = newComp["Quantity"].InnerText;
                                    int qty = int.Parse(qtyStr);
                                    if (qty != 1)
                                    {
                                        return SetXmlError(returnXml, "Error - You must issue only one component for the part number " + compPN);
                                    }

                                }

                            }
                        }
                    }


                  } // FA
               } //WUR Repair
               SetXmlSuccess(returnXml);
               return returnXml;
            }//execute
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
           private string lookupFAFF(string path, string defect, string action, string comp, string ffname)
           {
               return path.Replace("DEFECTCODE", defect).Replace("ACTIONCODE", action).Replace("compPN", comp).Replace("ffName",ffname);
           }
           private string validateWarrantSetFFResult( HP_ManufactureDate manufactureDate, HP_Warranty warranty, int warrantyPeriod)
           {
             DateTime manuDate;
		      try 
              {
                if ( manufactureDate.getDate() != null && manufactureDate.getWarrantyPeriod() != -1 )
                {
                  manuDate = manufactureDate.getDate();
				  warrantyPeriod = manufactureDate.getWarrantyPeriod();
                  warranty = new HP_Warranty( manufactureDate );
                  if ( warranty != null ) 
                  {
					if ( warranty.getInWarranty() ) 
                    {
                       this.resultC="In Warranty";
                       this.condition = "Defective In Warranty";
                       this.warranty = true;
                    } 
                    else
                    {
                        this.resultC = "Out of Warranty";
                        this.condition = "SCRAP-NoValue";
                    }
                    if (manuDate != null)
                    {
                        this.manuDateStr = manuDate.ToString("MM-dd-yyyy");
                    }
                    if (warrantyPeriod != 0)
                    {
                        this.warrantyPeriodStr = warrantyPeriod.ToString();
                    }

                  }
                }
               return null;
              } catch(Exception e){return e.ToString();}
            
           }

        //methods

    }
    
}
