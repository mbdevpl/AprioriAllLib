﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Base class for all AprioriAll tests. Defines common routines used by the tests.
	/// </summary>
	public class AprioriAllTestBase : AprioriTestBase
	{

		public string GetAprioriAllTestResults(List<Customer> expected, List<Customer> actual)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Expected:");
			foreach (Customer c in expected)
				result.AppendFormat(" - {0}\n", c);

			result.AppendLine("Actual:");
			foreach (Customer c in actual)
				result.AppendFormat(" - {0}\n", c);

			return result.ToString();
		}

		//public void RunAprioriAll(CustomerList input, double support)
		//{
		//}

		public void PrintAprioriAllOutput(List<Customer> results)
		{
			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in results)
				Console.Out.WriteLine(" - {0}", c);
		}

	}
}
