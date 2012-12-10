using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using OpenCL.Net;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for parallel versions of Apriori and AprioriAll.
	/// </summary>
	[TestClass]
	public class AprioriAllParallelTest
	{
		private static InputData data;

		[ClassInitialize]
		public static void InitializeTestSuite(TestContext testContext)
		{
			data = new InputData();
		}

		[ClassCleanup]
		public static void CleanupTestSuite()
		{
		}

	}
}
