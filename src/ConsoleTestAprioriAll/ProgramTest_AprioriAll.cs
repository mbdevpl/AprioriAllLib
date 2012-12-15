using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// Runs AprioriAll algorithm on random input.
	/// </summary>
	public class ProgramTest_AprioriAll : AprioriAllTestBase
	{

		private static void Main(string[] args)
		{
			TestBaseInitialize();
			Console.Out.WriteLine("Serialized AprioriAll algorithm test");

			//Arrange
			CustomerList input
				//= InputGenerator.GenerateRandomList(700, 7, 7);
				//= XmlReader.ReadFromXmlFile("dataset3.xml");
				= data.Example1;
			double support = 0.2;

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in input.Customers)
				Console.Out.WriteLine(" - {0}", c);

			//Act
			Console.Out.WriteLine("\nComputation:");
			AprioriAll all = new AprioriAll(input);
			List<Customer> results = all.RunAprioriAll(support, true);

			//Assert
			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in results)
				Console.Out.WriteLine(" - {0}", c);

			Console.Write("End.");
			Console.ReadKey();
		}

	}
}
