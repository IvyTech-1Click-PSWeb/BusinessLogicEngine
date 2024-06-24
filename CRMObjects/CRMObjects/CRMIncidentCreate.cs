using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CRMObjects.CRMIncidentCreate
{

    [XmlRoot("dataroot")]
    public class IncidentCreate : List<Report>
    {
        
    }

    public class Report
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public string Service_Ticket_Number { get; set; }
        public string Serial_Number { get; set; }
        public string Warranty_Category { get; set; }
        public string Override_Reason { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Closed_Date { get; set; }
        public string Created_Date { get; set; }
        public string Customer_ID_Name { get; set; }
        public string Address_Line_0 { get; set; }
        public string Address_Line_1 { get; set; }
        public string Address_Line_2 { get; set; }
        public string Country { get; set; }
        public string CAM_Postal_Code { get; set; }
        public string CAM_FirstTelephoneNo { get; set; }
        public string E_Mail_Address { get; set; }
        public string Problem_Log { get; set; }

    }


    //list Built for the Results of the Entries

    
}
