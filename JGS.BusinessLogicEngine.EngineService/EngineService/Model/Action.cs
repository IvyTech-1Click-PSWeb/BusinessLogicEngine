using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JGS.BusinessLogicEngine.Support;
using System.Text.RegularExpressions;
using System.Xml;
using System.Configuration;
using System.Reflection;
using JGS.BusinessLogicEngine.API;
using System.Threading;
using System.Web.Compilation;
using JGS.Shared;
using JGS.Shared.Package;

namespace JGS.BusinessLogicEngine.Model
{
	internal class Action
	{
		public enum ActionType
		{
			FUNCTION,
			DLL,
			DATABASE,
			API,
			STOP,
			SET,
			CONTINUE,
			SELECT,
			RESULTCODE,
			RESULT
		}

		private static Regex _action = new Regex(@"(^|(?<=;))[A-Z]+");
		private static Regex _actionParameter = new Regex(@"(?<={).*(?=})");
		private static Regex _returnField = new Regex(@"^\[[a-zA-Z]\w*\]((?=\=)|(?=\s?$))");
		private static Regex _selectLiteral = new Regex(@"^\^.*\^(?=(\s?$))");
		private static Regex _functionCall = new Regex(@"(((?<={)[A-Za-z])|(?<==)).*(?=\()");
		private static Regex _functionParameters = new Regex(@"(?<=\().+(?=\))");
		private static Regex _functionParameter = new Regex(@"\^.*\^(?=,)|\^.*\^$|(\[[a-zA-Z]\w*\])");
		private static Regex _literal = new Regex(@"\^.[^\^]*\^");
		private static Regex _literalValue = new Regex(@"(?<=\^).[^\^]*(?=\^)");

		private static Dictionary<string, Assembly> _knownAssemblies = new Dictionary<string, Assembly>();

		public static List<Action> GetActionsFromString(string actionsstring)
		{
			List<string> actionstrings = actionsstring.Split(';').ToList();
			List<Action> newActions = new List<Action>();

			foreach (string actionstring in actionstrings)
			{
				if (!string.IsNullOrEmpty(actionstring))
				{
					Action newAction = new Action();
					newAction.Actionstring = actionstring;
					newActions.Add(newAction);
				}
			}
			return newActions;
		}

		private ReaderWriterLockSlim _actionStringLock = new ReaderWriterLockSlim();
		private string _actionString = string.Empty;
		public string Actionstring
		{
			get
			{
				_actionStringLock.EnterReadLock();
				string retValue = _actionString;
				_actionStringLock.ExitReadLock();
				return retValue;
			}
			set
			{
				_actionStringLock.EnterWriteLock();
				_actionString = value;
				_actionStringLock.ExitWriteLock();
			}
		}

		public string TypeName
		{
			get
			{
				return _action.Match(Actionstring).ToString();
			}
		}
		public ActionType Type
		{
			get
			{
				return (ActionType)Enum.Parse(typeof(ActionType), this.TypeName);
			}
		}
		public string Parameter
		{
			get
			{
				return _actionParameter.Match(Actionstring).ToString();
			}
		}
		public string ReturnFieldName
		{
			get
			{
				return Parser.FieldName(_returnField.Match(Parameter).ToString());
			}
		}
		public string SelectParameter
		{
			get
			{
				if (_returnField.Matches(Parameter).Count > 0)
				{
					return ReturnFieldName;
				}
				else if (_selectLiteral.Matches(Parameter).Count > 0)
				{
					return _selectLiteral.Match(Parameter).ToString();
				}
				return null;
			}
		}
		public bool SelectLiteral
		{
			get
			{
				if (_returnField.Matches(Parameter).Count > 0)
				{
					return false;
				}
				else if (_selectLiteral.Matches(Parameter).Count > 0)
				{
					return true;
				}
				return false;
			}
		}
		public string FunctionName
		{
			get
			{
				return _functionCall.Match(Actionstring).ToString();
			}
		}

		private string GetFunctionParameters()
		{
			return _functionParameters.Match(Actionstring).ToString();
		}
		private string GetLiteralValue(string literal, List<Field> fields, XmlDocument document)
		{
			string value =  _literalValue.Match(literal).ToString();
			foreach (string fieldName in Parser.FieldNames(value))
			{
				if((from p in fields
				 where p.Name == Parser.FieldName(fieldName)
				 select p).Count() != 0) {
					 Field field = (from p in fields
									where p.Name == Parser.FieldName(fieldName)
									select p).First();
					 value = value.Replace(fieldName, document.GetValue(field.XPath, string.Empty));
				 }
			}
			return value;
		}

		public List<string> FunctionParameters
		{
			get
			{
				return (from Match p in
							_functionParameter.Matches(GetFunctionParameters())
						select p.ToString()).ToList();
			}
		}

		internal bool CarryOut(Package package, RuleResult result,
			List<Field> fields)
		{
			package.TraceEvent("Executing action " + this.Actionstring, PackageTraceLevel.Verbose);
			try
			{
				switch (Type)
				{
					case ActionType.RESULT:
						ActionResult(package, fields);
						return true;
					case ActionType.RESULTCODE:
						ActionResultCode(package, fields);
						return true;
					case ActionType.SELECT:
						ActionSelect(package, fields);
						return true;
					case ActionType.CONTINUE:
						//Nothing to do, carry on
						return true;
					case ActionType.STOP:
						result.IsContinue = false;
						return false;
					case ActionType.DLL:
						ActionDll(package, fields);
						break;
					case ActionType.DATABASE:
						ActionDatabase(package, fields);
						break;
					case ActionType.API:
						ActionApi(package, fields);
						break;
					case ActionType.FUNCTION:
						ActionFunction(package, fields);
						break;
					case ActionType.SET:
						ActionSet(package, fields);
						break;
					default:
						package.TraceEvent("The action specified (" + Actionstring + ") is not valid", PackageTraceLevel.Error);
						package.TransactionInformation.Status = TransactionStatus.Aborted;
						break;
				}
			}
			catch (Exception ex)
			{
				package.TraceEvent("An exception occurred processing action " + this.Actionstring + ": " + ex.Message, PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return false;
			}
			return true;
		}

		private void ActionResultCode(Package package, List<Field> fields)
		{
			if (FunctionParameters.Count != 2)
			{
				package.TraceEvent("Wrong number of arguments specified for RESULT action, expecting 2 (CODE, VALUE)", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}
			if ((from p in fields
				 where p.Name == "RESULT_CODES"
				 select p).Count() == 0)
			{
				package.TraceEvent("The Result Codes (RESULT_CODES) field is not defined in the document definition", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}
			XmlNode resultCodes = package.Document.SelectSingleNode((from p in fields
																	 where p.Name == "RESULT_CODES"
																	 select p).First().XPath);
			if (resultCodes == null)
			{
				resultCodes = package.Document.CreateXPathNode((from p in fields
																where p.Name == "RESULT_CODES"
																select p).First().XPath);
			}
			XmlNode resultCode = resultCodes
				.SelectSingleNode("ResultCode[Name='" + GetLiteralValue(FunctionParameters[0],fields,package.Document) + "']");
			XmlNode resultCodeName = null;
			XmlNode resultCodeValue = null;
			if (resultCode == null)
			{
				resultCode = package.Document.CreateElement("ResultCode");
				resultCodeName = package.Document.CreateElement("Name");
				resultCode.AppendChild(resultCodeName);
				resultCodeValue = package.Document.CreateElement("Value");
				resultCode.AppendChild(resultCodeValue);
				resultCodes.AppendChild(resultCode);
			}
			else
			{
				resultCodeName = resultCode.SelectSingleNode("Name");
				resultCodeValue = resultCode.SelectSingleNode("Value");
			}
			if (resultCodeName == null)
			{
				resultCodeName = package.Document.CreateElement("Name");
				resultCode.AppendChild(resultCodeName);
			}
			if (resultCodeValue == null)
			{
				resultCodeValue = package.Document.CreateElement("Value");
				resultCode.AppendChild(resultCodeValue);
			}

			if (GetLiteralValue(FunctionParameters[0],fields,package.Document) == string.Empty)
			{
				resultCodeName.InnerText = package.CurrentNode.GetValue<string>(
									(from p in fields
									 where p.Name == Parser.FieldName(FunctionParameters[0])
									 select p).First().XPath);
			}
			else
			{
				resultCodeName.InnerText = GetLiteralValue(FunctionParameters[0],fields,package.Document);
			}

			if (GetLiteralValue(FunctionParameters[1], fields, package.Document) == string.Empty)
			{
				resultCodeValue.InnerText = package.CurrentNode.GetValue<string>(
									(from p in fields
									 where p.Name == Parser.FieldName(FunctionParameters[1])
									 select p).First().XPath);
			}
			else
			{
				resultCodeValue.InnerText = GetLiteralValue(FunctionParameters[1], fields, package.Document);
			}
		}

		private void ActionResult(Package package, List<Field> fields)
		{
			if (FunctionParameters.Count != 2)
			{
				package.TraceEvent("Wrong number of arguments specified for RESULT action, expecting 2 (RESULT, MESSAGE)", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			string result = GetLiteralValue(FunctionParameters[0], fields, package.Document);
			string message = GetLiteralValue(FunctionParameters[1], fields, package.Document);
			if (string.IsNullOrEmpty(result))
			{
				result = package.CurrentNode.GetValue<string>(
					(from p in fields
					 where p.Name == Parser.FieldName(FunctionParameters[0])
					 select p).First().XPath);
			}
			if (string.IsNullOrEmpty(message))
			{
				message = package.CurrentNode.GetValue<string>(
					(from p in fields
					 where p.Name == Parser.FieldName(FunctionParameters[1])
					 select p).First().XPath);
			}

			if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(message))
			{
				package.TraceEvent("Blank arguments specified for RESULT action, expecting (RESULT, MESSAGE)", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			Field returnField =
				(from p in fields
				 where p.Name == "RESULT"
				 select p).DefaultIfEmpty(null).First();
			if (returnField == null)
			{
				package.TraceEvent("Result field not specified in document definition", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			Field messageField =
				(from p in fields
				 where p.Name == "MESSAGE"
				 select p).DefaultIfEmpty(null).First();
			if (returnField == null)
			{
				package.TraceEvent("Message field not specified in document definition", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			package.Document.SetValue(returnField.XPath, result);
			package.Document.SetValue(messageField.XPath, message);
		}

		private void ActionSelect(Package package, List<Field> fields)
		{
			if (FunctionParameters.Count > 0)
			{
				package.TraceEvent("The SELECT action does not take any parameters", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}
			if (string.IsNullOrEmpty(SelectParameter))
			{
				package.TraceEvent("The SELECT action requires either a field name or literal XPath to select", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			string xPath = null;
			if (SelectLiteral)
			{
				xPath = GetLiteralValue(SelectParameter, fields, package.Document);

			}
			else
			{
				Field field = (from p in fields
							   where p.Name == ReturnFieldName
							   select p).DefaultIfEmpty(null).First();
				if (field != null)
				{
					xPath = field.XPath;
				}
			}
			if (!string.IsNullOrEmpty(xPath))
			{
				XmlNode selectedNode = package.Document.SelectSingleNode(xPath);
				package.CurrentNode = selectedNode;
			}
			else
			{
				package.CurrentNode = null;
			}
		}

		private void ActionFunction(Package package, List<Field> fields)
		{
			string returnPath = string.Empty;
			if (!string.IsNullOrEmpty(ReturnFieldName))
			{
				Field returnField = GetReturnField(fields);
				if (returnField.IsCollection != 0)
				{
					package.TraceEvent("Assignment to collections is not supported", PackageTraceLevel.Error);
					package.TransactionInformation.Status = TransactionStatus.Aborted;
					return;
				}
			}
			else
			{
				package.TraceEvent("Function actions require a return field", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}


			returnPath = (from p in fields
						  where p.Name == Parser.FieldName(FunctionParameters[0])
						  select p).First().XPath;
			switch (FunctionName.ToUpper())
			{
				case "TOUPPER":
					if (string.IsNullOrEmpty(package.CurrentNode.GetValue<string>(returnPath)))
					{
						SetReturnField(package.CurrentNode, fields, string.Empty);
						break;
					}
					SetReturnField(package.CurrentNode, fields, package.CurrentNode.GetValue<string>(returnPath).ToUpper());
					break;
				case "TOLOWER":
					if (string.IsNullOrEmpty(package.CurrentNode.GetValue<string>(returnPath)))
					{
						SetReturnField(package.CurrentNode, fields, string.Empty);
						break;
					}
					SetReturnField(package.CurrentNode, fields, package.CurrentNode.GetValue<string>(returnPath).ToLower());
					break;
				case "TRIM":
					if (string.IsNullOrEmpty(package.CurrentNode.GetValue<string>(returnPath)))
					{
						SetReturnField(package.CurrentNode, fields, string.Empty);
						break;
					}
					SetReturnField(package.CurrentNode, fields, package.CurrentNode.GetValue<string>(returnPath).Trim());
					break;
				default:
					package.TraceEvent("Unknown function " + FunctionName + " specified for FUNCTION action " + Actionstring, PackageTraceLevel.Error);
					package.TransactionInformation.Status = TransactionStatus.Aborted;
					return;
			}
		}

		private void ActionSet(Package package, List<Field> fields)
		{
			if (FunctionParameters.Count != 1)
			{
				package.TraceEvent("Wrong number of arguments specified for SET action, expecting 1", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return;
			}

			if (GetLiteralValue(FunctionParameters[0], fields, package.Document) == string.Empty)
			{

				SetReturnField(package.CurrentNode, fields,
					package.CurrentNode.GetValue<string>(
					(from p in fields
					 where p.Name == Parser.FieldName(FunctionParameters[0])
					 select p).First().XPath));
			}
			else
			{
				SetReturnField(package.CurrentNode, fields, GetLiteralValue(FunctionParameters[0], fields, package.Document));
			}
		}

		private void ActionDatabase(Package package, List<Field> fields)
		{
			try
			{
				if (!string.IsNullOrEmpty(ReturnFieldName))
				{
					Field returnField = GetReturnField(fields);
					if (returnField.IsCollection != 0)
					{
						DbHelper.ExecuteDbFetchCollection(package.CurrentNode, returnField, fields, "StoredProcedure",
							FunctionName, GetFunctionParameters());
					}
					else
					{
						DbHelper.ExecuteDbFetch(package.CurrentNode, returnField, fields, "StoredProcedure",
							FunctionName, GetFunctionParameters());
					}
				}
				else
				{
					DbHelper.ExecuteDbDirect(package.CurrentNode, fields, "StoredProcedure", FunctionName,
						GetFunctionParameters());
				}
			}
			catch (Exception ex)
			{
				package.TraceEvent("An exception occurred processing action " + this.Actionstring + ": " + ex.Message, PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
			}
		}

		private void ActionDll(Package package, List<Field> fields)
		{
			CallLibrary(package, fields, FunctionName);
		}

		private void ActionApi(Package package, List<Field> fields)
		{
			CallLibrary(package, fields, FunctionName);
		}

		private void CallLibrary(Package package, List<Field> fields, string className)
		{
			try
			{
				IAPI api = null;

				BuildManager.GetReferencedAssemblies();

				if (_knownAssemblies.ContainsKey(className))
				{
					api = (IAPI)_knownAssemblies[className]
						.CreateInstance("JGS.BusinessLogicEngine.API." + className);
				}
				else
				{
					List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
					foreach (Assembly assembly in assemblies)
					{
						try
						{
							if (Array.Find(assembly.GetTypes(), p => p.Name == className) != null)
							{
								api = (IAPI)assembly.CreateInstance("JGS.BusinessLogicEngine.API." + className);
								_knownAssemblies.Add(className, assembly);
								break;
							}
						}
						catch
						{
						}
						try
						{
							if (Array.Find(assembly.GetExportedTypes(), p => p.Name == className) != null)
							{
								api = (IAPI)assembly.CreateInstance("JGS.BusinessLogicEngine.API." + className);
								_knownAssemblies.Add(className, assembly);
								break;
							}
						}
						catch
						{
						}
					}
				}
				if (api != null)
				{
					package.Document = api.Execute(package.Document, fields);
					api = null;
				}
				else
				{
					package.TraceEvent("Unknown API or DLL call in action (" + Actionstring + ")", PackageTraceLevel.Error);
					package.TransactionInformation.Status = TransactionStatus.Aborted;
					return;
				}
			}
			catch (Exception ex)
			{
				package.TraceEvent("An exception occurred processing action " + this.Actionstring + ": " + ex.Message, PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
			}
		}

		private Field GetReturnField(List<Field> fields)
		{
			Field returnField =
				(from p in fields
				 where p.Name == ReturnFieldName
				 select p).DefaultIfEmpty(null).First();
			if (returnField == null)
			{
				throw new ArgumentException("The action specified (" +
					Actionstring + ") refers to an undefined field ("
					+ ReturnFieldName + ")");
			}
			return returnField;
		}

		private void SetReturnField(XmlNode document, List<Field> fields, string value)
		{
			Field returnField = GetReturnField(fields);
			document.SetValue(returnField.XPath, value);
		}

	}
}
