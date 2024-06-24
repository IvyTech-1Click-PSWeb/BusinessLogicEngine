using JGS.BusinessLogicEngine.Model;
using System.Collections.Generic;

namespace JGS.BusinessLogicEngine.API
{
    public class UnitTest : IAPI
	{
		#region IAPI Members

		public System.Xml.XmlDocument Execute(System.Xml.XmlDocument document, List<Field> fields)
		{
			return document;
		}

		#endregion
	}
}
