using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCL.Net;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Tests for existence of the OpenCL environment. Do not expect the parallel tests to go well
	/// if any of the tests contained here fails.
	/// </summary>
	[TestClass]
	public class OpenCLTest
	{

		[TestMethod]
		public void Test_OpenCL_PlatformsCount()
		{
			//Arrange

			//Act
			uint count = OpenCLChecker.PlatformsCount();

			//Assert
			Assert.IsTrue(count >= 1, "number of OpenCL platforms must be greater or equal 1");
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
			Assert.IsTrue(devicesCount >= 1);
		}

		[TestMethod]
		public void Test_OpenCL_KernelsSourceCodeReading()
		{
			//Arrange
			string[] source = null;
			IntPtr[] lenghts = null;

			//Act
			OpenCLToolkit.GetSourceCodeFromLocalResource("distinct.cl", out source, out lenghts);
			
			//Assert
			Assert.IsTrue(source.Length > 0, "the resource 'distinct.cl' should not be empty");
			Assert.AreEqual(source.Length, lenghts.Length);
		}

	}
}
