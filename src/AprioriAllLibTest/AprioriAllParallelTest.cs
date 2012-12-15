using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using OpenCL.Net;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Unit tests for parallel version of AprioriAll.
	/// </summary>
	[TestClass]
	public class AprioriAllParallelTest : AprioriAllTestBase
	{

		[ClassInitialize]
		public static void InitializeTestSuite(TestContext testContext)
		{
			TestBaseInitialize();
		}

		[ClassCleanup]
		public static void CleanupTestSuite()
		{
		}

	}
}
