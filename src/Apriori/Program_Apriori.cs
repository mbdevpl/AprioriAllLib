﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// A console application that runs Apriori on input given in command line parameters.
	/// </summary>
	class Program_Apriori
	{
		/// <summary>
		/// Main function.
		/// </summary>
		/// <param name="args">exactly 2 command line arguments are needed by the program:
		/// 1. <code>path</code> - path to the XML file with customers database
		/// 2. <code>support</code> - minimum support, value from range (0,1]</param>
		static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

			Console.Out.WriteLine("Apriori algorithm implementation in .NET\n");

			List<ICustomer> customerList = null;
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
				Console.Out.WriteLine("Usage: apriori <filename> <support>");
				Console.Out.WriteLine("  <filename> : path to XML file containing database of customers");
				Console.Out.WriteLine("  <support> : real number greater than 0 and less or equal 1");
				return;
			}

			Console.Out.WriteLine("Input:");
			foreach (ICustomer c in customerList)
				Console.Out.WriteLine(" - {0}", c);

			Console.Out.WriteLine("\nComputation:");
			Apriori apriori = new Apriori(customerList);
			var aprioriAllResult = apriori.RunApriori(support, true);

			Console.Out.WriteLine("\nResults:");
			foreach (ILitemset l in aprioriAllResult)
				Console.Out.WriteLine(" - {0}", l);

			Console.Write("\nThe end.");
			Console.ReadKey();
		}
	}
}
