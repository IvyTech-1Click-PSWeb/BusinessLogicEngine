using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.Web.TriggerProviders;
using System.Globalization;

namespace JGS.Web.TriggerProviders
{
    public class HP_Warranty
    {
        private bool inWarranty = false;

        public HP_Warranty() { }

        
        public HP_Warranty(HP_ManufactureDate manufactureDate) 
        {
		 this.setWarranty( manufactureDate );
	    }

        public HP_Warranty(HP_ManufactureDate manufactureDate, int warrantyPeriod)
        {
		 this.setWarranty( manufactureDate, warrantyPeriod );
	    }

        public bool getInWarranty() 
        {
		 return this.inWarranty;
	    }

        
        public void setWarranty(HP_ManufactureDate manufactureDate) 
        {
		  if ( manufactureDate != null ) 
          {
			this.setWarranty( manufactureDate, manufactureDate.getWarrantyPeriod() );
		  }
	    }

        
        public void setWarranty(HP_ManufactureDate manufactureDate, int warrantyPeriod) 
        {
		  bool warranty = false;

		  if ( manufactureDate != null && manufactureDate.getDate() != null && warrantyPeriod >= 0 ) 
          {
            CultureInfo cultureInfo = new CultureInfo("en-US", false);
            int weekOfYear = manufactureDate.getWeekOfYear();
            int year = manufactureDate.getYear();
            int dayOfWeek = manufactureDate.getDayOfWeek();
            int dayOfMonth = manufactureDate.getDayOfMonth();
            int monthOfYear = manufactureDate.getMonthOfYear();
            int currDay = 0;
            int currWeekOfYear = 0;
            int currMonth = 0;
            int currYear = 0;
            string currFormat = null;
            string oowFormat = null;
            bool isEqual = false;
            DateTime currentDate = DateTime.Today;
            DateTime outOfWarrantyDate = manufactureDate.getDate();
            DateTime newOOWDate = outOfWarrantyDate.AddMonths(warrantyPeriod);
            int oow = newOOWDate.CompareTo(currentDate);

            if ((weekOfYear == -1 && year == -1 && outOfWarrantyDate != null)
                || (dayOfWeek == -1 && weekOfYear != -1 && outOfWarrantyDate!=null))
            {
              currWeekOfYear = cultureInfo.Calendar.GetWeekOfYear(currentDate, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek);
              currYear = currentDate.Year;
              currFormat = Convert.ToString(currWeekOfYear) + Convert.ToString(currYear);
              oowFormat = Convert.ToString(weekOfYear) + Convert.ToString(year);
              if (currFormat.Equals(oowFormat))
              {
                  isEqual = true;
              }
            } else if(dayOfWeek != -1 && weekOfYear != -1 && year != -1 ) 
			{
                int currdayOfWeekInt = dayOfWeekIntCal(currentDate.DayOfWeek.ToString());
                currWeekOfYear = cultureInfo.Calendar.GetWeekOfYear(currentDate, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek);
                currYear = currentDate.Year;
                currFormat = Convert.ToString(currdayOfWeekInt) + Convert.ToString(currWeekOfYear) + Convert.ToString(currYear);
                oowFormat = Convert.ToString(dayOfWeek) + Convert.ToString(weekOfYear) + Convert.ToString(year);
                if (currFormat.Equals(oowFormat))
                {
                    isEqual = true;
                }
            } else if( dayOfMonth == -1 && monthOfYear != -1 && year != -1 ) 
            {
                  currMonth = currentDate.Month;
                  currYear = currentDate.Year;
                  currFormat = Convert.ToString(currMonth) + Convert.ToString(currYear);
                  oowFormat = Convert.ToString(monthOfYear) + Convert.ToString(year);
                  if (currFormat.Equals(oowFormat))
                  {
                      isEqual = true;
                  }

            }else if (dayOfMonth != -1 && monthOfYear != -1 && year != -1)
            {
                currDay =currentDate.Day;
                currMonth = currentDate.Month;
                currYear = currentDate.Year;
                currFormat = Convert.ToString(currDay) + Convert.ToString(currMonth) + Convert.ToString(currYear);
                oowFormat = Convert.ToString(dayOfMonth) + Convert.ToString(monthOfYear) + Convert.ToString(year);
                if (currFormat.Equals(oowFormat))
                {
                    isEqual = true;
                }
            }
            
            if (warrantyPeriod == 99 || oow > 0 || isEqual==true)
			{
				warranty = true;
			}
		  }
		
		 this.inWarranty = warranty;
	    }

        private int dayOfWeekIntCal(string day)
        {
            if (day.Equals("Sunday")) return 1;
            else if (day.Equals("Monday")) return 2;
            else if (day.Equals("Tuesday")) return 3;
            else if (day.Equals("Wednesday")) return 4;
            else if (day.Equals("Thursday")) return 5;
            else if (day.Equals("Friday")) return 6;
            else if (day.Equals("Saturday")) return 7;
            else return 1;//default 1
        }
    }
}
