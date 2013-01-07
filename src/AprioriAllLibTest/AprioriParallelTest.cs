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

		[TestMethod]
		public void Test_AprioriParallel_Example1_LowSupport()
		{
			//Arrange
			Apriori aprioriSerialized = new Apriori(Data.Example1);
			List<Litemset> expected = aprioriSerialized.RunApriori(0.2);
			Assert.AreEqual(19, expected.Count);

			//Act
			Apriori apriori = new Apriori(Data.Example1);
			List<Litemset> oneLitemsets = apriori.RunParallelApriori(0.2, true);
			apriori.Dispose();

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Test_AprioriParallel_Example1_HighSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 70));
			Assert.AreEqual(2, expected.Count);

			//Act
			Apriori apriori = new Apriori(Data.Example1);
			List<Litemset> oneLitemsets = apriori.RunParallelApriori(0.6, true);
			apriori.Dispose();

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

	}
}
