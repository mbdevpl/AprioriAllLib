using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test.InConsole
{
	/// <summary>
	/// Application that is able to benchmark and compare the efficiency of both versions 
	/// of the algorithm on various inputs. It creates reports in CSV format.
	/// </summary>
	class Program_Benchmark : AprioriTestBase
	{
		static void Main(string[] args)
		{
			BenchmarkParameters parameters = new BenchmarkParameters(args);

			AprioriSerializedBenchmark asb = new AprioriSerializedBenchmark();
			asb.Initialize(parameters);
			asb.RunAllTests();
			asb.Close();

			AprioriOpenCLBenchmark aob = new AprioriOpenCLBenchmark();
			aob.Initialize(parameters);
			aob.RunAllTests();
			aob.Close();

			AprioriAllSerializedBenchmark aasb = new AprioriAllSerializedBenchmark();
			aasb.Initialize(parameters);
			aasb.RunAllTests();
			aasb.Close();

			//AprioriAllOpenCLBenchmark aaob = new AprioriAllOpenCLBenchmark();
			//b.Initialize(parameters);
			//b.RunAllTests();
			//b.Close();

		}

	}
}
