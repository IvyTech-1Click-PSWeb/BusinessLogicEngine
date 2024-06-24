using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace JGS.BusinessLogicEngine.WCF
{
	[ServiceBehavior(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
	public class TriggerService : ITriggerService
	{
		private static BLEService _service = new BLEService();


		#region ITriggerService Members

		public string ExecuteByString(string xmlString)
		{
			return _service.ExecuteByString(xmlString);
		}

		#endregion
	}
}
