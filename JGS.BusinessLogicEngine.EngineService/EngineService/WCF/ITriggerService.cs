using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace JGS.BusinessLogicEngine.WCF
{
	// NOTE: If you change the interface name "ITriggerService" here, you must also update the reference to "ITriggerService" in App.config.
	[ServiceContract(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public interface ITriggerService
	{
		[OperationContract]
		string ExecuteByString(string xmlString);
	}
}
