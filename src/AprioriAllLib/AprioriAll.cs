using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenCL.Net;
using System.Diagnostics;

namespace AprioriAllLib
{
	/// <summary>
	/// AprioriAll algorithm implementation. 
	/// </summary>
	public class AprioriAll : Apriori
	{

		/// <summary>
		/// Constructs a new AprioriAll instance.
		/// </summary>
		/// <param name="customerList">list of customers, who have transactions that have items</param>
		public AprioriAll(CustomerList customerList)
			: base(customerList)
		{
		}

		/// <summary>
		/// Generates encoding and decoding dictionaries for a given set of 1-sequences (large itemsets).
		/// </summary>
		/// <param name="oneLitemsets">set of 1-sequences (large itemsets)</param>
		/// <param name="encoding">encoding dictionary</param>
		/// <param name="decoding">decoding dictionary</param>
		protected void GenerateEncoding(List<Litemset> oneLitemsets, out Dictionary<Litemset, int> encoding,
			out Dictionary<int, Litemset> decoding)
		{
			encoding = new Dictionary<Litemset, int>();
			decoding = new Dictionary<int, Litemset>();

			int i = 1;
			foreach (Litemset li in oneLitemsets)
			{
				encoding.Add(li, i);
				decoding.Add(i, li);
				++i;
			}

		}

		/// <summary>
		/// Generates a dictionary that remembers for every litemset x: litemsets that contain x.
		/// 
		/// For example:
		/// if {milk} => 1, {water} => 2, {milk, water} => 3, {milk, water, juice} => 4
		/// then 1 is-contained-in {3, 4}, 2 is-contained-in {3, 4}, 3 is-contained-in {4}
		/// </summary>
		/// <param name="oneLitemsets">large itemests (i.e. 1-sequences)</param>
		/// <param name="encoding">encoding dictionary for the </param>
		/// <returns></returns>
		protected Dictionary<int, List<int>> GenerateContainmentRules(List<Litemset> oneLitemsets,
			Dictionary<Litemset, int> encoding)
		{
			Dictionary<int, List<int>> litemsetsContaining = new Dictionary<int, List<int>>();

			foreach (Litemset l in oneLitemsets)
			{
				foreach (Litemset other in oneLitemsets)
				{
					if (ReferenceEquals(other, l))
						continue;

					if (l.Items.All(item => other.Items.Contains(item)))
					{
						int encoded = encoding[l];
						if (!litemsetsContaining.ContainsKey(encoded))
							litemsetsContaining[encoded] = new List<int>();
						int otherEncoded = encoding[other];
						if (!litemsetsContaining[encoded].Contains(otherEncoded))
							litemsetsContaining[encoded].Add(otherEncoded);
					}
				}
			}

			return litemsetsContaining;
		}

		/// <summary>
		/// Corresponds to 3rd step of Apriori All algorithm, namely "Transformation Phase".
		/// </summary>
		/// <param name="oneLitemsets">1-sequences, i.e. large itemsets</param>
		/// <param name="encoding">an encoding dictionary</param>
		/// <returns>encoded customer list</returns>
		protected List<List<List<int>>> EncodeCustomerList(List<Litemset> oneLitemsets, Dictionary<Litemset, int> encoding)
		{
			var encodedList = new List<List<List<int>>>();

			foreach (Customer c in customerList.Customers)
			{
				var encodedCustomer = new List<List<int>>();

				foreach (Transaction t in c.Transactions)
				{
					var encodedTransaction = new List<int>();
					int id = -1; // temp. variable used as in-out param

					// adding litemsets of length >= 2
					foreach (Litemset li in oneLitemsets)
					{
						if (li.Items.Count == 1)
							continue;
						bool someMissing = false;
						foreach (Item litem in li.Items)
						{
							if (!t.Items.Contains(litem))
							{
								someMissing = true;
								break;
							}
						}

						if (!someMissing)
						{
							if (encoding.TryGetValue(li, out id))
								encodedTransaction.Add(id);
						}

					}

					foreach (Item i in t.Items)
					{

						// adding litemsets of length == 1
						//for (int index = oneLitemsets.Count - 1; index >= 0; --index)
						foreach (Litemset li in oneLitemsets)
						{
							//Litemset li = oneLitemsets[index];
							if (li.Items.Count > 1)
								continue;
							Item item = li.Items[0];

							if (item.Equals(i) && encoding.TryGetValue(li, out id))
								encodedTransaction.Insert(0, id);
						}

					}

					if (encodedTransaction.Count > 0)
					{
						encodedTransaction.Sort();
						encodedCustomer.Add(encodedTransaction);
					}
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
		/// <param name="litemsetCount">number of distinct litemsets</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>candidates for k-sequences</returns>
		protected Dictionary<List<int>, int> GenerateCandidates(List<List<int>> prev, int litemsetCount, bool progressOutput)
		{
			var candidates = new Dictionary<List<int>, int>();

			int prevCount = prev.Count;
			if (prevCount == 0)
				return candidates;
			int prevLen = prev[0].Count;

			Stopwatch generationWatch = new Stopwatch();

			generationWatch.Start();
			{
				RadixTree withoutFirst = new RadixTree(litemsetCount + 1);
				foreach (List<int> prevList in prev)
					withoutFirst.TryAdd(prevList, 0, true);
				//if (!withoutFirst.TryAdd(prevList, 0, true))
				//throw new ArgumentException("error in previous candidate list","prev");

				foreach (List<int> prevList in prev)
				{
					RadixTreeNode node = withoutFirst.GetNode(prevList, prevLen - 1);
					if (node != null)
					{
						foreach (int value in node.Values)
						{
							if (prevLen == 1 && value == prevList[0])
								continue;
							List<int> newCandidate = new List<int>(prevList);
							newCandidate.Insert(0, value);
							candidates.Add(newCandidate, 0);

							if (progressOutput)
								if (candidates.Count > 0 && candidates.Count % 50000 == 0)
									Trace.WriteLine(String.Format("   {0} and counting...", candidates.Count));
						}
					}
				}
			}
			generationWatch.Stop();

			//generationWatch.Start();
			//var enum1 = prev.GetEnumerator();
			//for (int i1 = 0; i1 < prevCount; ++i1)
			//{
			//	enum1.MoveNext();
			//	List<int> l1 = enum1.Current; //prev[i1];

			//	var enum2 = prev.GetEnumerator();
			//	for (int i2 = 0; i2 < prevCount; ++i2)
			//	{
			//		enum2.MoveNext();
			//		if (i1 == i2)
			//			continue;
			//		List<int> l2 = enum2.Current; //prev[i2];

			//		// check if last n-1 elements of first list sequence
			//		//  are equal to the first n-1 elements of the 2nd list
			//		bool partEqual = true;
			//		//var l1enum = l1.GetEnumerator();
			//		//var l2enum = l2.GetEnumerator();
			//		//l1enum.MoveNext();
			//		for (int i = 0; i < prevLen - 1; ++i)
			//		{
			//			//l1enum.MoveNext();
			//			//l2enum.MoveNext();
			//			if (!l1[i + 1].Equals(l2[i]))
			//			//if (!l1enum.Current.Equals(l2enum.Current))
			//			{
			//				partEqual = false;
			//				break;
			//			}
			//		}
			//		if (!partEqual)
			//			continue;

			//		// join l1 and l2
			//		List<int> candidate = new List<int>(l1);
			//		candidate.Add(l2[prevLen - 1]);

			//		candidates.Add(candidate, 0);
			//		if (progressOutput)
			//			if (candidates.Count > 0 && candidates.Count % 50000 == 0)
			//				Trace.WriteLine(String.Format("   {0} and counting...", candidates.Count));
			//	}
			//}
			//generationWatch.Stop();

			if (progressOutput)
				Trace.Write(String.Format("Found {0} candidates", candidates.Count));

			if (candidates.Count == 0 || prevLen == 1)
			{
				if (progressOutput)
					Trace.WriteLine(".");
				return candidates;
			}
			if (progressOutput)
				Trace.Write(",");

			RadixTree radixTree = new RadixTree(litemsetCount + 1); // litemsets IDs are starting from 1

			Stopwatch sw3 = new Stopwatch();
			sw3.Start();

			foreach (List<int> onePrev in prev)
				if (!radixTree.TryAdd(onePrev))
					throw new ArgumentException("found duplicates in previous k-sequences", "prev");

			sw3.Stop();

			Stopwatch sw2 = new Stopwatch();
			sw2.Start();

			// build a list of candidates that haven't got all their sub-sequences
			// in set of previous k-sequences
			Dictionary<List<int>, int>.KeyCollection keys = candidates.Keys;
			var keysEnum = keys.GetEnumerator();
			List<List<int>> keysToRemove = new List<List<int>>();
			for (int ic = keys.Count - 1; ic >= 0; --ic)
			{
				keysEnum.MoveNext();
				List<int> currentList = keysEnum.Current;

				//if (currentList.Count == 3 && currentList.SequenceEqual(new int[] { 11, 5, 8 }))
				//	ic = ic;

				bool invalidCandidate = false;
				for (int not = prevLen; not >= 0; --not)
				{
					//List<int> sublist = new List<int>(keys.ElementAt(ic));
					//sublist.RemoveAt(not);

					if (!radixTree.Check(currentList, not))
						continue; // if it cannot be added, then it exists in the tree

					invalidCandidate = true;
					break;
				}
				if (invalidCandidate)
					keysToRemove.Add(currentList);
				if (progressOutput)
					if (ic > 0 && ic % 50000 == 0)
						Trace.WriteLine(String.Format("   {0} remaining...", ic));
			}

			sw2.Stop();

			//remove invalid candidates
			foreach (List<int> key in keysToRemove)
				candidates.Remove(key);

			if (progressOutput)
			{
				Trace.WriteLine(String.Format(" {0} valid, previous sequences did not contain {1}.",
					candidates.Count, keysToRemove.Count));
				Trace.WriteLine(String.Format(" generation: {0}ms, prev-to-tree: {1}ms, containment check: {2}ms",
					generationWatch.ElapsedMilliseconds, sw3.ElapsedMilliseconds, sw2.ElapsedMilliseconds));
			}

			return candidates;
		}

		/// <summary>
		/// Tries to match the candidate k-sequence to an encoded customer, and returns true if 
		/// the customer contains a candidate. That means that the configuration of the transactions of the customer 
		/// is such that it supports the candidate sequence.
		/// </summary>
		/// <param name="candidate">a candidate k-sequence</param>
		/// <param name="encodedCustomer">encoded customer, i.e. a sequence of sets of items</param>
		/// <returns>true if the customer supports the candidate</returns>
		protected bool MatchCandidateToEncodedCustomer(List<int> candidate, List<List<int>> encodedCustomer)
		{
			//if (encodedCustomer.Count < candidate.Count)
			//   return false; // the candidate is too long to possibly match the customer

			bool allNeededItemsArePresent = true;
			int prevFoundIndex = 0/*-1*/;
			List<int> usedProductsFromCurrentCandidate = new List<int>();
			for (int j = 0; j < candidate.Count; ++j)
			{
				int jthCandidateElem = candidate[j];
				bool jthCandidateFound = false;
				for (int i = prevFoundIndex /*+ 1*/ /*omitted*/; i </*=*/ encodedCustomer.Count /*- (candidate.Count - j)*/; ++i)
				{
					List<int> ithTransaction = encodedCustomer[i];
					// try to match j-th element of candidate
					//  with i-th transaction of the current customer
					bool foundJthItemInIthTransaction = false;
					for (int k = 0; k < ithTransaction.Count; ++k)
					{
						if (usedProductsFromCurrentCandidate.Contains(k))
							continue;
						if (ithTransaction[k] == jthCandidateElem)
						{
							foundJthItemInIthTransaction = true;
							prevFoundIndex = i;
							usedProductsFromCurrentCandidate.Add(k);
							jthCandidateFound = true;
							break;
						}
					}
					if (foundJthItemInIthTransaction)
						break;
					//if (ithTransaction.Contains(jthCandidateElem))
					//{
					//	prevFoundIndex = i;
					//	jthCandidateFound = true;
					//	break;
					//}
					usedProductsFromCurrentCandidate.Clear();
				}
				if (!jthCandidateFound)
				{
					allNeededItemsArePresent = false;
					break; //return false;
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
		/// <param name="oneLitemsets">1-sequences</param>
		/// <param name="encoding">encoding dictionary</param>
		/// <param name="encodedList">encoded list of customers</param>
		/// <param name="minSupport">minimum number of occurances</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of k-sequences, partitioned by k. i.e. i-th element of resulting List 
		/// contains all i-sequences</returns>
		protected List<List<List<int>>> FindAllFrequentSequences(List<Litemset> oneLitemsets,
			Dictionary<Litemset, int> encoding, List<List<List<int>>> encodedList, int minSupport, bool progressOutput)
		{
			var kSequences = new List<List<List<int>>>();

			kSequences.Add(new List<List<int>>()); // placeholder for 0-sequences (whatever it means)
			kSequences.Add(new List<List<int>>()); // 1-seq, already done, just copy:
			foreach (Litemset li in oneLitemsets)
			{
				var lst = new List<int>();
				lst.Add(encoding[li]);
				kSequences[1].Add(lst);
			}

			for (int k = 2; kSequences.Count >= k && kSequences[k - 1].Count > 0; ++k)
			{
				if (progressOutput)
					Trace.WriteLine(String.Format("Looking for {0}-sequences...", k));
				// list of kSequences, initially empty
				kSequences.Add(new List<List<int>>());
				var prev = kSequences[k - 1];

				// generate candidates
				Dictionary<List<int>, int> candidates = GenerateCandidates(prev, oneLitemsets.Count, progressOutput);

				// calculate support of each candidate by analyzing the whole encoded input

				Dictionary<List<int>, int>.KeyCollection keysOrig = candidates.Keys;
				List<List<int>> keys = new List<List<int>>(keysOrig);
				for (int n = 0; n < keys.Count; ++n)
				{
					List<int> candidate = keys[n]; // n-th candidate

					// check every customer for compatibility with the current candidate
					foreach (List<List<int>> encodedCustomer in encodedList)
					{

						if (MatchCandidateToEncodedCustomer(candidate, encodedCustomer))
						{
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
					Trace.WriteLine(String.Format(" Found {0} sequences that have sufficient support.", kSequences[k].Count));
			}

			return kSequences;
		}

		protected bool PurgeUsingInclusionRules(List<List<List<int>>> kSequences, Dictionary<int, List<int>> containmentRules,
			bool progressOutput)
		{
			if (kSequences == null || kSequences.Count == 0)
				return false;
			int initialK = kSequences.Count - 1
				- 1 // additional "-1" because all largest k-sequences are for sure maximal
				- 1; // additional "-1" because the last entry in the list is empty
			if (kSequences[kSequences.Count - 1].Count > 0)
				throw new ArgumentException("last entry of kSequences is supposed to be empty", "kSequences");

			Stopwatch watchForEachK = new Stopwatch();
			bool somethingChanged = false;
			int totalRemoved = 0;

			for (int k = 1; k <= initialK; ++k)
			{
				watchForEachK.Restart();
				List<List<int>> sequencesOfLengthK = kSequences[k];
				if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
					continue;
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
				{
					// we analyze n-th k-sequence:
					List<int> sequence = sequencesOfLengthK[n];
					// compare it to every k+1-sequence:
					foreach (List<int> longerSequence in kSequences[k + 1])
						if (IsSubSequence(sequence, longerSequence, containmentRules))
						{
							// 'sequence' is a sub-seqence of 'longerSequence'
							sequencesOfLengthK.RemoveAt(n);
							somethingChanged = true;
							++totalRemoved;
							break;
						}
				}
				watchForEachK.Stop();
				if (progressOutput)
					Trace.Write(String.Format(" k={0}: {1}ms,", k, watchForEachK.ElapsedMilliseconds));
			}
			if (progressOutput)
				Trace.Write(String.Format(" removed {0} non-maximal in total", totalRemoved));
			//for (int k = initialK; k >= 0; --k)
			//{
			//	List<List<int>> sequencesOfLengthK = kSequences[k];
			//	if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
			//		continue;
			//	// we need a flag not to fall behind allowed memebers
			//	bool removedAny = false;
			//	for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
			//	{
			//		// we analyze n-th k-sequence:
			//		List<int> sequence = sequencesOfLengthK[n];
			//		for (int i = k + 1; i < kSequences.Count; ++i)
			//		{
			//			foreach (List<int> longerSequence in kSequences[i])
			//			{
			//				//if (n < 0)
			//				//	break;
			//				//if (i == 5
			//				//	&& sequence.Count == 1
			//				//	//&& sequence.Contains(4) && sequence.Contains(8) 
			//				//	&& sequence.Contains(5)
			//				//	//&& !sequence.Contains(17)
			//				//	&& longerSequence.Count == 2
			//				//	&& longerSequence.Contains(1) && longerSequence.Contains(5)
			//				//	//&& longerSequence.Contains(9)
			//				//	//&& longerSequence.Contains(7) && longerSequence.Contains(17)
			//				//	)
			//				//	i = i;
			//				if (IsSubSequence(sequence, longerSequence, containmentRules))
			//				{
			//					// if 'sequence' is a sub-seqence of 'longerSequence'
			//					PurgeAllSubSeqsOf(kSequences, k, n);
			//					sequencesOfLengthK.RemoveAt(n);
			//					--n;
			//					removedAny = true;
			//					break;
			//				}
			//			}
			//			if (removedAny)
			//				break;
			//		}
			//		if (removedAny)
			//		{
			//			somethingChanged = true;
			//			++n;
			//			removedAny = false;
			//		}
			//	}
			//}
			return somethingChanged;
		}

		protected bool PurgeUsingInclusionRulesWithinSameSize(List<List<List<int>>> kSequences,
			Dictionary<int, List<int>> containmentRules)
		{
			if (kSequences == null || kSequences.Count == 0)
				return false;
			int initialK = kSequences.Count - 1
				- 1 // additional "-1" because all largest k-sequences are for sure maximal
				- 1; // additional "-1" because the last entry in the list is empty
			if (kSequences[kSequences.Count - 1].Count > 0)
				throw new ArgumentException("last entry of kSequences is supposed to be empty", "kSequences");

			bool somethingChanged = false;

			for (int k = initialK; k >= 0; --k)
			{
				List<List<int>> sequencesOfLengthK = kSequences[k];
				if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
					continue;

				for (int n1 = sequencesOfLengthK.Count - 1; n1 >= 0; --n1)
				{
					var sequence = sequencesOfLengthK[n1];
					for (int n2 = n1 - 1; n2 >= 0; --n2)
					{
						var maybeSubsequence = sequencesOfLengthK[n2];
						if (IsSubSequence(maybeSubsequence, sequence, containmentRules))
						{
							sequencesOfLengthK.RemoveAt(n2);
							--n1;
							somethingChanged = true;
						}
					}
				}
			}

			return somethingChanged;
		}

		protected bool PurgeUsingSequenceInnerRedundancy(List<List<List<int>>> kSequences,
			Dictionary<int, List<int>> containmentRules)
		{
			bool somethingChanged = false;
			// remove elements contained in other elements of the same maximal sequence
			foreach (List<List<int>> seqs in kSequences)
			{
				foreach (List<int> seq in seqs)
				{
					for (int n = seq.Count - 1; n >= 0; --n)
					{
						var currentLitem = seq[n];
						if (!containmentRules.ContainsKey(currentLitem))
							continue;

						var litemsContainingCurrentLitem = containmentRules[currentLitem];
						if (n > 0 && litemsContainingCurrentLitem.Contains(seq[n - 1]))
						{
							// current element is contained in the earlier element
							seq.RemoveAt(n);
							if (n <= seq.Count - 1)
								++n;
							somethingChanged = true;
						}
						else if (n < seq.Count - 1 && litemsContainingCurrentLitem.Contains(seq[n + 1]))
						{
							// current element is contained in the later element
							seq.RemoveAt(n);
							++n;
							somethingChanged = true;
						}
					}
				}
			}

			if (!somethingChanged)
				return false;

			// rearrange sequences to reflect recent changes
			// at the same time removes the duplicate sequences created by removal process
			for (int k = kSequences.Count - 2; k >= 0; --k)
			{
				List<List<int>> sequencesOfLengthK = kSequences[k];
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
				{
					List<int> sequence = sequencesOfLengthK[n];
					int sequenceCount = sequence.Count;
					if (sequenceCount < k)
					{
						bool alreadyExists = false;
						foreach (List<int> existingSequence in kSequences[sequenceCount])
							if (IsSubSequence(sequence, existingSequence))
							{
								alreadyExists = true;
								break;
							}
						if (!alreadyExists)
							kSequences[sequenceCount].Add(sequence);
						sequencesOfLengthK.RemoveAt(n);
					}
				}
			}

			return true;

		}

		/// <summary>
		/// Deletes all non-maximal seqences from list of k-sequences.
		/// 
		/// Corresponds to 5th step of Apriori All algorithm, namely "Maximal Phase".
		/// </summary>
		/// <param name="kSequences">list of all k-sequences, partitioned by k</param>
		/// <param name="containmentRules">rules of inclusion between encoded litemsets</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		protected void PurgeAllNonMax(List<List<List<int>>> kSequences, Dictionary<int, List<int>> containmentRules,
			bool progressOutput)
		{
			if (kSequences == null || kSequences.Count == 0)
				return;

			Stopwatch purgingStopwatch = new Stopwatch();

			bool shouldKeepRunning = true;
			while (shouldKeepRunning)
			{
				if (progressOutput)
					Trace.Write(" started new run,");
				shouldKeepRunning = false;

				purgingStopwatch.Reset();
				purgingStopwatch.Start();
				if (PurgeUsingInclusionRules(kSequences, containmentRules, progressOutput))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Trace.Write(String.Format("\n inclusion of smaller: {0}ms,", purgingStopwatch.ElapsedMilliseconds));

				purgingStopwatch.Reset();
				purgingStopwatch.Start();
				if (PurgeUsingSequenceInnerRedundancy(kSequences, containmentRules))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Trace.Write(String.Format(" inner redundancy: {0}ms,", purgingStopwatch.ElapsedMilliseconds));

				purgingStopwatch.Reset();
				purgingStopwatch.Start();
				if (PurgeUsingInclusionRulesWithinSameSize(kSequences, containmentRules))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Trace.WriteLine(String.Format(" same size: {0}ms", purgingStopwatch.ElapsedMilliseconds));
			}
		}

		/// <summary>
		/// Checks if one sequence is a subsequence of the other.
		/// </summary>
		/// <typeparam name="T">type of element of both lists</typeparam>
		/// <param name="hyptheticalSubSequence">supposed sub-sequence</param>
		/// <param name="sequence">suppoed super-sequence</param>
		/// <returns>true if 1st parameter is a subsequence of the 2nd</returns>
		protected bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence)
			where T : IComparable
		{
			return IsSubSequence<T>(hyptheticalSubSequence, sequence, null);
		}

		protected bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence,
			Dictionary<T, List<T>> containmentRules)
			where T : IComparable
		{
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
			bool previousWasContainment = false;
			bool sequenceContainsElementOfSubsequence = false;
			List<T>.Enumerator sequenceEnumerator = sequence.GetEnumerator();
			foreach (T elementOfHypotheticalSubsequence in hyptheticalSubSequence)
			{
				found = false;
				//int i = found_index + 1;
				int nextDiff = previousWasContainment ? 0 : 1;
				for (int i = found_index + nextDiff;
					((sequenceContainsElementOfSubsequence && previousWasContainment) ? true : sequenceEnumerator.MoveNext())
					/*&& i < sequence.Count*/; ++i)
				{
					T elementOfSequence = sequenceEnumerator.Current;
					//sequenceEnumerator.MoveNext();
					//T elementOfSequence = sequence[i];
					sequenceContainsElementOfSubsequence = false;

					if (elementOfHypotheticalSubsequence.Equals(elementOfSequence))
					{
						sequenceContainsElementOfSubsequence = true;
						previousWasContainment = false;
					}

					if (containmentRules != null && containmentRules.ContainsKey(elementOfHypotheticalSubsequence)
						&& containmentRules[elementOfHypotheticalSubsequence].Contains(elementOfSequence))
					{
						sequenceContainsElementOfSubsequence = true;
						previousWasContainment = true;
					}

					if (sequenceContainsElementOfSubsequence)
					{
						found = true;
						found_index = i;
						break;
					}
				}
				if (!found)
				{
					if (previousWasContainment)
						previousWasContainment = false;
					else
						return false;
				}
			}

			return true;

		}

		/// <summary>
		/// Deletes all subsequences of ii-th kk-sequence from the list of k-sequences.
		/// </summary>
		/// <param name="kSequences">list of k-sequences partitioned by k</param>
		/// <param name="kk">corresponds to k in k-sequence</param>
		/// <param name="ii">index of sequence in the list of kk-sequences</param>
		protected void PurgeAllSubSeqsOf(List<List<List<int>>> kSequences, int kk, int ii)
		{
			//if (ii < 0)
			//	throw new ArgumentException(String.Format("kSequences.Count={2}, kk={1} ii={0}",
			//		ii, kk, kSequences.Count), "ii");
			if (kk <= 1)
				return;
			List<int> sequence = kSequences[kk][ii];
			for (int k = kk - 1; k >= 0; --k)
			{

				List<List<int>> sequencesOfLengthK = kSequences[k];

				for (int i = 0; i < sequencesOfLengthK.Count; ++i)
				{
					if (IsSubSequence(sequencesOfLengthK[i], sequence))
					{
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
		protected List<List<int>> CompactSequencesList(List<List<List<int>>> kSequences)
		{
			var compacted = new List<List<int>>();

			//for (int k = kSequences.Count - 1; k >= 0; --k) {
			for (int k = 0; k < kSequences.Count; ++k)
			{
				//for (int n = kSequences[k].Count - 1; n >= 0; --n) {
				for (int n = 0; n < kSequences[k].Count; ++n)
				{
					if (kSequences[k][n].Count > 0)
						//kSequences[n].RemoveAt(k);
						compacted.Add(kSequences[k][n]);
				}

				//if (kSequences[k].Count == 0)
				//   kSequences.RemoveAt(k);
			}

			return compacted;
		}

		/// <summary>
		/// Prepares cleaned-up and user-ready data that can be used as final output of AprioriAll algoritm.
		/// </summary>
		/// <param name="encodedList">encoded input</param>
		/// <param name="kSequences">all found maximal k-sequences, partitioned by k</param>
		/// <param name="decoding">decoding dictionary</param>
		/// <returns>cleaned-up data that can be used as final output of AprioriAll</returns>
		protected List<Customer> InferRealResults(List<List<List<int>>> encodedList, List<List<List<int>>> kSequences,
			Dictionary<int, Litemset> decoding)
		{
			var decodedList = new List<Customer>();

			var compactSequences = CompactSequencesList(kSequences);

			foreach (List<int> sequence in compactSequences)
			{
				Customer c = new Customer();
				foreach (int encodedLitemset in sequence)
				{
					Transaction t = new Transaction(decoding[encodedLitemset].Items);
					c.Transactions.Add(t);
				}
				decodedList.Add(c);
			}

			// it seems that the below code is obsolete:

			//foreach (Customer customer in decodedList)
			//{
			//	for (int tn = customer.Transactions.Count - 1; tn >= 0; --tn)
			//	{
			//		// tn : transaction no.
			//		Transaction t = customer.Transactions[tn];
			//		foreach (Transaction comparedTransaction in customer.Transactions)
			//		{
			//			if (Object.ReferenceEquals(t, comparedTransaction))
			//				continue;
			//			if (t.Items.Count >= comparedTransaction.Items.Count)
			//				continue;
			//			if (IsSubSequence<Item>(t.Items, comparedTransaction.Items))
			//			{
			//				customer.Transactions.RemoveAt(tn);
			//				break;
			//			}
			//		}
			//	}
			//}

			//// compare each pair of customers for equality
			//List<int> duplicateCustomers = new List<int>();
			//for (int i1 = 0; i1 < decodedList.Count; i1++)
			//{
			//	Customer customer1 = decodedList[i1];
			//	for (int i2 = i1 + 1; i2 < decodedList.Count; i2++)
			//	{
			//		//if (i1 == i2)
			//		//	continue;
			//		Customer customer2 = decodedList[i2];
			//		if (customer1.Transactions.Count != customer2.Transactions.Count)
			//			continue;
			//		if (customer1.Equals(customer2))
			//		{
			//			duplicateCustomers.Add(i1);
			//			break;
			//		}
			//	}
			//}
			//for (int index = duplicateCustomers.Count - 1; index >= 0; --index)
			//	decodedList.RemoveAt(duplicateCustomers[index]);

			return decodedList;
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<Customer> RunAprioriAll(double threshold)
		{
			return RunAprioriAll(threshold, false);
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<Customer> RunAprioriAll(double threshold, bool progressOutput)
		{
			if (customerList == null)
				throw new ArgumentNullException("customers list is null", "customerList");
			if (threshold > 1 || threshold <= 0)
				throw new ArgumentException("threshold is out of range = (0,1]", "threshold");

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * threshold);
			if (progressOutput)
				Trace.WriteLine(String.Format("Threshold = {0}  =>  Minimum support = {1}", threshold, minSupport));

			if (minSupport <= 0)
				throw new ArgumentException("minimum support must be positive", "minSupport");

			// 1. sort the input!
			if (progressOutput)
				Trace.WriteLine("1) Sort Phase - list is already sorted as user sees fit");

			// corresponds to 1st step of Apriori All algorithm, namely "Sort Phase".

			// already done because input is sorted by the user in an apropriate way

			// 2. find all frequent 1-sequences
			if (progressOutput)
				Trace.WriteLine("2) Litemset Phase");
			if (progressOutput)
				Trace.WriteLine("Launching Apriori...");
			// this corresponds to 2nd step of Apriori All algorithm, namely "Litemset Phase".
			List<Litemset> oneLitemsets = RunApriori(threshold, progressOutput);

			// 3. transform input into list of IDs
			if (progressOutput)
				Trace.WriteLine("3) Transformation Phase");

			// 3.a) give an ID to each 1-seq
			Dictionary<Litemset, int> encoding;
			Dictionary<int, Litemset> decoding;

			GenerateEncoding(oneLitemsets, out encoding, out decoding);

			Dictionary<int, List<int>> litemsetsContaining = GenerateContainmentRules(oneLitemsets, encoding);

			if (progressOutput)
			{
				Trace.WriteLine("Encoding dictionary for litemsets:");
				foreach (KeyValuePair<int, Litemset> kv in decoding)
				{
					Trace.Write(String.Format(" {0} <= {1}", kv.Key, kv.Value));
					if (litemsetsContaining.ContainsKey(kv.Key))
					{
						List<int> superLitemsets = litemsetsContaining[kv.Key];
						Trace.Write(String.Format("; {0} is in {1}", kv.Key, String.Join(", ", superLitemsets.ToArray())));
					}
					Trace.WriteLine("");
				}
			}

			//if (progressOutput)
			//{
			//	Console.Out.WriteLine("Containment rules for litemsets:");
			//	foreach (KeyValuePair<int, List<int>> kv in litemsetsContaining)
			//		Console.Out.WriteLine(" {0} is in {1}", kv.Key, String.Join(", ", kv.Value.ToArray()));
			//}

			// 3.b) using created IDs, transform the input

			// list of lists of list of IDs:
			// - each list of IDs is a frequent itemset
			// - list of those means a list of frequent itemsets, 
			//   meaning a list of transaction represented as a list of frequent 
			//   itemsets performed by one customer
			// - outer list means list of customers

			if (progressOutput)
				Trace.WriteLine("Encoding input data...");
			var encodedList = EncodeCustomerList(oneLitemsets, encoding);

			if (progressOutput)
			{
				var customersEnumerator = customerList.Customers.GetEnumerator();
				Trace.WriteLine("How the input is encoded:");
				foreach (List<List<int>> c in encodedList)
				{
					customersEnumerator.MoveNext();
					//var transactionsEnumerator = customersEnumerator.Current.Transactions.GetEnumerator();
					Trace.Write(String.Format(" - {0} => (", customersEnumerator.Current));
					foreach (List<int> t in c)
					{
						//transactionsEnumerator.MoveNext();
						//var itemsEnumerator = transactionsEnumerator.Current.Items.GetEnumerator();
						Trace.Write("{");
						bool first = true;
						foreach (int i in t)
						{
							//itemsEnumerator.MoveNext();
							if (!first)
								Trace.Write(" ");
							if (first)
								first = false;
							Trace.Write(String.Format("{0}", i));
						}
						Trace.Write("}");
					}
					Trace.WriteLine(")");
				}
			}

			// 4. find all frequent sequences in the input
			if (progressOutput)
				Trace.WriteLine("4) Sequence Phase");

			if (progressOutput)
				Trace.WriteLine("Searching for all possible k-sequences");
			var kSequences = FindAllFrequentSequences(oneLitemsets, encoding, encodedList, minSupport, progressOutput);
			if (progressOutput)
				Trace.WriteLine(String.Format("Maximal k is {0}.", kSequences.Count - 2));

			// 5. purge all non-maximal sequences
			if (progressOutput)
				Trace.WriteLine("5) Maximal Phase");

			if (progressOutput)
				Trace.WriteLine("Purging all non-maximal sequences...");
			PurgeAllNonMax(kSequences, litemsetsContaining, progressOutput);

			// 6. decode results
			if (progressOutput)
				Trace.WriteLine("Decoding results and purging again...");
			var decodedList = InferRealResults(encodedList, kSequences, decoding);

			if (progressOutput)
			{
				var decodedEnumerator = decodedList.GetEnumerator();
				Trace.WriteLine("How maximal sequences are decoded:");
				foreach (List<List<int>> kSequencesPartition in kSequences)
					foreach (List<int> sequene in kSequencesPartition)
					{
						Trace.Write(" - <");
						bool first = true;
						foreach (int i in sequene)
						{
							if (!first)
								Trace.Write(" ");
							if (first)
								first = false;
							Trace.Write(String.Format("{0}", i));
						}
						Trace.Write(">");
						if (decodedEnumerator.MoveNext())
							Trace.Write(String.Format(" => {0}", decodedEnumerator.Current));
						Trace.WriteLine("");
					}
			}

			// 7. return results
			return decodedList;
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold. It uses parallel version 
		/// of the algorithm, unless no OpenCL platforms are available.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<Customer> RunParallelAprioriAll(double threshold)
		{
			return RunParallelAprioriAll(threshold, false);
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold. It uses parallel version 
		/// of the algorithm, unless no OpenCL platforms are available.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<Customer> RunParallelAprioriAll(double threshold, bool progressOutput)
		{
			if (OpenCLChecker.PlatformsCount() == 0)
				return RunAprioriAll(threshold, progressOutput);

			if (customerList == null)
				throw new ArgumentNullException("customerList", "customerList is null.");
			if (threshold > 1 || threshold <= 0)
				throw new ArgumentException("threshold", "threshold is out of range = (0,1]");

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * threshold);
			if (progressOutput)
				Trace.WriteLine(String.Format("Threshold = {0}  =>  Minimum support = {1}", threshold, minSupport));

			if (minSupport <= 0)
				throw new ArgumentException("minimum support must be positive", "minSupport");

			List<Litemset> oneLitemsets = RunParallelApriori(threshold, progressOutput);

			return null;
		}

	}
}
