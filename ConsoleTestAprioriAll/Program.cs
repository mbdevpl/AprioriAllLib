using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AprioriAllLib;

namespace ConsoleTestAprioriAll {

	public class Program {

		public static CustomerList GenerateRandomList(int length) {
			Random random = new Random();
			CustomerList randomCustomerList = new CustomerList();

			Customer c;
			Transaction t;
			for (int i = 0; i < length; ++i) {
				c = new Customer();
				int transactionsCount = random.Next() % 5 + 1;
				for (int tn = 0; tn < transactionsCount; ++tn) {
					int n = random.Next() % 8 + 1;
					t = new Transaction();
					for (int it = 0; it < n; ++it)
						t.AddItem((random.Next() % 9 + 1) * 10);
					c.AddTransaction(t);
				}
				randomCustomerList.Customers.Add(c);
			}
			return randomCustomerList;
		}

		public static void Main(string[] args) {
			Console.Out.WriteLine("AprioriAll algorithm implementation in .NET");
			//Arrange
			CustomerList randomExample = GenerateRandomList(100);
			double support = 0.5;

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in randomExample.Customers) {
				Console.Out.WriteLine(" - {0}", c);
			}

			//Act
			Console.Out.WriteLine("\nComputation:");
			List<Customer> results = AprioriAllAlgorithm.Execute(randomExample, support, true);

			//Assert
			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in results) {
				Console.Out.WriteLine(" - {0}", c);
			}
		}

	}

}
