using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{

	public class AprioriBenchmarkLogEntry
	{
		public static readonly string CsvHeader;

		static AprioriBenchmarkLogEntry()
		{
			StringBuilder s = new StringBuilder();
			s.Append("DateAndTime,");
			s.Append("Algorithm,OpenCL,InputCustomers,Support,");
			s.Append("Repeats,NewEachTime,Average1,Average2");
			CsvHeader = s.ToString();
		}

		public DateTime DateAndTime;

		public string Algorithm;
		public bool OpenCL;
		public IEnumerable<ICustomer> Input;
		public double Support;

		public uint Repeats;
		public bool NewEachTime;
		public double Average1;
		public double Average2;

		public AprioriBenchmarkLogEntry(DateTime dateAndTime, 
			string algorithm, bool openCL, IEnumerable<ICustomer> input, double support,
			uint repeats, bool newEachTime, double average1, double average2)
		{
			this.DateAndTime = dateAndTime;

			this.Algorithm = algorithm;
			this.OpenCL = openCL;
			this.Input = input;
			this.Support = support;

			this.Repeats = repeats;
			this.NewEachTime = newEachTime;
			this.Average1 = average1;
			this.Average2 = average2;
		}

		public string ToCsvString()
		{
			StringBuilder s = new StringBuilder();

			s.AppendFormat("{2:00}/{1:00}/{0:0000},",
				DateAndTime.Year, DateAndTime.Month, DateAndTime.Day);
			s.AppendFormat("{0:00}:{1:00}:{2:00},",
				DateAndTime.Hour, DateAndTime.Minute, DateAndTime.Second);

			s.AppendFormat("{0},{1},{2},", Algorithm, OpenCL, Input.Count());
			s.AppendFormat("{0:0.000},", Support);

			s.AppendFormat("{0},{1},", Repeats, NewEachTime);
			s.AppendFormat("{0:0.00},{1:0.00}", Average1, Average2);

			return s.ToString();
		}

		public string ToLatexPgfplotsString()
		{
			StringBuilder s = new StringBuilder();

			s.Append("(");
			s.AppendFormat("{0},", Input.Count());
			s.AppendFormat("{0:0.000}", Average1);
			s.Append(")");

			return s.ToString();
		}

		public override string ToString()
		{
			return ToCsvString();
		}

	}

}
