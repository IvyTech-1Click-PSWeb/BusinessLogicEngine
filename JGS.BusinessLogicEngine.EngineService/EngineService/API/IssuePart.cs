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
    public class IssuePart : IAPI
    {
        private FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient _client = new FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient();

        #region IAPI Members

        public System.Xml.XmlDocument Execute(System.Xml.XmlDocument document, List<JGS.BusinessLogicEngine.Model.Field> fields)
        {
            FailureAnalysisWrapper.IssuePartsInfo info = new FailureAnalysisWrapper.IssuePartsInfo();

            info.User = new FailureAnalysisWrapper.UserPwd();
            info.User.UserName = document.GetElementValue("USERNAME", string.Empty, fields);
            info.User.Password = document.GetElementValue("PASSWORD", string.Empty, fields);

            info.issueNonInventoryParts = new List<FailureAnalysisWrapper.IssueRemoveNonInventoryPartDO>();
            foreach (XmlNode dcNode in document.SelectNodes(fields.Where(prop => prop.Name == "FA_DEFECT_CODE").First().XPath))
            {
                foreach (XmlNode acNode in dcNode.SelectNodes(fields.Where(prop => prop.Name == "FA_ACTION_CODE").First().XPath))
                {
                    foreach (XmlNode npNode in acNode.SelectNodes(fields.Where(prop => prop.Name == "FA_NEW_COMPONENT").First().XPath))
                    {
                        FailureAnalysisWrapper.IssueRemoveNonInventoryPartDO issue = new FailureAnalysisWrapper.IssueRemoveNonInventoryPartDO();
                        issue.componentLocation = npNode.GetNodeValue("FA_COMPONENT_LOC",fields);
                        issue.description = npNode.GetNodeValue("FA_COMPONENT_DESC",fields);
                        issue.manufacturer = npNode.GetNodeValue("FA_COMPONENT_MAN",fields);
                        issue.manufacturerPartNo = npNode.GetNodeValue("FA_COMPONENT_MPN",fields);
                        issue.partNo = npNode.GetNodeValue("FA_COMPONENT_PN",fields);
                        issue.quantity =
                            string.IsNullOrEmpty(npNode.GetNodeValue("FA_COMPONENT_QTY",fields))
                            ? 1
                            : int.Parse(npNode.GetNodeValue("FA_COMPONENT_QTY",fields));
                        issue.serialNumber = npNode.GetNodeValue("FA_COMPONENT_SN",fields);
                        info.issueNonInventoryParts.Add(issue);

                        issue.flexFields = new List<FailureAnalysisWrapper.FlexFields>();
                        if (fields.Where(p => p.Name == "FA_FLEXFIELDS").Count() > 0)
                        {
                            foreach (XmlNode ffNode in npNode.SelectNodes(fields.Where(prop => prop.Name == "FA_FLEXFIELDS").First().XPath))
                            {
                                FailureAnalysisWrapper.FlexFields ff = new FailureAnalysisWrapper.FlexFields();
                                ff.Name = ffNode.GetNodeValue("FLEXFIELDNAME",fields);
                                ff.Value = ffNode.GetNodeValue("FLEXFIELDVALUE",fields);
                                issue.flexFields.Add(ff);
                            }
                        }
                        if (issue.flexFields.Count == 0)
                        {
                            issue.flexFields = null;
                        }

                        FailureAnalysisWrapper.actionCodes action = new FailureAnalysisWrapper.actionCodes();
                        action.actionCode = acNode.GetNodeValue("FA_ACTIONCODENAME",fields);
                        action.assemblyCode = acNode.GetNodeValue("FA_ACTIONCODEASSEMBLY",fields);
                        action.defectCode = dcNode.GetNodeValue("FA_DEFECTCODENAME",fields);
                        action.ecoCode = string.Empty;
                        action.Notes = acNode.GetNodeValue("FA_NOTES",fields);
                        action.Operation = FailureAnalysisWrapper.FAOperations.Add;

                        action.FlexFieldList = new List<FailureAnalysisWrapper.FlexFields>();
                        if (fields.Where(p => p.Name == "FA_FLEXFIELDS").Count() > 0)
                        {
                            foreach (XmlNode ffNode in acNode.SelectNodes(fields.Where(prop => prop.Name == "FA_FLEXFIELDS").First().XPath))
                            {
                                FailureAnalysisWrapper.FlexFields ff = new FailureAnalysisWrapper.FlexFields();
                                ff.Name = ffNode.GetNodeValue("FLEXFIELDNAME",fields);
                                ff.Value = ffNode.GetNodeValue("FLEXFIELDVALUE",fields);
                                action.FlexFieldList.Add(ff);
                            }
                        }
                        if (action.FlexFieldList.Count == 0)
                        {
                            action.FlexFieldList = null;
                        }

                        info.actionCodeValue = action;
                    }
                }
            }
            if (info.issueNonInventoryParts.Count == 0)
            {
                info.issueNonInventoryParts = null;
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
            string result = _client.PerformIssueParts(info, false);
            document.SetValue(fields.Where(p => p.Name == "MESSAGE").First().XPath, result);
            document.SetValue(fields.Where(p => p.Name == "RESULT").First().XPath,
                result.ToUpper() == "SUCCESS" ? "SUCCESS" : "ERROR");

            return document;
        }

        #endregion
    }
}
