using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AprioriAllLib;

namespace ConsoleTestAprioriAll {

	public class Program {

		public static CustomerList GenerateRandomList(int customersCount, int maxTransactionCount, int maxTransactionLength) {
			Random random = new Random();
			CustomerList randomCustomerList = new CustomerList();

			Customer c;
			Transaction t;
			for (int i = 0; i < customersCount; ++i) {
				c = new Customer();
				int transactionsCount = random.Next() % maxTransactionCount + 1;
				for (int tn = 0; tn < transactionsCount; ++tn) {
					int n = random.Next() % maxTransactionLength + 1;
					t = new Transaction();
					for (int it = 0; it < n; ++it) {
						int itemVal = (random.Next() % maxTransactionLength + 10) * 10;
						if (t.Contains(itemVal)) {
							--it;
							continue;
						}
						t.AddItem(itemVal);
					}
					c.AddTransaction(t);
				}
				randomCustomerList.Customers.Add(c);
			}
			return randomCustomerList;
		}

		public static void Main(string[] args) {
			Console.Out.WriteLine("AprioriAll algorithm implementation in .NET");
			//Arrange
			CustomerList randomExample = GenerateRandomList(700, 7, 7);
			double support = 0.4;

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
