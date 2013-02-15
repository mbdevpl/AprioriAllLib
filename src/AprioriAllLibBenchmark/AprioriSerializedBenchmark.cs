using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AprioriAllLib.Test
{

	public class AprioriSerializedBenchmark : AprioriBenchmark
	{

		public AprioriSerializedBenchmark()
			: base()
		{
			// nothing needed here
		}

		protected override bool BeginAllTests()
		{
			if (!parameters.Apriori || !parameters.Serialized)
				return false;
			return true;
		}

		protected override Apriori ConstructTestedInstance(IEnumerable<ICustomer> input)
		{
			return new Apriori(input);
		}

		protected override IEnumerable<object> RunTestedInstance(Apriori apriori, double support, bool progressOutput)
		{
			return apriori.RunApriori(support, parameters.PrintProgress);
		}

		protected override void DestroyTestedInstance(Apriori apriori)
		{
			// nothing needed here
		}

	}

}
