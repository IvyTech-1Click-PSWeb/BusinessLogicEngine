using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using JGS.Shared.Package;
using JGS.DAL;
using Oracle.DataAccess.Client;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using CRMObjects;
using CRMObjects.CRMInput;
using CRMObjects.Methods;
using CRMObjects.CRMIncidentCreate;
using System.Web.Configuration;
using System.Data;
using System.Net;
using JGS.BusinessLogicEngine;
using CRMObjects.ReceiptWrapper;
using CRMObjects.CRMIncidentCreateMethods;

namespace JGS.BusinessLogicEngine.WCF
{
	[ServiceBehavior(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public class CRMOneClickService : ICRMOneClickService
	{
        // Logging Configuration notes. 
        // To switch off/on the detailed tracing information about in/out xml parameters you need:
        // 1. Go to JGSWebServices project's web.config file 
        // 2. Open web.config using Enterprise Library Configuration tool
        // 3. In "webServices" Category change a 'Minimum Severity Level'  to All - to see all tracing info or to Warning - to see errors only.


        #region "Publicly exposed Methods"
        public string path;

        public string CRMLogContactInfo(string xmlInput)
        {
            CRM1ClickService CRMXml = null;
            try
            {
                //Convert to Object 
                CRMXml = CROMethods.string2CRM1ClickObj(xmlInput);

                if (path == null)
                {
                    path = "C:\\CRMSvc\\xml\\" + CRMXml.detail.serialno + "\\" + DateTime.Now.ToString("dddMMMddyyyyHHmm");
                }
                if (Directory.Exists(path))
                {
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlInput);
                doc.Save(path + "\\1in.xml");

                // Save input request to log
                //Logger.AddEntry(Logger.EventSource.Process, "CRMLogContactInfo, 1-in", System.Diagnostics.TraceEventType.Information, xmlInput);

                #region "Crossreference table for Location/Client"

                CRMXml = CROMethods.GetClientInfo(CRMXml, xmlInput);

                #endregion

                ////Call Create incident
                ////XmlDocument rDoc = new XmlDocument();
                ////rDoc.LoadXml(CROMethods.obj2String(CRMXml));
                ////rDoc.Save(path + "\\1out.xml");

                // Save output to log file
                //Logger.AddEntry(Logger.EventSource.Process, "CRMLogContactInfo, 1-out", System.Diagnostics.TraceEventType.Information, CROMethods.obj2String(CRMXml));
            }
            catch (Exception exc)
            {
                //Logger.AddEntry(Logger.EventSource.Process, "CRMLogContactInfo", exc);
                CRMXml.detail.result = "Failure";
                CRMXml.detail.resultmessage = "CRMLogContactInfo - " + exc.Message;
            }
            return CROMethods.obj2String(CRMXml);
        }
         
        public string CRMPartAvailability(string xmlInput)
        {
            CRM1ClickService outObj = null;
            try
            {
                outObj = CROMethods.string2CRM1ClickObj(xmlInput);
            }
            catch (Exception exc)
            {
                //Logger.AddEntry(Logger.EventSource.Process, "CRMPartAvailability", exc);
                throw exc;
            }

            return outObj.ToString();
           
        }

        public string CRMCreateOrder(string xmlInput)
        {
            CRM1ClickService CRM = null;
            try
            {
                xmlInput = CRMLogContactInfo(xmlInput);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlInput);
                doc.Save(path + "\\2in.xml");
                //Logger.AddEntry(Logger.EventSource.Process, "CRMCreateOrder, 2-in", System.Diagnostics.TraceEventType.Information, xmlInput);


                CRM = CROMethods.string2CRM1ClickObj(xmlInput);
                if (CRM.detail.createorderinfo.returninfo.result != "Success")
                    return FailReturn(CRM);
                else
                { CRM.detail.createorderinfo.returninfo.result = ""; CRM.detail.createorderinfo.returninfo.failmessage = ""; }

                //CRM = CROMethods.PartValidate(CRM);
                //if (CRM.detail.createorderinfo.returninfo.result != "Success")
                //    return FailReturn(CRM);
                //else
                //{ CRM.detail.createorderinfo.returninfo.result = ""; CRM.detail.createorderinfo.returninfo.failmessage = ""; }


                CRM = CROMethods.ProcessOrder(CRM);

                XmlDocument rDoc = new XmlDocument();
                rDoc.LoadXml(CROMethods.obj2String(CRM));
                rDoc.Save(path + "\\2out.xml");
                //Logger.AddEntry(Logger.EventSource.Process, "CRMCreateOrder, 2-out", System.Diagnostics.TraceEventType.Information, xmlInput);

            }
            catch (Exception exc)
            {
                //Logger.AddEntry(Logger.EventSource.Process, "CRMCreateOrder", exc);
				if (CRM != null)
                {
                    CRM.detail.result = "Failure";
                    CRM.detail.resultmessage = exc.Message;
                }
                else
                {
                    throw exc;
                }
            }
            return CROMethods.obj2String(CRM); 
        }

        public string CRMCreateIncident(string xmlInput)
        {
            string rs = string.Empty;
            try
            {
                CRMIncidentCreateMethods method = new CRMIncidentCreateMethods();
                //Creating the input string into xmlDocument class
                //Preparing the new XML for Serialization. and Bringing it to an Object. List<record>
                //XmlNode oldroot = iDoc.RemoveChild(iDoc.DocumentElement);
                //XmlNode newRoot = iDoc.AppendChild(iDoc.CreateElement("IncidentCreate"));
                //newRoot.AppendChild(oldroot);
                // Old way of Batch processing. (Doesnt Work)
                //RNObject[] outArray = CRMIncidentCreateMethods.GetRNObjectArray(iDoc.OuterXml);
                //RNObject[] retArray = CRMIncidentCreateMethods.CreateIncidents(outArray, out message);

                XmlDocument iDoc = new XmlDocument();

                iDoc.LoadXml(xmlInput);
                //iDoc.Save("C:\\CRMSvc\\xml\\IncidentIn.xml");
                //Logger.AddEntry(Logger.EventSource.Process, "CRMCreateIncident, IncidentIn", System.Diagnostics.TraceEventType.Information, xmlInput);

                rs = method.ProcessFile(iDoc.OuterXml);
            }
            catch (Exception exc)
            {
                //Logger.AddEntry(Logger.EventSource.Process, "CRMCreateIncident", exc);
				rs = "CRMCreateIncident - " + exc.Message;
            }
            return rs.ToString();
        }
        
        public string FailReturn(CRM1ClickService CRM)
        {
            string s = CROMethods.obj2String(CRM);
            return s;
        }

        public string CRMPartAvailavableLookupByPartNum(string xmlInput)
        {
            string partsXml = string.Empty;

            try
            { 
                CRM1ClickService CRMXml = CROMethods.string2CRM1ClickObj(xmlInput);
                CRMObjects.CRMInput.PartsList parts = CROMethods.PartAvailavableLookupByPartNum(CRMXml);
                if (parts == null)
                {
                    partsXml = CROMethods.obj2String(CRMXml);
                }
                else
                {
                    partsXml = CROMethods.obj2String(parts);
                }

                //Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookup", System.Diagnostics.TraceEventType.Information, partsXml);
            }
            catch (Exception exc)
            {
                //Logger.AddEntry(Logger.EventSource.Process, "PartAvailavableLookup", new Exception("PartAvailavableLookup, ", exc));
                throw exc;
            }
            return partsXml;
        }

        public string CRMTest(string xmlInput)
        {
             string res = string.Empty;

             try
             {
                 CRM1ClickService CRM = CROMethods.string2CRM1ClickObj(xmlInput);
                 res = CROMethods.TestOrder(CRM);
             }
             catch (Exception exc)
             {
                 //Logger.AddEntry(Logger.EventSource.Process, "CRMTest", new Exception("CRMTest, ", exc));
             }

             return res;
        }

        // New Dell Code
        public string CRMUpdateDellOrder(string xmlInput)
        {
            CRM1ClickService CRMXml = null;

            //Convert to Object 
            CRMXml = CROMethods.string2CRM1ClickObj(xmlInput);

            path = @"C:\CRMSvc\xml\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(xmlInput);
                //doc.Save(path + "\\1in.xml");

                CRMXml = CROMethods.UpdateOrder(CRMXml);

                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(CROMethods.obj2String(CRMXml));
                doc2.Save(path + "\\2in.xml");

                CRMXml.detail.result = "Success";
            }
            catch (Exception ex)
            {
                CRMXml.detail.result = "Failure";
                CRMXml.detail.resultmessage = "CRMUpdateDellOrder - " + ex.Message.ToString();

                XmlDocument rDoc = new XmlDocument();
                rDoc.LoadXml(CROMethods.obj2String(CRMXml));
                rDoc.Save(path + "\\2err.xml");
            }

            return CROMethods.obj2String(CRMXml);
        }

        public string CRMGetHolidays(string xmlInput)
        {
            Holidays CRMxml = null;

            StringReader reader = new StringReader(xmlInput);
            XmlSerializer dserializer = new XmlSerializer(typeof(Holidays));
            CRMxml = (Holidays)dserializer.Deserialize(reader);

            path = @"C:\CRMSvc\xml\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                CRMxml = CROMethods.CRMGetDBHolidays(CRMxml);
            }
            catch (Exception ex)
            {
                CRMxml.resultMessage = "ERR: " + ex.Message.ToString();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Holidays));
            StringWriter Outputstring = new StringWriter();
            XmlTextWriter writeXML = new XmlTextWriter(Outputstring);

            serializer.Serialize(writeXML, CRMxml);

            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(Outputstring.ToString());
            doc2.Save(path + "\\Holidays.xml");

            return Outputstring.ToString();
        }

        #endregion

    }

   
}
#region " Old Code for http post"

            //NetworkCredential nc = new NetworkCredential("ONEILLDE", "Admin1");
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(xmlInput);
            //doc.Save("c:\\CRMsvc\\xml.xml");
            //CRM1ClickService CRMXml = string2Obj(xmlInput);
            //string y = obj2String(CRMXml);
            //CreateOrderObject CROXml = CROMethods.CRM1ClickService2CreateOrderObject(CRMXml);
            //string s = obj2String(CROXml);
            
            //try
            //{
            //    byte[] bytes = Encoding.UTF8.GetBytes(s);
            //    HttpWebRequest rq = (HttpWebRequest)WebRequest.Create("http://jgsslapistaging.corp.jabil.org:8001/ws/order/createReferenceOrder"); //"http://jgsslapistaging.corp.jabil.org:8001/ws/order/createReferenceOrder" "http://jgsslapidev.corp.jabil.org:8001/ws/order/createReferenceOrder"
            //    rq.Method = "POST";
            //    rq.ContentLength = bytes.Length;
            //    rq.ContentType = "application/xml";
            //    rq.Credentials = nc;
            //    using (Stream rqStream = rq.GetRequestStream())
            //    {
            //        rqStream.Write(bytes, 0, bytes.Length);
            //    }
            //    HttpWebResponse response = (HttpWebResponse)rq.GetResponse();

            //    CRMXml.detail.createorderinfo.returninfo.result = response.StatusCode.ToString();

            //    if (response.StatusCode != HttpStatusCode.OK)
            //    {
            //        CRMXml.detail.createorderinfo.returninfo.failmessage = response.StatusDescription;
            //    }
            //    else
            //    {
            //        CRMXml.detail.createorderinfo.returninfo.rnrreferenceorderid = response.StatusDescription;
            //    }
            //}
            //catch(WebException ex)
            //{
            //    string message = "";
            //    WebResponse wresponse = ex.Response;
            //    Stream response = wresponse.GetResponseStream();
            //    Encoding responseEncoding = Encoding.Default;
            //    StreamReader responseReader = new StreamReader(response, responseEncoding);
            //    string responseContent = responseReader.ReadToEnd();
            //    if (responseContent.IndexOf("Request denied:") < 0)
            //    {
            //        XmlTextReader rdrstream = new XmlTextReader(new StringReader(responseContent));
            //        rdrstream.WhitespaceHandling = System.Xml.WhitespaceHandling.None;
            //        rdrstream.Read();
            //        while (rdrstream.Read())
            //        {
            //            if (rdrstream.IsStartElement())
            //            {
            //                if (rdrstream.Name == "Message")
            //                {
            //                    message = rdrstream.ReadElementString("Message");
            //                }
            //            }
            //        }
            //        response.Close();
            //        if (message == null)
            //        {
            //            message = responseContent;
            //        }
            //        else
            //        {
            //            if (message.Length <= 0)
            //                message = responseContent;
            //        }
            //    }
            //    else
            //        message = responseContent;
            //}
#endregion
