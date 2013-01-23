using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// Application that is able to benchmark and compare the efficiency of both versions 
	/// of the algorithm on various inputs. It creates reports in CSV format.
	/// </summary>
	class Program_Benchmark
	{
		static public string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		static void Main(string[] args)
		{
			int testsCount = 100;
			bool newEachTime = false;
			bool openCL = false;
			bool serialized = false;
			bool warmUp = false;
			double support = 0.5;

			//int custCount = 2;

			CustomerList input = null;

			int custCountMin = 3;
			int custCountMax = 3;
			int transactCount = 3;
			int itemCount = 3;
			int uniqueIds = 10;

			#region parsing string[] args
			foreach (string arg in args)
			{
				if (arg == null || arg.Length < 6)
					continue;

				else if (arg.StartsWith("testsCount="))
					Int32.TryParse(arg.Substring(11), out testsCount);
				else if (arg.Equals("newEachTime"))
					newEachTime = true;
				else if (arg.Equals("openCL"))
					openCL = true;
				else if (arg.Equals("serialized"))
					serialized = true;
				else if (arg.Equals("warmUp"))
					warmUp = true;
				else if (arg.StartsWith("support="))
					Double.TryParse(arg.Substring(8), out support);

				else if (arg.StartsWith("input="))
				{
					InputData data = new InputData();

					string inputName = arg.Substring(6);

					Type type = typeof(InputData);
					FieldInfo field = type.GetField(inputName);
					object inputObj = field.GetValue(data);

					if (inputObj is CustomerList)
					{
						input = (CustomerList)inputObj;
					}
				}

				else if (arg.StartsWith("custCountMin="))
					Int32.TryParse(arg.Substring(13), out custCountMin);
				else if (arg.StartsWith("custCountMax="))
					Int32.TryParse(arg.Substring(13), out custCountMax);
				else if (arg.StartsWith("transactCount="))
					Int32.TryParse(arg.Substring(14), out transactCount);
				else if (arg.StartsWith("itemCount="))
					Int32.TryParse(arg.Substring(10), out itemCount);
				else if (arg.StartsWith("uniqueIds="))
					Int32.TryParse(arg.Substring(10), out uniqueIds);
			}
			#endregion

			int[] transactions = new int[transactCount];
			for (int i = 0; i < transactions.Length; ++i)
				transactions[i] = itemCount;
			int[][] customers = new int[custCountMax][];
			for (int i = 0; i < customers.Length; ++i)
				customers[i] = transactions;

			CustomerList maxInput = null;
			if (input == null)
				maxInput = InputGenerator.GenerateRandomList(customers, uniqueIds);
			else
			{
				custCountMin = custCountMax = input.Customers.Count;
				transactCount = input.Customers[0].Transactions.Count;
				itemCount = input.Customers[0].Transactions[0].Items.Count;
			}

			if (serialized)
			{
				if (input == null)
				{
					input = new CustomerList();
					for (int i = 0; i < custCountMin - 1; ++i)
						input.Customers.Add(maxInput.Customers[i]);
				}
				for (int custCount = custCountMin; custCount <= custCountMax; ++custCount)
				{
					if (maxInput != null)
						input.Customers.Add(maxInput.Customers[custCount - 1]);
					RunBenchmark(input, custCount, transactCount, itemCount,
						uniqueIds, support, newEachTime, false, warmUp, testsCount);
				}
			}

			if (openCL)
			{
				if (input == null)
				{
					input = new CustomerList();
					for (int i = 0; i < custCountMin - 1; ++i)
						input.Customers.Add(maxInput.Customers[i]);
				}
				for (int custCount = custCountMin; custCount <= custCountMax; ++custCount)
				{
					if (maxInput != null)
						input.Customers.Add(maxInput.Customers[custCount - 1]);
					RunBenchmark(input, custCount, transactCount, itemCount,
						uniqueIds, support, newEachTime, true, warmUp, testsCount);
				}
			}
		}

		private static void RunBenchmark(CustomerList input, int custCount, int transactCount, int itemCount,
			int uniqueIds, double support, bool newEachTime, bool openCL, bool warmUp, int testsCount)
		{
			DateTime dt = DateTime.Now;

			StringBuilder logFilepath = new StringBuilder();
			logFilepath.Append(AssemblyDirectory);
			logFilepath.Append("\\");
			logFilepath.AppendFormat("benchmark_run_{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
				dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

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
			logFilepath.AppendFormat("_{0:0}.csv", average);

			WriteAllBenchmarksLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
				dt, testsCount, average, average2);

			WriteNewBenchmarkLog(custCount, transactCount, itemCount, uniqueIds, support, newEachTime, openCL,
				dt, logFilepath.ToString(), testsCount, times, average, average2);

			//Console.ReadKey();
		}

		private static void WriteAllBenchmarksLog(int custCount, int transactCount, int itemCount,
			int uniqueIds, double support, bool newEachTime, bool openCL,
			DateTime dt, int testsCount, double average1, double average2)
		{
			string pathAll = new StringBuilder().Append(AssemblyDirectory).Append("\\benchmark_all.csv").ToString();

			if (!File.Exists(pathAll))
			{
				FileStream fsAll = null;
				StreamWriter fswAll = null;

				fsAll = File.Create(pathAll);
				fswAll = new StreamWriter(fsAll);

				fswAll.Write("date,time,testsCount,");
				fswAll.Write("customers,transactionsPerCustomer,itemsPerTransaction,possibleUniqueIds,");
				fswAll.Write("support,newEachTime,openCL,");
				fswAll.WriteLine("avg1,avg2");

				fswAll.Close();
				fsAll.Close();
			}

			StringBuilder s = new StringBuilder();

			s.AppendFormat("{2:00}/{1:00}/{0:0000},{3:00}:{4:00}:{5:00},",
				dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
			s.AppendFormat("{0},", testsCount);
			s.AppendFormat("{0},{1},{2},{3},", custCount, transactCount, itemCount, uniqueIds);
			s.AppendFormat("{0:0.000},{1},{2},", support, newEachTime, openCL);
			s.AppendFormat("{0:0.00},{1:0.00}", average1, average2);
			s.AppendLine();

			File.AppendAllText(pathAll, s.ToString());
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
