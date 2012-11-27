using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

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
		private static void GenerateCandidates(List<List<int>> prev, Dictionary<List<int>, int> candidates) {

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

					// Invalid statement:

					// we ain't gonna add any lists, that do not exist partially in the previous set,
					//  I tell ya!
					//for (int ic = 0; ic < candidate.Count; ++ic) {
					// nothing
					//}

					candidates.Add(candidate, 0);
				}

			}
		}

		/// <summary>
		/// Corresponds to 4th step of Apriori All algorithm, namely "Sequence Phase".
		/// </summary>
		/// <param name="oneLitemsets"></param>
		/// <param name="encoding"></param>
		/// <param name="encodedList">encoded list of customers</param>
		/// <param name="minSupport">minimum number of occurances</param>
		/// <returns>list of k-sequences, partitioned by k. i.e. i-th element of resulting List 
		/// contains all i-sequences</returns>
		private static List<List<List<int>>> FindAllFrequentSequences(List<Litemset> oneLitemsets,
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
		private static void PurgeAllNonMax(List<List<List<int>>> kSequences) {

			// additional "-1" because all largest k-sequences are for sure maximal
			for (int k = kSequences.Count - 1 - 1; k >= 0; --k) {
				List<List<int>> sequencesOfLengthK = kSequences[k];
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n) {

					List<int> sequence = sequencesOfLengthK[n];
					for (int i = k + 1; i < kSequences.Count; ++i) {

						foreach (List<int> longerSequence in kSequences[i])
							if (IsSubSequence(sequence, longerSequence)) { // if sequence is a sub-seqence of s
								// purge sequence and all its subsequences
								PurgeAllSubSeqsOf(kSequences, k, n);
								sequencesOfLengthK.RemoveAt(n);
							}

					}

				}
			}
		}

		// this method assumes that both lists are sorted!
		private static bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence)
				where T : IComparable {
			if (hyptheticalSubSequence.Count == 0)
				return true;
			if (hyptheticalSubSequence.Count > sequence.Count)
				return false;

			int i1 = 0;
			int i2 = 0;
			while (true) {
				if (i1 >= hyptheticalSubSequence.Count)
					break;
				else if (i2 >= sequence.Count)
					return false;

				if (hyptheticalSubSequence[i1].Equals(sequence[i2])) {
					// in the next move we check next element
					++i1;
				} else if (hyptheticalSubSequence[i1].CompareTo(sequence[i2]) < 0) {
					// since both lists are sorted, we cannot encounter elem i1 anywhere later, 
					//  because we know that all other further elements i2 are also larger
					return false;
				}

				++i2;
			}

			return true;
		}

		private static void PurgeAllSubSeqsOf(List<List<List<int>>> kSequences, int kk, int ii) {
			if (kk <= 1)
				return;
			List<int> sequence = kSequences[kk][ii];
			for (int k = kk - 1; k >= 0; --k) {

				List<List<int>> sequencesOfLengthK = kSequences[k];

				for (int i = 0; i < sequencesOfLengthK.Count; ++i) {
					if (IsSubSequence(sequencesOfLengthK[i], sequence)) {
						PurgeAllSubSeqsOf(kSequences, k, i);
						sequencesOfLengthK.RemoveAt(i);
					}
				}

			}
		}

		private static List<List<int>> CompactSequencesList(List<List<List<int>>> kSequences) {
			var compacted = new List<List<int>>();

			//for (int k = kSequences.Count - 1; k >= 0; --k) {
			for (int k = 0; k < kSequences.Count; ++k) {
				//for (int n = kSequences[k].Count - 1; n >= 0; --n) {
				for (int n = 0; n < kSequences[k].Count; ++n) {
					if (kSequences[k][n].Count > 0)
						//kSequences[n].RemoveAt(k);
						compacted.Add(kSequences[k][n]);
				}

				//if (kSequences[k].Count == 0)
				//   kSequences.RemoveAt(k);
			}

			return compacted;
		}

		private static List<Customer> InferRealResults(List<List<List<int>>> encodedList, List<List<List<int>>> kSequences,
				Dictionary<int, Litemset> decoding, CustomerList customerList) {
			var decodedList = new List<Customer>();

			var compactSequences = CompactSequencesList(kSequences);
			//var decodedSequences = new List<Customer>();

			foreach (List<int> sequence in compactSequences) {
				Customer c = new Customer();
				foreach (int encodedLitemset in sequence) {
					Transaction t = new Transaction(decoding[encodedLitemset].Items);
					c.Transactions.Add(t);
				}
				decodedList.Add(c);
			}

			foreach (Customer customer in decodedList) {
				for (int tn = customer.Transactions.Count - 1; tn >= 0; --tn) {
					// tn : transaction no.
					Transaction t = customer.Transactions[tn];

					foreach (Transaction comparedTransaction in customer.Transactions) {
						if (Object.ReferenceEquals(t, comparedTransaction))
							continue;
						if (t.Items.Count >= comparedTransaction.Items.Count)
							continue;
						if (IsSubSequence<Item>(t.Items, comparedTransaction.Items)) {
							customer.Transactions.RemoveAt(tn);
							break;
						}
					}
				}

			}

			return decodedList;
		}

		/// <summary>
		/// Executes Apriori All algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="list">list of customers, who have transactions that have items</param>
		/// <param name="threshold">from 0 to 1</param>
		/// <returns></returns>
		public static List<Customer> execute(CustomerList customerList, double threshold) {
			if (customerList == null)
				throw new ArgumentNullException("customerList", "customerList is null.");
			if (threshold > 1 || threshold < 0)
				throw new ArgumentException("threshold", "threshold is out of range = [0,1]");

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * threshold);

			// 1. sort the input!

			// corresponds to 1st step of Apriori All algorithm, namely "Sort Phase".

			// already done because input is sorted by the user in an apropriate way

			// 2. find all frequent 1-sequences
			Apriori apriori = new Apriori(customerList);
			// this corresponds to 2nd step of Apriori All algorithm, namely "Litemset Phase".
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(threshold);

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
			var decodedList = InferRealResults(encodedList, kSequences, decoding, customerList);

			// 7. return results
			return decodedList;
		}

	}
}
