using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AprioriAllLib;

namespace ConsoleTestAprioriAll {

	public class Program {

		public static void Main(string[] args) {
			Console.Out.WriteLine("AprioriAll algorithm implementation in .NET");
			//Arrange
			CustomerList randomExample = InputGenerator.GenerateRandomList(700, 7, 7);
			double support = 0.4;

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in randomExample.Customers) {
				Console.Out.WriteLine(" - {0}", c);
			}

			//Act
			Console.Out.WriteLine("\nComputation:");
			AprioriAll all = new AprioriAll(randomExample);
			List<Customer> results = all.Execute(support, true);

			//Assert
			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in results) {
				Console.Out.WriteLine(" - {0}", c);
			}
		}

	}

}
