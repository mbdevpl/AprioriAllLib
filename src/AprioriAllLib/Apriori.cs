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
		protected IEnumerable<ICustomer> customerList;

		protected int customersCount;

		#region OpenCL

		/// <summary>
		/// Indicator whether OpenCL platorm and device information is alread gathered
		/// by this instance of Apriori, and whether context was created for it.
		/// </summary>
		private bool clInitialized;

		private Platform platform;

		private Device device;

		private Context context;

		#endregion

		/// <summary>
		/// Inditator whether source code of OpenCL kernels was already read by this instance.
		/// </summary>
		private bool clProgramsInitialized;

		#region Kernels.(...)

		//private Semaphore[] sem;

		Kernels.AssignZero kernelZero;
		Kernels.AssignConst kernelConst;
		Kernels.Increment kernelIncrement;
		Kernels.MultiplyByTwo kernelMultiplyByTwo;

		Kernels.ChangeValueToConst kernelSubstitute;
		Kernels.ChangeAllButValueToConst kernelSubstituteIfNotEqual;

		//Kernels.Identity kernelCopy;
		//Kernels.Identity kernelCopySingle;
		Kernels.CopyOnlyOneValue kernelCopyIfEqual;

		Kernels.ReductionMin kernelMin;
		Kernels.ReductionMax kernelMax;
		Kernels.ReductionSum kernelSum;

		//Kernels.PairwiseAnd kernelPairwiseAnd;

		Kernels.ReductionOr kernelOr;
		//Kernels.And kernelAnd;
		Kernels.SegmentedReductionOr kernelSegmentedOr;

		#endregion

		//private Program programBasicFunctions;

		///// <summary>
		///// Program created from file 'distinct.cl'.
		///// </summary>
		//private Program programDistinct;

		///// <summary>
		///// Program created from file 'separation.cl'.
		///// </summary>
		//private Program programSeparation;

		/// <summary>
		/// Program created from file 'apriori.cl'.
		/// </summary>
		private Program programApriori;

		//private bool clKernelsInitialized;

		private MultiItemSupportKernel kernelMulitItemSupport;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="customerList">CustomerList object containing a list of Customers from a database</param>
		[Obsolete]
		public Apriori(CustomerList customerList)
		{
			this.customerList = customerList.Customers;
			this.customersCount = customerList.Customers.Count;

			clInitialized = false;
			clProgramsInitialized = false;
			//clKernelsInitialized = false;
		}

		public Apriori(IEnumerable<ICustomer> customers)
		{
			this.customerList = customers;
			this.customersCount = customers.Count();
		}

		protected void InitOpenCL(bool progressOutput)
		{
			if (clInitialized)
				return; // already initialized

			//Stopwatch initWatch = new Stopwatch();
			//initWatch.Start();

			platform = new Platform();
			device = new Device(platform, DeviceType.GPU);
			context = new Context(device);
			Kernels.Device = device;
			Kernels.Context = context;

			//initWatch.Stop();
			//if (progressOutput)
			//	Log.WriteLine("Initialized OpenCL in {0}ms.", initWatch.ElapsedMilliseconds);

			clInitialized = true;
		}

		protected void InitOpenCLPrograms(bool parallelBuild, bool progressOutput)
		{
			if (clProgramsInitialized)
				return;

			//Stopwatch buildWatch = new Stopwatch();
			//buildWatch.Start();

			ResourceManager manager = AprioriAllLib.Properties.Resources.ResourceManager;

			//	//programBasicFunctions = new Program(context, new SourceCode(manager, "cl_basicFunctions"));
			//	//programBasicFunctions.Build(device);
			//	programDistinct = new Program(context, new SourceCode(manager, "cl_distinct"));
			//	programDistinct.Build(device);
			//	programSeparation = new Program(context, new SourceCode(manager, "cl_separation"));
			//	programSeparation.Build(device);
			programApriori = new Program(context, new SourceCode(manager, "cl_apriori"));
			programApriori.Build(device);

			kernelMulitItemSupport = new MultiItemSupportKernel(programApriori, "multiItemSupport");

			//if (parallelBuild)
			//{
			//	#region parallel program building

			//	sem = new Semaphore[5];

			//	sem[0] = new Semaphore(1, 1);
			//	new Thread(() =>
			//	{
			//		sem[0].WaitOne();
			//		kernelCopy = new Kernels.Identity(true);
			//		kernelCopySingle = new Kernels.Identity(true);
			//		kernelCopyIfEqual = new Kernels.PairwiseCopyIfEqual(true);
			//		sem[0].Release();
			//	}
			//	).Start();

			//	sem[1] = new Semaphore(1, 1);
			//	new Thread(() =>
			//	{
			//		sem[1].WaitOne();
			//		kernelSum = new Kernels.Sum(true);
			//		kernelMin = new Kernels.Min(true);
			//		kernelMax = new Kernels.Max(true);
			//		sem[1].Release();
			//	}
			//	).Start();

			//	sem[2] = new Semaphore(1, 1);
			//	new Thread(() =>
			//	{
			//		sem[2].WaitOne();
			//		kernelZero = new Kernels.AssignZero(true);
			//		kernelConst = new Kernels.AssignConst(true);
			//		kernelSubstitute = new Kernels.SubstituteIfEqual(true);
			//		kernelSubstituteIfNotEqual = new Kernels.SubstituteIfNotEqual(true);
			//		kernelMultiplyByTwo = new Kernels.MultiplyByTwo();
			//		kernelIncrement = new Kernels.Increment();
			//		sem[2].Release();
			//	}
			//	).Start();

			//	sem[3] = new Semaphore(1, 1);
			//	new Thread(() =>
			//	{
			//		sem[3].WaitOne();
			//		//kernelPairwiseAnd = new Kernels.PairwiseAnd(true);
			//		sem[3].Release();
			//	}
			//	).Start();

			//	sem[4] = new Semaphore(1, 1);
			//	new Thread(() =>
			//	{
			//		sem[4].WaitOne();
			//		kernelOr = new Kernels.Or(true);
			//		kernelSegmentedOr = new Kernels.SegmentedOr(true);
			//		//kernelAnd = new Kernels.And(true);
			//		sem[4].Release();
			//	}
			//	).Start();

			//	//sem[5] = new Semaphore(1, 1);
			//	//new Thread(() =>
			//	//{
			//	//	sem[5].WaitOne();
			//	//	sem[5].Release();
			//	//}
			//	//).Start();

			//	//sem[6] = new Semaphore(1, 1);
			//	//new Thread(() =>
			//	//{
			//	//	sem[6].WaitOne();
			//	//	sem[6].Release();
			//	//}
			//	//).Start();

			//	//sem[7] = new Semaphore(1, 1);
			//	//new Thread(() =>
			//	//{
			//	//	sem[7].WaitOne();
			//	//	sem[7].Release();
			//	//}
			//	//).Start();

			//	//sem[8] = new Semaphore(1, 1);
			//	//new Thread(() =>
			//	//{
			//	//	sem[8].WaitOne();
			//	//	sem[8].Release();
			//	//}
			//	//).Start();

			//	//Thread.Sleep(700);

			//	sem[0].WaitOne();
			//	sem[1].WaitOne();
			//	sem[2].WaitOne();
			//	sem[3].WaitOne();
			//	sem[4].WaitOne();
			//	//sem[5].WaitOne();
			//	//sem[6].WaitOne();
			//	//sem[7].WaitOne();
			//	//sem[8].WaitOne();

			//	#endregion
			//}
			//else
			{
				//kernelCopy = new Kernels.Identity();
				//kernelCopySingle = new Kernels.Identity();
				kernelCopyIfEqual = new Kernels.CopyOnlyOneValue();

				kernelMin = new Kernels.ReductionMin();
				kernelMax = new Kernels.ReductionMax();
				kernelSum = new Kernels.ReductionSum();

				kernelZero = new Kernels.AssignZero();
				kernelConst = new Kernels.AssignConst();
				kernelSubstitute = new Kernels.ChangeValueToConst();
				kernelSubstituteIfNotEqual = new Kernels.ChangeAllButValueToConst();
				kernelMultiplyByTwo = new Kernels.MultiplyByTwo();
				kernelIncrement = new Kernels.Increment();

				//kernelPairwiseAnd = new Kernels.PairwiseAnd(true);

				kernelOr = new Kernels.ReductionOr();
				kernelSegmentedOr = new Kernels.SegmentedReductionOr();
				//kernelAnd = new Kernels.And(true);
			}

			//buildWatch.Stop();
			//if (progressOutput)
			//	Log.WriteLine("Built OpenCL programs in {0}ms.", buildWatch.ElapsedMilliseconds);

			clProgramsInitialized = true;
		}

		public void Dispose()
		{
			if (clProgramsInitialized)
			{
				// kernels from abstraction layer
				//kernelCopy.Dispose();
				//kernelCopySingle.Dispose();
				kernelCopyIfEqual.Dispose();

				kernelMin.Dispose();
				kernelMax.Dispose();
				kernelSum.Dispose();

				kernelZero.Dispose();
				kernelConst.Dispose();
				kernelSubstitute.Dispose();
				kernelSubstituteIfNotEqual.Dispose();
				kernelMultiplyByTwo.Dispose();
				kernelIncrement.Dispose();

				//kernelPairwiseAnd.Dispose();

				kernelOr.Dispose();
				kernelSegmentedOr.Dispose();
				//kernelAnd.Dispose();

				// own kernels
				kernelMulitItemSupport.Dispose();
				programApriori.Dispose();

				clProgramsInitialized = false;
			}
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
		protected List<ILitemset> GenerateCandidateLitemsets(List<IItem> items)
		{
			int count = items.Count;
			int i = 0;
			List<List<IItem>> candLitemsets = new List<List<IItem>>();

			// add a frequent sequence containing all the elements in this transaction
			candLitemsets.Add(new List<IItem>(items));
			count--;
			while (count != 0)
			{
				List<IItem> temp;
				foreach (Item item in candLitemsets[i])
				{
					temp = new List<IItem>(candLitemsets[i]);
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
			var l = new List<ILitemset>();
			foreach (List<IItem> j in candLitemsets)
				l.Add(new Litemset(j));
			return l;
		}

		protected void RemoveNonMaximal(List<ILitemset> litemsets, bool progressOutput)
		{

		}

		protected void RemoveNonMaximal(List<ILitemset> litemsets, bool useOpenCL, bool progressOutput)
		{

		}

		/// <summary>
		/// Finds all litemsets that have the minimal support.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<ILitemset> RunApriori(double minimalSupport)
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
		public List<ILitemset> RunApriori(double minimalSupport, bool progressOutput)
		{
			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			if (customerList == null)
				return null;

			if (customersCount == 0)
				return new List<ILitemset>();

			//common part - initialization
			minimalSupport *= customersCount;
			var litemsets = new List<ILitemset>();

			// serialized version of the algorithm

			Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			int cIndex = 0;
			var idsInLitemsets = new Dictionary<ILitemset, List<int>>();
			foreach (ICustomer c in customerList)
			{
				foreach (ITransaction t in c.GetTransactions())
				{
					//generate subsets (candidates for litemsets)
					var candidateLitemsets = GenerateCandidateLitemsets(new List<IItem>(t.GetItems()));

					//check if they already exist in litemsets list; if not, add a litemset to litemsets
					foreach (Litemset lset in candidateLitemsets)
					{
						IEnumerable<ILitemset> l = litemsets.Where(litemset => (litemset.GetItemsCount() == lset.GetItemsCount()) &&
							 litemset.GetItems().All(item => lset/*.items.Exists*/.GetItems().Any(lsetItem => lsetItem.CompareTo(item) == 0)));

						List<int> IDs_lset = null;
						if (!idsInLitemsets.TryGetValue(lset, out IDs_lset))
						{
							IDs_lset = new List<int>();
							idsInLitemsets.Add(lset, IDs_lset);
						}

						if (l.Count() == 0 && !IDs_lset.Contains(cIndex))
						{
							litemsets.Add(lset);
							lset.IncrementSupport();
							IDs_lset.Add(cIndex);
						}
						else
						{
							ILitemset litset = l.FirstOrDefault();

							List<int> IDs_litset = null;
							if (!idsInLitemsets.TryGetValue(litset, out IDs_litset))
							{
								IDs_litset = new List<int>();
								idsInLitemsets.Add(lset, IDs_litset);
							}

							if (!IDs_litset.Contains(cIndex))
							{
								litset.IncrementSupport();
								IDs_litset.Add(cIndex);
							}
						}
					}
				}
				cIndex++;
				//if (progressOutput)
				//{
				//	if (cIndex % 10 == 9)
				//	{
				//		watch.Stop();
				//		Log.Write("{0}ms ", watch.ElapsedMilliseconds);
				//		watch.Start();
				//	}
				//	++cIndex;
				//}

			}
			//if (progressOutput)
			//{
			//	Log.WriteLine();
			//}


			if (progressOutput)
				Log.WriteLine("Found {0} subsets.", litemsets.Count);

			// rewrite the litemsets with support >= minimum to a new list
			var properLitemsets = new List<ILitemset>();
			foreach (ILitemset litemset in litemsets)
				if (litemset.GetSupport() >= minimalSupport)
					properLitemsets.Add(litemset);

			if (progressOutput)
				Log.WriteLine("Purged unsupported, {0} remain.", properLitemsets.Count);

			foreach (Litemset l in properLitemsets)
				l.SortItems();
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

		public List<ILitemset> RunAprioriWithPruning(double minimalSupport, bool progressOutput)
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
		public List<ILitemset> RunParallelApriori(double minimalSupport)
		{
			return RunParallelApriori(minimalSupport, false);
		}

		public List<ILitemset> RunParallelAprioriWithPruning(double minimalSupport, bool progressOutput)
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
		public List<ILitemset> RunParallelApriori(double minimalSupport, bool progressOutput)
		{
			if (Platforms.InitializeAll().Length == 0)
				return RunApriori(minimalSupport, progressOutput);

			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			if (customerList == null)
				return null;

			if (this.customersCount == 0)
				return new List<ILitemset>();

			int minSupport = (int)Math.Ceiling((double)this.customersCount * minimalSupport);

			Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			#region conversion of input to int[]

			int itemsCountInt = 0;
			int transactionsCountInt = 0;
			int customersCountInt = 0;

			foreach (ICustomer c in customerList)
			{
				++customersCountInt;
				foreach (ITransaction t in c.GetTransactions())
				{
					++transactionsCountInt;
					foreach (IItem item in t.GetItems())
					{
						++itemsCountInt;
					}
				}
			}

			uint itemsCountUInt = (uint)itemsCountInt;
			uint transactionsCountUInt = (uint)transactionsCountInt;
			uint customersCountUInt = (uint)customersCountInt;

			int[] items = new int[itemsCountInt];
			int[] itemsTransactions = new int[itemsCountInt];
			int[] itemsCustomers = new int[itemsCountInt];
			int[] transactionsStarts = new int[transactionsCountInt];
			int[] transactionsLengths = new int[transactionsCountInt];
			int[] customersStarts = new int[customersCountInt]; // in terms of transactions!
			int[] customersLengths = new int[customersCountInt]; // in terms of transactions!
			{
				int currItem = 0;
				int currTransaction = 0;
				int currCustomer = 0;

				int currTransactionLenth = 0;
				int currCustomerLength = 0;
				foreach (ICustomer c in customerList)
				{
					currCustomerLength = 0;
					customersStarts[currCustomer] = currTransaction;
					foreach (ITransaction t in c.GetTransactions())
					{
						currTransactionLenth = 0;
						transactionsStarts[currTransaction] = currItem;
						foreach (IItem item in t.GetItems())
						{
							items[currItem] = item.GetId();
							itemsTransactions[currItem] = currTransaction;
							itemsCustomers[currItem] = currCustomer;
							++currItem;
							++currTransactionLenth;
						}
						++currCustomerLength;
						transactionsLengths[currTransaction] = currTransactionLenth;
						++currTransaction;
					}
					customersLengths[currCustomer] = currCustomerLength;
					++currCustomer;
				}
			}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("input converted @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			//Abstract.Diagnostics = true;

			uint localSize = 32;

			InitOpenCL(progressOutput);

			CommandQueue queue = new CommandQueue(device, context);

			#region input buffers allocation and initialization

			Buffer<int> itemsBuf = new Buffer<int>(context, queue, items);

			Buffer<int> transactionsStartsBuf = new Buffer<int>(context, queue, transactionsStarts);

			Buffer<int> customersStartsBuf = new Buffer<int>(context, queue, customersStarts);

			int[] itemsCount = new int[] { itemsCountInt };
			Buffer<int> itemsCountBuf = new Buffer<int>(context, queue, itemsCount);

			int[] transactionsCount = new int[] { transactionsCountInt };
			Buffer<int> transactionsCountBuf = new Buffer<int>(context, queue, transactionsCount);

			int[] customersCount = new int[] { customersCountInt };
			Buffer<int> customersCountBuf = new Buffer<int>(context, queue, customersCount);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("buffers written @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			InitOpenCLPrograms(false, progressOutput);

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("programs built @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region empty buffers allocation and initialization

			Buffer<int> Zero = new Buffer<int>(context, queue, new int[] { 0 });
			Zero.Write(false);
			Buffer<int> One = new Buffer<int>(context, queue, new int[] { 1 });
			One.Write(false);

			Buffer<int> tempItemsBuf = new Buffer<int>(context, queue, itemsCountUInt);
			//kernelCopy.SetArgument(0, itemsBuf);
			//kernelCopy.SetArgument(4, tempItemsBuf);
			//kernelCopy.SetArguments(null, itemsCountInt, 0, itemsCountInt,
			//	null, itemsCountInt, 0, itemsCountInt);
			//kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);
			itemsBuf.Copy(0, itemsCountUInt, tempItemsBuf, 0);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Initialized OpenCL in {0}ms.", watch.ElapsedMilliseconds);
				watch.Restart();
			}

			// time: n*log^2(n)
			#region finding empty id

			int[] tempValue = new int[] { -1 };

			kernelMax.SetArgument(0, tempItemsBuf);
			kernelMax.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			uint globalSize = itemsCountUInt;
			for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;
			}

			//tempItemsBuf.Read(tempValue, 0, 1); // debug

			int emptyId = -1;
			Buffer<int> emptyIdBuf = new Buffer<int>(context, queue, 1);
			kernelIncrement.SetArgument(0, tempItemsBuf);
			kernelIncrement.SetArguments(null, 1, 0, 1);
			kernelIncrement.Launch1D(queue, 1, 1);

			tempItemsBuf.Copy(0, 1, emptyIdBuf, 0);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("empty id @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: 3n + n * ( 1.5n + log^2(n) ) == O(n^2)
			#region finding distinct item ids

			Buffer<int> itemsRemainingBuf = new Buffer<int>(context, queue, itemsCountUInt);
			itemsBuf.Copy(itemsRemainingBuf);
			itemsBuf.Copy(tempItemsBuf);

			int uniqueItemsCount = 0;

			int[] uniqueItems = new int[itemsCountInt];
			Buffer<int> uniqueItemsBuf = new Buffer<int>(context, queue, uniqueItems);

			kernelZero.SetArgument(0, uniqueItemsBuf);
			kernelZero.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			kernelMin.SetArgument(0, tempItemsBuf);
			kernelMin.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(0, itemsRemainingBuf);
			kernelSubstitute.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(4, emptyIdBuf);
			kernelSubstitute.SetArgument(5, tempItemsBuf);

			while (true)
			{
				globalSize = itemsCountUInt;
				for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
				{
					kernelMin.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelMin.Launch1D(queue, globalSize, localSize);
					globalSize /= localSize;
				}

				if (emptyId == -1)
				{
					emptyIdBuf.Read();
					emptyId = emptyIdBuf.Value;
				}
				tempItemsBuf.Read(tempValue, 0, 1);
				if (tempValue[0] == emptyId)
					break; // reached end of computation

				uniqueItems[uniqueItemsCount] = tempValue[0];
				uniqueItemsCount += 1;
				uniqueItemsBuf.Write(false, 0, (uint)uniqueItemsCount);

				kernelSubstitute.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

				//queue.Finish(); // debug only
				//itemsRemainingBuf.Read(); // debug only
				//tempItemsBuf.Read(); // debug only

				itemsRemainingBuf.Copy(0, itemsCountUInt, tempItemsBuf, 0);
			}

			uint uniqueItemsCountUInt = (uint)uniqueItemsCount;

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("distinct items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for items

			int itemsSupportsCountInt = itemsCountInt * uniqueItemsCount;
			uint itemsSupportsCountUInt = (uint)itemsSupportsCountInt;

			int[] itemsSupportsCount = new int[] { itemsSupportsCountInt };
			Buffer<int> itemsSupportsCountBuf = new Buffer<int>(context, queue, itemsSupportsCount);

			int[] itemsSupports = new int[itemsSupportsCountInt];
			Buffer<int> itemsSupportsBuf = new Buffer<int>(context, queue, itemsSupports);

			kernelZero.SetArgument(0, itemsSupportsBuf);
			kernelZero.SetArguments(null, itemsSupportsCountInt, null, itemsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);

			Buffer<int> uniqueItemBuf = new Buffer<int>(context, queue, 1);

			kernelCopyIfEqual.SetArgument(0, itemsBuf);
			kernelCopyIfEqual.SetArgument(4, itemsSupportsBuf);
			kernelCopyIfEqual.SetArguments(null, itemsCountInt, 0, itemsCountInt,
				null, itemsSupportsCountInt, null, itemsCountInt);
			kernelCopyIfEqual.SetArgument(8, uniqueItemBuf);

			int uniqueItemIndex = 0;
			for (int supportsOffset = 0; supportsOffset < itemsSupports.Length; supportsOffset += items.Length)
			{
				uniqueItemBuf.Value = uniqueItems[uniqueItemIndex];
				uniqueItemBuf.Write();
				kernelCopyIfEqual.SetArgument(6, supportsOffset);

				kernelCopyIfEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

				//uniqueItemBuf.Read(); // debug
				//offsetBuf.Read(); // debug
				//itemsSupportsBuf.Read(); // debug
				++uniqueItemIndex;
			}

			//itemsSupportsBuf.Read(); // debug
			kernelSubstituteIfNotEqual.SetArgument(0, itemsSupportsBuf);
			kernelSubstituteIfNotEqual.SetArguments(null, itemsSupportsCountInt, 0, itemsSupportsCountInt);
			kernelSubstituteIfNotEqual.SetArgument(4, One);
			kernelSubstituteIfNotEqual.SetArgument(5, Zero);
			kernelSubstituteIfNotEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);

			//queue.Finish(); // debug
			//itemsSupportsBuf.Read(); // debug

			//if (progressOutput)
			//{
			//	watch.Stop();
			//	Log.WriteLine("supports array @ {0}ms", watch.ElapsedMilliseconds);
			//	watch.Start();
			//}

			#endregion

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for transactions

			int transactionsSupportsCountInt = transactionsCountInt * uniqueItemsCount;
			uint transactionsSupportsCountUInt = (uint)transactionsSupportsCountInt;

			int[] transactionsSupportsCount = new int[] { transactionsSupportsCountInt };
			Buffer<int> transactionsSupportsCountBuf = new Buffer<int>(context, queue, transactionsSupportsCount);

			int[] transactionsSupports = new int[transactionsSupportsCountInt];
			Buffer<int> transactionsSupportsBuf = new Buffer<int>(context, queue, transactionsSupports);

			kernelZero.SetArgument(0, transactionsSupportsBuf);
			kernelZero.SetArguments(null, transactionsSupportsCountInt, null, transactionsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsSupportsCountUInt), localSize);

			Buffer<int> itemsSupportsBufCopy = new Buffer<int>(context, queue, itemsSupports);

			itemsSupportsBuf.Copy(0, itemsSupportsCountUInt, itemsSupportsBufCopy, 0);

			kernelSegmentedOr.SetArgument(0, itemsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, itemsSupportsCountInt, null, null,
				null, itemsCountInt);

			//queue.Finish(); // debug
			//itemsSupportsBufCopy.Read(); // debug

			for (int tn = 0; tn < transactionsCountInt; ++tn)
			{
				kernelSegmentedOr.SetArgument(2, transactionsStarts[tn]);
				kernelSegmentedOr.SetArgument(3, transactionsLengths[tn]);

				//itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				globalSize = (uint)transactionsLengths[tn];
				for (int scaling = 1; scaling < transactionsLengths[tn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
					//itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				}

				//queue.Finish(); // debug
				//itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results

				int offset = transactionsStarts[tn];
				int outputOffset = tn;
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					itemsSupportsBufCopy.Copy((uint)offset, 1, transactionsSupportsBuf, (uint)outputOffset);

					//queue.Finish(); // debug
					//transactionsSupportsBuf.Read(); // debug

					if (i == uniqueItemsCount - 1)
						break;

					offset += itemsCountInt;
					outputOffset += transactionsCountInt;
				}
			}

			//queue.Finish(); // debug
			//transactionsSupportsBuf.Read(); // debug

			#endregion

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for customers

			int customersSupportsCountInt = customersCountInt * uniqueItemsCount;
			uint customersSupportsCountUInt = (uint)customersSupportsCountInt;

			int[] customersSupportsCount = new int[] { customersSupportsCountInt };
			Buffer<int> customersSupportsCountBuf = new Buffer<int>(context, queue, customersSupportsCount);

			int[] customersSupports = new int[customersSupportsCountInt];
			Buffer<int> customersSupportsBuf = new Buffer<int>(context, queue, customersSupports);

			kernelZero.SetArgument(0, customersSupportsBuf);
			kernelZero.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, customersSupportsCountUInt), localSize);

			Buffer<int> transactionsSupportsBufCopy = new Buffer<int>(context, queue, transactionsSupports);

			transactionsSupportsBuf.Copy(0, transactionsSupportsCountUInt, transactionsSupportsBufCopy, 0);

			kernelSegmentedOr.SetArgument(0, transactionsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, transactionsSupportsCountInt, null, null,
				null, transactionsCountInt);

			//transactionsSupportsBufCopy.Read(); // debug

			for (int cn = 0; cn < customersCountInt; ++cn)
			{
				kernelSegmentedOr.SetArgument(2, customersStarts[cn]);
				kernelSegmentedOr.SetArgument(3, customersLengths[cn]);

				globalSize = (uint)customersLengths[cn];
				for (int scaling = 1; scaling < customersLengths[cn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
					//transactionsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				}

				//transactionsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results

				int offset = customersStarts[cn];
				int outputOffset = cn;
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					transactionsSupportsBufCopy.Copy((uint)offset, 1, customersSupportsBuf, (uint)outputOffset);

					//customersSupportsBuf.Read(); // debug

					if (i == uniqueItemsCount - 1)
						break;

					offset += transactionsCountInt;
					outputOffset += customersCountInt;
				}
			}

			#endregion

			// time: n + n * ( n*log^2(n) + n ) == O(n^2 * log^2(n))
			#region litemsets of size 1

			Buffer<int> customersSupportsBufCopy = new Buffer<int>(context, queue, customersSupports);

			//queue.Finish(); // debug
			//customersSupportsBuf.Read(); // debug
			//customersSupportsBufCopy.Read(); // debug
			customersSupportsBuf.Copy(0, customersSupportsCountUInt, customersSupportsBufCopy, 0);
			//customersSupportsBuf.Read(); // debug
			//customersSupportsBufCopy.Read(); // debug

			kernelSum.SetArgument(0, customersSupportsBufCopy);
			kernelSum.SetArguments(null, customersSupportsCountInt, null, customersCountInt);

			var litemsets = new List<ILitemset>();
			var supportedLocations = new List<int>();

			//queue.Finish(); // debug
			//customersSupportsBuf.Read(); // debug

			for (int un = 0; un < uniqueItemsCount; ++un)
			{
				int offset = un * customersCountInt;
				kernelSum.SetArgument(2, offset);

				globalSize = customersCountUInt;
				for (int scaling = 1; scaling < customersCountInt; scaling *= (int)localSize)
				{
					kernelSum.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSum.Launch1D(queue, globalSize, localSize);
					globalSize /= localSize;
				}

				//queue.Finish(); // debug
				//customersSupportsBufCopy.Read(); // debug

				customersSupportsBufCopy.Read(tempValue, (uint)offset, 1);

				int support = tempValue[0];

				if (support < 0 || support > customersCountInt)
				{
					Zero.Read();
					One.Read();
					emptyIdBuf.Read();
					itemsSupportsBuf.Read();
					transactionsSupportsBufCopy.Read();
					customersSupportsBufCopy.Read();
					itemsSupportsBuf.Read();
					transactionsSupportsBuf.Read();
					customersSupportsBuf.Read();
					throw new Exception(String.Format("support={0} is out of theoretical bounds [0,{1}]!",
						support, customersCountInt));
				}

				if (support < minSupport)
					continue;

				litemsets.Add(new Litemset(support, uniqueItems[un]));
				supportedLocations.Add(un);
			}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("single items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: n*log^2(n) + n + n * ( 0.5n + n^2 + 0.5n * n^2 * log^2(n) + n*log^2(n) ) == O(n^4 * log^2(n))
			#region litemsets of size 2 and above

			bool moreCanBeFound = true;

			//queue.Finish(); // debug
			//customersSupportsBufCopy.Read(); // debug

			kernelMax.SetArgument(0, customersSupportsBufCopy);
			kernelMax.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			globalSize = customersSupportsCountUInt;
			for (int scaling = 1; scaling < customersSupportsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;
				//queue.Finish(); // debug
				//customersSupportsBufCopy.Read(); // debug
			}

			bool[] newSupportedLocations = new bool[uniqueItemsCount];
			for (int i = 0; i < uniqueItemsCount; ++i)
				newSupportedLocations[i] = false;

			//customersSupportsBufCopy.Read(); // debug
			customersSupportsBufCopy.Read(tempValue, 0, 1);
			if (tempValue[0] < minSupport)
				moreCanBeFound = false;
			else if (supportedLocations.Count == 1)
				moreCanBeFound = false;

			if (moreCanBeFound)
			{
				int[] indices = new int[uniqueItemsCount];

				int[] locations = new int[uniqueItemsCount];
				Buffer<int> locationsBuf = new Buffer<int>(context, queue, locations);

				int currLength = 1;
				Buffer<int> currLengthBuf = new Buffer<int>(context, queue, 1);

				kernelMulitItemSupport.SetArgument(0, transactionsSupportsBufCopy);
				kernelMulitItemSupport.SetArguments(null, transactionsSupportsCountInt,
					transactionsCountInt);
				kernelMulitItemSupport.SetArgument(3, locationsBuf);
				kernelMulitItemSupport.SetArgument(4, currLengthBuf);

				kernelOr.SetArgument(0, transactionsSupportsBufCopy);
				kernelOr.SetArguments(null, transactionsSupportsCountInt, null, null);

				while (moreCanBeFound)
				{
					++currLength;
					currLengthBuf.Value = currLength;
					currLengthBuf.Write(false);

					uint litemsetOffset = (uint)litemsets.Count;

					for (int i = 0; i < currLength; ++i)
						indices[i] = i;
					int currIndex = currLength - 1;

					//queue.Finish(); // debug
					//supportsBuf.Read(); // debug only
					//supportsBakBuf.Read(); // debug only

					#region debugging
					//{ // debug
					//	indices[0] = 0;
					//	indices[1] = 5;
					//	indices[2] = 6;
					//	currLength = 3;
					//	currLengthBuf.Value = currLength;
					//	currLengthBuf.Write();
					//}
					#endregion

					//queue.Finish(); // debug
					//transactionsSupportsBufCopy.Read(); // debug

					while (true)
					{
						#region debugging
						//if (currLength == 2
						//	&& uniqueItems[supportedLocations[indices[0]]] == 2
						//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
						//	Abstract.Diagnostics = true;
						#endregion

						#region initialization of int[] locations
						for (int i = 0; i < currLength; ++i)
							locations[i] = supportedLocations[indices[i]];
						locationsBuf.Write(false, 0, (uint)currLength);
						#endregion

						#region copying of relevant parts of int[] transactionsSupports
						for (int i = 0; i < currLength; ++i)
						{
							#region debugging
							//if (currLength == 2
							//	&& uniqueItems[supportedLocations[indices[0]]] == 2
							//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
							//{
							//	// debug only
							//	queue.Finish();
							//	transactionsSupportsBufCopy.Read();
							//	//transactionsSupportsBuf.Read();
							//}
							#endregion

							int offset = supportedLocations[indices[i]] * transactionsCountInt;
							transactionsSupportsBuf.Copy((uint)offset, transactionsCountUInt,
								transactionsSupportsBufCopy, (uint)offset);
						}
						#endregion

						#region debugging
						//if (currLength == 2
						//	&& uniqueItems[supportedLocations[indices[0]]] == 2
						//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
						//{
						//	// debug only
						//	queue.Finish();
						//	transactionsSupportsBufCopy.Read();
						//	//transactionsSupportsBuf.Read();
						//}
						#endregion

						//queue.Finish(); // debug
						//transactionsSupportsBufCopy.Read(); // debug

						int stepNo = 1;
						while (stepNo < currLength)
						{
							#region debugging
							//if (currLength == 2
							//	&& uniqueItems[supportedLocations[indices[0]]] == 2
							//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
							//{
							//	// debug only
							//	queue.Finish();
							//	//Zero.Read();
							//	//One.Read();
							//	//emptyIdBuf.Read();
							//	//itemsSupportsBufCopy.Read();
							//	transactionsSupportsBufCopy.Read();
							//	//customersSupportsBufCopy.Read();
							//	//itemsSupportsBuf.Read();
							//	transactionsSupportsBuf.Read();
							//	//customersSupportsBuf.Read();
							//}
							#endregion

							kernelMulitItemSupport.SetArgument(5, stepNo);
							kernelMulitItemSupport.Launch2D(queue, transactionsCountUInt, 1, (uint)currLength, 1);
							stepNo *= 2;
							queue.Finish();
						}

						#region debugging
						//if (currLength == 2
						//	&& uniqueItems[supportedLocations[indices[0]]] == 2
						//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
						//{
						//	// debug only
						//	queue.Finish();
						//	//Zero.Read();
						//	//One.Read();
						//	//emptyIdBuf.Read();
						//	//itemsSupportsBufCopy.Read();
						//	transactionsSupportsBufCopy.Read();
						//	//customersSupportsBufCopy.Read();
						//	//itemsSupportsBuf.Read();
						//	transactionsSupportsBuf.Read();
						//	//customersSupportsBuf.Read();
						//}
						#endregion

						for (int cn = 0; cn < customersCountInt; ++cn)
						{
							kernelOr.SetArgument(2, supportedLocations[indices[0]] * transactionsCountInt + customersStarts[cn]);
							kernelOr.SetArgument(3, customersLengths[cn]);

							globalSize = (uint)customersLengths[cn];
							for (int scaling = 1; scaling < customersLengths[cn]; scaling *= (int)localSize)
							{
								kernelOr.SetArgument(4, scaling);
								globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
								kernelOr.Launch1D(queue, globalSize, localSize);
								globalSize /= localSize;
							}
						}

						kernelSum.SetArgument(0, transactionsSupportsBufCopy);
						kernelSum.SetArguments(null, transactionsSupportsCountInt, null, transactionsCountInt);

						//queue.Finish(); // debug
						//transactionsSupportsBufCopy.Read(); // debug

						int offset2 = supportedLocations[indices[0]] * transactionsCountInt;
						kernelSum.SetArgument(2, offset2);

						globalSize = transactionsCountUInt;
						for (int scaling = 1; scaling < transactionsCountInt; scaling *= (int)localSize)
						{
							kernelSum.SetArgument(4, scaling);
							globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
							kernelSum.Launch1D(queue, globalSize, localSize);
							globalSize /= localSize;
						}

						transactionsSupportsBufCopy.Read(tempValue, (uint)offset2, 1);

						if (tempValue[0] > transactionsSupportsCountInt)
						{
							// this indicates that data became corrupted due to:
							//  a) some gpu memory error,
							//  b) bad synchronization,
							//  c) or some other unknown thing
							// this situation occurs rarely, only with very large data
							// specific cause is unknown
							itemsSupportsBufCopy.Read();
							transactionsSupportsBufCopy.Read();
							customersSupportsBufCopy.Read();
							itemsSupportsBuf.Read();
							transactionsSupportsBuf.Read();
							customersSupportsBuf.Read();
							// this error is severe: it prevents algorithm from continuing
							throw new Exception(String.Format("{0} is greater than {1}, impossible!\n{2}",
								tempValue[0], transactionsSupportsCountInt, String.Join(",", transactionsSupports)));
						}

						#region debugging
						//if (currLength == 2
						//	&& uniqueItems[supportedLocations[indices[0]]] == 2
						//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
						//	Abstract.Diagnostics = false;
						//supportsBuf.Read(); // debug only
						//supportsBakBuf.Read(); // debug only
						#endregion

						if (tempValue[0] >= minSupport)
						{
							ILitemset l = new Litemset(new List<IItem>());
							l.SetSupport(tempValue[0]);
							for (int i = 0; i < currLength; ++i)
							{
								int index = indices[i];
								int spprtd = supportedLocations[index];
								l.AddItem(new Item(uniqueItems[spprtd]));
								newSupportedLocations[spprtd] = true;
							}

							#region debugging
							//if (currLength == 2 && l.Items.Count == 2
							//	&& l.Items[0].Value == 2 && l.Items[1].Value == 3)
							//{
							//	Zero.Read();
							//	One.Read();
							//	emptyIdBuf.Read();
							//	itemsSupportsBufCopy.Read();
							//	transactionsSupportsBufCopy.Read();
							//	customersSupportsBufCopy.Read();
							//	itemsSupportsBuf.Read();
							//	transactionsSupportsBuf.Read();
							//	customersSupportsBuf.Read();
							//	throw new Exception("this litemset is not supported");
							//}
							#endregion

							litemsets.Add(l);
						}

						#region selecting new indices (from supportedLocations) to analyze
						if ((currIndex == 0 && indices[currIndex] == supportedLocations.Count - currLength)
											|| currLength == supportedLocations.Count)
							break;
						if (indices[currIndex] == supportedLocations.Count - currLength + currIndex)
						{
							++indices[currIndex - 1];
							if (indices[currIndex - 1] + 1 < indices[currIndex])
							{
								while (currIndex < currLength - 1)
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
						#endregion
					}

					if (progressOutput)
						Log.WriteLine("at length {0}, so far found {1} litemsets in total", currLength, litemsets.Count);


					if (litemsets.Count <= litemsetOffset || currLength >= uniqueItemsCount)
					{
						moreCanBeFound = false;
						break;
					}

					supportedLocations.Clear();
					for (int i = 0; i < uniqueItemsCount; ++i)
					{
						if (newSupportedLocations[i])
						{
							supportedLocations.Add(i);
							newSupportedLocations[i] = false;
						}
					}

					if (currLength >= supportedLocations.Count)
					{
						// too few supported single elements to make a candidate of required length
						moreCanBeFound = false;
						break;
					}
				}

				queue.Finish();

				currLengthBuf.Dispose();
				locationsBuf.Dispose();
			}
			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Generated all litemsets, found {0} in {1}ms.", litemsets.Count, watch.ElapsedMilliseconds);
			}

			queue.Finish();

			#region disposing of buffers

			// input buffers
			itemsBuf.Dispose();
			transactionsStartsBuf.Dispose();
			customersStartsBuf.Dispose();
			itemsCountBuf.Dispose();
			transactionsCountBuf.Dispose();
			customersCountBuf.Dispose();

			// building itemsSupports
			emptyIdBuf.Dispose();

			// unique items storage
			uniqueItemBuf.Dispose();
			uniqueItemsBuf.Dispose();

			// supports storage
			itemsSupportsBuf.Dispose();
			itemsSupportsCountBuf.Dispose();
			itemsSupportsBufCopy.Dispose();
			transactionsSupportsBuf.Dispose();
			transactionsSupportsCountBuf.Dispose();
			transactionsSupportsBufCopy.Dispose();
			customersSupportsBuf.Dispose();
			customersSupportsCountBuf.Dispose();
			customersSupportsBufCopy.Dispose();

			// temporary memory
			tempItemsBuf.Dispose();
			itemsRemainingBuf.Dispose();

			// constants
			Zero.Dispose();
			One.Dispose();

			#endregion

			queue.Dispose();
			Kernels.Dispose();

			return litemsets;
		}

	}

	/// @}
}
