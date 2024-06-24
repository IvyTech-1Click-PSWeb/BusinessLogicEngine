using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Globalization;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using JGS.Web.TriggerProviders;
using JGS.DAL;
using System.Web.Configuration;

namespace JGS.Web.TriggerProviders
{
    public class HP_ManufactureDate
    {
        private DateTime date;
        private string fieldName1 = null;
        private string fieldName2 = null;
        private string fieldPattern1 = null;
        private string fieldPattern2 = null;
        private string fieldPattern3 = null;
        private string fieldPattern4 = null;
        private string slqErrorMessage = null;
        private int dayOfWeek = -1;
        private int dayOfMonth = -1;
        private int weekOfYear = -1;
        private int monthOfYear = -1;
        private int year = -1;
        private int warrantyPeriod = -1;
        private bool manualCalculation = false;
        private string connectionString = null;
        private StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
        private bool warrsetup = false;
        private string schemaName = "WEBAPP1";

        public HP_ManufactureDate() { }//default constructor

        public HP_ManufactureDate(string connectionString, string manuDateString, string locationName, string clientName, string contractName,
            string supplier, string platformNameOrProductClass, string subClass, string vendorName, string username)
        {
            this.connectionString = connectionString;
            this.setDate(manuDateString, locationName, clientName, contractName, supplier, platformNameOrProductClass, subClass,
                            vendorName, username);
        }

        //GETTER METHODS 
        public String getFieldName1()
        {
            return this.fieldName1;
        }

        public String getFieldName2()
        {
            return this.fieldName2;
        }

        public String getFieldPattern1()
        {
            return this.fieldPattern1;
        }

        public String getFieldPattern2()
        {
            return this.fieldPattern2;
        }

        public String getFieldPattern3()
        {
            return this.fieldPattern3;
        }

        public String getFieldPattern4()
        {
            return this.fieldPattern4;
        }

        public int getDayOfWeek()
        {
            return this.dayOfWeek;
        }

        public int getDayOfMonth()
        {
            return this.dayOfMonth;
        }

        public int getWeekOfYear()
        {
            return this.weekOfYear;
        }

        public int getMonthOfYear()
        {
            return this.monthOfYear;
        }

        public int getYear()
        {
            return this.year;
        }

        public DateTime getDate()
        {
            return this.date;
        }

        public int getWarrantyPeriod() 
        {
		 return this.warrantyPeriod;
	    }

        public string getSQLErrorMessage()
        {
            return this.slqErrorMessage;
        }

        public bool getManualCalculation()
        {
            return this.manualCalculation;
        }

        public void setDate(string manuDateString, string locationName, string clientName, string contractName,
			string supplier, string platformNameOrProductClass, string subClass, string vendorName, string username ) 
	     {
		   fieldName1 = null;
		   fieldName2 = null;
		   fieldPattern1 = null;
		   fieldPattern2 = null;
		   fieldPattern3 = null;
		   fieldPattern4 = null;
		   slqErrorMessage = null;
		   dayOfWeek = -1;
		   dayOfMonth = -1;
		   weekOfYear = -1;
		   monthOfYear = -1;
		   year = -1;
		   warrantyPeriod = -1;
		   manualCalculation = false;


		   string error = null;

           if (!string.IsNullOrEmpty(locationName) && !string.IsNullOrEmpty(clientName)
               && !string.IsNullOrEmpty(contractName)
                 && !string.IsNullOrEmpty(supplier) && (supplier.Equals("ODM", ignoreCase) || supplier.Equals("OEM", ignoreCase))
                   && !string.IsNullOrEmpty(platformNameOrProductClass)
                     //&& !string.IsNullOrEmpty(subClass)for odm is null
                       && !string.IsNullOrEmpty(vendorName))
           {

               //Variables to store DB data
               string manuDateName1 = null;
               string manuDateName2 = null;
               string manuDatePattern1 = null;
               string dayRange1 = null;
               string weekRange1 = null;
               string monthRange1 = null;
               string yearRange1 = null;
               string manuDatePattern2 = null;
               string dayRange2 = null;
               string weekRange2 = null;
               string monthRange2 = null;
               string yearRange2 = null;
               string manuDatePattern3 = null;
               string dayRange3 = null;
               string weekRange3 = null;
               string monthRange3 = null;
               string yearRange3 = null;
               string manuDatePattern4 = null;
               string dayRange4 = null;
               string weekRange4 = null;
               string monthRange4 = null;
               string yearRange4 = null;
               string manuDatePattern = null;
               string dayRange = null;
               string weekRange = null;
               string monthRange = null;
               string yearRange = null;
               string warrantyPeriod1String = null;
               string warrantyPeriod2String = null;
               int warrantyPeriod1 = -1;
               int warrantyPeriod2 = -1;
               int wrntyPeriod = -1;

               DataSet  warrMatrix = new DataSet();
               OracleParameter[] myParam = new OracleParameter[9];

               myParam[0] = new OracleParameter("locationName", OracleDbType.Varchar2, locationName, ParameterDirection.Input);
               myParam[1] = new OracleParameter("clientName", OracleDbType.Varchar2, clientName, ParameterDirection.Input);
               myParam[2] = new OracleParameter("contractName", OracleDbType.Varchar2, contractName, ParameterDirection.Input);
               myParam[3] = new OracleParameter("vendorName", OracleDbType.Varchar2, vendorName, ParameterDirection.Input);
               myParam[4] = new OracleParameter("supplier", OracleDbType.Varchar2, supplier, ParameterDirection.Input);
               myParam[5] = new OracleParameter("platformNameOrProductClass", OracleDbType.Varchar2, platformNameOrProductClass, ParameterDirection.Input);
               myParam[6] = new OracleParameter("subClass", OracleDbType.Varchar2, subClass, ParameterDirection.Input);
               myParam[7] = new OracleParameter("username", OracleDbType.Varchar2, username, ParameterDirection.Input);
               myParam[8] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

               warrMatrix = ODPNETHelper.ExecuteDataset(this.connectionString, CommandType.StoredProcedure, schemaName+".HPWURComponentsFA.hpwarrantymatrix", myParam);

               if (warrMatrix == null || warrMatrix.Tables.Count == 0 || warrMatrix.Tables[0].Rows.Count==0) 
               {
                   error = "Platform-ProductClass " + platformNameOrProductClass + " Vendor Name " + vendorName
							+" Supplier " +supplier + " Combination does not have an Entry in the Warranty Matrix Table";
               }
               if (error == null && warrMatrix.Tables[0].Rows.Count > 0)
               {
                   foreach (DataRow DR in warrMatrix.Tables[0].Rows)
                   {
                     try
                     {
                         manuDateName1 = isNull(DR["MANU_DATE_NAME1"].ToString());
                         warrantyPeriod1String = isNull(DR["WARRANTY_PERIOD1"].ToString());
                         manuDateName2 = isNull(DR["MANU_DATE_NAME2"].ToString());
                         warrantyPeriod2String = isNull(DR["WARRANTY_PERIOD2"].ToString());
                         manuDatePattern1 = isNull(DR["MANU_DATE_PATTERN1"].ToString());
                         dayRange1 = isNull(DR["DAY_RANGE1"].ToString());
                         weekRange1 = isNull(DR["WEEK_RANGE1"].ToString());
                         monthRange1 = isNull(DR["MONTH_RANGE1"].ToString());
                         yearRange1 = isNull(DR["YEAR_RANGE1"].ToString());
                         manuDatePattern2 = isNull(DR["MANU_DATE_PATTERN2"].ToString());
                         dayRange2 = isNull(DR["DAY_RANGE2"].ToString());
                         weekRange2 = isNull(DR["WEEK_RANGE2"].ToString());
                         monthRange2 = isNull(DR["MONTH_RANGE2"].ToString());
                         yearRange2 = isNull(DR["YEAR_RANGE2"].ToString());
                         manuDatePattern3 = isNull(DR["MANU_DATE_PATTERN3"].ToString());
                         dayRange3 = isNull(DR["DAY_RANGE3"].ToString());
                         weekRange3 = isNull(DR["WEEK_RANGE3"].ToString());
                         monthRange3 = isNull(DR["MONTH_RANGE3"].ToString());
                         yearRange3 = isNull(DR["YEAR_RANGE3"].ToString());
                         manuDatePattern4 = isNull(DR["MANU_DATE_PATTERN4"].ToString());
                         dayRange4 = isNull(DR["DAY_RANGE4"].ToString());
                         weekRange4 = isNull(DR["WEEK_RANGE4"].ToString());
                         monthRange4 = isNull(DR["MONTH_RANGE4"].ToString());
                         yearRange4 = isNull(DR["YEAR_RANGE4"].ToString());

                     }
                     catch (Exception ex) { error = ex.ToString(); } 
                   } //data row
                } //matrix table count             

               if (error == null)
               {
                   if (manuDateName1 != null && manuDateName1.Equals("MANUAL", ignoreCase))
                   {
                       manualCalculation = true;
                   }
                   else
                   {
                       if (manuDateName1!=null)
                       {
                           fieldName1 = manuDateName1;
                       }
                       if (manuDateName2!=null)
                       {
                           fieldName2 = manuDateName2;
                       }
                       if (manuDatePattern1!=null)
                       {
                           fieldPattern1 = manuDatePattern1;
                       }
                       if (manuDatePattern2!=null)
                       {
                           fieldPattern2 = manuDatePattern2;
                       }
                       if (manuDatePattern3!=null)
                       {
                           fieldPattern3 = manuDatePattern3;
                       }
                       if (manuDatePattern4!=null)
                       {
                           fieldPattern4 = manuDatePattern4;
                       }
                       if (manuDateString!=null)
                       {
                           if (warrantyPeriod1String!=null)
                           {
                               warrantyPeriod1 = Convert.ToInt32(warrantyPeriod1String);
                           }
                           if (warrantyPeriod2String!=null)
                           {
                               warrantyPeriod2 = Convert.ToInt32(warrantyPeriod2String);
                           }
                           if (manuDatePattern1!=null && manuDateString.Length == manuDatePattern1.Length)
                           {
                               wrntyPeriod = warrantyPeriod1;
                               manuDatePattern = manuDatePattern1;
                               dayRange = dayRange1;
                               weekRange = weekRange1;
                               monthRange = monthRange1;
                               yearRange = yearRange1;
                           }
                           else if (manuDatePattern2 !=null && manuDateString.Length == manuDatePattern2.Length)
                           {
                               wrntyPeriod = warrantyPeriod1;
                               manuDatePattern = manuDatePattern2;
                               dayRange = dayRange2;
                               weekRange = weekRange2;
                               monthRange = monthRange2;
                               yearRange = yearRange2;
                           }
                           else if (manuDatePattern3!=null && manuDateString.Length == manuDatePattern3.Length)
                           {
                               if (manuDateName2 == null)
                               {
                                   wrntyPeriod = warrantyPeriod1;
                               }
                               else
                               {
                                   wrntyPeriod = warrantyPeriod2;
                               }
                               manuDatePattern = manuDatePattern3;
                               dayRange = dayRange3;
                               weekRange = weekRange3;
                               monthRange = monthRange3;
                               yearRange = yearRange3;
                           }
                           else if (manuDatePattern4 != null && manuDateString.Length == manuDatePattern4.Length)
                           {
                               if (manuDateName2 == null)
                               {
                                   wrntyPeriod = warrantyPeriod1;
                               }
                               else
                               {
                                   wrntyPeriod = warrantyPeriod2;
                               }
                               manuDatePattern = manuDatePattern4;
                               dayRange = dayRange4;
                               weekRange = weekRange4;
                               monthRange = monthRange4;
                               yearRange = yearRange4;
                           }
                           else
                           {
                               this.slqErrorMessage = "Wrong CT/SeialNumber/Date Code length it does not match to any pattern in HP_WARRANTY_MATRIX table";
                           }

                           if ( weekRange == null && yearRange == null && manuDatePattern != null ) 
                           {
                               this.calculateDatePattern(manuDateString, manuDatePattern, locationName, clientName, contractName, username);
						   } 
                           else if ( dayRange == null && weekRange != null && yearRange != null ) 
                           {
							   this.calculateDate( manuDateString, manuDatePattern, weekRange, yearRange, username );
						   } 
                           else if ( dayRange != null && weekRange != null && yearRange != null ) 
                           {
							   this.calculateDate( manuDateString, manuDatePattern, dayRange, weekRange, yearRange, username );
						   } 
                           else if ( dayRange == null && monthRange != null && yearRange != null ) 
                           {
							   this.calculateDate( manuDateString, manuDatePattern, monthRange, yearRange, username );
						   } 
                           else if ( dayRange != null && monthRange != null && yearRange != null ) 
                           {
							this.calculateDate( manuDateString, manuDatePattern, dayRange, monthRange, yearRange, username );
						   }
                       
                          if ( this.date != null && this.date.Year != 0001) 
                          {
                              //change warrsetup

                            if (supplier.Equals("OEM", ignoreCase))
                            {
                                if (platformNameOrProductClass.Equals("DRIVE",ignoreCase) && subClass.Equals("HDD",ignoreCase)
                                    && (vendorName.Equals("Samsung",ignoreCase)||vendorName.Equals("Seagate",ignoreCase)))
                                {
                                    DateTime datePoint = new DateTime(2012, 1, 1);
                                    string datePointF = string.Format("{0:yyyyMd}", datePoint);
                                    string dateManuF  = string.Format("{0:yyyyMd}",this.date);
                                    if (datePointF.Equals(dateManuF) || int.Parse(dateManuF) > int.Parse(datePointF))
                                    {
                                        if (!string.IsNullOrEmpty(warrantyPeriod2String) && warrantyPeriod2String.Length != 0)
                                        {
                                            this.warrantyPeriod = int.Parse(warrantyPeriod2String);
                                            warrsetup = true;
                                        }
                                        else {
                                            error = "Couldn't find WarrantyPeriod2 info for product class " + platformNameOrProductClass +
                                                " productSubClass " + subClass + " vendor " + vendorName;
                                        }
                                            
                                    }
                                }
                              
                            }

                            if (!warrsetup)
                            {
                                if (wrntyPeriod != -1)
                                {
                                    this.warrantyPeriod = wrntyPeriod;
                                }
                                else
                                {
                                    this.calculatePeriod(manuDateString, manuDatePattern, locationName, clientName, contractName, username);
                                }
                            }
						  } //date
                      }//manudate isnotnullorempty
                   }//else
               }//error
            }//loc,client,contr,supplier,plat and vendor
           
           if (error != null)
           {
               this.slqErrorMessage = error;
           }  

        }//setDate

       private void calculateDatePattern( string manuDateString, string manuDatePattern, string locationName,
			string clientName, string contractName, string userName )
        {
            string datestr = null;
            string errorcheck = null;
            DataSet calculateDateManuPattern = new DataSet();
            OracleParameter[] myParam = new OracleParameter[7];

            myParam[0] = new OracleParameter("manuDateString", OracleDbType.Varchar2, manuDateString, ParameterDirection.Input);
            myParam[1] = new OracleParameter("manuDatePattern", OracleDbType.Varchar2, manuDatePattern, ParameterDirection.Input);
            myParam[2] = new OracleParameter("locationName", OracleDbType.Varchar2, locationName, ParameterDirection.Input);
            myParam[3] = new OracleParameter("clientName", OracleDbType.Varchar2, clientName, ParameterDirection.Input);
            myParam[4] = new OracleParameter("contractName", OracleDbType.Varchar2, contractName, ParameterDirection.Input);
            myParam[5] = new OracleParameter("username", OracleDbType.Varchar2, userName, ParameterDirection.Input);
            myParam[6] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            calculateDateManuPattern = ODPNETHelper.ExecuteDataset(this.connectionString, CommandType.StoredProcedure, schemaName+ ".HPWURComponentsFA.calculateDateManuPattern", myParam);

            if (calculateDateManuPattern.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow DR in calculateDateManuPattern.Tables[0].Rows)
                {
                    try
                    {

                        errorcheck = isNull(DR["ERRORMSG"].ToString());
                        if (errorcheck != null && errorcheck.StartsWith("ERR:"))
                        {
                            this.slqErrorMessage = errorcheck;
                        }
                        else
                        {
                            this.weekOfYear = isNullInt(DR["WEEKOFYEAR"].ToString());
                            this.year = isNullInt(DR["YEAR"].ToString());
                            datestr = isNull(DR["MANUFACTUREDATE"].ToString());
                            if (datestr != null)
                            {
                                this.date = Convert.ToDateTime(datestr);
                            }
                        }
                    }
                    catch (Exception ex) { this.slqErrorMessage = ex.ToString(); }
                } //data row
            } //matrix table count             

         }//calculateDatePattern

       private void calculateDate(string manuDateString, string manuDatePattern, string weekOrMonthRange, string yearRange, string userName)
       {
           int dayOfWeek = -1;
           string datestr = null;
           DataSet calculateDateManuPattern = new DataSet();
           OracleParameter[] myParam = new OracleParameter[6];

           myParam[0] = new OracleParameter("manuDateString", OracleDbType.Varchar2, manuDateString, ParameterDirection.Input);
           myParam[1] = new OracleParameter("manuDatePattern", OracleDbType.Varchar2, manuDatePattern, ParameterDirection.Input);
           myParam[2] = new OracleParameter("weekOrMonthRange", OracleDbType.Varchar2, weekOrMonthRange, ParameterDirection.Input);
           myParam[3] = new OracleParameter("yearRange", OracleDbType.Varchar2, yearRange, ParameterDirection.Input);
           myParam[4] = new OracleParameter("username", OracleDbType.Varchar2, userName, ParameterDirection.Input);
           myParam[5] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

           calculateDateManuPattern = ODPNETHelper.ExecuteDataset(this.connectionString, CommandType.StoredProcedure, schemaName+ ".HPWURComponentsFA.calculateDateWM", myParam);

           if (calculateDateManuPattern.Tables[0].Rows.Count > 0)
           {
               foreach (DataRow DR in calculateDateManuPattern.Tables[0].Rows)
               {
                   try
                   {
                       dayOfWeek = isNullInt(DR["DAYOFWEEK"].ToString());
                       this.weekOfYear = isNullInt(DR["WEEKOFYEAR"].ToString());
                       this.dayOfMonth = isNullInt(DR["DAYOFMONTH"].ToString());
                       this.monthOfYear = isNullInt(DR["MONTHOFYEAR"].ToString());
                       this.year = isNullInt(DR["YEAR"].ToString());
                       datestr = isNull(DR["MANUDATE"].ToString());
                       if (datestr != null)
                       {
                           this.date = Convert.ToDateTime(datestr);
                       }
                       
                    }
                   catch (Exception ex) { this.slqErrorMessage = ex.ToString(); }
               } //data row
           } //matrix table count             
        }//calculateDate

       private void calculateDate(string manuDateString, string manuDatePattern, string dayRange, string weekOrMonthRange,
            string yearRange, string userName)
       {
           // calculateDateDay( manuDateString IN VARCHAR2, manuDatePattern IN VARCHAR2, dayRange IN VARCHAR2, weekOrMonthRange IN VARCHAR2,  yearRange IN VARCHAR, USERNAME IN VARCHAR2, OUT_CURSOR OUT type_cursor );
           int dayOfWeek = -1;
           string datestr = null;
           DataSet DateManuPattern = new DataSet();
           OracleParameter[] myParam = new OracleParameter[7];

           myParam[0] = new OracleParameter("manuDateString", OracleDbType.Varchar2, manuDateString, ParameterDirection.Input);
           myParam[1] = new OracleParameter("manuDatePattern", OracleDbType.Varchar2, manuDatePattern, ParameterDirection.Input);
           myParam[2] = new OracleParameter("dayRange", OracleDbType.Varchar2, dayRange, ParameterDirection.Input);
           myParam[3] = new OracleParameter("weekOrMonthRange", OracleDbType.Varchar2, weekOrMonthRange, ParameterDirection.Input);
           myParam[4] = new OracleParameter("yearRange", OracleDbType.Varchar2, yearRange, ParameterDirection.Input);
           myParam[5] = new OracleParameter("username", OracleDbType.Varchar2, userName, ParameterDirection.Input);
           myParam[6] = new OracleParameter("out_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

           DateManuPattern = ODPNETHelper.ExecuteDataset(this.connectionString, CommandType.StoredProcedure, schemaName+ ".HPWURComponentsFA.calculateDateDay", myParam);

           if (DateManuPattern.Tables[0].Rows.Count > 0)
           {
               foreach (DataRow DR in DateManuPattern.Tables[0].Rows)
               {
                   try
                   {
                       this.dayOfWeek = isNullInt(DR["DAYOFWEEK"].ToString());
                       this.weekOfYear = isNullInt(DR["WEEKOFYEAR"].ToString());
                       this.dayOfMonth = isNullInt(DR["DAYOFMONTH"].ToString());
                       this.monthOfYear = isNullInt(DR["MONTHOFYEAR"].ToString());
                       this.year = isNullInt(DR["YEAR"].ToString());
                       datestr = isNull(DR["MANUDATE"].ToString());
                       if (datestr != null)
                       {
                           this.date = Convert.ToDateTime(datestr);
                       }

                   }
                   catch (Exception ex) { this.slqErrorMessage = ex.ToString(); }
               } //data row
           } //matrix table count             

        }//calculateDate

       private void calculatePeriod(string manuDateString, string manuDatePattern, string locationName,
           string clientName, string contractName, string userName)
       { 
           string periord = null;
            List<OracleParameter> myParams = new List<OracleParameter>();
            myParams.Add(new OracleParameter("manuDateString", OracleDbType.Varchar2, manuDateString.Length, ParameterDirection.Input) { Value = manuDateString });
            myParams.Add(new OracleParameter("manuDatePattern", OracleDbType.Varchar2, manuDatePattern.Length, ParameterDirection.Input) { Value = manuDatePattern }); 
            myParams.Add(new OracleParameter("locationName", OracleDbType.Varchar2, locationName.Length, ParameterDirection.Input) { Value = locationName });
            myParams.Add(new OracleParameter("clientName", OracleDbType.Varchar2, clientName.Length, ParameterDirection.Input) { Value = clientName });
            myParams.Add(new OracleParameter("contractName", OracleDbType.Varchar2, contractName.Length, ParameterDirection.Input) { Value = contractName });
            myParams.Add(new OracleParameter("userName", OracleDbType.Varchar2, userName.Length, ParameterDirection.Input) { Value = userName });
            periord = Functions.DbFetch(this.connectionString, schemaName, "HPWURComponentsFA", "calculatePeriod", myParams);

            if (!string.IsNullOrEmpty(periord))
            {
                this.warrantyPeriod = Convert.ToInt32(periord);
            }
            else 
            {
                this.dayOfWeek = -1;
                this.dayOfMonth = -1;
                this.weekOfYear = -1;
                this.monthOfYear = -1;
                this.year = -1;
            }
       
       
       
       }


        private DateTime GetDateTime(int year, int week, int day, CultureInfo cultureInfo)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            int firstWeek = cultureInfo.Calendar.GetWeekOfYear(firstDayOfYear, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek);
            int dayOffSet = day - (int)cultureInfo.DateTimeFormat.FirstDayOfWeek;
            return firstDayOfYear.AddDays((week - (firstWeek + 1)) * 7 + dayOffSet);
        }

        private string isNull(string stringParam)
        {
            if (string.IsNullOrEmpty(stringParam)) return null;
            else return stringParam;
        }
        private int isNullInt(string stringParam)
        {
            if (string.IsNullOrEmpty(stringParam)) return -1;
            else return Convert.ToInt32(stringParam);
        }

    }
}
