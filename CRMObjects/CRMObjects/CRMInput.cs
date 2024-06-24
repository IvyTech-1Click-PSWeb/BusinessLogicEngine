using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace CRMObjects.CRMInput
{

    [Serializable]
    [XmlRoot("CRM1ClickService")]
    public class CRM1ClickService
    {
        [XmlElement("BLEHeader")]
        public BLEHeader header { get; set; }

        [XmlElement("BLEDetail")]
        public BLEDetail detail { get; set; }

        public CRM1ClickService()
        {
            BLEHeader bHeader = new BLEHeader();
            BLEDetail bDetail = new BLEDetail();

            header = bHeader;
            detail = bDetail;
        }
    }

    [Serializable]
    public class BLEHeader
    {
        [XmlElement("ProcessType")]
        public string processtype { get; set; }

        [XmlElement("ProcessName")]
        public string processname { get; set; }

        [XmlElement("Location")]
        public string location { get; set; }

        [XmlElement("Client")]
        public string client { get; set; }

        [XmlElement("Contract")]
        public string contract { get; set; }

        [XmlElement("OrderProcessType")]
        public string orderprocesstype { get; set; }

        [XmlElement(ElementName = "WorkCenter")]
        public string workcenter { get; set; }

        [XmlElement("UserObj")]
        public UserObj userobj { get; set; }

        //[XmlElement("HeaderFlexFields")]
        public List<FlexField> HeaderFlexFields { get; set; }

        public BLEHeader()
        {
            UserObj uo = new UserObj();
            List<FlexField> hff = new List<FlexField>();
            processtype = null;
            processname = null;
            location = null;
            client = null;
            contract = null;
            orderprocesstype = null;
            workcenter = null;
            userobj = uo;
            HeaderFlexFields = hff;
        }
    }



    [Serializable]
    public class BLEDetail
    {
        [XmlElement("CRMSiteCode")]
        public string crmsitecode { get; set; }

        [XmlElement("CRMInterfaceName")]
        public string crminterfacename { get; set; }

        [XmlElement("DispositionType")]
        public string dispositiontype { get; set; }

        [XmlElement("CustomerRMA")]
        public string customerrma { get; set; }

        [XmlElement("SerialNo")]
        public string serialno { get; set; }

        [XmlElement("CRMIncidentNO")]
        public string crmincidentno { get; set; }

        [XmlElement("OrderType")]
        public string ordertype { get; set; }

        [XmlElement("IncidentCreateDate")]
        public string incidentcreatedate { get; set; }

        [XmlElement("AgentName")]
        public string agentname { get; set; }

        [XmlElement("Result")]
        public string result { get; set; }

        [XmlElement("ResultMessage")]
        public string resultmessage { get; set; }

        [XmlElement("Geographic")] // either Ankara or Coventry, use to be Homelocation, comes from Geography on Desktop
        public string geographic { get; set; }

        [XmlElement("ShipTo")]
        public string shipto { get; set; }

        [XmlElement("ContactInfo")]
        public ShippingAddress contactinfo { get; set; }

        [XmlElement("PartAvailabilityInfo")]
        public PartAvailabilityInfo partavailabilityinfo { get; set; }

        [XmlElement("CreateOrderInfo")]
        public CreateOrderInfo createorderinfo { get; set; }

        [XmlElement("WholeUnitReturn")]
        public WholeUnitReturn wholeunitreturn { get; set; }

        //[XmlElement("wsFlexFields")]
        public List<FlexField> wsFlexFields { get; set; }
        public BLEDetail()
        {

            PartAvailabilityInfo pai = new PartAvailabilityInfo();
            CreateOrderInfo coi = new CreateOrderInfo();
            ShippingAddress ci = new ShippingAddress();
            WholeUnitReturn wur = new WholeUnitReturn();
            List<FlexField> wff = new List<FlexField>();
            crmsitecode = null;
            crminterfacename = null;
            dispositiontype = null;
            customerrma = null;
            serialno = null;
            crmincidentno = null;
            ordertype = null;
            incidentcreatedate = null;
            agentname = null;
            result = null;
            resultmessage = null;
            geographic = null;
            shipto = null;
            contactinfo = ci;

            partavailabilityinfo = pai;

            createorderinfo = coi;

            wholeunitreturn = wur;

            wsFlexFields = wff;

        }

    }


    [Serializable]
    public class UserObj
    {
        [XmlElement("HostName")]
        public string hostname { get; set; }

        [XmlElement("UserName")]
        public string username { get; set; }

        [XmlElement("Password")]
        public string password { get; set; }

        public UserObj()
        {
            hostname = null;
            username = null;
            password = null;
        }
    }

    [Serializable]
    public class PartAvailabilityInfo
    {
        [XmlElement("Product")]
        public string product { get; set; }

        // new
        [XmlElement("ProductLine")]
        public string productline { get; set; }

        [XmlElement("DispositionCode")]
        public string dispositioncode { get; set; }

        [XmlElement("Quantity")]
        public string quantity { get; set; }

        [XmlElement("Priority")]
        public string priority { get; set; }

        [XmlElement("ShippingTerms")]
        public string shippingterms { get; set; }

        [XmlElement("OrderDeliveryDate")]
        public string orderdeliverydate { get; set; }

        [XmlElement("Warranty")]
        public string warranty { get; set; }

        [XmlElement("CustomerInfo")]
        public CustomerInfo customerinfo { get; set; }

        [XmlElement("UnitPrice")]
        public PriceInfo unitprice { get; set; }

        [XmlElement("ReturnInfo")]
        public ReturnInfoPAI returninfo { get; set; }

        public PartAvailabilityInfo()
        {
            CustomerInfo ci = new CustomerInfo();
            PriceInfo pi = new PriceInfo();
            ReturnInfoPAI ri = new ReturnInfoPAI();
            dispositioncode = null;
            quantity = null;
            priority = null;
            shippingterms = null;
            orderdeliverydate = null;
            warranty = null;
            product = null;
            productline = null;
            customerinfo = ci;
            unitprice = pi;
            returninfo = ri;
        }
    }

    [Serializable]
    public class CreateOrderInfo
    {
        [XmlElement("OrderHeader")]
        public OrderHeader orderheader { get; set; }

        [XmlElement("OrderLineDetail")]
        public OrderLineDetail orderlinedetail { get; set; }

        [XmlElement("ReturnInfo")]
        public ReturnInfoCOI returninfo { get; set; }

        public CreateOrderInfo()
        {
            OrderHeader oh = new OrderHeader();
            OrderLineDetail old = new OrderLineDetail();
            ReturnInfoCOI ri = new ReturnInfoCOI();
            orderheader = oh;
            orderlinedetail = old;
            returninfo = ri;
        }
    }

    [Serializable]
    public class CustomerInfo
    {
        [XmlElement("CustomerName")]
        public string customername { get; set; }

        [XmlElement("ShippingAddress")]
        public ShippingAddress shippingaddress { get; set; }

        [XmlElement("BillingAddress")]
        public BillingAddress billingaddress { get; set; }

        public CustomerInfo()
        {
            ShippingAddress sa = new ShippingAddress();
            BillingAddress ba = new BillingAddress();
            customername = null;
            shippingaddress = sa;
            billingaddress = ba;
        }
    }

    [Serializable]
    public class PriceInfo
    {
        [XmlElement("UnitPrice")]
        public string unitprice { get; set; }

        [XmlElement("Currency")]
        public string currency { get; set; }

        public PriceInfo()
        {
            unitprice = null;
            currency = null;
        }
    }

    [Serializable]
    public class ReturnInfoPAI
    {
        [XmlElement("Result")]
        public string result { get; set; }

        [XmlElement("FailMessage")]
        public string failmessage { get; set; }

        [XmlElement("OriginalPartNo")]
        public string originalpartno { get; set; }

        [XmlElement("OrignialPartDesc")]
        public string orignialpartdesc { get; set; }

        [XmlElement("AltPartNo")]
        public string altpartno { get; set; }

        [XmlElement("AltPartDesc")]
        public string altpartdesc { get; set; }

        [XmlElement("Owner")]
        public string owner { get; set; }

        [XmlElement("Condition")]
        public string condition { get; set; }

        [XmlElement("Warehouse")]
        public string warehouse { get; set; }

        [XmlElement("ShipDateETA")]
        public string shipdateeta { get; set; }

        [XmlElement("Returnable")]
        public string returnable { get; set; }

        public ReturnInfoPAI()
        {
            result = null;
            failmessage = null;
            originalpartno = null;
            orignialpartdesc = null;
            altpartno = null;
            altpartdesc = null;
            owner = null;
            condition = null;
            warehouse = null;
            shipdateeta = null;
            returnable = null;
        }
    }

    [Serializable]
    public class ReturnInfoCOI
    {
        [XmlElement("RNRReferenceOrderID")]
        public string rnrreferenceorderid { get; set; }

        [XmlElement("Result")]
        public string result { get; set; }

        [XmlElement("FailMessage")]
        public string failmessage { get; set; }

        [XmlElement("Part_No_B")]
        public string replacepartnumber { get; set; }

        [XmlElement("Serial_No_B")]
        public string replaceserialnumber { get; set; }


        public ReturnInfoCOI()
        {
            rnrreferenceorderid = null;
            result = null;
            failmessage = null;
            replacepartnumber = null;
            replaceserialnumber = null;
        }
    }




    [Serializable]
    public class OrderHeader
    {

        [XmlElement("BTT")]
        public string btt { get; set; }

        [XmlElement("CustomerTradingPartner")]
        public string customertradingpartner { get; set; }

        [XmlElement("TradingPartner")]
        public string tradingpartner { get; set; }

        [XmlElement("TCC_Location")]
        public string tcc_location { get; set; }

        [XmlElement("TCC_City")] //this is either Intanbal or London, comes from TCC Location on Desktop
        public string tcc_city { get; set; }

        [XmlElement("OrderStatus")]
        public string orderstatus { get; set; }

        [XmlElement("CustomerInfo")]
        public CustomerInfo customerinfo { get; set; }

        [XmlElement("NOTE")]
        public string note { get; set; }
        public OrderHeader()
        {

            CustomerInfo ci = new CustomerInfo();
            btt = null;
            customertradingpartner = null;
            tradingpartner = null;
            tcc_location = null;
            tcc_city = null;
            orderstatus = null;
            note = null;
            customerinfo = ci;

        }

    }

 


    [Serializable]
    public class OrderLineDetail
    {
        [XmlElement("Part_No")]
        public string part_no { get; set; }

        [XmlElement("Quantity")]
        public string quantity { get; set; }

        [XmlElement("Condition")]
        public string condition { get; set; }

        [XmlElement("Priority")]
        public string priority { get; set; }

        [XmlElement("ShippingTerms")]
        public string shippingterms { get; set; }

        [XmlElement("OrderDeliveryDate")]
        public string orderdeliverydate { get; set; }

        [XmlElement("Warranty")]
        public string warranty { get; set; }

        [XmlElement("WarrantyCode")]
        public string warrantycode { get; set; }

        [XmlElement("PreferredShipWH")]
        public string preferredshipwh { get; set; }

        [XmlElement("ReturnLocation")]
        public string returnlocation { get; set; }

        [XmlElement("UnitPrice")]
        public string unitprice { get; set; }

        [XmlElement("Currency")]
        public string currency { get; set; }

        [XmlElement("NOTE")]
        public string note { get; set; }

        [XmlElement("ReasonCode")]
        public string reasoncode { get; set; }

        [XmlElement("ReasonNotes")]
        public string reasonnotes { get; set; }

        [XmlElement("Bin")]
        public string bin { get; set; }

        //[XmlElement("LineFlexFields")]
        public List<FlexField> LineFlexFields { get; set; }

        public OrderLineDetail()
        {
            List<FlexField> lff = new List<FlexField>();
            part_no = null;
            quantity = null;
            condition = null;
            priority = null;
            shippingterms = null;
            orderdeliverydate = null;
            warranty = null;
            warrantycode = null;
            preferredshipwh = null;
            returnlocation = null;
            unitprice = null;
            currency = null;
            note = null;
            reasoncode = null;
            reasonnotes = null;
            bin = null;
            LineFlexFields = lff;
        }
    }

    [Serializable]
    public class ShippingAddress
    {
        [XmlElement("ContactName")]
        public string contactname { get; set; }

        [XmlElement("ContactEmail")]
        public string contactemail { get; set; }

        [XmlElement("ContactPhone")]
        public ContactPhone contactphone { get; set; }

        [XmlElement("ContactAddress1")]
        public string contactaddress1 { get; set; }

        [XmlElement("ContactAddress2")]
        public string contactaddress2 { get; set; }

        [XmlElement("ContactAddress3")]
        public string contactaddress3 { get; set; }

        [XmlElement("ContactCity")]
        public string contactcity { get; set; }

        [XmlElement("ContactState")]
        public string contactstate { get; set; }

        [XmlElement("ContactPostalCode")]
        public string contactpostalcode { get; set; }

        [XmlElement("ContactCountry")]
        public string contactcountry { get; set; }

        public ShippingAddress()
        {
            ContactPhone cp = new ContactPhone();
            contactname = null;
            contactemail = null;
            contactaddress1 = null;
            contactaddress2 = null;
            contactaddress3 = null;
            contactcity = null;
            contactstate = null;
            contactpostalcode = null;
            contactcountry = null;
            contactphone = cp;
        }
    }

    [Serializable]
    public class BillingAddress
    {
        [XmlElement("ContactName")]
        public string contactname { get; set; }

        [XmlElement("ContactEmail")]
        public string contactemail { get; set; }

        [XmlElement("ContactPhone")]
        public ContactPhone contactphone { get; set; }

        [XmlElement("ContactAddress1")]
        public string contactaddress1 { get; set; }

        [XmlElement("ContactAddress2")]
        public string contactaddress2 { get; set; }

        [XmlElement("ContactAddress3")]
        public string contactaddress3 { get; set; }

        [XmlElement("ContactCity")]
        public string contactcity { get; set; }

        [XmlElement("ContactState")]
        public string contactstate { get; set; }

        [XmlElement("ContactPostalCode")]
        public string contactpostalcode { get; set; }

        [XmlElement("ContactCountry")]
        public string contactcountry { get; set; }

        public BillingAddress()
        {
            ContactPhone cp = new ContactPhone();
            contactname = null;
            contactemail = null;
            contactaddress1 = null;
            contactaddress2 = null;
            contactaddress3 = null;
            contactcity = null;
            contactstate = null;
            contactpostalcode = null;
            contactcountry = null;
            contactphone = cp;
        }
    }

    [Serializable]
    public class WholeUnitReturn
    {
        [XmlElement("Loaner")]
        public string loaner { get; set; }

        public WholeUnitReturn()
        {
            loaner = null;
        }
    }

    [Serializable]
    public class ContactPhone
    {
        [XmlElement("PhoneType")]
        public string phonetype { get; set; }

        [XmlElement("PhoneNo")]
        public string phoneno { get; set; }

        public ContactPhone()
        {
            phonetype = null;
            phoneno = null;
        }
    }

    [Serializable]
    public class TransactionInformation
    {
        [XmlElement("Messages")]
        public Messages messages { get; set; }

        [XmlElement("Status")]
        public string status { get; set; }

        [XmlElement("DistributedIdentifier")]
        public string distributedIdentifier { get; set; }

        [XmlElement("CreationTime")]
        public string creationtime { get; set; }

        public TransactionInformation()
        {
            Messages m = new Messages();
            status = null;
            distributedIdentifier = null;
            creationtime = null;
            messages = m;
        }
    }

    [Serializable]
    public class Messages
    {
        [XmlElement("Message1")]
        public string message1 { get; set; }

        [XmlElement("Message2")]
        public string message2 { get; set; }

        [XmlElement("Message3")]
        public string message3 { get; set; }

        public Messages()
        {
            message1 = null;
            message2 = null;
            message3 = null;
        }
    }


    [Serializable]
    public class FlexField
    {
        public string name { get; set; }
        public string value { get; set; }
    }


    /// <summary>
    /// Detailed Part information.
    /// </summary>
    [Serializable]
    public class Part
    {
        //public string Id { get; set; }
        //public string PartNumber { get; set; }
        //public string SerialNumber { get; set;  }
        //public string Location { get; set; }
        //public int Quantity { get; set; }
        //public DateTime DeliveryDate { get; set; }

        public string ErrorStr { get; set; }            // Error message
        public string OriginalPartDesc { get; set; }    // Original Part Desc
        public string ProductClass { get; set; }        // Product Class Name
        public string ProductSubclass { get; set; }     // Product Subclass Name
        public string RequestLineNo { get; set; }       //REQUEST_LINE_NO                
        public string PartNo { get; set; }              //PART_NO                       
        public string OwnerName { get; set; }           //OWNER_NAME                     
        public string ConditionName { get; set; }       //CONDITION_NAME                   
        public string Quantity { get; set; }            //QUANTITY
        public string WarehouseName { get; set; }       //WAREHOUSE_NAME                 
        public string ResrvPartNo { get; set; }         //RESRV_PART_NO                 
        public string ResrvOwnerName { get; set; }      //RESRV_OWNER_NAME    
        public string ResrvPartDesc  { get; set; }      //Reserved part desc.      
        public string ResrvConditionName { get; set; }  //RESRV_CONDITION_NAME           
        public DateTime EtaDate { get; set; }           //ETA_DATE
    }

    /// <summary>
    /// List of available parts
    /// </summary>
    [Serializable]
    [XmlRoot("Parts")]
    public class PartsList : List<Part>
    {
    }

    [Serializable]
    public class HDays
    {
        public string Holiday { get; set; }
        public string DayOfWeek { get; set; }
    }

    [Serializable]
    [XmlRoot("Holidays")]
    public class Holidays
    {
        [XmlElement("CtyCode")]
        public string countrycode { get; set; }
        public string OCD { get; set; }
        public string RCD { get; set; }
        public string resultMessage { get; set; }

        public List<HDays> HDaysFlexFields { get; set; }

        public Holidays()
        {
            List<HDays> hff = new List<HDays>();

            countrycode = null;
            resultMessage = null;
            HDaysFlexFields = hff;
        }
    }
}

