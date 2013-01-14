using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading;
using OpenCL.Abstract;

namespace AprioriAllLib
{
	/*!
	 * \addtogroup aprioriandall
	 * @{
	 */

	/// <summary>
	/// Used to get list of large itemsets from a set of customers' transactions.
	/// 
	/// Authors of the implementation:
	/// - serialized version by Karolina Baltyn
	/// - parallel version by Mateusz Bysiek
	/// </summary>
	public class Apriori : IDisposable
	{
		/// <summary>
		/// Input data for the algorithm.
		/// </summary>
		protected CustomerList customerList;

		/// <summary>
		/// Indicator whether OpenCL platorm and device information is alread gathered
		/// by this instance of Apriori, and whether context was created for it.
		/// </summary>
		private bool clInitialized;

		private Platform platform;

		private Device device;

		private Context context;

		/// <summary>
		/// Inditator whether source code of OpenCL kernels was already read by this instance.
		/// </summary>
		private bool clProgramsInitialized;

		//private Program programBasicFunctions;

		///// <summary>
		///// Program created from file 'distinct.cl'.
		///// </summary>
		//private Program programDistinct;

		///// <summary>
		///// Program created from file 'separation.cl'.
		///// </summary>
		//private Program programSeparation;

		///// <summary>
		///// Program created from file 'support.cl'.
		///// </summary>
		//private Program programSupport;

		//private bool clKernelsInitialized;

		//private Cl.Kernel kernel;

		private Semaphore[] sem;

		Kernels.PairwiseCopy kernelCopy;

		Kernels.Max kernelMax;

		Kernels.AssignZero kernelZero;

		Kernels.Min kernelMin;

		Kernels.SubstituteIfEqual kernelSubstitute;

		Kernels.PairwiseCopyIfEqual kernelCopyIfEqual;

		Kernels.SubstituteIfNotEqual kernelSubstituteIfNotEqual;

		Kernels.Sum kernelSum;

		Kernels.PairwiseAnd kernelPairwiseAnd;

		Kernels.Or kernelOr;

		Kernels.SegmentedOr kernelSegmentedOr;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="customerList">CustomerList object containing a list of Customers from a database</param>
		public Apriori(CustomerList customerList)
		{
			this.customerList = customerList;

			clInitialized = false;
			clProgramsInitialized = false;
			//clKernelsInitialized = false;
		}

		protected void InitOpenCL(bool progressOutput)
		{
			if (clInitialized)
				return; // already initialized

			Stopwatch initWatch = new Stopwatch();
			initWatch.Start();

			platform = new Platform();
			device = new Device(platform, DeviceType.Any);
			context = new Context(device);
			Kernels.Device = device;
			Kernels.Context = context;

			initWatch.Stop();

			if (progressOutput)
				Log.WriteLine("Initialized OpenCL in {0}ms.", initWatch.ElapsedMilliseconds);

			clInitialized = true;
		}

		protected void InitOpenCLPrograms(bool parallelBuild, bool progressOutput)
		{
			if (clProgramsInitialized)
				return;

			Stopwatch buildWatch = new Stopwatch();
			buildWatch.Start();

			//	ResourceManager manager = AprioriAllLib.Properties.Resources.ResourceManager;

			//	//programBasicFunctions = new Program(context, new SourceCode(manager, "cl_basicFunctions"));
			//	//programBasicFunctions.Build(device);
			//	programDistinct = new Program(context, new SourceCode(manager, "cl_distinct"));
			//	programDistinct.Build(device);
			//	programSeparation = new Program(context, new SourceCode(manager, "cl_separation"));
			//	programSeparation.Build(device);
			//	programSupport = new Program(context, new SourceCode(manager, "cl_support"));
			//	programSupport.Build(device);

			if (parallelBuild)
			{
				sem = new Semaphore[5];

				sem[0] = new Semaphore(1, 1);
				new Thread(() =>
				{
					sem[0].WaitOne();
					kernelCopy = new Kernels.PairwiseCopy();
					kernelCopyIfEqual = new Kernels.PairwiseCopyIfEqual();
					sem[0].Release();
				}
				).Start();

				sem[1] = new Semaphore(1, 1);
				new Thread(() =>
				{
					sem[1].WaitOne();
					kernelMax = new Kernels.Max();
					kernelMin = new Kernels.Min();
					kernelSum = new Kernels.Sum();
					sem[1].Release();
				}
				).Start();

				sem[2] = new Semaphore(1, 1);
				new Thread(() =>
				{
					sem[2].WaitOne();
					kernelZero = new Kernels.AssignZero();
					kernelSubstitute = new Kernels.SubstituteIfEqual();
					kernelSubstituteIfNotEqual = new Kernels.SubstituteIfNotEqual();
					sem[2].Release();
				}
				).Start();

				sem[3] = new Semaphore(1, 1);
				new Thread(() =>
				{
					sem[3].WaitOne();
					kernelPairwiseAnd = new Kernels.PairwiseAnd();
					sem[3].Release();
				}
				).Start();

				sem[4] = new Semaphore(1, 1);
				new Thread(() =>
				{
					sem[4].WaitOne();
					kernelOr = new Kernels.Or();
					kernelSegmentedOr = new Kernels.SegmentedOr();
					sem[4].Release();
				}
				).Start();

				//sem[5] = new Semaphore(1, 1);
				//new Thread(() =>
				//{
				//	sem[5].WaitOne();
				//	sem[5].Release();
				//}
				//).Start();

				//sem[6] = new Semaphore(1, 1);
				//new Thread(() =>
				//{
				//	sem[6].WaitOne();
				//	sem[6].Release();
				//}
				//).Start();

				//sem[7] = new Semaphore(1, 1);
				//new Thread(() =>
				//{
				//	sem[7].WaitOne();
				//	sem[7].Release();
				//}
				//).Start();

				//sem[8] = new Semaphore(1, 1);
				//new Thread(() =>
				//{
				//	sem[8].WaitOne();
				//	sem[8].Release();
				//}
				//).Start();

				//Thread.Sleep(700);

				sem[0].WaitOne();
				sem[1].WaitOne();
				sem[2].WaitOne();
				sem[3].WaitOne();
				sem[4].WaitOne();
				//sem[5].WaitOne();
				//sem[6].WaitOne();
				//sem[7].WaitOne();
				//sem[8].WaitOne();
			}
			else
			{
				kernelCopy = new Kernels.PairwiseCopy();

				kernelMax = new Kernels.Max();

				kernelZero = new Kernels.AssignZero();

				kernelMin = new Kernels.Min();

				kernelSubstitute = new Kernels.SubstituteIfEqual();

				kernelCopyIfEqual = new Kernels.PairwiseCopyIfEqual();

				kernelSubstituteIfNotEqual = new Kernels.SubstituteIfNotEqual();

				kernelSum = new Kernels.Sum();

				kernelPairwiseAnd = new Kernels.PairwiseAnd();

				kernelOr = new Kernels.Or();

				kernelSegmentedOr = new Kernels.SegmentedOr();
			}

			buildWatch.Stop();

			if (progressOutput)
				Log.WriteLine("Built OpenCL programs in {0}ms.", buildWatch.ElapsedMilliseconds);

			clProgramsInitialized = true;
		}

		public void Dispose()
		{
			//if (clProgramsInitialized)
			//{
			//	//programBasicFunctions.Dispose();
			//	programDistinct.Dispose();
			//	programSeparation.Dispose();
			//	programSupport.Dispose();
			//	clProgramsInitialized = false;
			//}
			if (clInitialized)
			{
				context.Dispose();
				clInitialized = false;
			}
		}

		/// <summary>
		/// Produces all subsets of a given list of items in a transaction.
		/// </summary>
		/// <param name="items">list of items contained in one transaction</param>
		/// <returns>list of candidates for litemsets (large itemsets)</returns>
		protected List<Litemset> GenerateCandidateLitemsets(List<Item> items)
		{
			int count = items.Count;
			int i = 0;
			List<List<Item>> candLitemsets = new List<List<Item>>();

			// add a frequent sequence containing all the elements in this transaction
			candLitemsets.Add(new List<Item>(items));
			count--;
			while (count != 0)
			{
				List<Item> temp;
				foreach (Item item in candLitemsets[i])
				{
					temp = new List<Item>(candLitemsets[i]);
					temp.Remove(item);
					// check if there's already such subsequence in the list, add
					// if it doesn't exist
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

		protected void RemoveNonMaximal(List<Litemset> litemsets, bool progressOutput)
		{

		}

		protected void RemoveNonMaximal(List<Litemset> litemsets, bool useOpenCL, bool progressOutput)
		{

		}

		/// <summary>
		/// Finds all litemsets that have the minimal support.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> RunApriori(double minimalSupport)
		{
			return RunApriori(minimalSupport, false);
		}

		/// <summary>
		/// \ingroup aprioriandall
		/// 
		/// Finds all litemsets that have the minimal support.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> RunApriori(double minimalSupport, bool progressOutput)
		{
			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			//common part - initialization
			minimalSupport *= customerList.Customers.Count;
			List<Litemset> litemsets = new List<Litemset>();

			// serialized version of the algorithm

			Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			foreach (Customer c in customerList.Customers)
			{
				foreach (Transaction t in c.Transactions)
				{
					//generate subsets (candidates for litemsets)
					List<Litemset> candidateLitemsets = GenerateCandidateLitemsets(t.Items);

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

			if (progressOutput)
				Log.WriteLine("Found {0} subsets.", litemsets.Count);

			// rewrite the litemsets with support >= minimum to a new list
			List<Litemset> properLitemsets = new List<Litemset>();
			foreach (Litemset litemset in litemsets)
				if (litemset.Support >= minimalSupport)
					properLitemsets.Add(litemset);

			if (progressOutput)
				Log.WriteLine("Purged unsupported, {0} remain.", properLitemsets.Count);

			properLitemsets.Sort();

			if (progressOutput)
				Log.WriteLine("Sorted output.");

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Generated all litemsets, found {0} in {1}ms.", properLitemsets.Count, watch.ElapsedMilliseconds);
			}

			return properLitemsets;
		}

		public List<Litemset> RunAprioriWithPruning(double minimalSupport, bool progressOutput)
		{
			var results = RunApriori(minimalSupport, progressOutput);
			RemoveNonMaximal(results, progressOutput);
			return results;
		}

		/// <summary>
		/// Executes parallel version of the Apriori algorithm.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> RunParallelApriori(double minimalSupport)
		{
			return RunParallelApriori(minimalSupport, false);
		}

		public List<Litemset> RunParallelAprioriWithPruning(double minimalSupport, bool progressOutput)
		{
			var results = RunParallelApriori(minimalSupport, progressOutput);
			RemoveNonMaximal(results, true, progressOutput);
			return results;
		}

		/// <summary>
		/// Executes parallel version of the Apriori algorithm.
		/// </summary>
		/// <param name="minimalSupport"></param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> RunParallelApriori(double minimalSupport, bool progressOutput)
		{
			if (Platforms.InitializeAll().Length == 0)
				return RunApriori(minimalSupport, progressOutput);

			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * minimalSupport);

			//Abstract.Diagnostics = true;

			#region conversion of input data to regular int arrays

			Stopwatch watchConversion = null;
			if (progressOutput)
			{
				watchConversion = new Stopwatch();
				watchConversion.Start();
			}
			//List<List<int>> allTransactions = new List<List<int>>();
			List<int> flatTransactions = new List<int>();
			List<int> flatTransactionsIds = new List<int>();
			List<int> flatUsersIds = new List<int>();
			List<int> transactionCounts = new List<int>();
			List<int> userCounts = new List<int>();
			//List<int> transactionsLimits = new List<int>();
			{
				int currentUserId = 0;
				int currentTransactionId = 0;
				int currentUserCount = 0;
				int currentTransactionCount = 0;
				foreach (Customer c in customerList.Customers)
				{
					currentUserCount = 0;
					currentTransactionId = 0;
					foreach (Transaction t in c.Transactions)
					{
						currentTransactionCount = 0;
						//List<int> transaction = new List<int>();
						//transactionsLimits.Add(limit);
						foreach (Item item in t.Items)
						{
							//transaction.Add(item.Value);
							flatTransactions.Add(item.Value);
							flatTransactionsIds.Add(currentTransactionId);
							flatUsersIds.Add(currentUserId);
							//++limit;
							++currentTransactionCount;
							++currentUserCount;
						}
						//allTransactions.Add(transaction);
						++currentTransactionId;
						transactionCounts.Add(currentTransactionCount);
					}
					++currentUserId;
					userCounts.Add(currentUserCount);
				}
			}
			if (progressOutput)
			{
				watchConversion.Stop();
				Log.WriteLine("Converted input in {0}ms.", watchConversion.ElapsedMilliseconds);
			}

			#endregion

			#region opencl initialization and initial buffers allocation

			InitOpenCL(progressOutput);

			CommandQueue queue = new CommandQueue(device, context);

			int[] items = flatTransactions.ToArray();
			Buffer<int> itemsBuf = new Buffer<int>(context, queue, items);
			itemsBuf.Write(false);

			int[] itemsCount = new int[] { items.Length };
			Buffer<int> itemsCountBuf = new Buffer<int>(context, queue, itemsCount);
			itemsCountBuf.Write(false);

			InitOpenCLPrograms(false, progressOutput);

			Buffer<int> itemsTempBuf = new Buffer<int>(context, queue, (uint)items.Length);
			kernelCopy.Launch(queue, itemsBuf, itemsTempBuf);

			//int[] itemsExcluded = new int[itemsCount[0]];
			//Buffer<int> itemsExcludedBuf = new Buffer<int>(context, queue, itemsExcluded);
			////Console.Out.WriteLine(String.Join(" ", itemsExcluded));
			//itemsCountBuf.Write(false);

			//int[] newItemsExcluded = new int[itemsCount[0]];
			//Buffer<int> newItemsExcludedBuf = new Buffer<int>(context, queue, newItemsExcluded);
			//newItemsExcludedBuf.Write(false);

			#endregion

			Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			#region finding empty id

			queue.Finish();
			kernelMax.Launch(queue, itemsTempBuf);

			queue.Finish();
			itemsTempBuf.Read(0, 1);

			int emptyId = itemsTempBuf.BackingCollection[0] + 1;
			Buffer<int> emptyIdBuf = new Buffer<int>(context, queue, 1);
			emptyIdBuf.BackingCollection[0] = emptyId;
			emptyIdBuf.Write(false);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("empty id @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region finding distinct item ids

			Buffer<int> itemsRemainingBuf = new Buffer<int>(context, queue, (uint)items.Length);
			kernelCopy.Launch(queue, itemsBuf, itemsRemainingBuf);

			int uniqueItemsCount = 0;
			//Buffer<int> uniqueItemsCountBuf = new Buffer<int>(context, queue, uniqueItemsCount);
			//uniqueItemsCountBuf.Write(false);

			int[] uniqueItems = new int[items.Length];
			Buffer<int> uniqueItemsBuf = new Buffer<int>(context, queue, uniqueItems);
			//uniqueItemsBuf.Write(false);

			kernelZero.Launch(queue, uniqueItemsBuf);

			queue.Finish();

			while (true)
			{
				kernelCopy.Launch(queue, itemsRemainingBuf, itemsTempBuf);

				queue.Finish();
				kernelMin.Launch(queue, itemsTempBuf);

				queue.Finish();
				itemsTempBuf.Read(0, 1);

				int foundValue = itemsTempBuf.BackingCollection[0];
				if (foundValue == emptyId)
					break; // reached end of computation

				uniqueItems[uniqueItemsCount] = foundValue;
				uniqueItemsBuf.Write(false);

				uniqueItemsCount += 1;
				//uniqueItemsCountBuf.Write(false);

				kernelSubstitute.Launch(queue, itemsRemainingBuf, emptyIdBuf, itemsTempBuf);
			}

			#endregion

			#region comments

			//int[] discovered = new int[itemsCount[0]];
			//Buffer<int> discoveredBuf = new Buffer<int>(context, queue, discovered);
			//discoveredBuf.Write(false);

			//int[] step = new int[] { 1 };
			//Buffer<int> stepBuf = new Buffer<int>(context, queue, step);

			//kernelZero.Launch(queue, itemsExcludedBuf);
			//kernelZero.Launch(queue, newItemsExcludedBuf);
			//kernelZero.Launch(queue, discoveredBuf); 

			//#region distinct items finding

			//Kernel kernelDistinct = new Kernel(programDistinct, "findNewDistinctItem");
			//kernelDistinct.SetArguments(itemsBuf, itemsCountBuf, itemsExcludedBuf, newItemsExcludedBuf,
			//	discoveredBuf, stepBuf);

			//Kernel kernelExcludeDistinct = new Kernel(programDistinct, "excludeLatestDistinctItem");
			//kernelExcludeDistinct.SetArguments(itemsBuf, itemsCountBuf, itemsExcludedBuf,
			//	uniqueItemsBuf, uniqueItemsCountBuf);

			//if (progressOutput)
			//	Log.WriteLine("Looking for unique items in all transactions.");

			//queue.Finish();
			//while (true)
			//{
			//	#region kernelDistinct launch

			//	step[0] = 1;
			//	while (step[0] <= itemsCount[0])
			//	{
			//		stepBuf.Write();
			//		kernelDistinct.Launch1D(queue, (uint)items.Length, (uint)1);

			//		//queue.Finish();

			//		itemsExcludedBuf.Read(); // only for debugging
			//		newItemsExcludedBuf.Read(); // only for debugging
			//		discoveredBuf.Read(); // only for debugging

			//		//if (progressOutput)
			//		//	Log.Write(String.Format(", step {0}", step[0]));

			//		step[0] *= 2;
			//	}

			//	//if (progressOutput)
			//	//	Log.WriteLine("");

			//	#endregion

			//	queue.Finish();

			//	discoveredBuf.Read(0, 1);

			//	if (discovered[0] == 0)
			//		break;

			//	if (progressOutput)
			//		Log.WriteLine(" found new unique item: {0}", discovered[0]);

			//	queue.Finish(); // only for debug
			//	discoveredBuf.Read(); // only for debug

			//	if (uniqueItems.Length <= uniqueItemsCount[0])
			//		throw new IndexOutOfRangeException(String.Format("at most {0} unique items expected but got {1}: {2}",
			//						uniqueItems.Length, uniqueItemsCount[0], String.Join(",", uniqueItems)));

			//	uniqueItems[uniqueItemsCount[0]] = discovered[0];
			//	uniqueItemsBuf.Write();

			//	uniqueItemsCount[0] += 1;
			//	uniqueItemsCountBuf.Write();

			//	//if (progressOutput)
			//	//	Log.Write(" launching kernelZero");

			//	kernelZero.Launch(queue, discoveredBuf);
			//	queue.Finish();

			//	kernelExcludeDistinct.Launch1D(queue, (uint)itemsCount[0], 1);

			//	queue.Finish();

			//	if (progressOutput)
			//		Log.WriteLine(" excluded distinct from future findings");

			//	//itemsExcludedBuf.Read();
			//	//for (int i = 0; i < itemsCount[0]; ++i)
			//	//	newItemsExcluded[i] = itemsExcluded[i];
			//	//newItemsExcludedBuf.Write();
			//	kernelCopy.Launch(queue, itemsExcludedBuf, newItemsExcludedBuf);

			//	queue.Finish();
			//}

			//queue.Finish();

			//if (progressOutput)
			//	Log.WriteLine("Found all unique items: {0}.", String.Join(", ", uniqueItems));

			//kernelDistinct.Dispose();
			//kernelExcludeDistinct.Dispose();

			////kernelOr.Dispose();
			////kernelZero.Dispose();

			//#endregion

			//if (progressOutput)
			//	Log.WriteLine("Calculating support of each unique item.");

			////if (progressOutput)
			////	Log.WriteLine("Finished subset generation.");

			//#region more buffers initialization

			//int[] itemsTransactions = flatUsersIds.ToArray();
			//Buffer<int> itemsTransactionsBuf = new Buffer<int>(context, queue, itemsTransactions);

			//#endregion

			//if (progressOutput)
			//{
			//	//Console.Out.WriteLine(String.Join(" ", items));
			//	//Console.Out.WriteLine(String.Join("  ", itemsTransactions));
			//}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("distinct items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region finding support for single items

			int[] supports = new int[itemsCount[0] * uniqueItemsCount];
			Buffer<int> supportsBuf = new Buffer<int>(context, queue, supports);
			kernelZero.Launch(queue, supportsBuf);

			//Kernel kernelInitSupports = new Kernel(queue, programSupport, "supportInitial");
			//kernelInitSupports.SetArguments(itemsBuf, itemsCountBuf, uniqueItemsBuf, uniqueItemsCountBuf,
			//	supportsBuf);

			//queue.Finish();
			//kernelInitSupports.Launch2D((uint)itemsCount[0], 1, (uint)uniqueItemsCount[0], 1);

			Buffer<int>[] uniqueItemBufs = new Buffer<int>[uniqueItemsCount];
			//for 
			//uniqueItemsCountBuf.Write(false);

			queue.Finish();
			{
				int uniqueItemIndex = 0;
				for (int supportsOffset = 0; supportsOffset < supports.Length; supportsOffset += items.Length)
				{
					uniqueItemBufs[uniqueItemIndex] = new Buffer<int>(context, queue, 1);
					uniqueItemBufs[uniqueItemIndex].BackingCollection[0] = uniqueItems[uniqueItemIndex];
					uniqueItemBufs[uniqueItemIndex].Write(false);

					queue.Finish();
					kernelCopyIfEqual.SetCopiedValue(uniqueItemBufs[uniqueItemIndex]);
					kernelCopyIfEqual.Launch(queue, itemsBuf, 0, (uint)items.Length,
						supportsBuf, (uint)supportsOffset, (uint)items.Length);

					//supportsBuf.Read(); // debug
					++uniqueItemIndex;
				}
			}

			Buffer<int> Zero = new Buffer<int>(context, queue, new int[] { 0 });
			Zero.Write(false);

			Buffer<int> One = new Buffer<int>(context, queue, new int[] { 1 });
			Zero.Write(false);

			queue.Finish();
			//supportsBuf.Read(); // debug
			kernelSubstituteIfNotEqual.Launch(queue, supportsBuf, One, Zero);

			//queue.Finish(); // debug
			//supportsBuf.Read(); // debug

			// TODO: optimize this scope, it is the slowest part
			{
				kernelSegmentedOr.SegmentStep = items.Length;
				kernelSegmentedOr.SegmentsCount = uniqueItemsCount;
				int offset = 0;
				//for (int i = 0; i < uniqueItemsCount; ++i) // for each unique item
				//{
				//offset = i * items.Length;
				for (int t = 0; t < userCounts.Count; ++t) // for each user
				{
					kernelSegmentedOr.Launch(queue, supportsBuf, (uint)offset, (uint)userCounts[t]);
					//kernelOr.Launch(queue, supportsBuf, (uint)offset, (uint)userCounts[t]);
					offset += userCounts[t];

					//if (progressOutput)
					//{
					//	watch.Stop();
					//	Log.WriteLine("@ {0}ms", watch.ElapsedMilliseconds);
					//	watch.Start();
					//}
				}
				//}
			}

			// copy data to a new buffer for use in multi-item support calculations
			int[] supportsBak = new int[supports.Length];
			Buffer<int> supportsBakBuf = new Buffer<int>(context, queue, supportsBak);
			queue.Finish();
			kernelCopy.Launch(queue, supportsBuf, supportsBakBuf);

			uint itemsCountUint = (uint)itemsCount[0];
			queue.Finish();
			for (uint offset = 0; offset < supports.Length; offset += itemsCountUint)
				kernelSum.Launch(queue, supportsBuf, offset, itemsCountUint);
			//supportsBuf.Read();

			queue.Finish();
			for (uint n = 1; n < uniqueItemsCount; ++n)
				kernelCopy.Launch(queue, supportsBuf, (uint)(n * items.Length), 1, supportsBuf, n, 1);

			queue.Finish();
			supportsBuf.Read(false, 0, (uint)uniqueItemsCount);

			List<Litemset> litemsets = new List<Litemset>();
			queue.Finish();
			List<int> supportedLocations = new List<int>();
			for (int i = 0; i < uniqueItemsCount; ++i)
			{
				if (supports[i] < minSupport)
					continue;
				litemsets.Add(new Litemset(supports[i], uniqueItems[i]));
				supportedLocations.Add(i);
			}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("single items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region finding support for paris, triples, ... of items

			int[] tempSupport = new int[] { 0 };
			bool moreCanBeFound = true;

			queue.Finish();
			kernelMax.Launch(queue, supportsBuf, 0, (uint)uniqueItemsCount);

			bool[] newSupportedLocations = new bool[uniqueItemsCount];
			for (int i = 0; i < uniqueItemsCount; ++i)
				newSupportedLocations[i] = false;

			queue.Finish();
			supportsBuf.Read(false, tempSupport, 0, 1);

			queue.Finish();
			if (tempSupport[0] < minSupport)
				moreCanBeFound = false;

			int currLength = 1;
			while (moreCanBeFound)
			{
				++currLength;
				uint itemsLength = (uint)items.Length;
				uint litemsetOffset = (uint)litemsets.Count;

				int start = 0;
				List<int> indices = new List<int>();
				for (int i = 0; i < currLength; ++i)
					indices.Add(i);
				int currIndex = indices.Count - 1;
				queue.Finish();
				//supportsBuf.Read(); // debug only
				//supportsBakBuf.Read(); // debug only
				while (start < supportedLocations.Count - currLength)
				{
					int locInit = supportedLocations[indices[0]];
					kernelCopy.Launch(queue, supportsBakBuf, (uint)locInit * itemsLength, itemsLength,
						supportsBuf, 0, itemsLength);

					for (int n = 1; n < indices.Count; ++n)
					{
						int locN = supportedLocations[indices[n]];
						queue.Finish();
						//supportsBuf.Read(); // debug only
						//supportsBakBuf.Read(); // debug only
						kernelPairwiseAnd.Launch(queue, supportsBakBuf, (uint)locN * itemsLength, itemsLength,
							supportsBuf, 0, itemsLength,
							supportsBuf, 0, itemsLength);
					}

					queue.Finish();
					kernelSum.Launch(queue, supportsBuf, 0, itemsLength);

					queue.Finish();
					supportsBuf.Read(false, 0, 1);

					queue.Finish();
					//supportsBuf.Read(); // debug only
					//supportsBakBuf.Read(); // debug only
					if (supports[0] >= minSupport)
					{
						Litemset l = new Litemset(new List<Item>());
						l.Support = supports[0];
						foreach (int index in indices)
						{
							l.Items.Add(new Item(uniqueItems[supportedLocations[index]]));
							newSupportedLocations[supportedLocations[index]] = true;
						}
						litemsets.Add(l);
					}

					if (currIndex == 0 && indices[currIndex] == supportedLocations.Count - indices.Count)
						break;
					if (indices[currIndex] == supportedLocations.Count - indices.Count + currIndex)
					{
						++indices[currIndex - 1];
						if (indices[currIndex - 1] + 1 < indices[currIndex])
						{
							while (currIndex < indices.Count - 1)
							{
								indices[currIndex] = indices[currIndex - 1] + 1;
								++currIndex;
							}
							indices[currIndex] = indices[currIndex - 1] + 1;
						}
						else
						{
							--currIndex;
						}
						continue;
					}
					indices[currIndex] += 1;
				}

				if (litemsets.Count == litemsetOffset || indices.Count == uniqueItems.Length)
				{
					moreCanBeFound = false;
					break;
				}

				supportedLocations.Clear();
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					if (newSupportedLocations[i])
						supportedLocations.Add(i);
					newSupportedLocations[i] = false;
				}

				//break; // only temporarily
			}

			#endregion

			//for (int i = 0; i < uniqueItemsCount[0]; ++i)
			//{
			//	int unique = uniqueItems[i];
			//	litemsets.Add(new Litemset(1, unique));
			//}

			//for (int i = 0; i < supports.Length; ++i)
			//{
			//	if (supports[i] >= minimalSupport)
			//		litemsets.Add(new Litemset(supports[i], allTransactions.ElementAt(i).ToArray()));
			//}

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Generated all litemsets, found {0} in {1}ms.", litemsets.Count, watch.ElapsedMilliseconds);
			}

			queue.Finish();

			itemsBuf.Dispose();
			itemsCountBuf.Dispose();
			itemsTempBuf.Dispose();
			itemsRemainingBuf.Dispose();
			uniqueItemsBuf.Dispose();
			supportsBuf.Dispose();
			supportsBakBuf.Dispose();
			foreach (var buf in uniqueItemBufs)
				buf.Dispose();
			Zero.Dispose();
			One.Dispose();

			queue.Dispose();
			Kernels.Dispose();

			return litemsets;
			//return null;
		}

	}

	/// @}
}

