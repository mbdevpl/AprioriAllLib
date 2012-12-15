using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	public class AprioriTestBase
	{
		protected static InputData data;

		protected static void TestBaseInitialize()
		{
			data = new InputData();
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		public string GetAprioriTestResults(List<Litemset> expected, List<Litemset> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			foreach (Litemset c in expected)
				result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			foreach (Litemset c in actual)
				result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}
	}
}
