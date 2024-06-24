using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Oracle.DataAccess.Client;
using JGS.BusinessLogicEngine.Model;
using JGS.Shared;
using System.Data;
using System.Xml;

namespace JGS.BusinessLogicEngine.Support
{
	internal static class Parser
	{
		private static Regex _field = new Regex(@"\[[a-zA-Z]\w*\]");
		private static Regex _fieldName = new Regex(@"(?<=\[).*(?=\])");

		public static string FieldName(string toParse)
		{
			return _fieldName.Match(toParse).ToString();
		}

		public static List<string> FieldNames(string toParse)
		{
			return (from Match p in _field.Matches(toParse) select p.ToString()).ToList();
		}

		public static List<OracleParameter> ParseDatabaseParameters(string toParse, List<Field> fields,
			XmlNode workingXml)
		{
			List<OracleParameter> newParameters = new List<OracleParameter>();
			foreach(Match match in _field.Matches(toParse))
			{
				string fieldName = Parser.FieldName(match.ToString());
				Field field = (from p in fields where p.Name == fieldName select p).First();

				OracleParameter newParameter = new OracleParameter(
					field.Name
					,DbHelper.GetDbType(field.DbDataType)
					,ParameterDirection.Input
					);
				newParameter.Value=workingXml.GetValue<string>(field.XPath);
				newParameters.Add(newParameter);
			}
			return newParameters;
		}

		public static string QuoteLiterals(string toParse)
		{
			return toParse.Replace("^", "\"");
		}
	}
}
