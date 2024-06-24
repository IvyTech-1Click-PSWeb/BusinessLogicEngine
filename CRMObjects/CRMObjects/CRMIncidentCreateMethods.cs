using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRMObjects.CRMIncidentCreate;
using CRMObjects.RightNowSync;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace CRMObjects.CRMIncidentCreateMethods
{
     public class CRMIncidentCreateMethods
    {
         private static RightNowSyncPortClient _client;
         private static CreateProcessingOptions _opt;
         private IncidentCreate GoodResult = new IncidentCreate();
         private IncidentCreate FailedResult = new IncidentCreate();
         

         #region "Constructor"
         public CRMIncidentCreateMethods()
         {
             _client = new RightNowSyncPortClient();
             _opt = new CreateProcessingOptions();
             _client.ClientCredentials.UserName.UserName = Properties.Settings.Default.UName;
             _client.ClientCredentials.UserName.Password = Properties.Settings.Default.PassW;
             _opt.SuppressExternalEvents = Properties.Settings.Default.Options;
             _opt.SuppressRules = Properties.Settings.Default.Rules;
         }
         #endregion

        #region "Public Methods"

        public string ProcessFile(string input)
        {
            
            IncidentCreate InBoundXML = string2IncidentObj(input);
            //int i = InBoundXML.Root.Count();
            foreach (Report r in InBoundXML)
            {
                try
                {
                    string[] s = r.Customer_ID_Name.Split(' ');
                    if (s.Count() == 1)
                    {
                        r.Result = "Failure";
                        r.Message = "Must have first and last name for each contact";
                        FailedResult.Add(r);
                        continue;
                    }
                    Contact C = ProcessContactObject(r);
                    Incident I = ProcessIncidentObject(r, C);
                    if (r.Result != "Success")
                    {
                        FailedResult.Add(r);
                    }
                    else
                    {
                        GoodResult.Add(r);
                    }
                }
                catch(Exception ex)
                {
                    r.Result = "Failure";
                    r.Message = ex.Message;
                    FailedResult.Add(r);
                    continue;
                }
            }
            GoodResult.AddRange(FailedResult);
            return Obj2String(GoodResult);
        }

        public static string Obj2String(object objIn)
         {
             XmlSerializer ser = new XmlSerializer(objIn.GetType());
             StringWriter sw = new StringWriter();
             XmlTextWriter xw = new XmlTextWriter(sw);
             ser.Serialize(xw, objIn);

             return sw.ToString();
         }

        #endregion

        #region "B2B Incident Create Private Methods"

        private static Contact ProcessContactObject(Report r)
         {
            

             //Check to see if the Contact exists
             string query = "SELECT Contact FROM Contact C WHERE C.Emails.Address = '" + r.E_Mail_Address + "'";
             // C.Name.First FirstName, C.Name.Last LastName | c.Emails.EmailList.Address[0] OR email_alt1 = '" + r.E_Mail_Address + "' OR email_alt2 = '" + r.E_Mail_Address + "'";
             ClientInfoHeader CHeader = new ClientInfoHeader();
             Contact template = new Contact();
             RNObject[] ObjTemplate = new RNObject[] { template };
             CHeader.AppID = "Contact Check";

             QueryResultData[] exists = _client.QueryObjects(CHeader, query, ObjTemplate, 1000);

             RNObject[] resultSet = exists[0].RNObjectsResult;

             if (resultSet.Count() > 0)
             {
                 r.Result = "Success";
                 r.Message = "Contact Exists";
                 return (Contact)resultSet[0];
             }
             else
             {
                 
                 ClientInfoHeader cHeader = new ClientInfoHeader();
                 cHeader.AppID = "Contact Create";
                 RNObject[] inArray = new RNObject[] { GetContactObj(r) };

                 RNObject[] outArray = _client.Create(cHeader, inArray, _opt);
                 RNObject retObj = outArray[0];
                 r.Result = "Success";
                 r.Message = "Contact Created";

                 return (Contact)retObj;

             }


         }

        private static RNObject GetContactObj(Report r)
         {
             Contact rnContact = new Contact();

             //Deal with the person portion of the Contact
             PersonName rnPersonFN = new PersonName();
             string[] n = r.Customer_ID_Name.Split(' ');
             switch (n.Count())
             {
                 case 2:
                     {
                         rnPersonFN.First = n[0];
                         rnPersonFN.Last = n[1];
                         break;
                     }
                 case 3:
                     {
                         rnPersonFN.First = n[0] + " " + n[1];
                         rnPersonFN.Last = n[2];
                         break;
                     }
                 case 4:
                     {
                         rnPersonFN.First = n[0] + " " + n[1];
                         rnPersonFN.Last = n[2] + " " + n[3];
                         break;
                     }
             }
             

             rnContact.Name = rnPersonFN;

             //Address portion of the contact record
             Address rnAddress = new Address();
             rnAddress.Street = r.Address_Line_1;
             rnAddress.City = r.Address_Line_2;
             rnAddress.PostalCode = r.CAM_Postal_Code;

             rnContact.Address = rnAddress;

             //NamedID Country Block
             NamedID rnCountry = new NamedID();
             rnCountry.ID = new ID();
             rnCountry.ID.id = 2;
             rnCountry.ID.idSpecified = true;
             if (r.Country.Length == 2)
                 rnCountry.Name = r.Country;
             else
                rnCountry.Name = "TR";

             XmlDocument doc = new XmlDocument();
             doc.LoadXml(Obj2String(rnCountry));
             doc.Save("C:\\CRMSvc\\xml\\Country.xml");
                 

             rnAddress.Country = rnCountry;

             //Phone number portion of contact Record
             Phone rnPhone = new Phone();
             rnPhone.Number = r.CAM_FirstTelephoneNo;
             rnPhone.action = ActionEnum.add;
             rnPhone.actionSpecified = true;

             NamedID rnPhoneType = new NamedID();
             rnPhoneType.ID = new ID();
             rnPhoneType.ID.id = 0;
             rnPhoneType.ID.idSpecified = true;
             rnPhoneType.Name = "ph_mobile";

             rnPhone.PhoneType = rnPhoneType;
             rnContact.Phones = new Phone[] { rnPhone };

             //Email portion of the contact info
             Email rnEmail = new Email();
             rnEmail.Address = r.E_Mail_Address;
             rnEmail.action = ActionEnum.add;
             rnEmail.actionSpecified = true;

             NamedID emailType = new NamedID();
             emailType.ID = new ID();
             emailType.ID.id = 0;
             emailType.ID.idSpecified = true;
             rnEmail.AddressType = emailType;

             rnContact.Emails = new Email[] { rnEmail };

             return rnContact;
         }
         
        private static Incident ProcessIncidentObject(Report r, Contact c)
        {
            //CreateProcessingOptions opt = new CreateProcessingOptions();
            //opt.SuppressExternalEvents = Properties.Settings.Default.Options;
            //opt.SuppressRules = Properties.Settings.Default.Rules;

            List<GenericField> cfList = new List<GenericField>();
            Incident rnIncident = new Incident();
            IncidentContact incContact = new IncidentContact();
            NamedID nc = new NamedID();
            nc.ID = c.ID;
            incContact.Contact = nc;

            rnIncident.PrimaryContact = incContact;
            rnIncident.Subject = r.Reason + " | " + r.Description;
            cfList.Add(CreateGenericField("c$callcenterticketnumber", r.Service_Ticket_Number, DataTypeEnum.STRING));
            cfList.Add(CreateGenericField("c$devicesn", r.Serial_Number, DataTypeEnum.STRING));
            //cfList.Add(CreateGenericField("c$prodwarrantycode", r.Warranty_Category, DataTypeEnum.NAMED_ID));
            rnIncident.CustomFields = cfList.ToArray();

            ClientInfoHeader cHeader = new ClientInfoHeader();
            cHeader.AppID = "Incident Create";
            RNObject[] inArray = new RNObject[] { rnIncident };

           

            RNObject[] outArray = _client.Create(cHeader, inArray, _opt);
            RNObject retObj = outArray[0];
            r.Result = "Success";
            r.Message = "Incident Number: " + retObj.ID.id.ToString();

            return (Incident)retObj;
        }

        private static GenericField CreateGenericField(string name, string value, DataTypeEnum type)
        {
            GenericField genField = new GenericField();

            genField.dataType = type;
            genField.dataTypeSpecified = true;
            genField.name = name;
            genField.DataValue = createDataValue(value, type);

            return genField;
        }

        private static DataValue createDataValue(string val, DataTypeEnum type)
        {
            DataValue dv = new DataValue();
            dv.Items = new object[] { val };
            switch (type)
            {
                case DataTypeEnum.STRING:
                    {
                        dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.StringValue };
                        return dv;
                        
                    }
                case DataTypeEnum.NAMED_ID:
                    {
                        dv.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.NamedIDValue };
                        return dv;
                    }
            }
            return dv;
        }

        private static IncidentCreate string2IncidentObj(string xmlIn)
        {
            XmlSerializer ser = new XmlSerializer(typeof(IncidentCreate));
            StringReader sr = new StringReader(xmlIn);
            IncidentCreate outObj = (IncidentCreate)ser.Deserialize(sr);
            return outObj;
        }

        #endregion
     }

     #region "Unused code"

     //public static RNObject[] CreateIncidents(RNObject[] InputArray, out string Err)
     //{
     //    CreateProcessingOptions Opt = new CreateProcessingOptions();
     //    Opt.SuppressExternalEvents = false;
     //    Opt.SuppressRules = false;

     //    try
     //    {
     //        ClientInfoHeader IHeader = new ClientInfoHeader();
     //        IHeader.AppID = "Test Create";
     //        RNObject[] result = _client.Create(IHeader, InputArray, Opt);
     //        Err = null;
     //        return result;
     //    }
     //    catch (Exception ex)
     //    {
     //        RNObject[] result = null;
     //        Err = ex.Message;
     //        return result;
     //    }


     //}

     #region "Dummy Data in the string format to be passed to the Incident Create"
     //string DummyData = "<Record><Service_Ticket_Number>8054258674</Service_Ticket_Number><Serial_Number>CNU1102FYF</Serial_Number> <Warranty_Category>Factory Warranty</Warranty_Category><Override_Reason>Not assigned</Override_Reason><Reason>Hardware Issue</Reason><Description>BLUE SCREEN ISSUE</Description><Customer_ID_Name>murat kara</Customer_ID_Name><Address_Line_1>fırat üniversitesi hastanesi tıbbi genetik laboratuvarı</Address_Line_1><Address_Line_2>TR-23149 elazığ</Address_Line_2><Country>Turkey</Country><CAM__Postal_Code>23149</CAM__Postal_Code><CAM_FirstTelephoneNo>+905325796929</CAM_FirstTelephoneNo><E_Mail_Address>dnagenom@gmail.com</E_Mail_Address><Problem_Log>     [REASON]:ZR01     [ACCESSORY]:     [Warranty information] ('yyyymmdd A' on accessory):     [Accessory replaced] y/n:     [Return kit sent] y/n:</Problem_Log></Record><Record><Service_Ticket_Number>8054258674</Service_Ticket_Number><Serial_Number>CNU1102FYF</Serial_Number> <Warranty_Category>Factory Warranty</Warranty_Category><Override_Reason>Not assigned</Override_Reason><Reason>Hardware Issue</Reason><Description>BLUE SCREEN ISSUE</Description><Customer_ID_Name>murat kara</Customer_ID_Name><Address_Line_1>fırat üniversitesi hastanesi tıbbi genetik laboratuvarı</Address_Line_1><Address_Line_2>TR-23149 elazığ</Address_Line_2><Country>Turkey</Country><CAM__Postal_Code>23149</CAM__Postal_Code><CAM_FirstTelephoneNo>+905325796929</CAM_FirstTelephoneNo><E_Mail_Address>dnagenom@gmail.com</E_Mail_Address><Problem_Log>     [REASON]:ZR01     [ACCESSORY]:     [Warranty information] ('yyyymmdd A' on accessory):     [Accessory replaced] y/n:     [Return kit sent] y/n:</Problem_Log></Record><Record><Service_Ticket_Number>8054258674</Service_Ticket_Number><Serial_Number>CNU1102FYF</Serial_Number> <Warranty_Category>Factory Warranty</Warranty_Category><Override_Reason>Not assigned</Override_Reason><Reason>Hardware Issue</Reason><Description>BLUE SCREEN ISSUE</Description><Customer_ID_Name>murat kara</Customer_ID_Name><Address_Line_1>fırat üniversitesi hastanesi tıbbi genetik laboratuvarı</Address_Line_1><Address_Line_2>TR-23149 elazığ</Address_Line_2><Country>Turkey</Country><CAM__Postal_Code>23149</CAM__Postal_Code><CAM_FirstTelephoneNo>+905325796929</CAM_FirstTelephoneNo><E_Mail_Address>dnagenom@gmail.com</E_Mail_Address><Problem_Log>     [REASON]:ZR01     [ACCESSORY]:     [Warranty information] ('yyyymmdd A' on accessory):     [Accessory replaced] y/n:     [Return kit sent] y/n:</Problem_Log></Record><Record><Service_Ticket_Number>8054258674</Service_Ticket_Number><Serial_Number>CNU1102FYF</Serial_Number> <Warranty_Category>Factory Warranty</Warranty_Category><Override_Reason>Not assigned</Override_Reason><Reason>Hardware Issue</Reason><Description>BLUE SCREEN ISSUE</Description><Customer_ID_Name>murat kara</Customer_ID_Name><Address_Line_1>fırat üniversitesi hastanesi tıbbi genetik laboratuvarı</Address_Line_1><Address_Line_2>TR-23149 elazığ</Address_Line_2><Country>Turkey</Country><CAM__Postal_Code>23149</CAM__Postal_Code><CAM_FirstTelephoneNo>+905325796929</CAM_FirstTelephoneNo><E_Mail_Address>dnagenom@gmail.com</E_Mail_Address><Problem_Log>     [REASON]:ZR01     [ACCESSORY]:     [Warranty information] ('yyyymmdd A' on accessory):     [Accessory replaced] y/n:     [Return kit sent] y/n:</Problem_Log></Record>";
     #endregion

     //internal class RightNowClient : RightNowSyncPortClient
     //{
     //    internal RightNowSyncPortClient _client;

     //    public RightNowClient()
     //    {
     //        _client = new RightNowSyncPortClient();
     //        _client.ClientCredentials.UserName.UserName = "WebConnector";
     //        _client.ClientCredentials.UserName.Password = "I$tanboo";

     //    }
     //}

     #endregion 
 }
