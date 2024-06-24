using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JGS.Shared.Validation
{
	public class Field
	{
		private static Regex _alphanumeric = new Regex(@"^\w*\s*$");
		private static Regex _numeric = new Regex(@"^[0-9]*\s*$");

		public string Name { get; set; }
		public bool IsNumeric { get; set; }
		public bool IsAlphaNumeric { get; set; }
		public string XPath { get; set; }

		public bool ValidateFormat(string value)
		{
			if(IsNumeric && _numeric.Matches(value).Count == 0)
			{
				return false;
			}

			if(IsAlphaNumeric && _alphanumeric.Matches(value).Count == 0)
			{
				return false;
			}
			return true;
		}
	}
}
