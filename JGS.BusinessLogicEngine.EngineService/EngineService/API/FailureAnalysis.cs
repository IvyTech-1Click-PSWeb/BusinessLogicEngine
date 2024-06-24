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
    public class FailureAnalysis : IAPI
    {
        private FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient _client = new FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient();

        #region IAPI Members

        public System.Xml.XmlDocument Execute(System.Xml.XmlDocument document, List<JGS.BusinessLogicEngine.Model.Field> fields)
        {
            FailureAnalysisWrapper.FAInfo info = new FailureAnalysisWrapper.FAInfo();

            info.UserPass = new FailureAnalysisWrapper.UserPwd();
            info.UserPass.UserName = document.GetElementValue("USERNAME", string.Empty, fields);
            info.UserPass.Password = document.GetElementValue("PASSWORD", string.Empty, fields);
            info.BCN = document.GetElementValue("BCN", string.Empty, fields);
            info.QueueInfo = new FailureAnalysisWrapper.Info4Queue();
            info.QueueInfo.Geography = document.GetElementValue("LOCATIONNAME", string.Empty, fields);
            info.QueueInfo.ClientName = document.GetElementValue("CLIENTNAME", string.Empty, fields);
            info.QueueInfo.ContractName = document.GetElementValue("CONTRACTNAME", string.Empty, fields);
            info.FAType = FailureAnalysisWrapper.OperationTypes.ProcessImmediate;

            info.defCodeList = new FailureAnalysisWrapper.ArrayOfDefCodes();
            info.actionCodeList = new FailureAnalysisWrapper.ArrayOfActionCodes();
            if (fields.Where(p => p.Name == "FA_DEFECT_CODE").Count() > 0)
            {
                foreach (XmlNode dcNode in document.SelectNodes(fields.Where(prop => prop.Name == "FA_DEFECT_CODE").First().XPath))
                {
                    FailureAnalysisWrapper.defCodes defCode = new FailureAnalysisWrapper.defCodes();
                    defCode.Operation = FailureAnalysisWrapper.FAOperations.Add;
                    defCode.Name = dcNode.GetNodeValue("FA_DEFECTCODENAME",fields);
                    defCode.Notes = dcNode.GetNodeValue("FA_NOTES",fields);

                    defCode.FlexFieldList = new List<FailureAnalysisWrapper.FAFlexFields>();
                    if (fields.Where(p => p.Name == "FA_FLEXFIELDS").Count() > 0)
                    {
                        foreach (XmlNode ffNode in dcNode.SelectNodes(fields.Where(prop => prop.Name == "FA_FLEXFIELDS").First().XPath))
                        {
                            FailureAnalysisWrapper.FAFlexFields ff = new FailureAnalysisWrapper.FAFlexFields();
                            ff.Name = ffNode.GetNodeValue("FLEXFIELDNAME",fields);
                            ff.Value = ffNode.GetNodeValue("FLEXFIELDVALUE",fields);
                            defCode.FlexFieldList.Add(ff);
                        }
                    }
                    if (defCode.FlexFieldList.Count == 0)
                    {
                        defCode.FlexFieldList = null;
                    }
                    
                    info.defCodeList.Add(defCode);

                    //Action Codes
                    foreach (XmlNode acNode in dcNode.SelectNodes(fields.Where(prop => prop.Name == "FA_ACTION_CODE").First().XPath))
                    {
                        FailureAnalysisWrapper.actionCodes actCode = new FailureAnalysisWrapper.actionCodes();
                        actCode.Operation = FailureAnalysisWrapper.FAOperations.Add;
                        actCode.defectCode = defCode.Name;
                        actCode.actionCode = acNode.GetNodeValue("FA_ACTIONCODENAME",fields);
                        actCode.assemblyCode = acNode.GetNodeValue("FA_ACTIONCODEASSEMBLY",fields);
                        actCode.ecoCode = string.Empty;
                        actCode.Notes = acNode.GetNodeValue("FA_NOTES",fields);

                        actCode.FlexFieldList = new List<FailureAnalysisWrapper.FlexFields>();
                        if (fields.Where(p => p.Name == "FA_FLEXFIELDS").Count() > 0)
                        {
                            foreach (XmlNode ffNode in acNode.SelectNodes(fields.Where(prop => prop.Name == "FA_FLEXFIELDS").First().XPath))
                            {
                                FailureAnalysisWrapper.FlexFields ff = new FailureAnalysisWrapper.FlexFields();
                                ff.Name = ffNode.GetNodeValue("FLEXFIELDNAME",fields);
                                ff.Value = ffNode.GetNodeValue("FLEXFIELDVALUE", fields);
                                actCode.FlexFieldList.Add(ff);
                            }
                        }
                        if (actCode.FlexFieldList.Count == 0)
                        {
                            actCode.FlexFieldList = null;
                        }

                        info.actionCodeList.Add(actCode);
                    }
                }
            }

            string clientIP = string.Empty;

            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties clientProp = context.IncomingMessageProperties;
                HttpRequestMessageProperty endpointLoadBalancer = clientProp[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                if (endpointLoadBalancer.Headers["clientIP"] != null)
                    clientIP = endpointLoadBalancer.Headers["clientIP"];
                if (string.IsNullOrEmpty(clientIP))
                {
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
            info.APIUsage_LocationName = document.GetElementValue("LOCATIONNAME", string.Empty, fields);
            info.APIUsage_ClientName = document.GetElementValue("CLIENT_NAME", string.Empty, fields);

            _client.CheckClient();
            string result = _client.PerformFA(info, false);
            document.SetValue(fields.Where(p => p.Name == "MESSAGE").First().XPath, result);
            document.SetValue(fields.Where(p => p.Name == "RESULT").First().XPath,
                result.ToUpper() == "SUCCESS" ? "SUCCESS" : "ERROR");

            return document;
        }

        #endregion
    }
}
