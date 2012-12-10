using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AprioriAllTest
{
	/// <summary>
	/// Summary description for AprioriTest
	/// </summary>
	[TestClass]
	public class AprioriTest
	{

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			;
		}

		//Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup]
		public static void MyClassCleanup() { ;}

		[TestMethod]
		public void TestMethod1()
		{
			//
			// TODO: Add test logic here
			//
		}

	}
}
