using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using JGS.Shared.Package;
using System.Data;

namespace JGS.BusinessLogicEngine.WCF
{
	[ServiceContract(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public interface IFieldOneClickService
	{
		[OperationContract]
		bool Send(Package inPackage);

		[OperationContract]
		Package Receive(String inGuid);

		[OperationContract]
		List<Package> ReceiveAll(string inGuid);

		[OperationContract]
		string ExecuteDirect(string package);

        [OperationContract]
        DataSet ValidateTesterName(string TesterName);

        [OperationContract]
        string InsertTesterLog(string TesterName, string Part_NO, string ITEM_SERIAl_NO, string TEST_RESULT, string XML_DATA_CONTENT, string FILE_PATH,
                                    string DETAIL_DATA_TABLE, string ELAPSED_TIME, string MANUFACTURER, string PRODUCT_NAME, string USER_NAME, string USER_ID,
                                    string SCRIPT_NAME, string STATIONMA, string OSTYPE, string OSVERSION, string FIRMWAREVERSION, string TESTERWVERSION,
                                    string OEMINSTALL, string KERNELLVERSION, string FIXED_ASSET_TAG, string START_DATE, string END_DATE);
	}
}
