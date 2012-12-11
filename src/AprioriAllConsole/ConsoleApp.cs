using System;
using System.Collections.Generic;
using System.Linq;

namespace AprioriAllLib.ConsoleTest
{
	/// <summary>
	/// Defines a console application that runs AprioriAll on input given in command line parameters.
	/// 
	/// This is modified version of application by Karolina Baltyn.
	/// </summary>
	public class ConsoleApp
	{

		private static void Main(string[] args)
		{
			Console.Out.WriteLine("AprioriAll algorithm implementation in .NET\n");

			CustomerList customerList = null;
			double support = -1;

			if (args.Count() == 2)
			{
				try
				{
					support = double.Parse(args[1]);
					if (support <= 0 || support > 1)
					{
						Console.WriteLine("Invalid support: should be between 0 and 1");
						return;
					}
					XmlReader reader = new XmlReader();
					customerList = reader.ReadFromXmlFile(args[0]);
				}
				catch (Exception)
				{
					throw new Exception("Invalid parameters");
				}
			}
			else
			{
				Console.Out.WriteLine("Usage: aprioriall <filename> <support>");
				Console.Out.WriteLine("  <filename> : path to XML file containing database of customers");
				Console.Out.WriteLine("  <support> : real number greater than 0 and less or equal 1");
				return;
			}

			Console.Out.WriteLine("Input:");
			foreach (Customer c in customerList.Customers)
				Console.Out.WriteLine(" - {0}", c);

			Console.Out.WriteLine("\nComputation:");
			AprioriAll aprioriAll = new AprioriAll(customerList);
			List<Customer> aprioriAllResult = aprioriAll.RunAprioriAll(support, true);

			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in aprioriAllResult)
				Console.Out.WriteLine(" - {0}", c);

			Console.Write("\nThe end.");
			Console.ReadKey();
		}

	}
}
