using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{

	public abstract class AprioriAllBenchmark : AprioriAllTestBase
	{
		
		protected BenchmarkParameters parameters;

		protected List<AprioriBenchmarkLogEntry> results;

		protected DateTime dt;

		protected string logFilepath;

		public AprioriAllBenchmark()
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

	}

}
