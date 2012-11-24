using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using AprioriAllLib;
using System.Linq;


namespace SequentialMining
{
    class Program
    {
        private static CustomerList customerList;

        static void Main(string[] args)
        {
            XmlReader reader = new XmlReader();
            customerList = reader.ReadFromXmlFile(@"C:\Users\Karolina\Desktop\dataset1.xml");
            List<Litemset> litemsets = FindOneLitemsets(2);
            //List<Item> items = new List<Item>();
            //items.Add(new Item(30));
            //items.Add(new Item(60));
            //items.Add(new Item(10));
            //items.Add(new Item(20));
            //items.Add(new Item(80));
            //List<Litemset> litemsets = generateCandidates(items);

            foreach (Litemset l in litemsets)
            {
                foreach (Item i in l.Items)
                {
                    Console.WriteLine(i.Value);
                }
                Console.WriteLine();
            }

        }

        public static List<Litemset> generateCandidates(List<Item> items)
        {
            int count = items.Count;
            int i = 0;
            List<List<Item>> candLitemsets = new List<List<Item>>();

            candLitemsets.Add(new List<Item>(items));
            count--;
            while (count != 0)
            {
                List<Item> temp;
                foreach (Item item in candLitemsets[i])
                {
                    temp = new List<Item>(candLitemsets[i]);
                    temp.Remove(item);
                    if (!candLitemsets.Exists(list => list.Count == count &&
                        temp.All(tempItem => list.Exists(listItem => listItem.CompareTo(tempItem) == 0))))
                        candLitemsets.Add(temp);
                }
                if (candLitemsets[i + 1].Count == candLitemsets[i].Count - 1)
                    count--;
                i++;
            }
            List<Litemset> l = new List<Litemset>();
            foreach (List<Item> j in candLitemsets)
                l.Add(new Litemset(j));
            return l;
        }

        public static List<Litemset> FindOneLitemsets(int minimalSupport)
        {
            List<Litemset> litemsets = new List<Litemset>();

            foreach (Customer c in customerList.Customers)
            {
                foreach (Transaction t in c.Transactions)
                {
                    //generate subsets (candidates for litemsets)
                    List<Litemset> candidateLitemsets = generateCandidates(t.Items);

                    //check if they already exist in litemsets list; if not, add a litemset to litemsets
                    foreach (Litemset lset in candidateLitemsets)
                    {
                        IEnumerable<Litemset> l = litemsets.Where(litemset => (litemset.Items.Count == lset.Items.Count) &&
                            litemset.Items.All(item => lset.Items.Exists(lsetItem => lsetItem.CompareTo(item) == 0)));

                        int custID = customerList.Customers.IndexOf(c);
                        if (l.Count() == 0 && !lset.IDs.Contains(custID))
                        {
                            litemsets.Add(lset);
                            lset.Support++;
                            lset.IDs.Add(custID);
                        }
                        else
                        {
                            Litemset litset = l.FirstOrDefault();
                            if (!litset.IDs.Contains(custID))
                            {
                                litset.Support++;
                                litset.IDs.Add(custID);
                            }
                        }     
                    }
                }
            }
            // rewrite the litemsets with support >= minimum to a new list
            List<Litemset> properLitemsets = new List<Litemset>();
            foreach (Litemset litemset in litemsets)
                if (litemset.Support >= minimalSupport)
                    properLitemsets.Add(litemset);

            properLitemsets.Sort();

            return properLitemsets;
        }
    }
}
