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
    public class LENOVO_COMP_PART_TRG : TriggerProviderBase
    {

        public static Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{   
             {"XML_TRIGGERTYPE","/Trigger/Header/TriggerType"}
            ,{"XML_LOCATIONID","/Trigger/Header/LocationID"} 
            ,{"XML_USERNAME","/Trigger/Header/UserObj/UserName"}
            ,{"XML_RESULT","/Trigger/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Trigger/Detail/TriggerResult/Message"}
        };
        public override string Name { get; set; }

        public LENOVO_COMP_PART_TRG()
        {
            this.Name = "LENOVO_COMP_PART_TRG";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;
            string Schema_name = "WEBAPP1";
            string Package_name = "LENOVO_WUR_FA";
            int locationID;
            string triggerType;
            string UserName;

            //-- Get Trigger Type
            if (!Functions.IsNull(xmlIn, _xPaths["XML_TRIGGERTYPE"]))
                triggerType = Functions.ExtractValue(xmlIn, _xPaths["XML_TRIGGERTYPE"]).Trim().ToUpper();
            else
                return SetXmlError(returnXml, "Trigger type can not be found.");
            //-- Get Location Id
            if (!Functions.IsNull(xmlIn, _xPaths["XML_LOCATIONID"]))
                locationID = Int32.Parse(Functions.ExtractValue(xmlIn, _xPaths["XML_LOCATIONID"]));
            else
                return SetXmlError(returnXml, "Geography Id can not be found.");
            //-- Get USERNAME
            if (!Functions.IsNull(xmlIn, _xPaths["XML_USERNAME"]))
                UserName = Functions.ExtractValue(xmlIn, _xPaths["XML_USERNAME"]).Trim();
            else
                return SetXmlError(returnXml, "UserName could not be found.");

            /*************************************************Starts FA Trigger*************************************************/
            if (triggerType.ToUpper() == "FAREMOVE_VALIDATION")
            {
                XmlNodeList compList = xmlIn.SelectNodes("/Trigger/Detail/FailureAnalysis/DefectiveList/ComponentCodeList/Component");
                foreach (XmlNode defComp in compList)
                {
                    string mpn = defComp["ComponentPartNo"].InnerText.ToUpper();
                    string manufacturer = null;
                    string lenovoPN= null;

                    string strResult = null;
                    DataSet spDataSet = new DataSet();
                    OracleParameter[] myParam = new OracleParameter[4];
                    myParam[0] = new OracleParameter("locationID", OracleDbType.Int32, locationID, ParameterDirection.Input);
                    myParam[1] = new OracleParameter("mpn", OracleDbType.Varchar2, mpn, ParameterDirection.Input);
                    myParam[2] = new OracleParameter("UserName", OracleDbType.Varchar2, UserName, ParameterDirection.Input);
                    myParam[3] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                    spDataSet = ODPNETHelper.ExecuteDataset(this.ConnectionString, CommandType.StoredProcedure, Schema_name + "." + Package_name + ".getLenovoPN", myParam);

                    if (spDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow DR in spDataSet.Tables[0].Rows)
                        {
                            try
                            {
                                strResult = DR["ERRORMSG"].ToString();
                                if (!string.IsNullOrEmpty(strResult))
                                {
                                    return SetXmlError(returnXml, strResult);
                                }
                                else
                                {
                                    manufacturer = DR["MANUNAME"].ToString();
                                    lenovoPN = DR["LENOVOPN"].ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                return SetXmlError(returnXml, ex.ToString());
                            }
                        }
                    }

                    defComp["ComponentPartNo"].InnerText = lenovoPN;
                    defComp["Manufacturer"].InnerText = manufacturer;
                    defComp["ManuFacturerPart"].InnerText = mpn;

                    /*XmlNode manufacturerNode = xmlIn.CreateElement("Manufacturer");
                    manufacturerNode.InnerText = manufacturer;
                    defComp.AppendChild(manufacturerNode);

                    XmlNode mpnNode = xmlIn.CreateElement("ManuFacturerPart");
                    mpnNode.InnerText = mpn;
                    defComp.AppendChild(mpnNode);*/

                }
            }//triggerPoint
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
    }
}
