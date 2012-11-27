using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AprioriAllLib;

namespace ConsoleTestAprioriAll {

	class Program {

		static void Main(string[] args) {
			//Arrange
			CustomerList list = new CustomerList();
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			list.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			list.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			list.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 90 }));
			list.Customers.Add(new Customer(new int[] { 90 }));
			//Assert.AreEqual(5, list.Customers.Count());

			List<Customer> expected = new List<Customer>();
			expected.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			expected.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }));
			//Assert.AreEqual(2, expected.Count());

			//Act
			List<Customer> results = AprioriAllAlgorithm.execute(list, 0.25);

			//Assert
			//CollectionAssert.AreEqual(expected, results);
		}

	}

}
