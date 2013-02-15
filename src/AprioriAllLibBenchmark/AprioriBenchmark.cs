using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{

	public abstract class AprioriBenchmark : AprioriAllLibTestBase
	{
		protected BenchmarkParameters parameters;

		protected List<AprioriBenchmarkLogEntry> results;

		protected DateTime dt;

		protected string logFilepath;

		public AprioriBenchmark()
		{
			dt = DateTime.MinValue;
			results = new List<AprioriBenchmarkLogEntry>();

			StringBuilder logFilepath = new StringBuilder();
			logFilepath.Append(AssemblyDirectory);
			logFilepath.Append("\\");

			this.logFilepath = logFilepath.ToString();
		}

		public void Initialize(BenchmarkParameters parameters)
		{
			if (this.parameters != null)
				throw new AccessViolationException("cannot initialize if initialized already");

			this.parameters = parameters;
			dt = DateTime.Now;
		}

		public void RunAllTests()
		{
			if (!BeginAllTests())
				return;

			if (parameters.PrintInput && parameters.Customers.Count == 1)
			{
				PrintInput(parameters.Input);
				Console.Out.WriteLine();
			}

			foreach (int customers in parameters.Customers)
			{
				List<ICustomer> input = new List<ICustomer>();
				for (int i = 0; i < customers; ++i)
					input.Add(parameters.Input[i]);
				foreach (double support in parameters.Supports)
				{
					RunOneTest(input, support);
				}
			}

			//if (!EndAllTests())
			//	return;
		}

		/// <summary>
		/// Should return false if and only if benchmark parameters disallow the tests to continue.
		/// </summary>
		/// <returns>true if it is ok to start the benchmark
		/// with current parameters</returns>
		protected abstract bool BeginAllTests();

		//protected abstract bool EndAllTests();

		protected void RunOneTest(IEnumerable<ICustomer> input, double support)
		{
			Apriori apriori = null;
			if (!parameters.NewEachTime)
				apriori = ConstructTestedInstance(input);

			if (parameters.PrintInput)
			{
				if (parameters.Customers.Count > 1)
				{
					PrintInput(input);
					Console.Out.WriteLine();
				}
			}

			if (parameters.PrintProgress)
			{
				Console.Out.Write("Starting benchmark, ");
				if (parameters.Customers.Count > 1)
					Console.Out.Write("{0} customers, ", input.Count());
				Console.Out.WriteLine(" support={0:0.000}", support);
			}

			if (parameters.WarmUp)
			{
				if (parameters.NewEachTime)
					apriori = ConstructTestedInstance(input);
				RunTestedInstance(apriori, support, false);
				if (parameters.NewEachTime)
					DestroyTestedInstance(apriori);
			}

			List<double> times = new List<double>();
			Stopwatch watchAll = new Stopwatch();
			Stopwatch watch = new Stopwatch();

			IEnumerable<object> aprioriResults = null;
			for (int n = 1; n <= parameters.Repeats; ++n)
			{
				watch.Restart();
				watchAll.Start();

				if (parameters.NewEachTime)
					apriori = new Apriori(input);
				aprioriResults = RunTestedInstance(apriori, support, parameters.PrintProgress);
				if (parameters.NewEachTime)
					apriori.Dispose();

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
				if (aprioriResults is IEnumerable<ICustomer>)
					PrintAprioriAllOutput(aprioriResults.Cast<ICustomer>());
				else
					PrintAprioriOutput(aprioriResults.Cast<ILitemset>());
				Console.Out.WriteLine();
			}

			results.Add(new AprioriBenchmarkLogEntry(dt, "Apriori", true, input, support,
				(uint)parameters.Repeats, parameters.NewEachTime, average1, average2));

			if (!parameters.NewEachTime)
				DestroyTestedInstance(apriori);
		}

		protected abstract Apriori ConstructTestedInstance(IEnumerable<ICustomer> input);

		protected abstract IEnumerable<object> RunTestedInstance(Apriori apriori,
			double support, bool progressOutput);

		protected abstract void DestroyTestedInstance(Apriori apriori);

		public void Close()
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

				fswAll.WriteLine(AprioriBenchmarkLogEntry.CsvHeader);

				fswAll.Close();
				fsAll.Close();
			}

			if (parameters.SaveInput && parameters.Customers.Count == 1)
			{
				s = new StringBuilder();
				s.Append(logFilepath).Append("benchmark_saved_input.txt");

				File.AppendAllText(s.ToString(), ToIntArrayInitializer(parameters.Input));
			}

			if (parameters.SaveLatex)
			{
				string pathLtx = new StringBuilder().Append(AssemblyDirectory)
					.AppendFormat("\\benchmark_run_{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
						dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second).Append(".txt").ToString();

				s = new StringBuilder();
				foreach (AprioriBenchmarkLogEntry result in results)
				{
					s.Append(result.ToLatexPgfplotsString()).AppendLine();
				}
				File.AppendAllText(pathLtx, s.ToString());
			}

			foreach (AprioriBenchmarkLogEntry result in results)
			{
				s = new StringBuilder();
				s.Append(result.ToCsvString());
				s.AppendLine();

				File.AppendAllText(pathAll, s.ToString());

				//s = new StringBuilder().AppendFormat(logFilepath);
				//s
				//s.AppendFormat("_{0:0}.csv", average1);
				//string path = s.ToString();
			}

			results.Clear();

			parameters = null;
			dt = DateTime.MinValue;
		}

		//private static void RunBenchmark(CustomerList input, int custCount, int transactCount, int itemCount,
		//	int uniqueIds, double support, bool newEachTime, bool openCL, bool warmUp, int testsCount)
		//{
		//	#region integrity checks

		//	if (input.Customers.Count != custCount)
		//		Console.Out.WriteLine("Error: customers count is wrong");
		//	if (input.Customers[0].Transactions.Count != transactCount)
		//		Console.Out.WriteLine("Error: transactions count is wrong");
		//	if (input.Customers[0].Transactions[0].Items.Count != itemCount)
		//		Console.Out.WriteLine("Error: items count is wrong");

		//	//Console.Out.WriteLine("Error: ");

		//	#endregion

		//	Stopwatch watchAll = new Stopwatch();
		//	Stopwatch watch = new Stopwatch();

		//	List<double> times = new List<double>();
		//	Apriori apriori = null;
		//	if (!newEachTime)
		//		apriori = new Apriori(input);
		//	int count = 0;
		//	if (warmUp)
		//		count = -1;
		//	while (++count <= testsCount)
		//	{
		//		if (count > 0)
		//			watchAll.Start();
		//		watch.Restart();

		//		if (newEachTime)
		//			apriori = new Apriori(input);

		//		List<Litemset> results = null;
		//		if (openCL)
		//			results = apriori.RunParallelApriori(support); //, true);
		//		else
		//			results = apriori.RunApriori(support);

		//		if (newEachTime && openCL)
		//			apriori.Dispose();

		//		watch.Stop();
		//		watchAll.Stop();

		//		Console.Out.WriteLine("{0}. {1} in {2} ms", count, results.Count, watch.ElapsedMilliseconds);
		//		if (count > 0)
		//			times.Add(watch.ElapsedMilliseconds);
		//	}

		//	if (!newEachTime && openCL)
		//		apriori.Dispose();

		//	double average = times.Average();
		//	double average2 = ((double)watchAll.ElapsedMilliseconds) / testsCount;

		//	Console.Out.WriteLine("redundant mean times{0} {1:0.00}ms and {2:0.00}ms",
		//		warmUp ? " (without 0th test)" : "", average, average2);
		//	//logFilepath.AppendFormat("_{0:0}.csv", average);

		//	//WriteAllBenchmarksLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
		//	//	dt, testsCount, average, average2);

		//	//WriteNewBenchmarkLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
		//	//	dt, logFilepath.ToString(), testsCount, times, average, average2);

		//	//Console.ReadKey();
		//}


		//private static void WriteAllBenchmarksLog(int custCount, int transactCount, int itemCount,
		//	int uniqueIds, double support, bool newEachTime, bool openCL,
		//	DateTime dt, int testsCount, double average1, double average2)
		//{

		//}

		//private static void WriteNewBenchmarkLog(int custCount, int transactCount, int itemCount,
		//	int uniqueIds, double support, bool newEachTime, bool openCL,
		//	DateTime dt, string logFilepath, int testsCount, List<double> times, double average1, double average2)
		//{
		//	FileStream fs = File.Create(logFilepath);
		//	StreamWriter fsw = new StreamWriter(fs);

		//	fsw.WriteLine("date,{2:00}/{1:00}/{0:0000}", dt.Year, dt.Month, dt.Day);
		//	fsw.WriteLine("time,{0:00}:{1:00}:{2:00}", dt.Hour, dt.Minute, dt.Second);
		//	fsw.WriteLine("testsCount,{0}", testsCount);
		//	fsw.WriteLine("customers,{0}", custCount);
		//	fsw.WriteLine("transactionsPerCustomer,{0}", transactCount);
		//	fsw.WriteLine("itemsPerTransaction,{0}", itemCount);
		//	fsw.WriteLine("possibleUniqueIds,{0}", uniqueIds);
		//	fsw.WriteLine("support,{0:0.000}", support);
		//	fsw.WriteLine("newEachTime,{0}", newEachTime);
		//	fsw.WriteLine("openCL,{0}", openCL);
		//	fsw.WriteLine("avg1,{0:0.00}", average1);
		//	fsw.WriteLine("avg2,{0:0.00}", average2);
		//	for (int i = 0; i < times.Count; ++i)
		//		fsw.WriteLine("{0},{1}", i + 1, times[i]);

		//	fsw.Close();
		//	fs.Close();
		//}

	}

}
