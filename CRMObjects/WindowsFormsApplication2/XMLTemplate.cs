using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WindowsFormsApplication2.XMLTemp
{
   
    //public _ReferenceOrderData ReferenceOrderData { get; set; } 

    //#region "Constructor"
    //public CreateOrderObject()
    //{
    //    _ReferenceOrderData referenceorderdata = new _ReferenceOrderData();
    //    ReferenceOrderData = referenceorderdata;
    //}
   
    [Serializable]
    public class _ReferenceOrderData 
    {
        public string ID { get; set; }
        public _BillingAddress BillingAddress { get; set; }
        public _ShippingAddress ShippingAddress { get; set; }
        public string BillingTerms { get; set; }
        public string BusinessTransactionType { get; set; }
        public string Client { get; set; }
        public string ClientReferenceNumber1 { get; set; }
        public string ClientReferenceNumber2 { get; set; }
        public string Contract { get; set; }
        public string Customer { get; set; }
        public string CustomerReferenceOrderNumber { get; set; }
        public string DefaultPurchaseOrder { get; set; }
        public string DefaultPaymentTerms { get; set; }
        public string DocumentNumber { get; set; }
        public string Location { get; set; }
        public string InboundAddress { get; set; }
        public string OutboundAddress { get; set; }
        public string Notes { get; set; }
        public string NonValidatedTradingPartner { get; set; }
        public string OrderStatus { get; set; }
        public _OrderTradingPartner OrderTradingPartner { get; set; }
        public string WareHouse { get; set; }
        public List<FlexField> FlexFieldList { get; set; }
        public List<ReferenceOrderLine> ReferenceOrderLineList { get; set; }

        #region "Constructor"
        public _ReferenceOrderData()
        {
            _BillingAddress billingaddress = new _BillingAddress();
            _ShippingAddress shippingaddress = new _ShippingAddress();
            _OrderTradingPartner ordertradingpartner = new _OrderTradingPartner();
            List<ReferenceOrderLine> referenceorderlinelist = new List<ReferenceOrderLine>();
            List<FlexField> flexfieldlist = new List<FlexField>();


            BillingAddress = billingaddress;
            ShippingAddress = shippingaddress;
            OrderTradingPartner = ordertradingpartner;
            ReferenceOrderLineList = referenceorderlinelist;
            FlexFieldList = flexfieldlist;

        }
        #endregion


    }

    [Serializable]
    public class _BillingAddress
    {
        public _ContactData ContactData { get; set; }
        public _AddressData AddressData { get; set; }

        #region "Constructor"
        public _BillingAddress()
        {
            _ContactData contactdata = new _ContactData();
            _AddressData addressdata = new _AddressData();

            ContactData = contactdata;
            AddressData = addressdata;
        }
        #endregion

    }

    [Serializable]
    public class _ShippingAddress
    {
        public _AddressData AddressData { get; set; }
        public _ContactData ContactData { get; set; }

        #region "Constructor"
        public _ShippingAddress()
        {
            _ContactData contactdata = new _ContactData();
            _AddressData addressdata = new _AddressData();

            ContactData = contactdata;
            AddressData = addressdata;
        }
        #endregion

    }

    [Serializable]
    public class _OrderTradingPartner
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string OrderPaymentTerms { get; set; }
        public string BillShipToSameContactAndAddress { get; set; }
    }

    [Serializable]
    public class _OrderShippingTerms
    {
        public string Name { get; set; }
        public string AllowPartialShipment { get; set; }
        public string Carrier { get; set; }
        public string CarrierType { get; set; }
        public string FobCode { get; set; }
        public string FreightAccountCode { get; set; }
        public string ServiceType { get; set; }
    }

    [Serializable]
    public class _ManufacturerPartNumber
    {
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
    }

    [Serializable]
    public class _UnitPrice
    {
        public string Currency { get; set; }
    }

    [Serializable]
    public class _ContactData
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string MobilePhone { get; set; }
        public string Pager { get; set; }
        public string PrimaryPhone { get; set; }
        public string Title { get; set; }
    }

    [Serializable]
    public class _AddressData
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

    }

    [Serializable]
    public class ReferenceOrderLine
    {
        public string LineNumber { get; set; }
        public string Part { get; set; }
        public string Quantity { get; set; }
        public _UnitPrice UnitPrice { get; set; }
        public string BomPart { get; set; }
        public string Condition { get; set; }
        public string CustomerOrderLineNumber { get; set; }
        public string CustomerPartNumber { get; set; }
        public _ManufacturerPartNumber ManufacturerPartNumber { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public _OrderShippingTerms OrderShippingTerms { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonNotes { get; set; }
        public string ShippingTerms { get; set; }
        public string Status { get; set; }
        public string TaxableInd { get; set; }
        public List<FlexField> FlexFieldList { get; set; }
        public List<ReferenceOrderPart> ReferenceOrderPartList { get; set; }

        #region "Constructor"
        public ReferenceOrderLine()
        {
            List<FlexField> flexfieldlist = new List<FlexField>();
            List<ReferenceOrderPart> referenceorderpartlist = new List<ReferenceOrderPart>();
            _ManufacturerPartNumber manufacturerpartnumber = new _ManufacturerPartNumber();
            _OrderShippingTerms ordershippingterms = new _OrderShippingTerms();
            _UnitPrice unitprice = new _UnitPrice();

            FlexFieldList = flexfieldlist;
            ReferenceOrderPartList = referenceorderpartlist;
            ManufacturerPartNumber = manufacturerpartnumber;
            OrderShippingTerms = ordershippingterms;
            UnitPrice = unitprice;
        }
        #endregion
    }

    [Serializable]
    public class ReferenceOrderPart
    {
        public string PurchaseOrderPart { get; set; } //Questions about this.
    }

    [Serializable]
    public class FlexField
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }


    //[Serializable]
    //public class _ReferenceOrderLineList
    //{
    //    public List<_ReferenceOrderLine> ReferenceOrderLineList { get; set; }
    //}

    //[Serializable]
    //public class _ReferenceOrderPartList
    //{
    //    public List<_ReferenceOrderPart> ReferenceOrderPartList { get; set; }

    //}

    //[Serializable]
    //public class FlexFieldList
    //{
    //    public List<FlexField> FlexFieldList { get; set; }
    //}

}
