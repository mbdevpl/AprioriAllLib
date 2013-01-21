using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib.Test.InConsole
{
	public class ProgramTest_ComparisonApriori : AprioriTestBase
	{
		private static bool Diagnostics = true;

		private static void Main(string[] args)
		{
			var program = new ProgramTest_ComparisonApriori();

			Console.Out.WriteLine("Apriori Serialized and Parallel versions comparison");

			CustomerList input =
				//Data.Example1;
				Data.Example2;
				//InputGenerator.GenerateRandomList(500000, 1, 1, 1);
				//InputGenerator.GenerateRandomList(1, 1, 15, 40);
				//InputGenerator.GenerateRandomList(20, 15, 15, 40);

			//new CustomerList();
			//input.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 20, 10 }));
			//input.Customers.Add(new Customer(new int[] { 20, 10 }, new int[] { 10, 20 }));
			//input.Customers.Add(new Customer(new int[] { 30 }, new int[] { 20 }));

			double[] supports =
				//new double[] { 0.001, 0.1, 0.3 };
				//new double[] { 0.9, 0.8, 0.7, 0.5, 0.3, 0.2, 0.1, 0.001 };
				new double[] { 0.3, 0.2, 0.1 };
				//new double[] { 0.5, 0.3 };
				//new double[] { 0.1 };
			string[] editions =
				//new string[] { "Parallel", "Serialized", "ParallelNewEachTime", "SerializedNewEachTime" };
				new string[] { "Parallel" };
				//new string[] { "Parallel", "Serialized" };

			program.PrintInput(input);

			Apriori apriori1 = new Apriori(input);
			Apriori apriori2 = null;
			Apriori apriori3 = new Apriori(input);
			Apriori apriori4 = null;

			List<Tuple<string, double, long>> results = new List<Tuple<string, double, long>>();
			foreach (string edition in editions)
				foreach (double support in supports)
				{
					if (Diagnostics)
						Console.Out.WriteLine("\n{0}, {1}:", edition, support);
					List<Litemset> litemsets = null;
					Stopwatch timer = new Stopwatch();
					timer.Start();
					if (edition.Equals("Serialized"))
					{
						litemsets = apriori1.RunApriori(support, Diagnostics);
					}
					if (edition.Equals("SerializedNewEachTime"))
					{
						apriori2 = new Apriori(input);
						litemsets = apriori2.RunApriori(support, Diagnostics);
					}
					else if (edition.Equals("Parallel"))
					{
						litemsets = apriori3.RunParallelApriori(support, Diagnostics);
					}
					else if (edition.Equals("ParallelNewEachTime"))
					{
						apriori4 = new Apriori(input);
						litemsets = apriori4.RunParallelApriori(support, Diagnostics);
						apriori4.Dispose();
					}
					timer.Stop();
					var result = new Tuple<string, double, long>(edition, support, timer.ElapsedMilliseconds);
					results.Add(result);
					if (!Diagnostics)
						Console.Out.WriteLine("{0}, {1}: {2}ms", result.Item1, result.Item2, result.Item3);
				}

			Stopwatch timer2 = new Stopwatch();
			if (editions.Contains("Parallel"))
			{
				timer2.Start();
				apriori3.Dispose();
				timer2.Stop();
				if (!Diagnostics)
					Console.Out.WriteLine("Parallel, disposing: {0} ms", timer2.ElapsedMilliseconds);
			}

			if (Diagnostics)
			{
				Console.Out.WriteLine();
				foreach (Tuple<string, double, long> result in results)
					Console.Out.WriteLine("{0}, {1}: {2}ms", result.Item1, result.Item2, result.Item3);
				Console.Out.WriteLine("Parallel, disposing: {0} ms", timer2.ElapsedMilliseconds);
			}

			Console.Out.Write("\nEnd.");
			Console.ReadKey();
		}
	}
}
