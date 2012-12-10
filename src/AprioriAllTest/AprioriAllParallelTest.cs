using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AprioriAllLib;

using OpenCL.Net;
using System.Collections.Generic;

namespace AprioriAllTest
{
	/// <summary>
	/// Unit tests for parallel versions of Apriori and AprioriAll.
	/// </summary>
	[TestClass]
	public class AprioriAllParallelTest
	{
		private static CustomerList Example1;

		private static CustomerList DataSet1;

		private static CustomerList DataSet2;

		private static CustomerList DataSet3;

		[ClassInitialize]
		public static void InitializeTestSuite(TestContext testContext)
		{
			Example1 = new CustomerList();
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			Example1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			Example1.Customers.Add(new Customer(new int[] { 90 }));
			Assert.AreEqual(5, Example1.Customers.Count);

			//DataSet1 = new CustomerList();
			//DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			//DataSet1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			//DataSet1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			//DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 90 }));
			//DataSet1.Customers.Add(new Customer(new int[] { 90 }));
			//Assert.AreEqual(5, DataSet1.Customers.Count);

			//DataSet2 = new CustomerList();
			//DataSet2.Customers.Add(new Customer(new int[] { 30 }, new int[] { 30, 40, 50 }, new int[] { 80 }, new int[] { 90 }));
			//DataSet2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			//DataSet2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30, 50, 70 }));
			//DataSet2.Customers.Add(new Customer(new int[] { 30, 40 }, new int[] { 50, 60 }, new int[] { 70, 80, 90 }));
			//DataSet2.Customers.Add(new Customer(new int[] { 80 }, new int[] { 90 }));
			//DataSet2.Customers.Add(new Customer(new int[] { 10, 50 }, new int[] { 80 }));
			//Assert.AreEqual(6, DataSet2.Customers.Count);

			//DataSet3 = new CustomerList();
			//DataSet3.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			//DataSet3.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			//DataSet3.Customers.Add(new Customer(new int[] { 30, 50, 70 }, new int[] { 10, 20 }));
			//DataSet3.Customers.Add(new Customer(new int[] { 30, 80 }, new int[] { 40, 70 }, new int[] { 90, 30, 40, 50 }));
			//DataSet3.Customers.Add(new Customer(new int[] { 90 }, new int[] { 80 }));
			//DataSet3.Customers.Add(new Customer(new int[] { 50, 10 }, new int[] { 80 }));
			//Assert.AreEqual(6, DataSet3.Customers.Count);

			// future sets may be read from xml files?
			//XmlReader reader = new XmlReader();
			//CustomerList cList = reader.ReadFromXmlFile(/* full path or whatever else works*/);
		}

		[ClassCleanup]
		public static void CleanupTestSuite()
		{
		}

		[TestMethod]
		public void Test_OpenCL_PlatformsCount()
		{
			//Arrange

			//Act
			uint count = OpenCLChecker.PlatformsCount();

			//Assert
			Assert.AreEqual(true, count >= 1, "number of OpenCL platforms must be greater or equal 1");
		}

		[TestMethod]
		public void Test_OpenCL_DevicesCount()
		{
			//Arrange
			uint devicesCount = 0;
			Cl.ErrorCode error = Cl.ErrorCode.Success;

			//Act
			uint count = OpenCLChecker.PlatformsCount();
			Assert.AreEqual(true, count >= 1, "if there are no platforms available, there can be no devices");

			Cl.Platform[] platforms = Cl.GetPlatformIDs(out error);
			Assert.AreEqual(Cl.ErrorCode.Success, error, "could not get list of available platforms");
			foreach (Cl.Platform platfrom in platforms)
			{
				Cl.Device[] devices = Cl.GetDeviceIDs(platfrom, Cl.DeviceType.All, out error);
				Assert.AreEqual(Cl.ErrorCode.Success, error, "could not get list of available devices");
				devicesCount += (uint)devices.Length;
			}

			//Assert
			Assert.AreEqual(true, devicesCount >= 1);
		}

		[TestMethod]
		public void Test_AprioriParallel_Example1_LowSupport()
		{
			//Arrange
			Apriori aprioriSerialized = new Apriori(Example1);
			List<Litemset> expected = aprioriSerialized.FindOneLitemsets(0.2, false);
			Assert.AreEqual(19, expected.Count);

			//Act
			Apriori apriori = new Apriori(Example1);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.2, true);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
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
			Apriori apriori = new Apriori(Example1);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.6, true);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

	}
}
