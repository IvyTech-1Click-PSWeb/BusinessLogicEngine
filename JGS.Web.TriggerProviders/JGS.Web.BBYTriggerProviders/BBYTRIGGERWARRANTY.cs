using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.Web.TriggerProviders;
using Oracle.DataAccess.Client;
using System.Data;
using System.Resources;
using Microsoft.VisualBasic;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERWARRANTY : JGS.Web.TriggerProviders.TriggerProviderBase
    {

        private Dictionary<string, string> _xPaths = new Dictionary<string, string>()
		{
			{"XML_SN","/Receiving/Detail/Order/Lines/Line/Items/Item/SerialNum"}  
            ,{"XML_FAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FixedAssetTag"} 
            ,{"XML_BTT","/Receiving/Header/BusinessTransactionType"} 
            ,{"XML_PN","/Receiving/Detail/Order/Lines/Line/PartNum"}    
            ,{"XML_WARRANTY","/Receiving/Detail/Order/Lines/Line/Items/Item/Warranty"}  
            ,{"XML_FFFAT","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='FF_FAT_LEG']/Value"}
            ,{"XML_FFYEAR","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='YEAR']/Value"}
            ,{"XML_FFMONTH","/Receiving/Detail/Order/Lines/Line/Items/Item/FlexFields/FlexField[Name='MONTH']/Value"}
            ,{"XML_USERNAME","/Receiving/Header/User/UserName"}     
            ,{"XML_RESULT","/Receiving/Detail/TriggerResult/Result"}
			,{"XML_MESSAGE","/Receiving/Detail/TriggerResult/Message"}            
		};

        public override string Name { get; set; }

        public BBYTRIGGERWARRANTY()
        {
            this.Name = "BBYTRIGGERWARRANTY";
        }

        public override XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            XmlDocument returnXml = xmlIn;

            //Build the trigger code here

            ////////////////////////////// Variables ///////////////////////////////////////////////////
            
            string UserName = string.Empty;
            string SN = string.Empty;
            string PN = string.Empty;
            string Manufacturer = string.Empty;
            string FAT = string.Empty;
            string BTT = string.Empty;
            string FF_FAT_LEG = string.Empty;
            string FF_MONTH = string.Empty;
            string FF_YEAR = string.Empty;
            string Privilege = string.Empty;            
            
            //-- Get SN
            if (!Functions.IsNull(xmlIn, _xPaths["XML_SN"]))
            {
                SN = Functions.ExtractValue(xmlIn, _xPaths["XML_SN"]).Trim().ToUpper();
            }
            else
            {
                return SetXmlError(returnXml, "SN can not be found.");
            }

            //-- Get FixedAssetTag
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FAT"]))
            {
                FAT = Functions.ExtractValue(xmlIn, _xPaths["XML_FAT"]).Trim().ToUpper();
            }
            else
            {
                FAT = "";
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

            //-- Get Part Number
            if (!Functions.IsNull(xmlIn, _xPaths["XML_PN"]))
            {
                PN = Functions.ExtractValue(xmlIn, _xPaths["XML_PN"]);
            }
            else
            {
                return SetXmlError(returnXml, "Part Number can not be found.");
            }
                        
            //-- Get FF FF_FAT_LEG
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFFAT"]))
            {
                FF_FAT_LEG = Functions.ExtractValue(xmlIn, _xPaths["XML_FFFAT"]).Trim().ToUpper();
            }
            else
            {
                FF_FAT_LEG = "";
            }

            //-- Get FF YEAR
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFYEAR"]))
            {
                FF_YEAR = Functions.ExtractValue(xmlIn, _xPaths["XML_FFYEAR"]);
            }
            else
            {
                FF_YEAR = "";
            }

            //-- Get FF MONTH
            if (!Functions.IsNull(xmlIn, _xPaths["XML_FFMONTH"]))
            {
                FF_MONTH = Functions.ExtractValue(xmlIn, _xPaths["XML_FFMONTH"]);
            }
            else
            {
                FF_MONTH = "";
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

            if (BTT.Trim().ToUpper() == "CR")
            {

                //////////////////// Check if User has RECEIPT privileges /////////////////////
                List<OracleParameter> myParams2;
                myParams2 = new List<OracleParameter>();
                myParams2.Add(new OracleParameter("Value", OracleDbType.Varchar2, "RECEIPT".Length, ParameterDirection.Input) { Value = "RECEIPT" });
                myParams2.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName.ToUpper() });
                Privilege = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetPriv", myParams2);

                // If variable Privilege is null, fill value with ""
                if (Privilege == null)
                {
                    Privilege = "";
                }

                if (Privilege.ToUpper() != "RECEIPT")
                {

                    //////////////////// Get Manufacture  /////////////////////

                    List<OracleParameter> myParamsMan;
                    myParamsMan = new List<OracleParameter>();
                    myParamsMan.Add(new OracleParameter("PartNo", OracleDbType.Varchar2, PN.Length, ParameterDirection.Input) { Value = PN });
                    myParamsMan.Add(new OracleParameter("UserName", OracleDbType.Varchar2, UserName.Length, ParameterDirection.Input) { Value = UserName });
                    Manufacturer = Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSBBYRECEIPT", "GetManufacture", myParamsMan);

                    if (Manufacturer == null)
                    {
                        Manufacturer = "";
                    }

                    if (Manufacturer.ToUpper() == "HTC")
                    {

                        int year;
                        int month;
                        int day;

                        year = "7890123".IndexOf(SN.Substring(2, 1)) + 2007;
                        month = "123456789ABC".IndexOf(SN.Substring(3, 1)) + 1;
                        day = "123456789ABCDEFGHJKLMNPRSTVWXYZ".IndexOf(SN.Substring(4, 1)) + 1;

                        if (year == 2006 || month == 0 || day == 0)
                        {
                            return SetXmlError(returnXml, "El formato del serial es incorrecto revise el formato del serial o comunique a su supervisor");
                        }

                        DateTime fechaAntigua = new DateTime(year, month, day);
                        DateTime fechaNueva = DateTime.Now;

                        int diferenciaEnMeses = CalcularMeses(fechaAntigua, fechaNueva);

                        if (diferenciaEnMeses > 15)
                        {
                            SetXmlFFWarranty(returnXml, "false");
                        }
                        else
                        {
                            SetXmlFFWarranty(returnXml, "true");
                        }
                    }
                    else if (Manufacturer.ToUpper() == "MOTOROLA")
                    {
                        //if (FF_FAT_LEG == "MSN")
                        //{
                        if (PN != "9750772")
                        {
                            if (FAT == "")
                            {
                                return SetXmlError(returnXml, "Necesita ingresar Fixed Asset Tag para unidad Motorola");
                            }

                            int d = 0;
                            int m = 0;
                            int m1 = 0;
                            int y = 0;

                            y = "JLNQS".IndexOf(FAT.Substring(4, 1)) + 2008;
                            m = "ACEGJLNQSUWY".IndexOf(FAT.Substring(5, 1)) + 1;
                            m1 = "BDFHKMPRTVXZ".IndexOf(FAT.Substring(5, 1)) + 1;
                            d = 15;

                            if (y == 2007)
                            {
                                return SetXmlError(returnXml, "El formato del FAT es incorrecto revise el formato del FAT o comunique a su supervisor");
                            }

                            DateTime fechaAntigua;

                            if (m == 0)
                            {
                                if (m1 == 0)
                                {
                                    return SetXmlError(returnXml, "El formato del FAT es incorrecto revise el formato del FAT o comunique a su supervisor");
                                }

                                fechaAntigua = new DateTime(y, m1, d);
                            }
                            else
                            {
                                fechaAntigua = new DateTime(y, m, d);
                            }

                            DateTime fechaNueva = DateTime.Now;

                            int diferenciaEnMeses = CalcularMeses(fechaAntigua, fechaNueva);

                            if (diferenciaEnMeses > 15)
                            {
                                SetXmlFFWarranty(returnXml, "false");
                            }
                            else
                            {
                                SetXmlFFWarranty(returnXml, "true");
                            }
                        }
                        //}
                    }
                    else if (Manufacturer.ToUpper() == "SAMSUNG" || Manufacturer.ToUpper() == "LGELECT")
                    {
                        if ((FF_YEAR == "") || (FF_MONTH == ""))
                        {
                            return SetXmlError(returnXml, "Los FlexFields YEAR y MONTH no deben de estar vacios, por favor elija una opcion");
                        }

                        int d = 30;

                        DateTime fechaAntigua = new DateTime(Convert.ToInt32(FF_YEAR), Convert.ToInt32(FF_MONTH), Convert.ToInt32(d));
                        DateTime fechaNueva = DateTime.Now;

                        int diferenciaEnMeses = CalcularMeses(fechaAntigua, fechaNueva);

                        if (diferenciaEnMeses > 15)
                        {
                            SetXmlFFWarranty(returnXml, "false");
                        }
                        else
                        {
                            SetXmlFFWarranty(returnXml, "true");
                        }
                    }
                    else if (Manufacturer.ToUpper() == "NOKIA")
                    {

                        if (FAT == "")
                        {
                            return SetXmlError(returnXml, "Necesita ingresar Fixed Asset Tag para unidad Nokia");
                        }

                        int d = 0;
                        int m = 0;
                        int y = 0;

                        m = "ABCDEFGHIJKL".IndexOf(FAT.Substring(7, 1)) + 1;
                        y = "QRS".IndexOf(FAT.Substring(8, 1)) + 2009;

                        if (System.Text.RegularExpressions.Regex.IsMatch(FAT.Substring(9, 2).ToUpper(), "^[0-9]+$"))
                            d = Convert.ToInt32(FAT.Substring(9, 2));
                        else
                        {
                            return SetXmlError(returnXml, "El formato del FAT es incorrecto revise el formato del FAT o comunique a su supervisor");
                        }

                        if (m == 0 || y == 2008)
                        {
                            return SetXmlError(returnXml, "El formato del FAT es incorrecto revise el formato del FAT o comunique a su supervisor");
                        }

                        DateTime fechaAntigua = new DateTime(y, m, d);
                        DateTime fechaNueva = DateTime.Now;

                        int diferenciaEnMeses = CalcularMeses(fechaAntigua, fechaNueva);

                        if (diferenciaEnMeses > 12)
                        {
                            SetXmlFFWarranty(returnXml, "false");
                        }
                        else
                        {
                            SetXmlFFWarranty(returnXml, "true");
                        }
                    }
                }
            }
           // Set Return Code to Success            
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

        private void SetXmlFFWarranty(XmlDocument returnXml, String Result)
        {
            Functions.UpdateXml(ref returnXml, _xPaths["XML_WARRANTY"], Result);
        }

        public static int CalcularMeses(DateTime fechaComienzo, DateTime fechaFin)
        {
            fechaComienzo = fechaComienzo.Date;
            fechaFin = fechaFin.Date;
            int count = 0;
            while (fechaComienzo < fechaFin)
            {
                fechaComienzo = fechaComienzo.AddMonths(1);
                count++;
            }
            return count;
        }

    }
}

