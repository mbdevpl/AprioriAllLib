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
			var program = new ProgramTest_AprioriAll();

			Console.Out.WriteLine("Serialized AprioriAll algorithm test");

			CustomerList input
				//= XmlReader.ReadFromXmlFile("dataset3.xml");
				//= Data.DataSet3;
				= InputGenerator.GenerateRandomList(2000, 5, 5);
			double support = 0.6;

			if (input.Customers.Count <= 100)
				program.PrintInput(input);

			Console.Out.WriteLine("\nComputation:");
			AprioriAll all = new AprioriAll(input);
			List<Customer> results = all.RunAprioriAll(support, true);

			program.PrintAprioriAllOutput(results);

			Console.Write("End.");
			Console.ReadKey();
		}

	}
}
