using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Basic routines needed to test Apriori algorithm. AprioriAll tests are also derived from this class.
	/// </summary>
	[TestClass]
	public class AprioriTestBase
	{
		private static string MsgCount = "lists lengths do not match\n{0}";
		private static string MsgElements = "mismatched elements at index {0}:\n{1} vs. {2}\n{3}";

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

		//private TextWriterTraceListener traceListener;

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public AprioriTestBase()
		{
			if (!initialized)
				AprioriTestBaseInitialize();
			//traceListener = new TextWriterTraceListener(Console.Out);
			//Trace.Listeners.Add(traceListener);
		}

		~AprioriTestBase()
		{
			//Trace.Listeners.Remove(traceListener);
		}

		[TestInitialize]
		public virtual void TestInitialize()
		{
			//traceListener = new TextWriterTraceListener(Console.Out);
			//Trace.Listeners.Add(traceListener);
		}

		[TestCleanup]
		public virtual void TestCleanup()
		{
			//Trace.Listeners.Remove(traceListener);
			//traceListener = null;
		}

		public string GetAprioriTestResults(List<Litemset> expected, List<Litemset> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			if (expected == null)
				result.AppendLine(" null");
			else
				foreach (Litemset c in expected)
					result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			if (actual == null)
				result.AppendLine(" null");
			else
				foreach (Litemset c in actual)
					result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}

		public void PrintInput(CustomerList customerList)
		{
			Console.Out.WriteLine("\nInput:");
			if (customerList.Customers.Count < 50)
			{
				foreach (Customer c in customerList.Customers)
					Console.Out.WriteLine(" - {0}", c);
			}
			else
			{
				const int header = 10;
				int i;

				for (i = 0; i < header; ++i)
				{
					Customer c = customerList.Customers[i];
					Console.Out.WriteLine(" - {0}", c);
				}

				Console.Out.WriteLine(" ... {0} entries omitted", customerList.Customers.Count - 2 * header);

				for (i = customerList.Customers.Count - header; i < customerList.Customers.Count; ++i)
				{
					Customer c = customerList.Customers[i];
					Console.Out.WriteLine(" - {0}", c);
				}

			}
		}

		public void PrintAprioriOutput(List<Litemset> results)
		{
			Console.Out.WriteLine("\nResults:");
			foreach (Litemset l in results)
				Console.Out.WriteLine(" - {0}", l);
		}

		protected void AprioriSerializedAndOpenCLLauncher(CustomerList input, double support)
		{
			//Arrange
			Apriori aprioriSerialized = new Apriori(input);
			List<Litemset> expected = aprioriSerialized.RunApriori(support);
			Assert.IsNotNull(expected, GetAprioriTestResults(expected, null));

			//Act
			Apriori apriori = new Apriori(input);
			List<Litemset> oneLitemsets = null;
			try
			{
				oneLitemsets = apriori.RunParallelApriori(support, true);
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e.ToString());
			}
			apriori.Dispose();

			//Assert
			Assert.IsNotNull(oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
			AprioriSerializedAndOpenCLResultsComparer(expected, oneLitemsets);
		}

		protected void AprioriSerializedAndOpenCLResultsComparer(List<Litemset> resultsSerialized,
			List<Litemset> resultsOpenCL)
		{
			Assert.AreEqual(resultsSerialized.Count, resultsOpenCL.Count,
				String.Format(MsgCount, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));

			for (int n = 0; n < resultsSerialized.Count; ++n)
			{
				Litemset expectedSet = resultsSerialized[n];
				Litemset actualSet = resultsOpenCL[n];


				Assert.AreEqual(expectedSet.Support, actualSet.Support,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
				Assert.AreEqual(expectedSet.Items.Count, expectedSet.Items.Count,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
				Assert.AreEqual(expectedSet, actualSet,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
			}

			CollectionAssert.AreEqual(resultsSerialized, resultsOpenCL, GetAprioriTestResults(resultsSerialized, resultsOpenCL));
		}

	}
}
