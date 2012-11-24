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
		static public List<Customer> execute(CustomerList list, double treshold) {
			// 1. sort the input!

			// TODO

			// 2. find all frequent 1-sequences
			Apriori apriori = new Apriori(list);
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(2);

			// 3. transform input into list of IDs

			// 3.a) give an ID to each 1-seq
			var encoding = new List<KeyValuePair<int, Litemset>>();
			var decoding = new List<KeyValuePair<Litemset, int>>();

			int i = 1;
			foreach (Litemset li in oneLitemsets) {
				encoding.Add(new KeyValuePair<int, Litemset>(i, li));
				decoding.Add(new KeyValuePair<Litemset, int>(li, i));
			}

			// 3.b) using created IDs, transform the input

			// list of lists of list of IDs:
			// - each list of IDs is a frequent itemset
			// - list of those means a list of frequent itemsets, 
			//   meaning a list of transaction represented as a list of frequent 
			//   itemsets performed by one customer
			// - outer list means list of customers
			var encodedList = new List<List<List<int>>>();



			// 4. find all frequent sequences in the input


			// 5. purge all non-maximal sequences


			return new List<Customer>();
		}

	}
}
