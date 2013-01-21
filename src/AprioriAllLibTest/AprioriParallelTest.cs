using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for parallel version of Apriori.
	/// </summary>
	[TestClass]
	public class AprioriParallelTest : AprioriTestBase
	{
		static string MsgCount = "lists lengths do not match\n{0}";
		static string MsgElements = "mismatched elements at index {0}:\n{1} vs. {2}\n{3}";

		private void AprioriSerializedAndParallelLauncher(CustomerList input, double support)
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
			AprioriParallelAndSerializedResultsComparer(expected, oneLitemsets);
		}

		private void AprioriParallelAndSerializedResultsComparer(List<Litemset> expected, List<Litemset> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count,
				String.Format(MsgCount, GetAprioriTestResults(expected, actual)));

			for (int n = 0; n < expected.Count; ++n)
			{
				Litemset expectedSet = expected[n];
				Litemset actualSet = actual[n];


				Assert.AreEqual(expectedSet.Support, actualSet.Support,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(expected, actual)));
				Assert.AreEqual(expectedSet.Items.Count, expectedSet.Items.Count,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(expected, actual)));
				Assert.AreEqual(expectedSet, actualSet,
					String.Format(MsgElements, n, expectedSet, actualSet, GetAprioriTestResults(expected, actual)));
			}

			CollectionAssert.AreEqual(expected, actual, GetAprioriTestResults(expected, actual));
		}

		[TestMethod]
		public void Test_AprioriParallel_Example1_LowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example1, 0.2);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example1_HighSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example1, 0.6);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example2_LowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example2, 0.2);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example2_MediumSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example2, 0.5);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example2_HighSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example2, 0.65);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example3_LowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example3, 0.3);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example3_MediumSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.Example3, 0.4);
		}

		[TestMethod]
		public void Test_AprioriParallel_DataSet2_VeryLowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.DataSet2, 0.1);
		}

		[TestMethod]
		public void Test_AprioriParallel_DataSet2_LowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.DataSet2, 0.2);
		}

		[TestMethod]
		public void Test_AprioriParallel_DataSet3_VeryLowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.DataSet2, 0.1);
		}

		[TestMethod]
		public void Test_AprioriParallel_DataSet3_LowSupport()
		{
			AprioriSerializedAndParallelLauncher(Data.DataSet2, 0.2);
		}

	}
}
