using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {

	/// <summary>
	/// Apriori All Algorithm :P
	/// </summary>
	public class AprioriAllAlgorithm {

		static private void GenerateEncoding(List<Litemset> oneLitemsets, out Dictionary<Litemset, int> encoding,
				out Dictionary<int, Litemset> decoding) {
			encoding = new Dictionary<Litemset, int>();
			decoding = new Dictionary<int, Litemset>();

			int i = 1;
			foreach (Litemset li in oneLitemsets) {
				encoding.Add(li, i);
				decoding.Add(i, li);
				++i;
			}

		}

		/// <summary>
		/// Corresponds to 3rd step of Apriori All algorithm, namely "Transformation Phase".
		/// </summary>
		/// <param name="customerList"></param>
		/// <param name="oneLitemsets"></param>
		/// <param name="encoding"></param>
		/// <param name="?"></param>
		/// <returns>encoded customer list</returns>
		static private List<List<List<int>>> EncodeCustomerList(CustomerList customerList, List<Litemset> oneLitemsets,
				Dictionary<Litemset, int> encoding) {
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

			return encodedList;
		}

		/// <summary>
		/// Generates all candidates for k-sequences, basing on set of (k-1)-sequences.
		/// </summary>
		/// <param name="prev">previous k-sequences, i.e. (k-1)-sequences</param>
		/// <param name="candidates">output, candidates for k-sequences</param>
		static private void GenerateCandidates(List<List<int>> prev, Dictionary<List<int>, int> candidates) {

			//for (List<List<int>>.Enumerator e1 = prev.GetEnumerator(); e1.MoveNext(); ) {
			//   // prev in kSequences[k - 1]
			//   List<int> l1 = e1.Current;
			//   for (List<List<int>>.Enumerator e2 = prev.GetEnumerator(); e2.MoveNext(); ) {
			//      // we discard candidates like (1 1), (2 2), ... (k k), ... , (1 2 1 2),
			//      // in general, all (a b .. k a b .. k)
			//      if (e1.Equals(e2))
			//         continue;
			//      List<int> l2 = e2.Current;
			//      int differentValue = -1;
			//      int diff = 0;
			//      foreach (int i1 in l1) {
			//         if (!l2.Contains(i1)) {
			//            ++diff;
			//            if (diff > 1)
			//               break;
			//            differentValue = i1;
			//         }
			//      }
			//      // we cannot form candidates from lists that differ in more than one element
			//      if (diff != 1)
			//         continue;
			//   }
			//}

			for (int i1 = 0; i1 < prev.Count; ++i1) {
				List<int> l1 = prev[i1];

				for (int i2 = i1 + 1; i2 < prev.Count; ++i2) {
					List<int> l2 = prev[i2];

					int differentValue = -1;
					int diff = 0;
					foreach (int elem1 in l1) {
						if (!l2.Contains(elem1)) {
							++diff;
							if (diff > 1)
								break;
							differentValue = elem1;
						}
					}

					// we cannot form candidates from lists that differ in more than one element
					if (diff != 1)
						continue;

					List<int> candidate = new List<int>(l2);
					candidate.Add(differentValue);
					candidate.Sort();

					// we don't want to add any duplicates
					bool foundEqual = false;
					foreach (List<int> cand in candidates.Keys)
						if (cand.Count == candidate.Count) {
							bool isEqual = true;
							for (int icc = 0; icc < candidate.Count; ++icc)
								if (cand[icc] != candidate[icc]) {
									isEqual = false;
									break;
								}
							if (isEqual) {
								foundEqual = true;
								break;
							}
						}
					if (foundEqual)
						continue;

					// we ain't gonna add any lists, that do not exist partially in the previous set,
					//  I tell ya!

					for (int ic = 0; ic < candidate.Count; ++ic) {
						// TODO
					}

					candidates.Add(candidate, 0);
				}

			}
		}

		/// <summary>
		/// Corresponds to 4th step of Apriori All algorithm, namely "Sequence Phase".
		/// </summary>
		/// <param name="oneLitemsets"></param>
		/// <param name="encoding"></param>
		/// <param name="encodedList"></param>
		/// <param name="minSupport"></param>
		/// <param name="kSequences"></param>
		static private List<List<List<int>>> FindAllFrequentSequences(List<Litemset> oneLitemsets,
			Dictionary<Litemset, int> encoding, List<List<List<int>>> encodedList, int minSupport) {
			var kSequences = new List<List<List<int>>>();

			kSequences.Add(new List<List<int>>()); // placeholder for 0-sequences (whatever it means)
			kSequences.Add(new List<List<int>>()); // 1-seq, already done, just copy:
			foreach (Litemset li in oneLitemsets) {
				var lst = new List<int>();
				lst.Add(encoding[li]);
				kSequences[1].Add(lst);
			}

			for (int k = 2; kSequences.Count >= k && kSequences[k - 1].Count > 0; ++k) {
				// list of kSequences, initially empty
				kSequences.Add(new List<List<int>>());
				var prev = kSequences[k - 1];

				// generate candidates
				var candidates = new Dictionary<List<int>, int>();

				GenerateCandidates(prev, candidates);

				// calculate support of each candidate by analyzing the whole encoded input

				Dictionary<List<int>, int>.KeyCollection keysOrig = candidates.Keys;
				List<List<int>> keys = new List<List<int>>(keysOrig);
				for (int i = 0; i < keys.Count; ++i) {
					List<int> candidate = keys[i];

					// check every customer for compatibility with the candidate
					foreach (List<List<int>> encodedCustomer in encodedList) {

						bool allNeededItemsArePresent = true;
						foreach (int candidateItem in candidate) {

							// check all transactions, item must exist in any of them
							bool foundCandidateItem = false;
							foreach (List<int> encodedTransaction in encodedCustomer) {
								//if (encodedTransaction.Count < k)
								//	continue;
								foundCandidateItem = encodedTransaction.Contains(candidateItem);
								if (foundCandidateItem)
									break;
							}

							// if item does not exist in any of the transactions, this customer
							// is not compatible with 'candidate' sequence
							if (!foundCandidateItem) {
								allNeededItemsArePresent = false;
								break;
							}
						}

						if (allNeededItemsArePresent) {
							candidates[candidate] += 1;
						}
					}

					// confront results with min. support
					if (candidates[candidate] >= minSupport)
						kSequences[k].Add(candidate);

				}

			}

			return kSequences;
		}

		/// <summary>
		/// Deletes all non-maximal seqences from list of k-sequences.
		/// 
		/// Corresponds to 5th step of Apriori All algorithm, namely "Maximal Phase".
		/// </summary>
		/// <param name="kSequences"></param>
		static private void PurgeAllNonMax(List<List<List<int>>> kSequences) {

			// additional "-1" because all largest k-sequences are for sure maximal
			for (int i = kSequences.Count - 1 - 1; i >= 0; ++i) {

			}
		}

		/// <summary>
		/// Executes Apriori All algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="list">list of customers, who have transactions that have items</param>
		/// <param name="threshold">from 0 to 1</param>
		/// <returns></returns>
		static public List<Customer> execute(CustomerList customerList, double threshold) {

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * threshold);

			// 1. sort the input!

			// corresponds to 1st step of Apriori All algorithm, namely "Sort Phase".

			// already done because input is sorted by the user in an apropriate way

			// 2. find all frequent 1-sequences
			Apriori apriori = new Apriori(customerList);
			// this corresponds to 2nd step of Apriori All algorithm, namely "Litemset Phase".
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(minSupport);

			// 3. transform input into list of IDs

			// 3.a) give an ID to each 1-seq
			Dictionary<Litemset, int> encoding;
			Dictionary<int, Litemset> decoding;

			GenerateEncoding(oneLitemsets, out encoding, out decoding);

			// 3.b) using created IDs, transform the input

			// list of lists of list of IDs:
			// - each list of IDs is a frequent itemset
			// - list of those means a list of frequent itemsets, 
			//   meaning a list of transaction represented as a list of frequent 
			//   itemsets performed by one customer
			// - outer list means list of customers

			var encodedList = EncodeCustomerList(customerList, oneLitemsets, encoding);

			// 4. find all frequent sequences in the input

			var kSequences = FindAllFrequentSequences(oneLitemsets, encoding, encodedList, minSupport);

			// 5. purge all non-maximal sequences

			PurgeAllNonMax(kSequences);

			// 6. decode results
			var decodedList = new List<Customer>();

			// 7. return results
			return decodedList;
		}

	}
}
