using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using JGS.Shared.Package;
using JGS.Shared;
using JGS.BusinessLogicEngine.Support;

namespace JGS.BusinessLogicEngine.Support
{
	public static class UnitTests
	{
		private static WCF.BLEService _service = new JGS.BusinessLogicEngine.WCF.BLEService();
		private static DateTime _startTime;
		private static DateTime _endTime;
		private static string _currentTests;
		private static string _currentTest;
		private static Package _package = new Package();

		public static void ExecuteTests()
		{
			_service.FlushCache();
			RunPreValidationUnitTests();
			RunDbFetchTests();
			RunRuleTests();
			RunProcessChainingTests();
			RunActionTests();
		}

		public static void StartTests(string testsName)
		{
			HttpContext.Current.Response.Write("<div><h2>Starting " + testsName + " tests</h2>\n");
			HttpContext.Current.Response.Flush();
			_currentTests = testsName;
		}

		public static void EndTests()
		{
			HttpContext.Current.Response.Write("<h2>End of " + _currentTests + " tests</h2></div>\n");
			HttpContext.Current.Response.Flush();
		}

		public static void StartTest(string testName)
		{
			HttpContext.Current.Response.Write("<div><h3>Starting " + testName + " test</h3>\n");
			HttpContext.Current.Response.Flush();
			_package = new Package();
			_currentTest = testName;
			_startTime = DateTime.Now;
		}

		public static void EndTest()
		{
			HttpContext.Current.Response.Write("<div style='padding-left: 4em; padding-bottom: 1em; font-weight: bold;'>TransactionStatus: " 
				+ _package.TransactionInformation.Status.ToString() + "</div>\n");
			foreach (string message in _package.TransactionInformation.Messages)
			{
				HttpContext.Current.Response.Write("<div style='padding-left: 4em;'>" + message + "</div>\n");
			}

			_endTime = DateTime.Now;
			HttpContext.Current.Response.Write("<div><i>Test completed in " + _endTime.Subtract(_startTime).TotalMilliseconds.ToString() +
								" milliseconds</i></div>\n");
			HttpContext.Current.Response.Write("<h3>End of " + _currentTest + " test</h3></div>\n");
			HttpContext.Current.Response.Flush();
		}

		public static void TestPassed()
		{
			HttpContext.Current.Response.Write("<h4>Test passed</h4>\n");
			HttpContext.Current.Response.Flush();
		}

		public static void TestFailed()
		{
			HttpContext.Current.Response.Write("<h4 class='fail'>Test failed</h4>\n");
			HttpContext.Current.Response.Flush();
		}

		public static Package ExecuteTest(string xmlPath)
		{
			XmlDocument xml = new XmlDocument();
			xml.Load(HttpContext.Current.Server.MapPath(xmlPath));

			return _service.ExecuteByPackage(new Package(xml));
		}

		private static void RunActionTests()
		{
			XmlDocument xml = new XmlDocument();
			StartTests("Actions");

			StartTest("Action");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/Actions.xml");
				xml = _package.UnPack();
				if(_package.TransactionInformation.Status == JGS.Shared.TransactionStatus.Aborted
				&& (xml.GetValue("/UnitTest/DbTests/DbFetch/FetchSingle/WORKCENTER_NAME", String.Empty) == "Pack out")
				&& xml.SelectNodes("/UnitTest/DbTests/DbFetch/FetchCollection/PROCESS_TYPE/NAME").Count > 0
				)
				{
					TestPassed();
				}
				else
				{
					TestFailed();
				}
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			EndTests();
		}

		private static void RunProcessChainingTests()
		{
			XmlDocument xml = new XmlDocument();
			StartTests("Process Chaining");

			StartTest("Chaining");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/Chaining.xml");
				if(_package.TransactionInformation.Status == JGS.Shared.TransactionStatus.Committed)
				{
					TestPassed();
				}
				else
				{
					TestFailed();
				}
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			EndTests();
		}

		private static void RunRuleTests()
		{
			XmlDocument xml = new XmlDocument();
			StartTests("Rule Parsing and Evaluation");

			StartTest("Rules");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/Rules.xml");
				if(_package.TransactionInformation.Status == JGS.Shared.TransactionStatus.Committed)
				{
					TestPassed();
				}
				else
				{
					TestFailed();
				}
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			EndTests();
		}

		private static void RunDbFetchTests()
		{
			XmlDocument xml = new XmlDocument();

			StartTests("Database Fetch");

			StartTest("Scalar Fetch");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/DbFetch.xml");
				xml = _package.UnPack();
				if(xml.GetValue("/UnitTest/DbTests/DbFetch/FetchSingle/WORKCENTER_NAME", String.Empty) == "Pack out")
				{
					TestPassed();
				}
				else
				{
					WriteMessage("Value returned was: " +
									 xml.GetValue("/UnitTest/DbTests/DbFetch/FetchSingle/WORKCENTER_NAME", String.Empty));
					TestFailed();
				}
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			EndTests();
		}

		private static void RunPreValidationUnitTests()
		{
			XmlDocument xml = new XmlDocument();

			StartTests("PreValidation");

			StartTest("Invalid Xml");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/InvalidXml.xml");
				TestFailed();
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestPassed();
			}
			EndTest();

			StartTest("Unknown Root Element");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/UnknownRoot.xml");
				TestFailed();
			}
			catch(FormatException)
			{
				TestPassed();
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			StartTest("Missing Required Elements");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/MissingRequired.xml");
				TestFailed();
			}
			catch(FormatException)
			{
				TestPassed();
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			StartTest("Valid Document");
			try
			{
				_package = ExecuteTest("~/BusinessLogicEngine/UnitTests/PreValidate.xml");
				xml = _package.UnPack();
				XmlElement xmlRet = xml.DocumentElement;
				if(xmlRet.InnerXml == xml.DocumentElement.InnerXml)
				{
					TestPassed();
				}
				else
				{
					WriteMessage("The output document was modified from the input document");
					TestFailed();
				}
			}
			catch(Exception ex)
			{
				WriteMessage(ex.Message);
				TestFailed();
			}
			EndTest();

			EndTests();
		}

		private static void WriteMessage(string message)
		{
			HttpContext.Current.Response.Write("<div><pre>\n");
			HttpContext.Current.Response.Write(message + "\n");
			HttpContext.Current.Response.Write("</pre></div>\n");
			HttpContext.Current.Response.Flush();
		}
	}
}
