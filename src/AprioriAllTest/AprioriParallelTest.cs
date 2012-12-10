using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCL.Net;

namespace AprioriAllLib.Test
{
	[TestClass]
	public class AprioriParallelTest
	{
		private static InputData data;

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			data = new InputData();
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
			Apriori aprioriSerialized = new Apriori(data.Example1);
			List<Litemset> expected = aprioriSerialized.FindOneLitemsets(0.2, false);
			Assert.AreEqual(19, expected.Count);

			//Act
			Apriori apriori = new Apriori(data.Example1);
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
			Apriori apriori = new Apriori(data.Example1);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(0.6, true);

			//Assert
			CollectionAssert.AreEqual(expected, oneLitemsets);
		}

	}
}
