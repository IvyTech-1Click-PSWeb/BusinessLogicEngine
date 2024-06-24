using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.BusinessLogicEngine.Model;

namespace JGS.BusinessLogicEngine.API
{
	interface IAPI
	{
		XmlDocument Execute(XmlDocument document,List<Field> fields);
	}
}
