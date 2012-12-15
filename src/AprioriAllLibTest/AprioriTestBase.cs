using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Basic routines needed to test Apriori algorithm. AprioriAll tests are also derived from this class.
	/// </summary>
	public class AprioriTestBase
	{

		private static bool initialized = false;

		private static object aprioriTestBaseLock = new object();

		private static InputData data;

		/// <summary>
		/// Contains several example data sets made for testing.
		/// </summary>
		public static InputData Data
		{
			get { return AprioriTestBase.data; }
			//set { AprioriTestBase.data = value; }
		}

		private static void AprioriTestBaseInitialize()
		{
			lock (aprioriTestBaseLock)
			{
				if (initialized)
					return;
				data = new InputData();

				initialized = true;
			}
		}

		private TextWriterTraceListener traceListener;

		public AprioriTestBase()
		{
			if (!initialized)
				AprioriTestBaseInitialize();
			traceListener = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(traceListener);
		}

		~AprioriTestBase()
		{
			Trace.Listeners.Remove(traceListener);
		}

		public string GetAprioriTestResults(List<Litemset> expected, List<Litemset> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			foreach (Litemset c in expected)
				result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			foreach (Litemset c in actual)
				result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}

		public void PrintInput(CustomerList customerList)
		{
			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in customerList.Customers)
				Console.Out.WriteLine(" - {0}", c);
		}

		public void PrintAprioriOutput(List<Litemset> results)
		{
			Console.Out.WriteLine("\nResults:");
			foreach (Litemset l in results)
				Console.Out.WriteLine(" - {0}", l);
		}

	}
}
