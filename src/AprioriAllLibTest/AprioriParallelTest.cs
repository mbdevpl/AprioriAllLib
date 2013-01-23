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
		public void AprioriOpenCL_Ex1Su020_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example1, 0.2);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex1Su060_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example1, 0.6);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex2Su010_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example2, 0.1);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex2Su020_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example2, 0.2);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex2Su030_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example2, 0.3);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex2Su050_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example2, 0.5);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex2Su065_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example2, 0.65);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex3Su030_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example3, 0.3);
		}

		[TestMethod]
		public void AprioriOpenCL_Ex3Su040_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.Example3, 0.4);
		}

		[TestMethod]
		public void AprioriOpenCL_Ds2Su010_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.DataSet2, 0.1);
		}

		[TestMethod]
		public void AprioriOpenCL_Ds2Su020_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.DataSet2, 0.2);
		}

		[TestMethod]
		public void AprioriOpenCL_Ds3Su010_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.DataSet2, 0.1);
		}

		[TestMethod]
		public void AprioriOpenCL_Ds3Su020_Test()
		{
			AprioriSerializedAndOpenCLLauncher(Data.DataSet2, 0.2);
		}

	}
}
