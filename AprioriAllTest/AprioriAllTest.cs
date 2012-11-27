using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AprioriAllLib;

namespace AprioriAllTest {

	[TestClass]
	public class AprioriAllTest {

		[TestMethod]
		public void TestAprioriForNoSupportSorted() {
			//Arrange
			CustomerList list = new CustomerList();
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			list.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			list.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			list.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, list.Customers.Count());

			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(1, 10)); // 2 ?
			expected.Add(new Litemset(1, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(1, 40)); // 2 ?
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
			Assert.AreEqual(9 + 8 + 2, expected.Count());

			//Act
			Apriori apriori = new Apriori(list);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void TestAprioriForNoSupportUnsorted() {
			//Arrange
			CustomerList list = new CustomerList();
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			list.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			list.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			list.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, list.Customers.Count());

			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(1, 80));
			expected.Add(new Litemset(1, 10, 20));
			expected.Add(new Litemset(1, 20));
			expected.Add(new Litemset(2, 10));
			expected.Add(new Litemset(1, 10, 60, 70));
			expected.Add(new Litemset(1, 60, 70));
			expected.Add(new Litemset(1, 10, 70));
			expected.Add(new Litemset(1, 10, 60));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(1, 60));
			expected.Add(new Litemset(1, 30, 50, 70));
			expected.Add(new Litemset(1, 50, 70));
			expected.Add(new Litemset(1, 30, 70));
			expected.Add(new Litemset(1, 30, 50));
			expected.Add(new Litemset(1, 50));
			expected.Add(new Litemset(1, 40, 70));
			expected.Add(new Litemset(1, 40));
			expected.Add(new Litemset(1, 90));
			Assert.AreEqual(19, expected.Count());

			//Act
			Apriori apriori = new Apriori(list);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}


		[TestMethod]
		public void TestCorrectnessForDataSet1() {

			// READING FROM THE DATABASE AND FINDING 1-LITEMSETS
			//XmlReader reader = new XmlReader();
			//CustomerList cList = reader.ReadFromXmlFile(/* full path or whatever else works*/);

			//Arrange
			CustomerList list = new CustomerList();
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			list.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			list.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 90 }));
			list.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, list.Customers.Count());

			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			Assert.AreEqual(2, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(list, 0.25);

			//Assert
			CollectionAssert.AreEqual(expected, results);
		}

	}

}
