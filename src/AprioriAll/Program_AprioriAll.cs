using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//! Applications (either for general use or just testing) that use console.
namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// Defines a console application that runs AprioriAll on input given in command line parameters.
	/// 
	/// This is modified version of application originally by Karolina Baltyn.
	/// </summary>
	public class Program_AprioriAll
	{
		/// <summary>
		/// Main function.
		/// </summary>
		/// <param name="args">exactly 2 command line arguments are needed by the program:
		/// 1. <code>path</code> - path to the XML file with customers database
		/// 2. <code>support</code> - minimum support, value from range (0,1]</param>
		private static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

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
					customerList = XmlReader.ReadFromXmlFile(args[0]);
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
