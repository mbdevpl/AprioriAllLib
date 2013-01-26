using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	public class AprioriAllSerializedBenchmark : AprioriAllBenchmark
	{
		public AprioriAllSerializedBenchmark()
			: base()
		{
		}

		public override void RunAllTests()
		{
			if (!parameters.AprioriAll || !parameters.Serialized)
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
			AprioriAll aprioriAll = null;
			if (!parameters.NewEachTime)
				aprioriAll = new AprioriAll(input);

			if (parameters.PrintInput)
			{
				if (parameters.Customers.Count > 1)
				{
					PrintInput(input);
					Console.Out.WriteLine();
				}
				Console.Out.WriteLine("Starting benchmark, support={0:0.000}", support);
			}

			if (parameters.WarmUp)
			{
				if (parameters.NewEachTime)
					aprioriAll = new AprioriAll(input);
				aprioriAll.RunAprioriAll(support);
				//if (parameters.NewEachTime)
				//	apriori.Dispose();
			}

			List<double> times = new List<double>();
			Stopwatch watchAll = new Stopwatch();
			Stopwatch watch = new Stopwatch();

			List<Customer> frequentCustomers = null;
			for (int n = 1; n <= parameters.Repeats; ++n)
			{
				watch.Restart();
				watchAll.Start();

				if (parameters.NewEachTime)
					aprioriAll = new AprioriAll(input);
				frequentCustomers = aprioriAll.RunAprioriAll(support, parameters.PrintProgress);
				//if (parameters.NewEachTime)
				//	apriori.Dispose();

				watch.Stop();
				watchAll.Stop();

				times.Add(watch.ElapsedMilliseconds);
			}

			double average1 = times.Average();
			double average2 = ((double)watchAll.ElapsedMilliseconds) / parameters.Repeats;

			if (parameters.PrintProgress)
				Console.Out.WriteLine("mean time {0:0.00}ms", average1);

			if (parameters.PrintOutput)
			{
				PrintAprioriAllOutput(frequentCustomers);
				Console.Out.WriteLine();
			}

			results.Add(new AprioriBenchmarkLogEntry(dt, "AprioriAll", false, input, support, 
				(uint)parameters.Repeats, parameters.NewEachTime, average1, average2));

			//if (!parameters.NewEachTime)
			//	aprioriAll.Dispose();
		}
	}
}
