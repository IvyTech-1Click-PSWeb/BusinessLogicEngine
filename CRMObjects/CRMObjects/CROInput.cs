using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CRMObjects.CROInput
{
    public struct CreateOrderObj
    {

        public string systemId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ReferenceOrderInfo OrderInfo { get; set; }
    }

    public struct ReferenceOrderInfo
    {
        public ContactAddressInfo billToDo { get; set; }
        public string billingTermsAbbr { get; set; }
        public string btType { get; set; }
        public string clientAbbr { get; set; }
        public string clientRefNo1 { get; set; }
        public string clientRefNo2 { get; set; }
        public string contract { get; set; }
        public string custOrderNo { get; set; }
        public string customerAbbr { get; set; }
        public string dfltPOAbbrv { get; set; }
        public string dfltPaymentTermsAbbrv { get; set; }
        public string documentNo { get; set; }
        public string geography { get; set; }
        public FlexFields[] headerFlexFields { get; set; }
        public string id { get; set; }
        public string inboundAddressAbbr { get; set; }
        public ReferenceOrderLine[] newLineList { get; set; }
        public string notes { get; set; }
        public string outboundAddressAbbr { get; set; }
        public ContactAddressInfo shipToDo { get; set; }
        public string status { get; set; }
        public string systemId { get; set; }
        public string warehouse { get; set; }
        public FlexFields[] wsFlexFields { get; set; }
    }

    public struct ContactAddressInfo
    {
        public AddressInfo AddressInfo { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }
    public struct AddressInfo
    {
        public string abbrv { get; set; }
        public string addrName { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string line3 { get; set; }
        public string line4 { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
    }

    public struct ContactInfo
    {
        public string abbrv { get; set; }
        public string contactDesc { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public string mobilePhone { get; set; }
        public string name { get; set; }
        public string pager { get; set; }
        public string primaryPhone { get; set; }
        public string title { get; set; }
    }

    public struct FlexFields
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public struct ReferenceOrderLine
    {
        public string ABomPN { get; set; }
        public string condition { get; set; }
        public string currency { get; set; }
        public string custOrderLineNo { get; set; }
        public string custPartNo { get; set; }
        public FlexFields[] lineFlexFields { get; set; }
        public string manufacturer { get; set; }
        public string manufacturerPartNo { get; set; }
        public string notes { get; set; }
        public string orderLineNo { get; set; }
        public RDEOrderPart[] partList { get; set; }
        public string partNo { get; set; }
        public string priority { get; set; }
        public string qty { get; set; }
        public string reasonCode { get; set; }
        public string reasonNotes { get; set; }
        public string shippingTermsAbbr { get; set; }
        public string status { get; set; }
        public string taxableFlag { get; set; }
        public string unitPrice { get; set; }
        public string warrantyFlag { get; set; }
    }

    public struct RDEOrderPart
    {
        public string bcn { get; set; }
        public string notes { get; set; }
        public string orderId { get; set; }
        public string orderLineId { get; set; }
        public FlexFields[] partFlexFields { get; set; }
        public string reasonCodeName { get; set; }
        public string reasonNotes { get; set; }
        public string serialNo { get; set; }
    }





   


}




