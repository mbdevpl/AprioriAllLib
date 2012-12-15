using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for serialized version of Apriori algorithm.
	/// </summary>
	[TestClass]
	public class AprioriTest
	{
		private static InputData data;

		// Use ClassInitialize to run code before running the first test in the class.
		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			data = new InputData();
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		[TestMethod]
		public void Test_Apriori_Example1_LowSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(1, 10));
			expected.Add(new Litemset(1, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(1, 40));
			expected.Add(new Litemset(1, 50));
			expected.Add(new Litemset(1, 60));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(1, 80));
			expected.Add(new Litemset(1, 90));
			expected.Add(new Litemset(1, 10, 20));
			expected.Add(new Litemset(1, 10, 60));
			expected.Add(new Litemset(1, 10, 70));
			expected.Add(new Litemset(1, 30, 50));
			expected.Add(new Litemset(1, 30, 70));
			expected.Add(new Litemset(1, 40, 70));
			expected.Add(new Litemset(1, 50, 70));
			expected.Add(new Litemset(1, 60, 70));
			expected.Add(new Litemset(1, 10, 60, 70));
			expected.Add(new Litemset(1, 30, 50, 70));
			Assert.AreEqual(19, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.Example1);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_Apriori_Example1_HighSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 70));
			Assert.AreEqual(2, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.Example1);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.6);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_Apriori_DataSet2_LowSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(2, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(2, 60));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			expected.Add(new Litemset(2, 10, 20));
			expected.Add(new Litemset(2, 30, 40));
			expected.Add(new Litemset(2, 30, 50));
			Assert.AreEqual(12, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.DataSet2);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets); // areEquivalent doesn't work
		}

		[TestMethod]
		public void Test_Apriori_DataSet2_HighSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			Assert.AreEqual(7, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.DataSet2);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.5);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

		[TestMethod]
		public void Test_Apriori_DataSet3_LowSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(2, 20));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			expected.Add(new Litemset(2, 10, 20));
			expected.Add(new Litemset(2, 30, 40));
			expected.Add(new Litemset(3, 30, 50));
			expected.Add(new Litemset(2, 40, 50));
			expected.Add(new Litemset(2, 40, 70));
			expected.Add(new Litemset(2, 30, 40, 50));
			Assert.AreEqual(14, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.DataSet3);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets); // areEquivalent doesn't work
		}

		[TestMethod]
		public void Test_Apriori_DataSet3_HighSupport()
		{
			//Arrange
			List<Litemset> expected = new List<Litemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			expected.Add(new Litemset(3, 30, 50));
			Assert.AreEqual(8, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.DataSet3);
			List<Litemset> oneLitemsets = apriori.RunApriori(0.5);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

	}
}
