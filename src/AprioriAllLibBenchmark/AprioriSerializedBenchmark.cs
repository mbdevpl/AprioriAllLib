using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{

	public class AprioriSerializedBenchmark : AprioriBenchmark
	{

		public AprioriSerializedBenchmark()
			: base()
		{
		}

		public override void RunAllTests()
		{
			if (!parameters.Apriori || !parameters.Serialized)
				return;

			if (parameters.PrintInput && parameters.Customers.Count == 1)
			{
				PrintInput(parameters.Input);
				Console.Out.WriteLine();
			}

			foreach (int customers in parameters.Customers)
			{
				CustomerList input = new CustomerList();
				for (int i = 0; i < customers; ++i)
					input.Customers.Add(parameters.Input.Customers[i]);
				foreach (double support in parameters.Supports)
					RunOneTest(input, support);
			}
		}

		protected override void RunOneTest(CustomerList input, double support)
		{
			Apriori apriori = null;
			if (!parameters.NewEachTime)
				apriori = new Apriori(input);

			if (parameters.PrintInput)
			{
				if (parameters.Customers.Count > 1)
				{
					PrintInput(parameters.Input);
					Console.Out.WriteLine();
				}
				Console.Out.WriteLine("Starting benchmark, support={0:0.000}", support);
			}

			if (parameters.WarmUp)
			{
				if (parameters.NewEachTime)
					apriori = new Apriori(input);
				apriori.RunApriori(support);
				//if (parameters.NewEachTime)
				//	apriori.Dispose();
			}

			List<double> times = new List<double>();
			Stopwatch watchAll = new Stopwatch();
			Stopwatch watch = new Stopwatch();

			List<Litemset> litemsets = null;
			for (int n = 1; n <= parameters.Repeats; ++n)
			{
				watch.Restart();
				watchAll.Start();

				if (parameters.NewEachTime)
					apriori = new Apriori(input);
				litemsets = apriori.RunApriori(support, parameters.PrintProgress);
				//if (parameters.NewEachTime)
				//	apriori.Dispose();

				watch.Stop();
				watchAll.Stop();

				times.Add(watch.ElapsedMilliseconds);
			}

			double average1 = times.Average();
			double average2 = ((double)watchAll.ElapsedMilliseconds) / parameters.Repeats;

			if (parameters.PrintOutput)
			{
				Console.Out.WriteLine("mean time {0:0.00}ms", average1);
				PrintAprioriOutput(litemsets);
				Console.Out.WriteLine();
			}

			results.Add(new AprioriBenchmarkLogEntry(dt, input, support, average1, average2));

			//if (!parameters.NewEachTime)
			//	apriori.Dispose();
		}

	}

}
