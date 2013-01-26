using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{

	public abstract class AprioriBenchmark : AprioriTestBase
	{
		protected BenchmarkParameters parameters;

		protected List<AprioriBenchmarkLogEntry> results;

		protected DateTime dt;

		protected string logFilepath;

		public AprioriBenchmark()
		{
			dt = DateTime.MinValue;
			results = new List<AprioriBenchmarkLogEntry>();
		}

		public void Initialize(BenchmarkParameters parameters)
		{
			if (this.parameters != null)
				throw new AccessViolationException("cannot initialize if initialized already");

			this.parameters = parameters;
			dt = DateTime.Now;

			StringBuilder logFilepath = new StringBuilder();
			logFilepath.Append(AssemblyDirectory);
			logFilepath.Append("\\");

			this.logFilepath = logFilepath.ToString();
		}

		public abstract void RunAllTests();

		protected abstract void RunOneTest(CustomerList input, double support);

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

				fswAll.Write("date,time,");
				fswAll.Write("newEachTime,openCL,");
				fswAll.Write("customers,");
				fswAll.Write("support,");
				//fswAll.Write("transactionsPerCustomer,itemsPerTransaction,possibleUniqueIds,");
				fswAll.WriteLine("repeats,average1,average2");

				fswAll.Close();
				fsAll.Close();
			}

			foreach (AprioriBenchmarkLogEntry result in results)
			{
				s = new StringBuilder();
				s.AppendFormat("{2:00}/{1:00}/{0:0000},{3:00}:{4:00}:{5:00},",
					dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
				s.AppendFormat("{0},{1},", parameters.NewEachTime, parameters.OpenCL);
				s.AppendFormat("{0},", result.Input.Customers.Count);
				s.AppendFormat("{0:0.000},", result.Support);
				//s.AppendFormat("{0},{1},{2},{3},", custCount, transactCount, itemCount, uniqueIds);
				s.AppendFormat("{0},", parameters.Repeats);
				s.AppendFormat("{0:0.00},{1:0.00}", result.Average1, result.Average2);
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
