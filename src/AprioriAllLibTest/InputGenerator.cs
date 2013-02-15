using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Tool for generating random input for Apriori and AprioriAll algorithms.
	/// </summary>
	public static class InputGenerator
	{

		/// <summary>
		/// Generates a random list of customers. Randomization is parameterized.
		/// </summary>
		/// <param name="customersCount">determines how many customers will be generated</param>
		/// <param name="maxTransactionCount">limit of number of transactions for each customer</param>
		/// <param name="maxTransactionLength">limit of number of items per transaction</param>
		/// <returns></returns>
		public static List<ICustomer> GenerateRandomList(int customersCount, int maxTransactionCount,
			int maxTransactionLength)
		{
			return GenerateRandomList(customersCount, maxTransactionCount,
				maxTransactionLength, maxTransactionLength);
		}

		public static List<ICustomer> GenerateRandomList(int customersCount, int maxTransactionCount,
			int maxTransactionLength, int maxUniqueItemsCount)
		{
			Random random = new Random();
			var randomCustomerList = new List<ICustomer>();

			ICustomer c;
			ITransaction t;
			IItem item;
			for (int i = 0; i < customersCount; ++i)
			{
				c = new Customer();
				int transactionsCount = random.Next() % maxTransactionCount + 1;
				for (int tn = 0; tn < transactionsCount; ++tn)
				{
					int n = random.Next() % maxTransactionLength + 1;
					t = new Transaction();
					for (int it = 0; it < n; ++it)
					{
						int itemVal = (random.Next() % maxUniqueItemsCount) + 1;
						item = new Item(itemVal);
						if (t.GetItems().Any(x => x.GetId() == item.GetId()))
						{
							--it;
							continue;
						}
						t.AddItem(item);
					}
					c.AddTransaction(t);
				}
				randomCustomerList.Add(c);
			}
			return randomCustomerList;
		}

		public static List<ICustomer> GenerateRandomList(int[][] sizes, int uniqueIds)
		{
			Random random = new Random();
			var randomCustomerList = new List<ICustomer>();

			int[] uniques = new int[uniqueIds];
			for (int i = 0; i < uniqueIds; ++i)
				uniques[i] = (i + 1) * 2;

			ICustomer c;
			ITransaction t;
			List<int> usedIds = new List<int>();
			for (int cn = 0; cn < sizes.Length; ++cn)
			{
				c = new Customer();
				randomCustomerList.Add(c);
				for (int tn = 0; tn < sizes[cn].Length; ++tn)
				{
					t = new Transaction();
					usedIds.Clear();
					for (int i = 0; i < sizes[cn][tn]; ++i)
					{
						int id = random.Next() % uniqueIds;
						int starting = id;
						while (usedIds.Contains(id))
						{
							++id;
							if (id >= uniqueIds)
								id = 0;
							if (id == starting)
								throw new Exception("number of unique elements must be as great as the length of any transaction");
						}
						t.AddItem(new Item(id));
						usedIds.Add(id);
					}
					c.AddTransaction(t);
				}
			}

			return randomCustomerList;
		}
	}
}
