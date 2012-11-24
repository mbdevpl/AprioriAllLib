using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {
	public class AprioriAllAlgorithm {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list">list of customers, who have transactions that have items</param>
		/// <param name="treshold">from 0 to 1</param>
		/// <returns></returns>
		static public List<Customer> execute(CustomerList customerList, double treshold) {
			// 1. sort the input!

			// TODO

			// 2. find all frequent 1-sequences
			Apriori apriori = new Apriori(customerList);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(2);

			// 3. transform input into list of IDs

			// 3.a) give an ID to each 1-seq
			var encoding = new Dictionary<Litemset, int>();
			var decoding = new Dictionary<int, Litemset>();

			{
				int i = 1;
				foreach (Litemset li in oneLitemsets) {
					encoding.Add(li, i);
					decoding.Add(i, li);
					++i;
				}
			}

			// 3.b) using created IDs, transform the input

			// list of lists of list of IDs:
			// - each list of IDs is a frequent itemset
			// - list of those means a list of frequent itemsets, 
			//   meaning a list of transaction represented as a list of frequent 
			//   itemsets performed by one customer
			// - outer list means list of customers
			var encodedList = new List<List<List<int>>>();

			foreach (Customer c in customerList.Customers) {
				var encodedCustomer = new List<List<int>>();

				foreach (Transaction t in c.Transactions) {
					var encodedTransaction = new List<int>();
					int id = -1; // temp. variable used as in-out param

					// adding litemsets of length >= 2
					foreach (Litemset li in oneLitemsets) {
						if (li.Items.Count == 1)
							continue;
						bool someMissing = false;
						foreach (Item litem in li.Items) {
							if (!t.Items.Contains(litem)) {
								someMissing = true;
								break;
							}
						}

						if (!someMissing) {
							if (encoding.TryGetValue(li, out id))
								encodedTransaction.Add(id);
						}

					}

					foreach (Item i in t.Items) {

						// adding litemsets of length == 1
						foreach (Litemset li in oneLitemsets) {
							if (li.Items.Count > 1)
								continue;
							Item item = li.Items[0];

							if (item.Equals(i)) {
								if (encoding.TryGetValue(li, out id))
									encodedTransaction.Add(id);
							}
						}

					}

					if (encodedTransaction.Count > 0)
						encodedCustomer.Add(encodedTransaction);
				}

				if (encodedCustomer.Count > 0)
					encodedList.Add(encodedCustomer);
			}

			// 4. find all frequent sequences in the input

			var kSequences = new List<List<Litemset>>();
			kSequences.Add(new List<Litemset>()); // placeholder for 0-sequences (whatever it means)
			kSequences.Add(oneLitemsets); // already done

			for (int k = 2; kSequences.Count >= k && kSequences[k - 1].Count > 0; ++k) {
				var candidates = new Dictionary<List<int>,int>();

				// TODO: generate candidates

				foreach (KeyValuePair<List<int>, int> candidate in candidates) {

					foreach (List<List<int>> encodedCustomer in encodedList) {

						foreach (List<int> encodedTransaction in encodedCustomer) {
							if (encodedTransaction.Count < k)
								continue;

							bool allNeededItemsArePresent = true;
							foreach (int candidateItem in candidate.Key) {
								if (!encodedTransaction.Contains(candidateItem)) {
									allNeededItemsArePresent = false;
									break;
								}
							}

							if (allNeededItemsArePresent) {
								candidates[candidate.Key] += 1;
							}
						}

					}

				}

				// TODO: confront results with min. support
			}

			// 5. purge all non-maximal sequences


			return new List<Customer>();
		}

	}
}
