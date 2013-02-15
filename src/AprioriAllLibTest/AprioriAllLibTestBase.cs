using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Basic routines needed to test Apriori algorithm, and also for all AprioriAll tests.
	/// Defines common routines used by all of the tests, and so Apriori and AprioriAll tests
	/// are derived from this class.
	/// </summary>
	[TestClass]
	public class AprioriAllLibTestBase
	{
		private static string MsgCount = "lists lengths do not match\n{0}";

		private static string MsgElements = "mismatched elements at index {0}:\n{1} vs. {2}\n{3}";

		private static bool initialized = false;

		private static object aprioriTestBaseLock = new object();

		/// <summary>
		/// Contains several example data sets made for testing.
		/// </summary>
		public static InputData Data { get { return AprioriAllLibTestBase.data; } }
		private static InputData data;

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

		public AprioriAllLibTestBase()
		{
			if (!initialized)
				AprioriTestBaseInitialize();
			//traceListener = new TextWriterTraceListener(Console.Out);
			//Trace.Listeners.Add(traceListener);
		}

		//~AprioriAllLibTestBase()
		//{
		//	//Trace.Listeners.Remove(traceListener);
		//}

		//[TestInitialize]
		//public virtual void TestInitialize()
		//{
		//	//traceListener = new TextWriterTraceListener(Console.Out);
		//	//Trace.Listeners.Add(traceListener);
		//}

		//[TestCleanup]
		//public virtual void TestCleanup()
		//{
		//	//Trace.Listeners.Remove(traceListener);
		//	//traceListener = null;
		//}

		public string GetAprioriTestResults(List<ILitemset> expected, List<ILitemset> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			if (expected == null)
				result.AppendLine(" null");
			else
				foreach (ILitemset c in expected)
					result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			if (actual == null)
				result.AppendLine(" null");
			else
				foreach (ILitemset c in actual)
					result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}

		public string GetAprioriAllTestResults(List<ICustomer> expected, List<ICustomer> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			if (expected == null)
				result.AppendLine(" null");
			else
				foreach (ICustomer c in expected)
					result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			if (actual == null)
				result.AppendLine(" null");
			else
				foreach (ICustomer c in actual)
					result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}

		[Obsolete]
		public void PrintInput(CustomerList customerList)
		{
			PrintInput(customerList.Customers);
		}

		public void PrintInput(IEnumerable<ICustomer> customers)
		{
			int customersCount = customers.Count();
			Console.Out.WriteLine("\nInput:");
			if (customersCount < 30)
			{
				foreach (Customer c in customers)
					Console.Out.WriteLine(" - {0}", c);
			}
			else
			{
				const int header = 10;
				const int headerT = 4;
				int i;
				int j;

				var enumerator = customers.GetEnumerator();

				for (i = 0; i < header; ++i)
				{
					enumerator.MoveNext();
					ICustomer c = enumerator.Current;
					if (c.GetTransactionsCount() < 10)
					{
						Console.Out.WriteLine(" - {0}", c);
					}
					else
					{
						Console.Out.Write(" - (");
						for (j = 0; j < headerT; ++j)
						{
							Console.Out.Write("{0}, ", c.GetTransaction(j));
						}

						Console.Out.Write("... ");

						for (j = c.GetTransactionsCount() - headerT; j < c.GetTransactionsCount(); ++j)
						{
							Console.Out.Write(", {0}", c.GetTransaction(j));
						}
						Console.Out.WriteLine(")");
					}
				}

				for (; i < customersCount - header; ++i)
					enumerator.MoveNext();

				Console.Out.WriteLine(" ... {0} entries omitted", customersCount - 2 * header);

				for (; i < customersCount; ++i)
				{
					ICustomer c = enumerator.Current;
					//Customer c = customers[i];
					if (c.GetTransactionsCount() < 10)
					{
						Console.Out.WriteLine(" - {0}", c);
					}
					else
					{
						Console.Out.Write(" - (");
						for (j = 0; j < headerT; ++j)
						{
							Console.Out.Write("{0}, ", c.GetTransaction(j));
						}

						Console.Out.Write("... ");

						for (j = c.GetTransactionsCount() - headerT; j < c.GetTransactionsCount(); ++j)
						{
							Console.Out.Write(", {0}", c.GetTransaction(j));
						}
						Console.Out.WriteLine(")");
					}
				}

			}
		}

		public void PrintAprioriOutput(IEnumerable<ILitemset> results)
		{
			Console.Out.WriteLine("\nResults:");
			foreach (ILitemset l in results)
				Console.Out.WriteLine(" - {0}", l);
		}

		public void PrintAprioriAllOutput(IEnumerable<ICustomer> results)
		{
			Console.Out.WriteLine("\nResults:");
			foreach (ICustomer c in results)
				Console.Out.WriteLine(" - {0}", c);
		}

		protected void AprioriAllSerializedLauncher(IEnumerable<ICustomer> input, double support,
			List<ICustomer> expectedResults)
		{
			//Arrange


			//Act
			AprioriAll aprioriSerialized = new AprioriAll(input);
			List<ICustomer> frequentSequences = null;
			try
			{
				frequentSequences = aprioriSerialized.RunAprioriAll(support, true);
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e.ToString());
			}

			//Assert
			AprioriAllResultsComparer(expectedResults, frequentSequences);
		}

		private void AprioriAllResultsComparer(List<ICustomer> resultsExpected, List<ICustomer> resultsActual)
		{
			Assert.IsNotNull(resultsActual, GetAprioriAllTestResults(resultsExpected, null));

			CollectionAssert.AreEqual(resultsExpected, resultsActual,
				GetAprioriAllTestResults(resultsExpected, resultsActual));
		}

		protected void AprioriSerializedAndOpenCLLauncher(IEnumerable<ICustomer> input, double support)
		{
			//Arrange
			Apriori aprioriSerialized = new Apriori(input);
			var expected = aprioriSerialized.RunApriori(support);
			Assert.IsNotNull(expected, GetAprioriTestResults(expected, null));

			//Act
			Apriori apriori = new Apriori(input);
			List<ILitemset> oneLitemsets = null;
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
			AprioriSerializedAndOpenCLResultsComparer(expected, oneLitemsets);
		}

		protected void AprioriSerializedAndOpenCLResultsComparer(List<ILitemset> resultsSerialized,
			List<ILitemset> resultsOpenCL)
		{
			Assert.IsNotNull(resultsOpenCL, GetAprioriTestResults(resultsSerialized, resultsOpenCL));

			Assert.AreEqual(resultsSerialized.Count, resultsOpenCL.Count,
				String.Format(MsgCount, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));

			for (int n = 0; n < resultsSerialized.Count; ++n)
			{
				ILitemset expectedSet = resultsSerialized[n];
				ILitemset actualSet = resultsOpenCL[n];


				Assert.AreEqual(expectedSet.GetSupport(), actualSet.GetSupport(),
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
				Assert.AreEqual(expectedSet.GetItemsCount(), expectedSet.GetItemsCount(),
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
				Assert.AreEqual(expectedSet, actualSet,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(resultsSerialized, resultsOpenCL)));
			}

			CollectionAssert.AreEqual(resultsSerialized, resultsOpenCL, GetAprioriTestResults(resultsSerialized, resultsOpenCL));
		}

		public string ToIntArrayInitializer(List<ICustomer> Customers)
		{
			StringBuilder s = new StringBuilder();

			if (Customers.Count > 0)
			{
				foreach (ICustomer c in Customers)
				{
					s.Append(" new int[][] {");
					foreach (ITransaction t in c.GetTransactions())
					{
						s.Append(" new int[] {");
						foreach (IItem i in t.GetItems())
						{
							s.AppendFormat(" {0},", i.GetId());
						}
						s.Remove(s.Length - 1, 1);
						s.Append(" },");
					}
					s.Remove(s.Length - 1, 1);
					s.Append("},\n");
				}
				s.Remove(s.Length - 2, 2);
			}

			return s.ToString();
		}


	}
}
