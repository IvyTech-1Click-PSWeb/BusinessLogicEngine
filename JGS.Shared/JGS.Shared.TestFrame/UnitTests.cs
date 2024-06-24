using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using JGS.Shared.Validation;
using JGS.Shared.Package;

namespace JGS.Shared.TestFrame
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class UnitTests
	{
		public UnitTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestInvalidDocument()
		{
			XmlDocument document = new XmlDocument();
			document.Load(@"MissingRequired.xml");
			string errors = document.Validate();
			Assert.IsTrue(errors.Length > 0);
		}

		[TestMethod]
		public void TestValidDocument()
		{
			XmlDocument document = new XmlDocument();
			document.Load(@"PreValidate.xml");
			string errors = document.Validate();
			Assert.IsTrue(errors.Length==0);
		}

		[TestMethod]
		public void TestTriggerDocument()
		{
			XmlDocument document = new XmlDocument();
			document.Load(@"Timeout.xml");
			string errors = document.Validate();
			Assert.IsTrue(errors.Length == 0);
		}

		[TestMethod]
		public void TestBadTriggerDocument()
		{
			XmlDocument document = new XmlDocument();
			document.Load(@"BadTimeout.xml");
			string errors = document.Validate();
			Assert.IsFalse(errors.Length == 0);
		}

		[TestMethod]
		public void TestIsValid()
		{
			XmlDocument document = new XmlDocument();

			document.Load(@"MissingRequired.xml");
			Assert.IsFalse(document.IsValid());

			document.Load(@"PreValidate.xml");
			Assert.IsTrue(document.IsValid());

			document.Load(@"Timeout.xml");
			Assert.IsTrue(document.IsValid());

		}

		[TestMethod]
		public void TestPackUnPack()
		{
			XmlDocument document = new XmlDocument();
			document.Load(@"PreValidate.xml");

			JGS.Shared.Package.Package testPackage= new Package.Package(document);
			testPackage.TransactionInformation.Status = JGS.Shared.TransactionStatus.Committed;
			testPackage.TransactionInformation.Messages.Add("Success");
			testPackage.TransactionInformation.DistributedIdentifier = Guid.NewGuid();

			XmlDocument retPackage = testPackage.UnPack();
			Assert.IsTrue(retPackage.GetValue<string>("/UnitTest/TransactionInformation/Status") == "Committed");
		}
	}
}
