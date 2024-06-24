using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.DAL;

namespace JGS.Web.TriggerProviders
{
    public class Comp_IssueRemove_Location
    {
        public static void getCompIssueRemoveLoc(string connectionString, int locationID, int clientID, int contractID, int optId, int wcId, int conditionId, string warranty,
         string groupDisp, string remCode, string dispCode, string vendorName,string issueRemove, string userName, ref string warehouse,
          ref string zone, ref string bin, out string errorMsg)
        {
          errorMsg = null;
          string v_errormsg = null;
          DataSet spDataSet = new DataSet();
          OracleParameter[] myParam = new OracleParameter[14];
          myParam[0] = new OracleParameter("locationID", OracleDbType.Int32, locationID, ParameterDirection.Input);
          myParam[1] = new OracleParameter("clientID", OracleDbType.Int32, clientID, ParameterDirection.Input);
          myParam[2] = new OracleParameter("contractID", OracleDbType.Int32, contractID, ParameterDirection.Input);
          myParam[3] = new OracleParameter("optId", OracleDbType.Int32, optId, ParameterDirection.Input);
          myParam[4] = new OracleParameter("wcId", OracleDbType.Int32, wcId, ParameterDirection.Input);
          myParam[5] = new OracleParameter("conditionId", OracleDbType.Int32, conditionId, ParameterDirection.Input);
          myParam[6] = new OracleParameter("warrranty", OracleDbType.Varchar2, warranty, ParameterDirection.Input);
          myParam[7] = new OracleParameter("groupDisp", OracleDbType.Varchar2, groupDisp, ParameterDirection.Input);
          myParam[8] = new OracleParameter("remCode", OracleDbType.Varchar2, remCode, ParameterDirection.Input);
          myParam[9] = new OracleParameter("dispCode", OracleDbType.Varchar2, dispCode, ParameterDirection.Input);
          myParam[10] = new OracleParameter("vendorName", OracleDbType.Varchar2, vendorName, ParameterDirection.Input);
          myParam[11] = new OracleParameter("issueRemove", OracleDbType.Varchar2, issueRemove, ParameterDirection.Input);
          myParam[12] = new OracleParameter("userName", OracleDbType.Varchar2, userName, ParameterDirection.Input);
          myParam[13] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
          spDataSet = ODPNETHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "WEBAPP1.comp_issueremove_location.getDestLocation", myParam);

          if (spDataSet.Tables[0].Rows.Count > 0)
           {
            foreach (DataRow DR in spDataSet.Tables[0].Rows)
              {
               try
                 {
                  v_errormsg = DR["ERRORMSG"].ToString();
                  if (!string.IsNullOrEmpty(v_errormsg))
                  {
                      errorMsg = v_errormsg;
                   }else
                    {
                      warehouse = DR["Warehouse"].ToString();
                      zone = DR["Zone"].ToString();
                      bin = DR["Bin"].ToString();
                      errorMsg = null;
                     }
                  }catch (Exception ex)
                  {
                     errorMsg = ex.ToString();
                  }
              }//foreach
           }//if
        }
     }
}
