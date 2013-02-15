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
	public class AprioriTest : AprioriAllLibTestBase
	{
		[TestMethod]
		public void Apriori_Ex1Su020_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
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
			Apriori apriori = new Apriori(Data.Example1);
			var oneLitemsets = apriori.RunApriori(0.2, true);

			//Assert
			// areEquivalent doesn't work ?!
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Apriori_Ex1Su060_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 70));
			Assert.AreEqual(2, expected.Count);

			//Act
			Apriori apriori = new Apriori(Data.Example1);
			var oneLitemsets = apriori.RunApriori(0.6, true);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Apriori_Ds2Su020_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
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
			Apriori apriori = new Apriori(Data.DataSet2);
			var oneLitemsets = apriori.RunApriori(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Apriori_Ds2Su050_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
			expected.Add(new Litemset(3, 10));
			expected.Add(new Litemset(4, 30));
			expected.Add(new Litemset(3, 40));
			expected.Add(new Litemset(4, 50));
			expected.Add(new Litemset(3, 70));
			expected.Add(new Litemset(4, 80));
			expected.Add(new Litemset(3, 90));
			Assert.AreEqual(7, expected.Count);

			//Act
			Apriori apriori = new Apriori(Data.DataSet2);
			var oneLitemsets = apriori.RunApriori(0.5);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Apriori_Ds3Su020_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
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
			Apriori apriori = new Apriori(Data.DataSet3);
			var oneLitemsets = apriori.RunApriori(0.2);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

		[TestMethod]
		public void Apriori_Ds3Su050_Test()
		{
			//Arrange
			var expected = new List<ILitemset>();
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
			Apriori apriori = new Apriori(Data.DataSet3);
			var oneLitemsets = apriori.RunApriori(0.5);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets, GetAprioriTestResults(expected, oneLitemsets));
		}

	}
}
