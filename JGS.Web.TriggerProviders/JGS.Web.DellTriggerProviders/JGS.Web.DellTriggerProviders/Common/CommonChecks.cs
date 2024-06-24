using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;



namespace JGS.Web.TriggerProviders
{
    public class CommonChecks : Object
    {
        

        public static string CheckFAT(XmlDocument xmlIn, string orderProcessType, string contractName, string Connection)
		{
           int contractId;
           string FAT = string.Empty;

           if(string.IsNullOrEmpty(orderProcessType))
           {
                //-- Get Order Process Type
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_OPT"]))
                {
                    orderProcessType = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_OPT"]).Trim().ToUpper();
                }
                else
                {
                    return  "Fixed Asset Tag check - OPT cannot be empty.";
                }
           }

           if (String.IsNullOrEmpty(contractName))
           {
               //-- Get contract Id 
               if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]))
               {
                   contractId = Int32.Parse(Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_CONTRACTID"]));
               }
               else
               {
                   return "Fixed Asset Tag check - Contract Id can not be found.";
               }

               Dictionary<string, OracleQuickQuery> queries = new Dictionary<string, OracleQuickQuery>() 
                {
                 {  contractName = "ContractName", new OracleQuickQuery("TP2","CONTRACT","UPPER(CONTRACT_NAME)","ContractName","CONTRACT_ID = {PARAMETER}")}
                };

               //Call the DB to get necessary data from Oracle ///////////////
               queries["ContractName"].ParameterValue = contractId.ToString();
               Functions.GetMultipleDbValues(Connection, queries);

               contractName = queries["ContractName"].Result;

               if (String.IsNullOrEmpty(contractName))
               {
                   return "Fixed Asset Tag check - Contract Name can not be found.";
               }
           }
        //////// check /////////////////
        if (!string.IsNullOrEmpty(contractName) && !string.IsNullOrEmpty(orderProcessType))
        {
            contractName = contractName.ToUpper();
            orderProcessType = orderProcessType.ToUpper();

            if ((contractName == "LCD REPAIR" && orderProcessType == "RACK")
               || (contractName == "LCD REPAIR" && orderProcessType == "SCRP1")
               || (contractName == "LCD REPAIR" && orderProcessType == "SCRP5")
               || (contractName == "LCD REPAIR" && orderProcessType == "WRP")
               || (contractName == "SUBASSEMBLY LCD REPAIR" && orderProcessType == "LCDE2")
               || (contractName == "SUBASSEMBLY LCD REPAIR" && orderProcessType == "LCDEX")
               || (contractName == "SUBASSEMBLY LCD REPAIR" && orderProcessType == "SCRAP")
               || (contractName == "SUBASSEMBLY LCD REPAIR" && orderProcessType == "SLVG2")
               || (contractName == "SUBASSEMBLY LCD REPAIR" && orderProcessType == "SLVGE")
                )
            {
                if (!Functions.IsNull(xmlIn, xPathDictionary._xPaths["XML_FIXEDASSETTAG"]))
                {
                    FAT = Functions.ExtractValue(xmlIn, xPathDictionary._xPaths["XML_FIXEDASSETTAG"]).Trim();
                    return FAT;
                }
                else
                {
                    return "Fixed Asset Tag check - Fixed Asset Tag could not be found.";
                }
            }             
        }              
           
           return FAT;             
		}
    }
}