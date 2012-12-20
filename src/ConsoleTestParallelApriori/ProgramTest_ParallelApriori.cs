using System;
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

			//Console.Out.WriteLine("\nOpenCL platforms with devices:");
			//foreach (String s in OpenCLChecker.AvailablePlatformsWithDevices())
			//	Console.Out.WriteLine("{0}", s);

			CustomerList input
				//= InputGenerator.GenerateRandomList(6, 2, 2);
				= Data.DataSet1;
			double support = 0.1;

			program.PrintInput(input);

			Console.Out.WriteLine("\nComputation:");
			Apriori apriori = new Apriori(input);
			List<Litemset> litemsets = apriori.RunParallelApriori(support, true);

			program.PrintAprioriOutput(litemsets);

			Console.Out.Write("End.");
			Console.ReadKey();
		}

	}
}
