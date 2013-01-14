using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AprioriAllLib;
using AprioriAllLib.Test;

namespace ConsoleTestComparisonApriori
{
	class ProgramTest_ComparisonApriori : AprioriTestBase
	{
		static void Main(string[] args)
		{
			var program = new ProgramTest_ComparisonApriori();

			Console.Out.WriteLine("Apriori Serialized and Parallel versions comparison");

			CustomerList input
				= InputGenerator.GenerateRandomList(60, 1, 12);

			program.PrintInput(input);
			{
				double support = 0.5;

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori2 = new Apriori(input);
				List<Litemset> litemsets2 = apriori2.RunApriori(support, true);

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori = new Apriori(input);
				List<Litemset> litemsets = apriori.RunParallelApriori(support, true);
				apriori.Dispose();
			}

			{
				double support = 0.3;

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori2 = new Apriori(input);
				List<Litemset> litemsets2 = apriori2.RunApriori(support, true);

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori = new Apriori(input);
				List<Litemset> litemsets = apriori.RunParallelApriori(support, true);
				apriori.Dispose();
			}

			{
				double support = 0.1;

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori2 = new Apriori(input);
				List<Litemset> litemsets2 = apriori2.RunApriori(support, true);

				Console.Out.WriteLine("\nComputation:");
				Apriori apriori = new Apriori(input);
				List<Litemset> litemsets = apriori.RunParallelApriori(support, true);
				apriori.Dispose();
			}

			Console.Out.Write("End.");
			Console.ReadKey();
		}
	}
}
