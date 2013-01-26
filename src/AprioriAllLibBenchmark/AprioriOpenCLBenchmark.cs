using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{
	public class AprioriOpenCLBenchmark : AprioriBenchmark
	{

		public AprioriOpenCLBenchmark()
			: base()
		{
			// nothing needed here
		}

		protected override bool BeginAllTests()
		{
			if (!parameters.Apriori || !parameters.OpenCL)
				return false;
			return true;
		}

		protected override Apriori ConstructTestedInstance(CustomerList input)
		{
			return new Apriori(input);
		}

		protected override List<Litemset> RunTestedInstance(Apriori apriori, double support, bool progressOutput)
		{
			return apriori.RunParallelApriori(support, parameters.PrintProgress);
		}

		protected override void DestroyTestedInstance(Apriori apriori)
		{
			apriori.Dispose();
		}

	}
}
