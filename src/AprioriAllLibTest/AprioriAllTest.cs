using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for serialized version of AprioriAll algorithm.
	/// </summary>
	[TestClass]
	public class AprioriAllTest : AprioriAllTestBase
	{

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			TestBaseInitialize();
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_Example1_LowSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30, 50, 70 }));
			//expected.Add(new Customer(new int[] { 30 }, new int[] { 80 })); // where is it ??
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }/*, new int[] { 40 }*/));
			expected.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			//Assert.AreEqual(5, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.Example1);
			List<Customer> results = all.RunAprioriAll(0.2, true);

			//Assert
			//areEquivalent doesn't work ?!
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_Example1_MediumSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(1, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.Example1);
			List<Customer> results = all.RunAprioriAll(0.4);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_Example1_HighSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(1, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.Example1);
			List<Customer> results = all.RunAprioriAll(0.6);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_DataSet1()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			Assert.AreEqual(2, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.DataSet1);
			List<Customer> results = all.RunAprioriAll(0.25);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_DataSet2_LowSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 50 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }, new int[] { 60 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 30, 40 }, new int[] { 50 }, new int[] { 80 }, new int[] { 90 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.DataSet2);
			List<Customer> results = all.RunAprioriAll(0.2, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_DataSet2_HighSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 50 }, new int[] { 80 }));
			expected.Add(new Customer(new int[] { 80 }, new int[] { 90 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.DataSet2);
			List<Customer> results = all.RunAprioriAll(0.4);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_DataSet3_LowSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 50 }, new int[] { 10 }));
			expected.Add(new Customer(new int[] { 10, 20 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 70 }, new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			Assert.AreEqual(5, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.DataSet3);
			List<Customer> results = all.RunAprioriAll(0.2, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod, TestCategory("AprioriAll"), TestCategory("Serialized")]
		public void Test_AprioriAll_DataSet3_HighSupport()
		{
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }));
			expected.Add(new Customer(new int[] { 80 }));
			expected.Add(new Customer(new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 30, 50 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(data.DataSet3);
			List<Customer> results = all.RunAprioriAll(0.4, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

	}
}
