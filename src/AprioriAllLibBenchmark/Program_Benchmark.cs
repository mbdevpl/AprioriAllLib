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
			//int i = 0;
			//foreach (string arg in args)
			//{
			//	if (Int32.TryParse(arg, out ints[i]))
			//		++i;
			//	if (i == 4)
			//		break;
			//}
			//InputData inputData = new InputData();
			int testCount = 100;
			bool newEachTime = true;
			bool openCl = true;
			double support = 0.5;
			int uniqueIds = 20;
			int[] cust = new int[] { 5, 5, 5, 5, 5 };
			int[] cust2 = new int[] { 4, 4, 4, 4 };
			int[] cust3 = new int[] { 3, 3, 3 };
			int[][] sizes = new int[2][];
			for (int i = 0; i < sizes.Length; ++i)
			{
				//sizes[i] = new int[5];
				//for (int j = 0; ; ++j)
				//{
				//	sizes[i][j] = 5;
				//}
				sizes[i] = cust3;
			}

			DateTime dt = DateTime.Now;

			StringBuilder logFilepath = new StringBuilder();

			logFilepath.Append(AssemblyDirectory);
			logFilepath.Append("\\");
			logFilepath.AppendFormat("benchmark_run_{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
				dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

			dt.ToString();
			var input = InputGenerator.GenerateRandomList(sizes, uniqueIds);

			input = new CustomerList();
			input.Customers.Add(new Customer(new Transaction(1)));

			Stopwatch watch = new Stopwatch();
			List<double> times = new List<double>();
			Apriori apriori = null;
			if (!newEachTime)
				apriori = new Apriori(input);
			int count = -1;
			while (++count <= testCount)
			{
				watch.Restart();

				if (newEachTime)
					apriori = new Apriori(input);

				List<Litemset> results = null;
				if (openCl)
					results = apriori.RunParallelApriori(support); //, true);
				else
					results = apriori.RunApriori(support);

				if (newEachTime && openCl)
					apriori.Dispose();

				watch.Stop();

				Console.Out.WriteLine("{0}. {1} in {2} ms", count, results.Count, watch.ElapsedMilliseconds);
				if (count > 0)
					times.Add(watch.ElapsedMilliseconds);
			}

			if (!newEachTime && openCl)
				apriori.Dispose();

			double average = times.Average();
			Console.Out.WriteLine("mean time (without 0th test) {0}ms", average);
			logFilepath.AppendFormat("_{0:0}.csv", average);

			#region benchmark_all

			string pathAll = new StringBuilder().Append(AssemblyDirectory).Append("\\").Append("benchmark_all.csv").ToString();

			FileStream fsAll = null;
			StreamWriter fswAll = null;
			if (!File.Exists(pathAll))
			{
				fsAll = File.Create(pathAll);
				fswAll = new StreamWriter(fsAll);

				fswAll.WriteLine("date,time,customers,transactionsPerCustomer,itemsPerTransaction,possibleUniqueIds,avg");
			}
			else
			{
				fsAll = File.OpenWrite(pathAll);
				fswAll = new StreamWriter(fsAll);
			}

			int custCount = input.Customers.Count;
			int tranactCount = input.Customers[0].Transactions.Count;
			int itemCount = input.Customers[0].Transactions[0].Items.Count;

			fswAll.Write("{2:00}/{1:00}/{0:0000},{3:00}:{4:00}:{5:00},",
				dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

			fswAll.WriteLine("{0},{1},{2},{3},{4:0.00}", custCount, tranactCount, itemCount, uniqueIds, average);

			fswAll.Close();
			fsAll.Close();

			#endregion

			string path = logFilepath.ToString();
			FileStream fs = File.Create(path);
			//File.OpenWrite(path);
			StreamWriter fsw = new StreamWriter(fs);
			//BufferedStream bfs = new BufferedStream(fs);

			fswAll.WriteLine("date,{2:00}/{1:00}/{0:0000}", dt.Year, dt.Month, dt.Day);
			fswAll.WriteLine("time,{0:00}:{1:00}:{2:00}", dt.Hour, dt.Minute, dt.Second);
			fsw.WriteLine("customers,{0}", custCount);
			fsw.WriteLine("transactionsPerCustomer,{0}", tranactCount);
			fsw.WriteLine("itemsPerTransaction,{0}", itemCount);
			fsw.WriteLine("possibleUniqueIds,{0}", uniqueIds);
			fsw.WriteLine("avg,{0:0.00}", average);
			for (int i = 0; i < times.Count; ++i)
				fsw.WriteLine("{0},{1}", i + 1, times[i]);

			fsw.Close();
			fs.Close();



			//Console.ReadKey();
		}
	}
}
