using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using JGS.BusinessLogicEngine.Model;
using JGS.Shared;

namespace JGS.BusinessLogicEngine.API.Support
{
	public static class Extensions
	{
		internal static T GetElementValue<T>(this XmlDocument document, string fieldName, T defaultValue, List<Field> fields)
			where T : System.IConvertible
		{
			if (fields.Where(p => p.Name == fieldName).Count() != 0)
			{
				return document.GetValue<T>(fields.Where(p => p.Name == fieldName).First().XPath, defaultValue);
			}
			else
			{
				return defaultValue;
			}
		}
	}
}
