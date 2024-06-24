using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using JGS.DAL;
using JGS.Shared;
using JGS.BusinessLogicEngine.Support;
using System.Xml;
using System.Configuration;
using System.Text.RegularExpressions;
using JGS.Shared.Package;

namespace JGS.BusinessLogicEngine.Model
{
	internal class Rule
	{
		private static string _connectionString = ConfigurationManager.
			ConnectionStrings["OracleConnectionString"].ConnectionString;
		private static string _schemaAndPackage = ConfigurationManager.AppSettings["BusinessLogicEngineSchemaAndPackage"];

		public long Id
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}
		public long ProcessId
		{
			get;
			set;
		}
		public string BooleanOperator
		{
			get;
			set;
		}
		public long FieldId1
		{
			get;
			set;
		}
		public long FieldId2
		{
			get;
			set;
		}
		public string ComparisonOperator
		{
			get;
			set;
		}
		public string FieldValue
		{
			get;
			set;
		}
		public long ParentRuleId
		{
			get;
			set;
		}
		private List<Action> _passActions = new List<Action>();
		public List<Action> PassActions
		{
			get
			{
				return _passActions;
			}
			set
			{
				_passActions = value;
			}
		}
		private List<Action> _failActions = new List<Action>();
		public List<Action> FailActions
		{
			get
			{
				return _failActions;
			}
			set
			{
				_failActions = value;
			}
		}

		private List<Rule> _childRules = new List<Rule>();
		public List<Rule> ChildRules
		{
			get
			{
				return _childRules;
			}
			set
			{
				_childRules = value;
			}
		}

		public static List<Rule> GetRules(long processId)
		{
			Rule ruleAlpha = new Rule()
			{
				ProcessId = processId,
				Id = 0,
				Name = "Alpha",
				FieldId1 = 0,
				FieldId2 = 0,
				FieldValue = null,
				BooleanOperator = null,
				ComparisonOperator = null,
				ParentRuleId = 0
			};

			List<Rule> newRules = new List<Rule>();
			newRules.Add(ruleAlpha);

			List<Rule> loadedRules = GetRulesFromDatabase(processId);

			ruleAlpha.AddChildren(loadedRules);

			return newRules;
		}

		private static List<Rule> GetRulesFromDatabase(long processId)
		{
			List<Rule> loadedRules = new List<Rule>();
			List<OracleParameter> myParams = new List<OracleParameter>();
			myParams.Add(new OracleParameter("ID", OracleDbType.Int64, processId, ParameterDirection.Input));
			myParams.Add(new OracleParameter("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output));

			DataSet ds = ODPNETHelper.ExecuteDataset(_connectionString,
																			CommandType.StoredProcedure,
																			_schemaAndPackage + ".GetProcessRulesByProcessId", myParams.ToArray());
			if (ds.Tables.Count != 1)
			{
				throw new DataException("The database returned an invalid process step list");
			}

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				Rule newRule = new Rule();

				newRule.ProcessId = processId;
				newRule.Id = row.GetItem<long>("RULE_ID");
				newRule.Name = row.GetItem<string>("RULE_NAME");
				newRule.FieldId1 = row.GetItem<long>("FIELD_ID1");
				newRule.FieldId2 = row.GetItem<long>("FIELD_ID2");
				newRule.ComparisonOperator = row.GetItem<string>("COMPARISON_OPERATOR");
				newRule.BooleanOperator = row.GetItem<string>("BOOLEAN_OPERATOR");
				newRule.ParentRuleId = row.GetItem<long>("PARENT_RULE_ID");
				newRule.FieldValue = row.GetItem<string>("FIELD_VALUE");
				newRule.PassActions = Action.GetActionsFromString(row.GetItem<string>("PASS_ACTION"));
				newRule.FailActions = Action.GetActionsFromString(row.GetItem<string>("FAIL_ACTION"));

				loadedRules.Add(newRule);
			}
			return loadedRules;
		}

		private void AddChildren(List<Rule> rules)
		{
			foreach (Rule rule in (from p in rules
								   where p.ParentRuleId == this.Id
								   select p))
			{
				this.ChildRules.Add(rule);
				rule.AddChildren(rules);
			}
		}

		public RuleResult Evaluate(Package package, List<Field> fields)
		{
			RuleResult result = new RuleResult()
			{
				IsPass = true,
				IsContinue = true
			};
			Evaluate(package, fields, result);
			return result;
		}

		public void Evaluate(Package package, List<Field> fields, RuleResult result)
		{
			bool isPassed = true;

			try
			{
				isPassed = EvaluateSelf(package, fields);
			}
			catch (Exception ex)
			{
				package.TraceEvent("An exception occurred processing rule " + this.Name + ": " + ex.Message, PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				isPassed = false;
			}
			if (package.TransactionInformation.Status == TransactionStatus.Aborted)
			{
				package.TraceEvent("Evaluate stopped because the transaction has been aborted.", PackageTraceLevel.Warning);
				return;
			}

			result.IsPass = result.IsPass && isPassed;

			//An action is always executed after an evaluation
			if (result.IsPass)
			{
				foreach (Action action in PassActions)
				{
					result.IsContinue &= action.CarryOut(package, result, fields);
					if (package.TransactionInformation.Status == TransactionStatus.Aborted)
					{
						package.TraceEvent("Pass action(s) stopped because the transaction has been aborted.", PackageTraceLevel.Warning);
						return;
					}

				}
			}
			else
			{
				foreach (Action action in FailActions)
				{
					result.IsContinue &= action.CarryOut(package, result, fields);
					if (package.TransactionInformation.Status == TransactionStatus.Aborted)
					{
						package.TraceEvent("Fail action(s) stopped because the transaction has been aborted.", PackageTraceLevel.Warning);
						return;
					}

				}
			}

			//Don't evaluate child rules if the result is not a pass or a action set IsFailed
			if (!result.IsContinue || !result.IsPass)
			{
				return;
			}

			//Child rules are only evaluated on pass
			result.IsPass &= EvaluateChildRules(package, fields, result);
		}

		private bool EvaluateChildRules(Package package, List<Field> fields, RuleResult result)
		{
			bool childRulesIsPassed = true;
			foreach (Rule childRule in ChildRules)
			{
				RuleResult childResult = new RuleResult()
				{
					IsPass = true,
					IsContinue = true
				};
				childRule.Evaluate(package, fields, childResult);
				if (package.TransactionInformation.Status == TransactionStatus.Aborted)
				{
					childResult.IsPass = false;
					childResult.IsContinue = false;
					return false;
				}

				if (childRule.BooleanOperator.ToUpper() == "OR")
				{
					childRulesIsPassed |= childResult.IsPass;
				}
				else if (childRule.BooleanOperator.ToUpper() == "AND" || string.IsNullOrEmpty(childRule.BooleanOperator))
				{
					childRulesIsPassed &= childResult.IsPass;
				}

				if (!childResult.IsContinue)
				{
					result.IsContinue = false;
					break;
				}
			}
			return childRulesIsPassed;
		}

		private bool EvaluateSelf(Package package, List<Field> fields)
		{
			//No condition to evaluate
			if (FieldId1 == 0)
			{
				package.TraceEvent("Rule " + this.Name + " has no condition", PackageTraceLevel.Verbose);
				return true;
			}

			//Invalid rule definition provided
			if ((FieldId2 == 0 && string.IsNullOrEmpty(FieldValue))
				|| string.IsNullOrEmpty(ComparisonOperator))
			{
				package.TraceEvent("Rule " + this.Name + " is incomplete", PackageTraceLevel.Error);
				package.TransactionInformation.Status = TransactionStatus.Aborted;
				return false;
			}

			Field leftField = (from p in fields
							   where p.Id == FieldId1
							   select p).First();
			string leftValue = package.CurrentNode.GetValue<string>(leftField.XPath, string.Empty);

			string rightValue = string.Empty;
			if (FieldId2 != 0)
			{
				Field rightField = (from p in fields
									where p.Id == FieldId2
									select p).First();
				if (DbHelper.MapDbType(rightField.DbDataType) != DbHelper.MapDbType(leftField.DbDataType))
				{
					package.TraceEvent("Rule " + this.Name + " has mismatching field types", PackageTraceLevel.Error);
					package.TransactionInformation.Status = TransactionStatus.Aborted;
					return false;
				}
				rightValue = package.CurrentNode.GetValue<string>(rightField.XPath, string.Empty);
			}
			else
			{
				rightValue = this.FieldValue;
			}

			bool retVal =  DoComparison(leftValue, rightValue, DbHelper.MapDbType(leftField.DbDataType));
			package.TraceEvent("Rule " + this.Name + " evaluated to " + retVal.ToString() + " (" +
				leftValue + " " + this.ComparisonOperator + " " + rightValue + ")",PackageTraceLevel.Verbose);
			return retVal;
		}

		private bool DoComparison(string leftValue, string rightValue, Type type)
		{
			//String operations and comparisons are more complex than numeric
			//and require special handling.
			//TODO - Refactor this
			switch (ComparisonOperator.ToUpper())
			{
				case "REGX!":
					return !(new Regex(rightValue).Match(leftValue).Success);
				case "REGEX":
					return new Regex(rightValue).Match(leftValue).Success;
				case "LEN<>":
					return !(leftValue.Length==int.Parse(rightValue));
				case "LEN<":
					return leftValue.Length < int.Parse(rightValue);
				case "LEN=":
					return leftValue.Length == int.Parse(rightValue);
				case "LEN>":
					return leftValue.Length > int.Parse(rightValue);
				case "ISNL":
					return leftValue == null;
			}
			switch (type.Name)
			{
				case "Int64":
				case "Int32":
				case "Int16":
					long leftlong = long.Parse(leftValue);
					long rightlong = long.Parse(rightValue);
					return DoComparison(leftlong, rightlong);
				case "Decimal":
					decimal leftdecimal = decimal.Parse(leftValue);
					decimal rightdecimal = decimal.Parse(rightValue);
					return DoComparison(leftdecimal, rightdecimal);
				case "Double":
				case "Single":
					double leftdouble = double.Parse(leftValue);
					double rightdouble = double.Parse(rightValue);
					return DoComparison(leftdouble, rightdouble);
				case "DateTime":
					DateTime leftDateTime = DateTime.Parse(leftValue);
					DateTime rightDateTime = DateTime.Parse(rightValue);
					return DoComparison(leftDateTime, rightDateTime);
				case "TimeSpan":
					TimeSpan leftTimeSpan = TimeSpan.Parse(leftValue);
					TimeSpan rightTimeSpan = TimeSpan.Parse(rightValue);
					return DoComparison(leftTimeSpan, rightTimeSpan);
				case "Byte":
					byte leftbyte = byte.Parse(leftValue);
					byte rightbyte = byte.Parse(rightValue);
					return DoComparison(leftbyte, rightbyte);
				case "String":
					return DoComparison(leftValue, rightValue);
				default:
					throw new ArgumentException
						("Unrecognized data type in process " + this.ProcessId + ", rule " + this.Name);
			}
		}

		private bool DoComparison<T>(T leftValue, T rightValue)
			where T : IComparable
		{
			switch (ComparisonOperator.ToUpper())
			{
				case "=":
				case "==":
					return leftValue.CompareTo(rightValue) == 0;
				case "<":
					return leftValue.CompareTo(rightValue) < 0;
				case ">":
					return leftValue.CompareTo(rightValue) > 0;
				case "<>":
				case "!=":
					return leftValue.CompareTo(rightValue) != 0;
				case ">=":
				case "=>":
					return leftValue.CompareTo(rightValue) >= 0;
				case "<=":
				case "=<":
					return leftValue.CompareTo(rightValue) <= 0;
				default:
					throw new ArgumentException("Invalid operator specified in rule "
						+ this.Name + " on process id " + this.ProcessId);
			}
		}
	}

	internal class RuleResult
	{
		public bool IsPass
		{
			get;
			set;
		}
		public bool IsContinue
		{
			get;
			set;
		}
		private List<string> _messages = new List<string>();
	}
}
