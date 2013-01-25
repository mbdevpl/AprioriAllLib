using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// Application that is able to benchmark and compare the efficiency of both versions 
	/// of the algorithm on various inputs. It creates reports in CSV format.
	/// </summary>
	class Benchmark : AprioriTestBase
	{
		static void Main(string[] args)
		{
			BenchmarkParameters parameters = new BenchmarkParameters(args);

			Benchmark b = new Benchmark();

			b.Initialize(parameters);

			b.RunAllTests();

			b.Close();

			//int[] transactions = new int[transactCount];
			//for (int i = 0; i < transactions.Length; ++i)
			//	transactions[i] = itemCount;
			//int[][] customers = new int[custCountMax][];
			//for (int i = 0; i < customers.Length; ++i)
			//	customers[i] = transactions;

			//CustomerList maxInput = null;
			//if (input == null)
			//	maxInput = InputGenerator.GenerateRandomList(customers, uniqueIds);
			//else
			//{
			//	custCountMin = custCountMax = input.Customers.Count;
			//	transactCount = input.Customers[0].Transactions.Count;
			//	itemCount = input.Customers[0].Transactions[0].Items.Count;
			//}

			//if (serialized)
			//{
			//	if (input == null)
			//	{
			//		input = new CustomerList();
			//		for (int i = 0; i < custCountMin - 1; ++i)
			//			input.Customers.Add(maxInput.Customers[i]);
			//	}
			//	for (int custCount = custCountMin; custCount <= custCountMax; ++custCount)
			//	{
			//		if (maxInput != null)
			//			input.Customers.Add(maxInput.Customers[custCount - 1]);
			//		RunBenchmark(input, custCount, transactCount, itemCount,
			//			uniqueIds, support, newEachTime, false, warmUp, testsCount);
			//	}
			//}

			//if (openCL)
			//{
			//	if (input == null)
			//	{
			//		input = new CustomerList();
			//		for (int i = 0; i < custCountMin - 1; ++i)
			//			input.Customers.Add(maxInput.Customers[i]);
			//	}
			//	for (int custCount = custCountMin; custCount <= custCountMax; ++custCount)
			//	{
			//		if (maxInput != null)
			//			input.Customers.Add(maxInput.Customers[custCount - 1]);
			//		RunBenchmark(input, custCount, transactCount, itemCount,
			//			uniqueIds, support, newEachTime, true, warmUp, testsCount);
			//	}
			//}
		}

		private BenchmarkParameters parameters;

		private List<Tuple<DateTime, CustomerList, double, double, double>> results;

		private DateTime dt;

		private string logFilepath;

		public Benchmark()
		{
			dt = DateTime.MinValue;
			results = new List<Tuple<DateTime, CustomerList, double, double, double>>();
		}

		private void Initialize(BenchmarkParameters parameters)
		{
			if (this.parameters != null)
				throw new AccessViolationException("cannot initialize if initialized already");

			this.parameters = parameters;
			dt = DateTime.Now;

			StringBuilder logFilepath = new StringBuilder();
			logFilepath.Append(AssemblyDirectory);
			logFilepath.Append("\\");

			this.logFilepath = logFilepath.ToString();

			//StringBuilder logFilepathAll = new StringBuilder();
			//logFilepathAll.Append(logFilepath.ToString());
		}

		private void RunAllTests()
		{
			if (parameters.Output && parameters.Customers.Count == 1)
			{
				PrintInput(parameters.Input);
				Console.Out.WriteLine();
			}

			if (parameters.OpenCL)
				foreach (int customers in parameters.Customers)
				{
					CustomerList input = new CustomerList();
					for (int i = 0; i < customers; ++i)
						input.Customers.Add(parameters.Input.Customers[i]);
					foreach (double support in parameters.Supports)
						RunOpenCLTest(input, support);
				}

			if (parameters.Serialized)
				foreach (int customers in parameters.Customers)
				{
					CustomerList input = new CustomerList();
					for (int i = 0; i < customers; ++i)
						input.Customers.Add(parameters.Input.Customers[i]);
					foreach (double support in parameters.Supports)
						RunSerializedTest(input, support);
				}
		}

		private void RunOpenCLTest(CustomerList input, double support)
		{
			Apriori apriori = null;
			if (!parameters.NewEachTime)
				apriori = new Apriori(input);

			if (parameters.WarmUp)
			{
				if (parameters.NewEachTime)
					apriori = new Apriori(input);
				apriori.RunParallelApriori(support);
				if (parameters.NewEachTime)
					apriori.Dispose();
			}

			List<double> times = new List<double>();
			Stopwatch watchAll = new Stopwatch();
			Stopwatch watch = new Stopwatch();

			for (int n = 1; n <= parameters.Repeats; ++n)
			{
				watch.Restart();
				watchAll.Start();

				if (parameters.NewEachTime)
					apriori = new Apriori(input);
				List<Litemset> litemsets = apriori.RunParallelApriori(support, true);
				if (parameters.NewEachTime)
					apriori.Dispose();

				watch.Stop();
				watchAll.Stop();

				times.Add(watch.ElapsedMilliseconds);
			}

			double average1 = times.Average();
			double average2 = ((double)watchAll.ElapsedMilliseconds) / parameters.Repeats;

			results.Add(new Tuple<DateTime, CustomerList, double, double, double>(dt, input, support, average1, average2));

			if (!parameters.NewEachTime)
				apriori.Dispose();
		}

		private void RunSerializedTest(CustomerList input, double support)
		{
			Apriori apriori = null;
			if (!parameters.NewEachTime)
				apriori = new Apriori(input);

			if (parameters.Output)
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
				apriori.RunParallelApriori(support);
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
				litemsets = apriori.RunApriori(support, true);
				//if (parameters.NewEachTime)
				//	apriori.Dispose();

				watch.Stop();
				watchAll.Stop();

				times.Add(watch.ElapsedMilliseconds);
			}

			double average1 = times.Average();
			double average2 = ((double)watchAll.ElapsedMilliseconds) / parameters.Repeats;

			if (parameters.Output)
			{
				Console.Out.WriteLine("mean time {0:0.00}ms", average1);
				PrintAprioriOutput(litemsets);
				Console.Out.WriteLine();
			}

			results.Add(new Tuple<DateTime, CustomerList, double, double, double>(dt, input, support, average1, average2));

			if (!parameters.NewEachTime)
				apriori.Dispose();
		}

		private void Close()
		{
			if (parameters == null)
				throw new AccessViolationException("cannot close if not initialized or closed already");

			StringBuilder s = new StringBuilder().Append(logFilepath).Append("benchmark_all.csv");
			string pathAll = s.ToString();

			if (!File.Exists(pathAll))
			{
				FileStream fsAll = null;
				StreamWriter fswAll = null;

				fsAll = File.Create(pathAll);
				fswAll = new StreamWriter(fsAll);

				fswAll.Write("date,time,");
				fswAll.Write("newEachTime,openCL,");
				fswAll.Write("customers,");
				fswAll.Write("support,");
				//fswAll.Write("transactionsPerCustomer,itemsPerTransaction,possibleUniqueIds,");
				fswAll.WriteLine("repeats,average1,average2");

				fswAll.Close();
				fsAll.Close();
			}

			foreach (Tuple<DateTime, CustomerList, double, double, double> result in results)
			{
				s = new StringBuilder();
				s.AppendFormat("{2:00}/{1:00}/{0:0000},{3:00}:{4:00}:{5:00},",
					dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
				s.AppendFormat("{0},{1},", parameters.NewEachTime, parameters.OpenCL);
				s.AppendFormat("{0},", result.Item2.Customers.Count);
				s.AppendFormat("{0:0.000},", result.Item3);
				//s.AppendFormat("{0},{1},{2},{3},", custCount, transactCount, itemCount, uniqueIds);
				s.AppendFormat("{0},", parameters.Repeats);
				s.AppendFormat("{0:0.00},{1:0.00}", result.Item4, result.Item5);
				s.AppendLine();

				File.AppendAllText(pathAll, s.ToString());

				//s = new StringBuilder().AppendFormat(logFilepath);
				//s.AppendFormat("benchmark_run_{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
				//	dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
				//s.AppendFormat("_{0:0}.csv", average1);
				//string path = s.ToString();
			}

			results.Clear();

			parameters = null;
			dt = DateTime.MinValue;
		}

		private static void RunBenchmark(CustomerList input, int custCount, int transactCount, int itemCount,
			int uniqueIds, double support, bool newEachTime, bool openCL, bool warmUp, int testsCount)
		{
			#region integrity checks

			if (input.Customers.Count != custCount)
				Console.Out.WriteLine("Error: customers count is wrong");
			if (input.Customers[0].Transactions.Count != transactCount)
				Console.Out.WriteLine("Error: transactions count is wrong");
			if (input.Customers[0].Transactions[0].Items.Count != itemCount)
				Console.Out.WriteLine("Error: items count is wrong");

			//Console.Out.WriteLine("Error: ");

			#endregion

			Stopwatch watchAll = new Stopwatch();
			Stopwatch watch = new Stopwatch();

			List<double> times = new List<double>();
			Apriori apriori = null;
			if (!newEachTime)
				apriori = new Apriori(input);
			int count = 0;
			if (warmUp)
				count = -1;
			while (++count <= testsCount)
			{
				if (count > 0)
					watchAll.Start();
				watch.Restart();

				if (newEachTime)
					apriori = new Apriori(input);

				List<Litemset> results = null;
				if (openCL)
					results = apriori.RunParallelApriori(support); //, true);
				else
					results = apriori.RunApriori(support);

				if (newEachTime && openCL)
					apriori.Dispose();

				watch.Stop();
				watchAll.Stop();

				Console.Out.WriteLine("{0}. {1} in {2} ms", count, results.Count, watch.ElapsedMilliseconds);
				if (count > 0)
					times.Add(watch.ElapsedMilliseconds);
			}

			if (!newEachTime && openCL)
				apriori.Dispose();

			double average = times.Average();
			double average2 = ((double)watchAll.ElapsedMilliseconds) / testsCount;

			Console.Out.WriteLine("redundant mean times{0} {1:0.00}ms and {2:0.00}ms",
				warmUp ? " (without 0th test)" : "", average, average2);
			//logFilepath.AppendFormat("_{0:0}.csv", average);

			//WriteAllBenchmarksLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
			//	dt, testsCount, average, average2);

			//WriteNewBenchmarkLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
			//	dt, logFilepath.ToString(), testsCount, times, average, average2);

			//Console.ReadKey();
		}


		private static void WriteAllBenchmarksLog(int custCount, int transactCount, int itemCount,
			int uniqueIds, double support, bool newEachTime, bool openCL,
			DateTime dt, int testsCount, double average1, double average2)
		{

		}

		private static void WriteNewBenchmarkLog(int custCount, int transactCount, int itemCount,
			int uniqueIds, double support, bool newEachTime, bool openCL,
			DateTime dt, string logFilepath, int testsCount, List<double> times, double average1, double average2)
		{
			FileStream fs = File.Create(logFilepath);
			StreamWriter fsw = new StreamWriter(fs);

			fsw.WriteLine("date,{2:00}/{1:00}/{0:0000}", dt.Year, dt.Month, dt.Day);
			fsw.WriteLine("time,{0:00}:{1:00}:{2:00}", dt.Hour, dt.Minute, dt.Second);
			fsw.WriteLine("testsCount,{0}", testsCount);
			fsw.WriteLine("customers,{0}", custCount);
			fsw.WriteLine("transactionsPerCustomer,{0}", transactCount);
			fsw.WriteLine("itemsPerTransaction,{0}", itemCount);
			fsw.WriteLine("possibleUniqueIds,{0}", uniqueIds);
			fsw.WriteLine("support,{0:0.000}", support);
			fsw.WriteLine("newEachTime,{0}", newEachTime);
			fsw.WriteLine("openCL,{0}", openCL);
			fsw.WriteLine("avg1,{0:0.00}", average1);
			fsw.WriteLine("avg2,{0:0.00}", average2);
			for (int i = 0; i < times.Count; ++i)
				fsw.WriteLine("{0},{1}", i + 1, times[i]);

			fsw.Close();
			fs.Close();
		}

	}
}
