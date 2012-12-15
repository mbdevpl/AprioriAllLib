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
	public class ProgramTest_AprioriParallel : AprioriTestBase
	{

		private static void Main(string[] args)
		{
			TestBaseInitialize();

			Console.Out.WriteLine("Parallel Apriori algorithm test");

			Console.Out.WriteLine("OpenCL plaforms:");
			foreach (String s in OpenCLChecker.AvailablePlatfroms())
				Console.Out.WriteLine("{0}", s);

			Console.Out.WriteLine("\nOpenCL platforms with devices:");
			foreach (String s in OpenCLChecker.AvailablePlatformsWithDevices())
				Console.Out.WriteLine("{0}", s);

			CustomerList randomExample = InputGenerator.GenerateRandomList(4, 2, 2);

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in randomExample.Customers)
				Console.Out.WriteLine(" - {0}", c);

			Console.Out.WriteLine("\nComputation:");
			Apriori apriori = new Apriori(randomExample);
			List<Litemset> litemsets = apriori.RunParallelApriori(0.001, true);
			//AprioriAll all = new AprioriAll(randomExample);
			//List<Customer> results = all.RunParallelApriori(0.5, true);

			Console.Out.WriteLine("\nResults:");
			foreach (Litemset l in litemsets)
				Console.Out.WriteLine(" - {0}", l);
			//foreach (Customer c in results)
			//	Console.Out.WriteLine(" - {0}", c);

			Console.Out.Write("Fin.");
			Console.ReadKey();
		}

	}
}
