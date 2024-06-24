using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.BusinessLogicEngine.Support;
using System.Xml;
using JGS.Shared;
using JGS.BusinessLogicEngine.Model;
using JGS.BusinessLogicEngine.API.Support;
// API USAGE NEED IT
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace JGS.BusinessLogicEngine.API
{
    public class TimeOut : IAPI
    {
        private TimeOutWrapper.TimeOutWrapperSoapClient _client = new JGS.BusinessLogicEngine.TimeOutWrapper.TimeOutWrapperSoapClient();
        
        #region IAPI Members

        
        public System.Xml.XmlDocument Execute(System.Xml.XmlDocument document, List<JGS.BusinessLogicEngine.Model.Field> fields)
        {
            TimeOutWrapper.TimeOutInfo info = new TimeOutWrapper.TimeOutInfo();

            info.Geography = document.GetElementValue("LOCATIONNAME", string.Empty, fields);
            info.BCN = document.GetElementValue("BCN", string.Empty, fields);
            info.SerialNumber = document.GetElementValue("SERIALNUMBER", string.Empty, fields);
            info.PartNumber = document.GetElementValue("PARTNUMBER", string.Empty, fields);
            info.WorkCenterId = document.GetElementValue("WORKCENTERNAME", string.Empty, fields);
            info.Warranty = document.GetElementValue("WARRANTY", false, fields);
            info.WoTimeLoggedInHours = document.GetElementValue("WORKCENTERTIMELOGGEDIN", string.Empty, fields);
            info.Notes = document.GetElementValue("TO_NOTES", string.Empty, fields);
            info.ResultCode = document.GetElementValue("TO_RESULTCODE", string.Empty, fields);
            info.TimeOutType = TimeOutWrapper.OperationTypes.ProcessImmediate;
            info.ClientName = document.GetElementValue("CLIENT_NAME", string.Empty, fields);
            info.ContractName = document.GetElementValue("CONTRACTNAME", string.Empty, fields);
            info.userName = document.GetElementValue("USERNAME", string.Empty, fields);
            info.userPass = document.GetElementValue("PASSWORD", string.Empty, fields);

            info.InfoCodeList = new List<TimeOutWrapper.InfoCodes>();
            info.InfoCodeList.AddRange(GetInfoCodes(document,fields,"TO_DIAGNOSTICCODELIST",TimeOutWrapper.CodeTypes.Diagnostic));
            info.InfoCodeList.AddRange(GetInfoCodes(document, fields, "TO_FAILURECODELIST", TimeOutWrapper.CodeTypes.Fail));
            info.InfoCodeList.AddRange(GetInfoCodes(document, fields, "TO_REPAIRCODELIST", TimeOutWrapper.CodeTypes.Repair));
            info.InfoCodeList.AddRange(GetInfoCodes(document, fields, "TO_SYMPTIOMCODELIST", TimeOutWrapper.CodeTypes.Symptom));
            if (info.InfoCodeList.Count == 0)
            {
                info.InfoCodeList = null;
            }

            info.FlexFieldList = new List<TimeOutWrapper.FlexFields>();
            if (fields.Where(p => p.Name == "TO_WCFLEXFIELDS ").Count() > 0)
            {
                foreach (XmlNode node in document.SelectNodes(fields.Where(prop => prop.Name == "TO_WCFLEXFIELDS ").First().XPath))
                {
                    TimeOutWrapper.FlexFields ff = new TimeOutWrapper.FlexFields();
                    ff.Name = node.SelectSingleNode(fields.Where(p => p.Name == "FLEXFIELDNAME").First().XPath).InnerText;
                    ff.Value = node.SelectSingleNode(fields.Where(p => p.Name == "FLEXFIELDVALUE").First().XPath).InnerText;
                    if (fields.Where(p => p.XPath.ToUpper().Contains(("Name='" + ff.Name.Trim() + "'").ToUpper())).Count() > 0)
                    {
                        info.FlexFieldList.Add(ff);
                    }
                }
            }
            if (info.FlexFieldList.Count == 0)
            {
                info.FlexFieldList = null;
            }

            string clientIP = string.Empty;

            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties clientProp = context.IncomingMessageProperties;
                //RemoteEndpointMessageProperty endpoint = clientProp[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                //HttpRequestMessageProperty endpointLoadBalancer = (HttpRequestMessageProperty)clientProp(HttpRequestMessageProperty.Name);
                HttpRequestMessageProperty endpointLoadBalancer = clientProp[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty; 
                if (endpointLoadBalancer.Headers["clientIP"] != null)
                    clientIP = endpointLoadBalancer.Headers["clientIP"];
                if (string.IsNullOrEmpty(clientIP))
                {
                    //RemoteEndpointMessageProperty endpoint = (RemoteEndpointMessageProperty)prop(RemoteEndpointMessageProperty.Name);
                    RemoteEndpointMessageProperty endpoint = clientProp[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty; 
                    clientIP = endpoint.Address;
                }
            }

            catch (Exception e)
            {
                clientIP = ("ER: " + e.Message).ToString().Substring(0, 20);
            }

            info.CallSource = "F1C";
            info.IP = clientIP; 
            info.APIUsage_LocationName = info.Geography;
            info.APIUsage_ClientName = info.ClientName;

            _client.CheckClient();
            string result = _client.PerformTimeOut(info, false);
            document.SetValue(fields.Where(p => p.Name == "MESSAGE").First().XPath, result);
            document.SetValue(fields.Where(p => p.Name == "RESULT").First().XPath, 
                result.ToUpper()=="SUCCESS"?"SUCCESS":"ERROR");

            return document;
        }

        #endregion

        private List<TimeOutWrapper.InfoCodes> GetInfoCodes(XmlDocument document, List<Field> fields, 
            string collName, TimeOutWrapper.CodeTypes codeType)
        {
            List<TimeOutWrapper.InfoCodes> codes = new List<TimeOutWrapper.InfoCodes>();
            if (fields.Where(p => p.Name == collName).Count() > 0)
            {
                foreach (XmlNode node in document.SelectNodes(fields.Where(prop => prop.Name == collName).First().XPath))
                {
                    TimeOutWrapper.InfoCodes code = new JGS.BusinessLogicEngine.TimeOutWrapper.InfoCodes();
                    code.CodeType = codeType;
                    code.CodeName = node.SelectSingleNode(fields.Where(p => p.Name == "CODENAME").First().XPath).InnerText;
                    code.InfoCode = node.SelectSingleNode(fields.Where(p => p.Name == "CODEVALUE").First().XPath).InnerText;
                    codes.Add(code);
                }
            }
            return codes;
        }
    }
}
