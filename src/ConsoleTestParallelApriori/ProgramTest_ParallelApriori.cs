﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// An example program that runs the parallel version of the Apriori algorithm.
	/// </summary>
	public class ProgramTest_ParallelApriori : AprioriTestBase
	{
		/// <summary>
		/// Main function.
		/// </summary>
		/// <param name="args">not used</param>
		private static void Main(string[] args)
		{
			var program = new ProgramTest_ParallelApriori();

			Console.Out.WriteLine("Parallel Apriori algorithm test");

			CustomerList input
				//= InputGenerator.GenerateRandomList(6, 2, 2);
				= Data.Example1;
			double support = 0.1;

			program.PrintInput(input);

			Console.Out.WriteLine("\nComputation:");
			Apriori apriori = new Apriori(input);
			List<Litemset> litemsets = apriori.RunParallelApriori(support, true);
			apriori.Dispose();

			program.PrintAprioriOutput(litemsets);

			Console.Out.Write("End.");
			Console.ReadKey();
		}

	}
}
