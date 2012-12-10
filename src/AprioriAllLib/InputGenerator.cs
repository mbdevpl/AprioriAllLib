using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
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
		public static CustomerList GenerateRandomList(int customersCount, int maxTransactionCount, int maxTransactionLength)
		{
			Random random = new Random();
			CustomerList randomCustomerList = new CustomerList();

			Customer c;
			Transaction t;
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
						int itemVal = (random.Next() % maxTransactionLength + 10) * 10;
						if (t.Contains(itemVal))
						{
							--it;
							continue;
						}
						t.AddItem(itemVal);
					}
					c.AddTransaction(t);
				}
				randomCustomerList.Customers.Add(c);
			}
			return randomCustomerList;
		}

	}
}
