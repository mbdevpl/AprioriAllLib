using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{

	public class AprioriBenchmarkLogEntry
	{
		public DateTime DateAndTime;
		public CustomerList Input;
		public double Support;
		public double Average1;
		public double Average2;

		public AprioriBenchmarkLogEntry(DateTime dateAndTime, CustomerList input, double support,
			double average1, double average2)
		{
			this.DateAndTime = dateAndTime;
			this.Input = input;
			this.Support = support;
			this.Average1 = average1;
			this.Average2 = average2;
		}
	}

}
