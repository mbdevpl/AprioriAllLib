using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AprioriAllLib.ConsoleTest
{
	/// <summary>
	/// Runs AprioriAll algorithm on random input.
	/// </summary>
	public class Test_AprioriAll
	{

		private static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

			Console.Out.WriteLine("Serialized AprioriAll algorithm test");

			//Arrange
			CustomerList Example1 = new CustomerList();
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			Example1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			Example1.Customers.Add(new Customer(new int[] { 90 }));

			CustomerList randomExample
				//= InputGenerator.GenerateRandomList(700, 7, 7);
				//= XmlReader.ReadFromXmlFile("dataset3.xml");
				= Example1;
			double support = 0.2;

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in randomExample.Customers)
			{
				Console.Out.WriteLine(" - {0}", c);
			}

			//Act
			Console.Out.WriteLine("\nComputation:");
			AprioriAll all = new AprioriAll(randomExample);
			List<Customer> results = all.RunAprioriAll(support, true);

			//Assert
			Console.Out.WriteLine("\nResults:");
			foreach (Customer c in results)
			{
				Console.Out.WriteLine(" - {0}", c);
			}

			Console.Write("Fin.");
			Console.ReadKey();
		}

	}
}
