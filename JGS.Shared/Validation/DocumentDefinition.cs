using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JGS.Shared.Validation
{
	public class DocumentDefinition
	{
		public string Root
		{
			get;
			set;
		}

		private Dictionary<string, Field> _requiredFields = new Dictionary<string, Field>();
		public Dictionary<string, Field> RequiredFields
		{
			get
			{
				return _requiredFields;
			}
		}

		private Dictionary<string, Dictionary<string, Field>> _requriedFieldGroups =
			new Dictionary<string, Dictionary<string, Field>>();
		public Dictionary<string, Dictionary<string, Field>> RequiredFieldGroups
		{
			get
			{
				return _requriedFieldGroups;
			}
		}

		private Dictionary<string, Field> _optionalFields = new Dictionary<string, Field>();
		public Dictionary<string, Field> OptionalFields
		{
			get
			{
				return _optionalFields;
			}
		}

		public Field GetField(string fieldName)
		{
			if (_requiredFields.ContainsKey(fieldName))
			{
				return _requiredFields[fieldName];
			}
			
			Dictionary<string, Field> fieldGroup =
				(from p in _requriedFieldGroups
				 where p.Value.ContainsKey(fieldName)
				 select p.Value)
				.DefaultIfEmpty(null).First();
			if (fieldGroup != null)
			{
				return fieldGroup[fieldName];
			}

			if (_optionalFields.ContainsKey(fieldName))
			{
				return _optionalFields[fieldName];
			}

			return null;
		}
	}
}
