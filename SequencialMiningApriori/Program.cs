using System;
using System.Collections.Generic;
using AprioriAllLib;
using System.Linq;


namespace SequentialMining
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReader reader = new XmlReader();
            CustomerList customerList = new CustomerList();
            double support = 0.4;

            if(args.Count() == 0)
                customerList = reader.ReadFromXmlFile("dataset1.xml");
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
                catch (Exception e)
                {
                    throw new Exception("Invalid parameters");
                }
            }
            else if (args.Count() == 1)
            {
                throw new Exception("Invalid number of arguments: should be 2");
            }
            Apriori apriori = new Apriori(customerList);
            List<Litemset> litemsets = apriori.FindOneLitemsets(support);
            List<Customer> aprioriAllResult = AprioriAllAlgorithm.execute(customerList, support);

            Console.WriteLine("Litemsets found: \n");
            foreach (Litemset l in litemsets)
            {
                Console.Write("(");
                foreach (Item i in l.Items)
                {
                    Console.Write(i.Value);
                    if(l.Items.IndexOf(i) != l.Items.Count - 1)
                        Console.Write(", ");
                }
                Console.Write(") ");
            }

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
