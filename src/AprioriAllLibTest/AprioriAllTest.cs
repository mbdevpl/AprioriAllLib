using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

//! Unit tests and testing tools related to Apriori and AprioriAll algorithms.
namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for serialized version of AprioriAll algorithm.
	/// </summary>
	[TestClass]
	public class AprioriAllTest : AprioriAllLibTestBase
	{
		[TestMethod]
		public void AprioriAll_Ex1Su020_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30, 50, 70 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			Assert.AreEqual(5, expected.Count);
			
			//Act & Assert
			AprioriAllSerializedLauncher(Data.Example1, 0.2, expected);
		}

		[TestMethod]
		public void AprioriAll_Ex1Su040_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(1, expected.Count);

			//Act & Assert
			AprioriAllSerializedLauncher(Data.Example1, 0.4, expected);
		}

		[TestMethod]
		public void AprioriAll_Ex1Su060_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(1, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.Example1);
			var results = all.RunAprioriAll(0.6);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod]
		public void AprioriAll_Ds1Su025_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			Assert.AreEqual(2, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.DataSet1);
			var results = all.RunAprioriAll(0.25);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod]
		public void AprioriAll_Ds2Su020_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 10 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 50 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }, new int[] { 60 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 30, 40 }, new int[] { 50 }, new int[] { 80 }, new int[] { 90 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.DataSet2);
			var results = all.RunAprioriAll(0.2, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod]
		public void AprioriAll_Ds2Su040_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 10 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			expected.Add(new Customer(new int[] { 50 }, new int[] { 80 }));
			expected.Add(new Customer(new int[] { 80 }, new int[] { 90 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.DataSet2);
			var results = all.RunAprioriAll(0.4);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod]
		public void AprioriAll_Ds3Su020_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 10, 20 }));
			expected.Add(new Customer(new int[] { 50 }, new int[] { 10 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 70 }, new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.DataSet3);
			var results = all.RunAprioriAll(0.2, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

		[TestMethod]
		public void AprioriAll_Ds3Su040_Test()
		{
			//Arrange
			var expected = new List<ICustomer>();
			expected.Add(new Customer(new int[] { 10 }));
			expected.Add(new Customer(new int[] { 80 }));
			expected.Add(new Customer(new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(6, expected.Count);

			//Act
			AprioriAll all = new AprioriAll(Data.DataSet3);
			var results = all.RunAprioriAll(0.4, true);

			//Assert
			CollectionAssert.AreEqual(expected, results, GetAprioriAllTestResults(expected, results));
		}

	}
}
