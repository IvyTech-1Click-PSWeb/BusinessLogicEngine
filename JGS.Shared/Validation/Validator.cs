using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace JGS.Shared.Validation
{
	/// <summary>
	/// Contains extension methods for validating XML document formats
	/// </summary>
	public static class Validator
	{
		private static Regex _alphanumeric = new Regex(@"^\w*\s*$");
		private static Regex _numeric = new Regex(@"^[0-9]*\s*$");

		private static XmlSchema _schema = XmlSchema.Read(Assembly.GetExecutingAssembly()
			.GetManifestResourceStream("JGS.Shared.Validation.DocumentDefinitions.xsd"),
			(o, e) => { });
		private static XmlSchemaSet _schemaSet = new XmlSchemaSet();
		private static XDocument _documentDefinitionsXml = new XDocument();

		private static Dictionary<string, DocumentDefinition> _documentDefinitions = 
			new Dictionary<string, DocumentDefinition>();

		private static DateTime _definitionsLoaded;

		static Validator()
		{
			_schemaSet.Add(_schema);
			Assembly ass = Assembly.GetExecutingAssembly();
			LoadDefinitions();
		}

		private static void UpdateDefinitions()
		{
			if(_definitionsLoaded.AddMinutes(5) <= DateTime.Now) { LoadDefinitions(); }
		}

		private static void LoadDefinitions()
		{
			try
			{
				_documentDefinitionsXml = XDocument.Load(@"DocumentDefinitions.xml");
			}
			catch
			{
				_documentDefinitionsXml = XDocument.Load(
					Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), @"DocumentDefinitions.xml"));
			}

			_documentDefinitionsXml.Validate(_schemaSet, (o, e) => { throw new XmlException(e.Message, e.Exception); }, false);
			_definitionsLoaded = DateTime.Now;

			BuildModel();
		}

		private static void BuildModel()
		{
			_documentDefinitions.Clear();

			foreach(XElement definition in _documentDefinitionsXml.Root.Elements("DocumentDefinition"))
			{
				DocumentDefinition newDefinition = new DocumentDefinition();
				newDefinition.Root = definition.Attribute("Root").Value;
				BuilsModelFields(definition, newDefinition);

				BuildModelFieldGroups(definition, newDefinition);
				_documentDefinitions.Add(newDefinition.Root, newDefinition);
			}
		}

		private static void BuildModelFieldGroups(XElement definition, DocumentDefinition newDefinition)
		{
			foreach(XElement fieldGroup in (from p in definition.Elements("RequiredFields").Elements("FieldGroup") select p))
			{
				Dictionary<string, Field> newGroup = new Dictionary<string, Field>();
				foreach(XElement field in (from p in fieldGroup.Elements("Field") select p))
				{
					Field newField = BuildModelField(field);
					newGroup.Add(newField.Name, newField);
				}
				newDefinition.RequiredFieldGroups.Add(fieldGroup.Attribute("Name").Value, newGroup);
			}
		}

		private static void BuilsModelFields(XElement definition, DocumentDefinition newDefinition)
		{
			foreach(XElement field in (from p in definition.Elements("RequiredFields").Elements("Field") select p))
			{
				Field newField = BuildModelField(field);
				newDefinition.RequiredFields.Add(newField.Name, newField);
			}
		}

		private static Field BuildModelField(XElement field)
		{
			Field newField = new Field();
			newField.Name = field.Attribute("Name").Value;
			if(field.Attribute("IsNumeric") != null) { newField.IsNumeric = true; }
			if(field.Attribute("IsAlphaNumeric") != null) { newField.IsAlphaNumeric = true; }
			newField.XPath = field.Value;
			return newField;
		}

		public static DocumentDefinition GetDocumentDefinition(string rootElementName)
		{
			if(!_documentDefinitions.ContainsKey(rootElementName)) { return null; }
			return _documentDefinitions[rootElementName];
		}
		
		private static string GetDescriptiveText(Field field)
		{
			return field.Name + " at " + field.XPath;
		}

		/// <summary>
		/// Indicates whether the document matches a known format
		/// </summary>
		/// <param name="document">An XmlDocument to evaluate</param>
		/// <returns>True if the document is in a known format</returns>
		public static bool IsValid(this XmlDocument document)
		{
			return (string.IsNullOrEmpty(Validate(document)));
		}

		/// <summary>
		/// Returns a description of any format issues found with a document
		/// </summary>
		/// <param name="document">An XmlDocument to evaluate</param>
		/// <returns>A string describing the issues found (if any)</returns>
		public static string Validate(this XmlDocument document)
		{
			UpdateDefinitions();
			if(!_documentDefinitions.ContainsKey(document.DocumentElement.LocalName))
			{
				throw new FormatException("Unknown XML document type: " + document.DocumentElement.LocalName);
			}

			DocumentDefinition definition = _documentDefinitions[document.DocumentElement.LocalName];
			if(definition == null) { return "Unknown XML document type"; }

			List<string> missingElements = new List<string>();
			foreach(Field field in definition.RequiredFields.Values)
			{
				if(!document.NodeExists(field.XPath) || document.NodeIsNullOrEmpty(field.XPath))
				{
					missingElements.Add(GetDescriptiveText(field));
				}
				else
				{
					if(!field.ValidateFormat(document.GetValue<string>(field.XPath)))
					{
							missingElements.Add(GetDescriptiveText(field) + " is not in the correct format");
					}
				}
			}

			foreach(KeyValuePair<string,Dictionary<string,Field>> fieldGroup in definition.RequiredFieldGroups)
			{
				bool foundOne = false;
				List<string> locations = new List<string>();
				foreach(Field field in fieldGroup.Value.Values)
				{
					locations.Add("\t" + GetDescriptiveText(field));
					if(!document.NodeIsNullOrEmpty(field.XPath) 
						&& field.ValidateFormat(document.GetValue<string>(field.XPath)))
					{
						foundOne = true;
						break;
					}
				}
				if(!foundOne) {
					missingElements.Add(fieldGroup.Key + " from ");
					missingElements.AddRange(locations);
				}
			}

			return string.Join("\n", missingElements.ToArray());
		}
	}
}
