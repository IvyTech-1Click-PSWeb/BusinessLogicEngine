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
	public class ChangePart : IAPI
	{

        private ChangePartWrapper.ChangePartWrapperSoapClient _client =
            new JGS.BusinessLogicEngine.ChangePartWrapper.ChangePartWrapperSoapClient();

		#region IAPI Members

		public System.Xml.XmlDocument Execute(System.Xml.XmlDocument document, List<Field> fields)
		{
			JGS.Shared.Validation.DocumentDefinition definition = JGS.Shared.Validation.Validator.GetDocumentDefinition(document.DocumentElement.LocalName);

			ChangePartWrapper.ChangePartInfo info = new ChangePartWrapper.ChangePartInfo();

			info.SesCustomerID = "1";
			info.RequestId = document.GetElementValue("REQUESTID","1",fields);

			info.BCN = document.GetElementValue( "BCN", "1", null);

            info.NewSerialNo = document.GetElementValue("CP_NEWSERIALNUMBER", default(string), fields);
            info.NewPartNo = document.GetElementValue( "CP_NEWPARTNUMBER", default(string), fields);
			info.NewRevisionLevel = document.GetElementValue( "CP_NEWREVISIONLEVEL", default(string), fields);
			info.NewFixedAssetTag = document.GetElementValue( "CP_NEWFIXEDASSETTAG", default(string), fields);
			info.Notes = document.GetElementValue( "CP_NOTES", default(string), fields);
            info.MustBeOnHold = document.GetElementValue("CP_MUSTBEONHOLD", false, fields);
            info.ReleaseIfHold = document.GetElementValue("CP_RELEASEIFHOLD", false, fields);
            info.MustBeTimedIn = document.GetElementValue("CP_MUSTBETIMEDIN", false, fields);
            info.TimedInWorkCenterName = document.GetElementValue( "CP_TIMEDINWORKCENTERNAME", default(string), fields);
			info.userName = document.GetElementValue( "USERNAME", default(string), fields);
            info.Password = document.GetElementValue("PASSWORD", default(string), fields);

			if (fields.Where(p => p.Name == "CP_FLEXFIELDS").Count() > 0)
			{
                info.FlexFieldList = new List<ChangePartWrapper.FlexFields>();
                foreach (XmlNode node in document.SelectNodes(fields.Where(prop => prop.Name == "CP_FLEXFIELDS").First().XPath))
				{
					ChangePartWrapper.FlexFields ff = new ChangePartWrapper.FlexFields();
                    ff.Name = node.GetNodeValue("FLEXFIELDNAME", fields);
                    ff.Value = node.GetNodeValue("FLEXFIELDVALUE", fields);
					info.FlexFieldList.Add(ff);
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
            string result = _client.PerformChangePart(info, false);
            document.SetValue(fields.Where(p => p.Name == "MESSAGE").First().XPath, result);
            document.SetValue(fields.Where(p => p.Name == "RESULT").First().XPath,
                result.ToUpper() == "SUCCESS" ? "SUCCESS" : "ERROR");

            return document;
		}
		#endregion
	}
}
