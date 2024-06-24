using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;

namespace JGS.Web.TriggerProviders
{
    public class LenovoLOU
    {
        private string destWhr = null;
        private string destZone = null;
        private string destBin = null;
        private string remCode = null;
        private string condition = null;
        private string warrantyStatus = null;
        private string dispositionCode = null;

        public LenovoLOU(string connectionString, int locationID, int clientID,string userName, string warranty, string cid, string repairable, string productClass, string productSubClass, ref string warrantyStatus,
             ref string dispositionCode, ref string remCode, ref string destWhr, ref string destZone, ref string destBin, ref string CompCond, out string strResult)
        {
         bool errorFound = false;
         string groupDisp = null;
         string Schema_name = "WEBAPP1";
         string Package_name = "LENOVO_WUR_FA";
         StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
         strResult = null;

         List<OracleParameter> myParams = new List<OracleParameter>();
         myParams.Add(new OracleParameter("geoId", OracleDbType.Int32, locationID.ToString().Length, ParameterDirection.Input) { Value = locationID });
         myParams.Add(new OracleParameter("clientId", OracleDbType.Int32, clientID.ToString().Length, ParameterDirection.Input) { Value = clientID });
         myParams.Add(new OracleParameter("productClass", OracleDbType.Varchar2, productClass.Length, ParameterDirection.Input) { Value = productClass });
         myParams.Add(new OracleParameter("productSubClass", OracleDbType.Varchar2, productSubClass.Length, ParameterDirection.Input) { Value = productSubClass });
         myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, userName.Length, ParameterDirection.Input) { Value = userName });
         groupDisp = Functions.DbFetch(connectionString, Schema_name, Package_name, "getGroupDisp", myParams);
         if (!string.IsNullOrEmpty(groupDisp))
         {
             if (groupDisp.StartsWith("Error") == true) strResult = groupDisp;
         }

         if (string.IsNullOrEmpty(strResult))
         {
          //IW
          if (warranty.Equals("IW", ignoreCase))
           {
               if (cid.Equals("YES", ignoreCase))
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "TBD";
                           this.condition = "TBE";
                           setRemCode(xLenovoDictionary._invalidRemCodes["IW_CND"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_WIP_OW"]);
                       }
                       else if (groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase) || groupDisp.Equals("SCRAP-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "CID";
                           this.remCode = "OWS";
                           this.condition = "SCRAP";
                           setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       if (groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase) || groupDisp.Equals("SCRAP-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "CID";
                           this.remCode = "OWS";
                           this.condition = "SCRAP";
                           setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   
                   }
               }
               else 
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase) || groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase))
                       {
                           this.warrantyStatus = "IW";
                           this.dispositionCode = "RET";
                           this.condition = "Defective In Warranty";
                           setRemCode(xLenovoDictionary._invalidRemCodes["IW_CND"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_RTV_IW"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       errorFound = true;
                   }
               }

           }
           //CND
           else if (warranty.Equals("CND", ignoreCase))
           {
               if (cid.Equals("YES", ignoreCase))
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "TBD";
                           this.condition = "TBE";
                           setRemCode(xLenovoDictionary._invalidRemCodes["IW_CND"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_WIP_OW"]);
                       }
                       else if (groupDisp.Equals("SCRAP-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "CID";
                           this.remCode = "OWS";
                           this.condition = "SCRAP";
                           setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       errorFound = true;
                   }
               }
               else
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase) || groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase))
                       {
                           this.warrantyStatus = "CND";
                           this.dispositionCode = "TBD";
                           this.condition = "TBE";
                           setRemCode(xLenovoDictionary._invalidRemCodes["IW_CND"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_WIP_IW"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       errorFound = true;
                   }
               }
           }
           //OOW
           else if (warranty.Equals("OOW", ignoreCase))
           {
               if (cid.Equals("YES", ignoreCase))
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "TBD";
                           this.condition = "TBE";
                           setRemCode(xLenovoDictionary._invalidRemCodes["OW"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_WIP_OW"]);
                       }
                       else if (groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase) || groupDisp.Equals("SCRAP-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "CID";
                           this.remCode = "OWS";
                           this.condition = "SCRAP";
                           setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       this.warrantyStatus = "OW";
                       this.dispositionCode = "CID";
                       this.remCode = "OWS";
                       this.condition = "SCRAP";
                       setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                   }
               
               }
               else
               {
                   if (repairable.Equals("YES", ignoreCase))
                   {
                       if (groupDisp.Equals("PROCESS-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "TBD";
                           this.condition = "TBE";
                           setRemCode(xLenovoDictionary._invalidRemCodes["OW"], remCode);
                           setLocation(xLenovoDictionary._locations["LOU_WIP_OW"]);
                       }
                       else if (groupDisp.Equals("SCRAP-OW-ONLY", ignoreCase) || groupDisp.Equals("SCRAP-ALL", ignoreCase))
                       {
                           this.warrantyStatus = "OW";
                           this.dispositionCode = "OOW";
                           this.remCode = "OWS";
                           this.condition = "SCRAP";
                           setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                       }
                       else
                       {
                           errorFound = true;
                       }
                   }
                   else
                   {
                       this.warrantyStatus = "OW";
                       this.dispositionCode = "OOW";
                       this.remCode = "OWS";
                       this.condition = "SCRAP";
                       setLocation(xLenovoDictionary._locations["LOU_SCRAP"]);
                   }
               }
          }//OW

         if (errorFound==true)
         {
             strResult = "Wrong data input, please verify CID and Repairable flexfields";
         }
         else
         {
             destWhr = this.destWhr;
             destZone = this.destZone;
             destBin = this.destBin;
             warrantyStatus = this.warrantyStatus;
             dispositionCode = this.dispositionCode;
             remCode = this.remCode;
             CompCond = this.condition;
         }
        }//strerror
        
        }



        private void setRemCode(string remStr, string remCode)
        {
            if (remStr.IndexOf(remCode) != -1) this.remCode = "TBD";
            else this.remCode = remCode;
        }
        private void setLocation(string locStr)
        {
            int setCtn = 0;
            string[] loc = locStr.Split('/');
            foreach (string value in loc)
            {
                switch (setCtn)
                {
                    case 0:
                        destWhr = value;
                        break;
                    case 1:
                        destZone = value;
                        break;
                    case 2:
                        destBin = value;
                        break;
                }
                setCtn = setCtn + 1;
            }
        }
    }
}
