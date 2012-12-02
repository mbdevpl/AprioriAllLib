using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace AprioriAllLib {

	/// <summary>
	/// Apriori All Algorithm :P
	/// </summary>
	public class AprioriAllAlgorithm {

		protected static void GenerateEncoding(List<Litemset> oneLitemsets, out Dictionary<Litemset, int> encoding,
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
		protected static List<List<List<int>>> EncodeCustomerList(CustomerList customerList, List<Litemset> oneLitemsets,
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
		/// <returns>candidates for k-sequences</returns>
		protected static Dictionary<List<int>, int> GenerateCandidates(List<List<int>> prev, bool progressOutput) {
			var candidates = new Dictionary<List<int>, int>();

			int prevCount = prev.Count;
			if (prevCount == 0)
				return candidates;
			int prevLen = prev[0].Count;

			for (int i1 = 0; i1 < prevCount; ++i1) {
				List<int> l1 = prev[i1];

				for (int i2 = 0; i2 < prevCount; ++i2) {
					if (i1 == i2)
						continue;
					List<int> l2 = prev[i2];

					// check if first n-1 elements of both lists are equal
					bool partEqual = true;
					for (int i = 0; i < prevLen - 1; ++i) {
						if (!l1[i].Equals(l2[i])) {
							partEqual = false;
							break;
						}
					}
					if (!partEqual)
						continue;

					// join l1 and l2
					List<int> candidate = new List<int>(l1);
					candidate.Add(l2[prevLen - 1]);

					// we don't want to add any duplicates
					bool foundEqual = false;
					bool isEqual;
					foreach (List<int> cand in candidates.Keys) {
						isEqual = true;
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

					candidates.Add(candidate, 0);
					if (progressOutput)
						if (candidates.Count > 0 && candidates.Count % 25000 == 0)
							Console.Out.WriteLine("   {0} and counting...", candidates.Count);
				}
			}

			// build a list of candidates that haven't got all their sub-sequences
			// in set of previous k-sequences
			Dictionary<List<int>, int>.KeyCollection keys = candidates.Keys;
			List<List<int>> keysToRemove = new List<List<int>>();
			for (int ic = keys.Count - 1; ic >= 0; --ic) {
				bool invalidCandidate = false;
				for (int not = prevLen; not >= 0; --not) {
					List<int> sublist = new List<int>(keys.ElementAt(ic));
					sublist.RemoveAt(not);
					bool foundInPrevious = false;
					for (int i = 0; i <= prevCount; ++i) {
						bool foundEqual = false;
						for (int n = 0; n < prevLen; ++n) {
							if (sublist[n] == prev[i][n]) {
								foundEqual = true;
								break;
							}
						}
						if (foundEqual) {
							foundInPrevious = true;
							break;
						}
					}
					if (!foundInPrevious) {
						invalidCandidate = true;
						break;
					}
				}
				if (invalidCandidate)
					keysToRemove.Add(keys.ElementAt(ic));
			}
			if (progressOutput)
				Console.Out.WriteLine("Found {0} candidates, of which previous sequences do not contain {1}.",
					candidates.Count, keysToRemove.Count);
			//remove invalid candidates
			foreach (List<int> key in keysToRemove)
				candidates.Remove(key);

			return candidates;
		}

		protected static bool MatchCandidateToEncodedCustomer(List<int> candidate, List<List<int>> encodedCustomer) {
			//if (encodedCustomer.Count < candidate.Count)
			//   return false; // the candidate is too long to possibly match the customer

			//int maxOmit = encodedCustomer.Count - candidate.Count;
			//for (int omitted = 0; omitted <= maxOmit; ++omitted) {
			// variable omitted decides how many initial transactions are omitted when matching is perfromed,
			//  i.e. how wide is the interval put on the customer's transaction list, to which the candidate is matched
			// finding procedure fails if and only if for none of the interval lengths
			//  the match was found

			bool allNeededItemsArePresent = true;
			int prevFoundIndex = 0/*-1*/;
			for (int j = 0; j < candidate.Count; ++j) {
				int jthCandidateElem = candidate[j];
				bool jthCandidateFound = false;
				for (int i = prevFoundIndex /*+ 1*/ /*omitted*/; i </*=*/ encodedCustomer.Count /*- (candidate.Count - j)*/; ++i) {
					List<int> ithTransaction = encodedCustomer[i];
					// try to match j-th element of candidate
					//  with i-th transaction of the current customer
					if (ithTransaction.Contains(jthCandidateElem)) {
						prevFoundIndex = i;
						jthCandidateFound = true;
						break;
					}
				}
				if (!jthCandidateFound) {
					allNeededItemsArePresent = false;
					return false; //break;
				}
			}
			if (allNeededItemsArePresent)
				return true;
			//}
			return false;
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
		protected static List<List<List<int>>> FindAllFrequentSequences(List<Litemset> oneLitemsets,
			Dictionary<Litemset, int> encoding, List<List<List<int>>> encodedList, int minSupport, bool progressOutput) {
			var kSequences = new List<List<List<int>>>();

			kSequences.Add(new List<List<int>>()); // placeholder for 0-sequences (whatever it means)
			kSequences.Add(new List<List<int>>()); // 1-seq, already done, just copy:
			foreach (Litemset li in oneLitemsets) {
				var lst = new List<int>();
				lst.Add(encoding[li]);
				kSequences[1].Add(lst);
			}

			for (int k = 2; kSequences.Count >= k && kSequences[k - 1].Count > 0; ++k) {
				if (progressOutput)
					Console.Out.WriteLine("Looking for {0}-sequences...", k);
				// list of kSequences, initially empty
				kSequences.Add(new List<List<int>>());
				var prev = kSequences[k - 1];

				// generate candidates
				Dictionary<List<int>, int> candidates = GenerateCandidates(prev, progressOutput);

				// calculate support of each candidate by analyzing the whole encoded input

				Dictionary<List<int>, int>.KeyCollection keysOrig = candidates.Keys;
				List<List<int>> keys = new List<List<int>>(keysOrig);
				for (int n = 0; n < keys.Count; ++n) {
					List<int> candidate = keys[n]; // n-th candidate

					// check every customer for compatibility with the current candidate
					foreach (List<List<int>> encodedCustomer in encodedList) {

						if (MatchCandidateToEncodedCustomer(candidate, encodedCustomer)) {
							candidates[candidate] += 1;
						}

						//bool allNeededItemsArePresent = true;
						//foreach (int candidateItem in candidate) {
						//   // check all transactions, item must exist in any of them
						//   bool foundCandidateItem = false;
						//   foreach (List<int> encodedTransaction in encodedCustomer) {
						//      //if (encodedTransaction.Count < k)
						//      //	continue;
						//      foundCandidateItem = encodedTransaction.Contains(candidateItem);
						//      if (foundCandidateItem)
						//         break;
						//   }
						//   // if item does not exist in any of the transactions, this customer
						//   // is not compatible with 'candidate' sequence
						//   if (!foundCandidateItem) {
						//      allNeededItemsArePresent = false;
						//      break;
						//   }
						//}
						//if (allNeededItemsArePresent) {
						//   candidates[candidate] += 1;
						//}
					}

					// confront results with min. support
					if (candidates[candidate] >= minSupport)
						kSequences[k].Add(candidate);

				}
				if (progressOutput)
					Console.Out.WriteLine("Found {0} sequences that have sufficient support.", kSequences[k].Count);
			}

			return kSequences;
		}

		/// <summary>
		/// Deletes all non-maximal seqences from list of k-sequences.
		/// 
		/// Corresponds to 5th step of Apriori All algorithm, namely "Maximal Phase".
		/// </summary>
		/// <param name="kSequences">list of all k-sequences, partitioned by k</param>
		protected static void PurgeAllNonMax(List<List<List<int>>> kSequences) {
			if (kSequences == null || kSequences.Count == 0)
				return;
			// additional "-1" because all largest k-sequences are for sure maximal
			for (int k = kSequences.Count - 1 - 1; k >= 0; --k) {
				List<List<int>> sequencesOfLengthK = kSequences[k];
				if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
					continue;
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n) {
					// we analyze n-th k-sequence:
					List<int> sequence = sequencesOfLengthK[n];
					for (int i = k + 1; i < kSequences.Count; ++i) {

						foreach (List<int> longerSequence in kSequences[i])
							if (IsSubSequence(sequence, longerSequence)) {
								// if 'sequence' is a sub-seqence of 'longerSequence'
								PurgeAllSubSeqsOf(kSequences, k, n);
								sequencesOfLengthK.RemoveAt(n);
								break;
							}

					}
				}
			}
		}

		/// <summary>
		/// Checks if one sequence is a subsequence of the other.
		/// </summary>
		/// <typeparam name="T">type of element of both lists</typeparam>
		/// <param name="hyptheticalSubSequence">supposed sub-sequence</param>
		/// <param name="sequence">suppoed super-sequence</param>
		/// <returns></returns>
		protected static bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence)
				where T : IComparable {
			if (hyptheticalSubSequence.Count == 0)
				return true;
			if (hyptheticalSubSequence.Count > sequence.Count)
				return false;

			// below code assumes that both lists are sorted, but it shouldn't!
			//int i1 = 0;
			//int i2 = 0;
			//while (true) {
			//   if (i1 >= hyptheticalSubSequence.Count)
			//      break;
			//   else if (i2 >= sequence.Count)
			//      return false;

			//   if (hyptheticalSubSequence[i1].Equals(sequence[i2])) {
			//      // in the next move we check next element
			//      ++i1;
			//   } else if (hyptheticalSubSequence[i1].CompareTo(sequence[i2]) < 0) {
			//      // since both lists are sorted, we cannot encounter elem i1 anywhere later, 
			//      //  because we know that all other further elements i2 are also larger
			//      return false;
			//   }

			//   ++i2;
			//}

			bool found;
			int found_index = -1;
			List<T>.Enumerator en = sequence.GetEnumerator();
			foreach (T element in hyptheticalSubSequence) {
				found = false;
				//int i = found_index + 1;
				for (int i = found_index + 1; en.MoveNext() /*&& i < sequence.Count*/; ++i) {
					T e = en.Current;
					//en.MoveNext();
					//T e = sequence[i];
					if (element.Equals(e)) {
						found = true;
						found_index = i;
						break;
					}
				}
				if (!found)
					return false;
			}

			return true;

		}

		protected static void PurgeAllSubSeqsOf(List<List<List<int>>> kSequences, int kk, int ii) {
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

		/// <summary>
		/// Concatenates all lists of k-sequences into a single list. Does nothing else.
		/// </summary>
		/// <param name="kSequences">k-sequences, partitioned with respect to k</param>
		/// <returns>not partitioned k-sequences list</returns>
		protected static List<List<int>> CompactSequencesList(List<List<List<int>>> kSequences) {
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

		protected static List<Customer> InferRealResults(List<List<List<int>>> encodedList, List<List<List<int>>> kSequences,
				Dictionary<int, Litemset> decoding, CustomerList customerList) {
			var decodedList = new List<Customer>();

			var compactSequences = CompactSequencesList(kSequences);

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

			// compare each pair of customers for equality
			List<int> duplicateCustomers = new List<int>();
			for (int i1 = 0; i1 < decodedList.Count; i1++) {
				Customer customer1 = decodedList[i1];
				for (int i2 = i1 + 1; i2 < decodedList.Count; i2++) {
					//if (i1 == i2)
					//	continue;
					Customer customer2 = decodedList[i2];
					if (customer1.Transactions.Count != customer2.Transactions.Count)
						continue;
					if (customer1.Equals(customer2)) {
						duplicateCustomers.Add(i1);
						break;
					}
				}
			}
			for (int index = duplicateCustomers.Count - 1; index >= 0; --index)
				decodedList.RemoveAt(duplicateCustomers[index]);

			return decodedList;
		}
		/// <summary>
		/// Executes Apriori All algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="customerList">list of customers, who have transactions that have items</param>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public static List<Customer> Execute(CustomerList customerList, double threshold) {
			return Execute(customerList, threshold, false);
		}

		/// <summary>
		/// Executes Apriori All algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="customerList">list of customers, who have transactions that have items</param>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public static List<Customer> Execute(CustomerList customerList, double threshold, bool progressOutput) {
			if (customerList == null)
				throw new ArgumentNullException("customerList", "customerList is null.");
			if (threshold > 1 || threshold <= 0)
				throw new ArgumentException("threshold", "threshold is out of range = (0,1]");

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * threshold);
			if (progressOutput)
				Console.Out.WriteLine("Threshold = {0}  =>  Minimum support = {1}", threshold, minSupport);

			// 1. sort the input!
			if (progressOutput)
				Console.Out.WriteLine("1) Sort Phase - list is already sorted as user sees fit");

			// corresponds to 1st step of Apriori All algorithm, namely "Sort Phase".

			// already done because input is sorted by the user in an apropriate way

			// 2. find all frequent 1-sequences
			if (progressOutput)
				Console.Out.WriteLine("2) Litemset Phase");
			if (progressOutput)
				Console.Out.WriteLine("Launching Apriori...");
			Apriori apriori = new Apriori(customerList);
			// this corresponds to 2nd step of Apriori All algorithm, namely "Litemset Phase".
			List<Litemset> oneLitemsets = apriori.FindOneLitemsets(threshold);
			if (progressOutput) {
				Console.Out.WriteLine("Litemsets:");
				foreach (Litemset l in oneLitemsets)
					Console.Out.WriteLine(" - {0}", l);
			}

			// 3. transform input into list of IDs
			if (progressOutput)
				Console.Out.WriteLine("3) Transformation Phase");

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

			if (progressOutput)
				Console.Out.WriteLine("Encoding input data...");
			var encodedList = EncodeCustomerList(customerList, oneLitemsets, encoding);

			// 4. find all frequent sequences in the input
			if (progressOutput)
				Console.Out.WriteLine("4) Sequence Phase");

			if (progressOutput)
				Console.Out.WriteLine("Searching for all possible k-sequences");
			var kSequences = FindAllFrequentSequences(oneLitemsets, encoding, encodedList, minSupport, progressOutput);
			if (progressOutput)
				Console.Out.WriteLine("Maximal k is {0}.", kSequences.Count - 2);

			// 5. purge all non-maximal sequences
			if (progressOutput)
				Console.Out.WriteLine("5) Maximal Phase");

			if (progressOutput)
				Console.Out.WriteLine("Purging all non-maximal sequences...");
			PurgeAllNonMax(kSequences);

			// 6. decode results
			if (progressOutput)
				Console.Out.WriteLine("Decoding results and purging again...");
			var decodedList = InferRealResults(encodedList, kSequences, decoding, customerList);

			// 7. return results
			return decodedList;
		}

	}
}
