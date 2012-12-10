using System;
using System.Collections.Generic;
using System.Linq;

namespace AprioriAllLib.ConsoleTest
{
	/// <summary>
	/// Defines a console application that runs AprioriAll on input given in command line parameters.
	/// 
	/// This is modified version of application by Karolina Baltyn.
	/// </summary>
	class ConsoleApp
	{
		static void Main(string[] args)
		{
			XmlReader reader = new XmlReader();
			CustomerList customerList = new CustomerList();
			double support = 1;

			if (args.Count() == 0)
			{
				//customerList = reader.ReadFromXmlFile("dataset1.xml");
				Console.Out.WriteLine("Usage: aprioriall <filename> <support>");
				Console.Out.WriteLine("  <filename> : XML file containing database of customers");
				Console.Out.WriteLine("  <support> : real number greater than 0 and less or equal 1");
			}
			else if (args.Count() == 2)
			{
				try
				{
					support = double.Parse(args[1]);
					if (support <= 0 || support > 1)
					{
						Console.WriteLine("Invalid support: should be between 0 and 1");
						return;
					}
					customerList = reader.ReadFromXmlFile(args[0]);
				}
				catch (Exception)
				{
					throw new Exception("Invalid parameters");
				}
			}
			else if (args.Count() == 1)
			{
				throw new Exception("Invalid number of arguments: should be 2");
			}

			Console.Out.WriteLine("\nInput:");
			foreach (Customer c in customerList.Customers)
				Console.Out.WriteLine(" - {0}", c);

			Apriori apriori = new Apriori(customerList);
			List<Litemset> litemsets = apriori.FindOneLitemsets(support);

			Console.WriteLine("Litemsets found: \n");
			foreach (Litemset l in litemsets)
			{
				Console.Write("(");
				foreach (Item i in l.Items)
				{
					Console.Write(i.Value);
					if (l.Items.IndexOf(i) != l.Items.Count - 1)
						Console.Write(", ");
				}
				Console.Write(") ");
			}

			AprioriAll aprioriAll = new AprioriAll(customerList);
			List<Customer> aprioriAllResult = aprioriAll.Execute(support);

			Console.WriteLine("\n\nSequences found: \n");
			foreach (Customer c in aprioriAllResult)
			{
				Console.Write("{");
				foreach (Transaction t in c.Transactions)
				{
					Console.Write("(");
					foreach (Item i in t.Items)
					{
						Console.Write(i.Value);
						if (t.Items.IndexOf(i) != t.Items.Count - 1)
							Console.Write(", ");
					}
					Console.Write(")");
				}
				Console.Write("} ");
			}



			Console.WriteLine("\n\nPress enter to continue");
			Console.ReadLine();
		}
	}
}
