using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using JGS.BusinessLogicEngine;
using System.Xml;
using JGS.BusinessLogicEngine.Model;

namespace JGS.BusinessLogicEngine.API
{
    static class APIHelper
    {
        public static void CheckClient(this FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient client)
        {
            if (client.State == CommunicationState.Faulted)
            {
                client = new FailureAnalysisWrapper.FailureAnalysisWrapperSoapClient();
            }
        }
        public static void CheckClient(this ChangePartWrapper.ChangePartWrapperSoapClient client)
        {
            if (client.State == CommunicationState.Faulted)
            {
                client = new ChangePartWrapper.ChangePartWrapperSoapClient();
            }
        }
        public static void CheckClient(this TimeOutWrapper.TimeOutWrapperSoapClient client)
        {
            if (client.State == CommunicationState.Faulted)
            {
                client = new TimeOutWrapper.TimeOutWrapperSoapClient();
            }
        }

        public static string GetNodeValue(this XmlNode node, string fieldName, List<Field> fields)
        {
            if (fields.Where(p => p.Name == fieldName).Count() == 0)
            {
                return string.Empty;
            }
            XmlNode valueNode = node.SelectSingleNode(fields.Where(p => p.Name == fieldName).First().XPath);
            if (valueNode == null)
            {
                return string.Empty;
            }
            return valueNode.InnerText;
        }
    }
}
