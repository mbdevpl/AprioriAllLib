using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AprioriAllLib;

namespace AprioriAllTest {

	[TestClass]
	public class AprioriAllTest {

		private static CustomerList Example1;

		private static CustomerList Example2;

		private static CustomerList DataSet1;

		private static CustomerList DataSet2;

		[ClassInitialize]
		public static void InitializeTestSuite(TestContext testContext) {

			Example1 = new CustomerList();
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			Example1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			Example1.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, Example1.Customers.Count());

			Example2 = new CustomerList();
			Example2.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			Example2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			Example2.Customers.Add(new Customer(new int[] { 30, 50, 70 }, new int[] { 10, 20 }));
			Example2.Customers.Add(new Customer(new int[] { 30, 80 }, new int[] { 40, 70 }, new int[] { 90, 30, 40, 50 }));
			Example2.Customers.Add(new Customer(new int[] { 90 }, new int[] { 80 }));
			Example2.Customers.Add(new Customer(new int[] { 50, 10 }, new int[] { 80 }));
			Assert.AreEqual(6, Example2.Customers.Count());

			DataSet1 = new CustomerList();
			DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			DataSet1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			DataSet1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 90 }));
			DataSet1.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, DataSet1.Customers.Count());

			DataSet2 = new CustomerList();
			DataSet2.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			DataSet2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			DataSet2.Customers.Add(new Customer(new int[] { 30, 50, 70 }, new int[] { 10, 20 }));
			DataSet2.Customers.Add(new Customer(new int[] { 30, 80 }, new int[] { 40, 70 }, new int[] { 90, 30, 40, 50 }));
			DataSet2.Customers.Add(new Customer(new int[] { 90 }, new int[] { 80 }));
			DataSet2.Customers.Add(new Customer(new int[] { 50, 10 }, new int[] { 80 }));
			Assert.AreEqual(6, DataSet2.Customers.Count());

			// future sets may be read from xml files?
			//XmlReader reader = new XmlReader();
			//CustomerList cList = reader.ReadFromXmlFile(/* full path or whatever else works*/);
		}

		[ClassCleanup]
		public static void CleanupTestSuite() {
		}

		[TestMethod]
		public void Test_Apriori_Example1_LowSupport() {
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(1, 10));
			expected.Add(new Litemset(1, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(1, 40));
			expected.Add(new Litemset(1, 50));
			expected.Add(new Litemset(1, 60));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(1, 80));
			expected.Add(new Litemset(1, 90));
			expected.Add(new Litemset(1, 10, 20));
			expected.Add(new Litemset(1, 10, 60));
			expected.Add(new Litemset(1, 10, 70));
			expected.Add(new Litemset(1, 30, 50));
			expected.Add(new Litemset(1, 30, 70));
			expected.Add(new Litemset(1, 40, 70));
			expected.Add(new Litemset(1, 50, 70));
			expected.Add(new Litemset(1, 60, 70));
			expected.Add(new Litemset(1, 10, 60, 70));
			expected.Add(new Litemset(1, 30, 50, 70));
			Assert.AreEqual(19, expected.Count());

			//Act
			Apriori apriori = new Apriori(Example1);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_Apriori_Example1_HighSupport() {
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 70));
			Assert.AreEqual(2, expected.Count());

			//Act
			Apriori apriori = new Apriori(Example1);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.6);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_Apriori_Example2_LowSupport() {
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(2, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			expected.Add(new Litemset(2, 10, 20));
			expected.Add(new Litemset(2, 30, 40));
			expected.Add(new Litemset(3, 30, 50));
			expected.Add(new Litemset(2, 40, 50));
			expected.Add(new Litemset(2, 40, 70));
			expected.Add(new Litemset(2, 30, 40, 50));
			Assert.AreEqual(14, expected.Count());

			//Act
			Apriori apriori = new Apriori(Example2);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets); // areEquivalent doesn't work
		}

		[TestMethod]
		public void Test_Apriori_Example2_HighSupport() {
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			expected.Add(new Litemset(3, 30, 50));
			Assert.AreEqual(8, expected.Count());

			//Act
			Apriori apriori = new Apriori(Example2);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.5);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_AprioriAll_Example1() {
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }));
			Assert.AreEqual(1, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(Example1, 0.25);

			//Assert
			CollectionAssert.AreEqual(expected, results);
		}

		[TestMethod]
		public void Test_AprioriAll_Example2() {
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 70 }, new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }, new int[] { 10, 20 }));
			expected.Add(new Customer(new int[] { 80 }, new int[] { 90 }, new int[] { 30, 40, 50 }));
			Assert.AreEqual(5, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(Example2, 0.3);

			//Assert
			CollectionAssert.AreEqual(expected, results);
		}

		[TestMethod]
		public void Test_AprioriAll_DataSet1() {
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			Assert.AreEqual(2, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(DataSet1, 0.25);

			//Assert
			CollectionAssert.AreEqual(expected, results); //areEquivalent doesn't work
		}

		[TestMethod]
		public void Test_AprioriAll_DataSet2_LowSupport() {
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 70 }, new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }, new int[] { 10, 20 }));
			expected.Add(new Customer(new int[] { 80 }, new int[] { 90 }, new int[] { 30, 40, 50 }));
			Assert.AreEqual(5, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(DataSet2, 0.2);

			//Assert
			CollectionAssert.AreEqual(expected, results); // areEquivalent doesn't work
		}

		[TestMethod]
		public void Test_AprioriAll_DataSet2_HighSupport() {
			//Arrange
			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 10 }, new int[] { 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			expected.Add(new Customer(new int[] { 70 }, new int[] { 30, 50 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 70 }, new int[] { 10, 20 }));
			expected.Add(new Customer(new int[] { 80 }, new int[] { 90 }, new int[] { 30, 40, 50 }));
			Assert.AreEqual(5, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(DataSet2, 0.4);

			//Assert
			CollectionAssert.AreEqual(expected, results); // areEquivalent doesn't work
		}

	}

}
