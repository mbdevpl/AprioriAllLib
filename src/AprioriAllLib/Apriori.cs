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

		Kernels.Identity kernelCopy;
		Kernels.Identity kernelCopySingle;
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
				kernelCopy = new Kernels.Identity();
				kernelCopySingle = new Kernels.Identity();
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
				kernelCopy.Dispose();
				kernelCopySingle.Dispose();
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

			if (customerList == null)
				return null;

			if (customerList.Customers.Count == 0)
				return new List<Litemset>();

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

			//int cIndex = 0;
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

			if (customerList == null)
				return null;

			if (customerList.Customers.Count == 0)
				return new List<Litemset>();

			int minSupport = (int)Math.Ceiling((double)customerList.Customers.Count * minimalSupport);

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

			foreach (Customer c in customerList.Customers)
			{
				++customersCountInt;
				foreach (Transaction t in c.Transactions)
				{
					++transactionsCountInt;
					foreach (Item item in t.Items)
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
			//List<int> itemsList = new List<int>();
			//List<int> itemsTransactionsList = new List<int>();
			//List<int> itemsCustomersList = new List<int>();
			//List<int> transactionsStartsList = new List<int>();
			//List<int> transactionsLengthsList = new List<int>();
			//List<int> customersStartsList = new List<int>();
			//List<int> customersLengthsList = new List<int>();
			{
				int currItem = 0;
				int currTransaction = 0;
				int currCustomer = 0;

				int currTransactionLenth = 0;
				int currCustomerLength = 0;
				foreach (Customer c in customerList.Customers)
				{
					currCustomerLength = 0;
					customersStarts[currCustomer] = currTransaction; // customersStartsList.Add(currItem);
					foreach (Transaction t in c.Transactions)
					{
						currTransactionLenth = 0;
						transactionsStarts[currTransaction] = currItem; // transactionsStartsList.Add(currItem);
						foreach (Item item in t.Items)
						{
							items[currItem] = item.Value; // itemsList.Add(item.Value);
							itemsTransactions[currItem] = currTransaction; // itemsTransactionsList.Add(currTransaction);
							itemsCustomers[currItem] = currCustomer; // itemsCustomersList.Add(currCustomer);
							++currItem;
							++currTransactionLenth;
						}
						++currCustomerLength;
						transactionsLengths[currTransaction] = currTransactionLenth; // transactionsLengthsList.Add(currTransactionLenth);
						++currTransaction;
					}
					customersLengths[currCustomer] = currCustomerLength; // customersLengthsList.Add(currCustomerLength);
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

			InitOpenCL(progressOutput);

			CommandQueue queue = new CommandQueue(device, context);

			#region input buffers allocation and initialization

			//int[] items = itemsList.ToArray();
			Buffer<int> itemsBuf = new Buffer<int>(context, queue, items);
			//itemsBuf.Write(false);

			//int[] transactionsStarts = transactionsStartsList.ToArray();
			Buffer<int> transactionsStartsBuf = new Buffer<int>(context, queue, transactionsStarts);
			//transactionsStartsBuf.Write(false);

			//int[] customersStarts = customersStartsList.ToArray();
			Buffer<int> customersStartsBuf = new Buffer<int>(context, queue, customersStarts);
			//customersStartsBuf.Write(false);

			int[] itemsCount = new int[] { itemsCountInt };
			Buffer<int> itemsCountBuf = new Buffer<int>(context, queue, itemsCount);
			//itemsCountBuf.Write(false);

			int[] transactionsCount = new int[] { transactionsCountInt };
			Buffer<int> transactionsCountBuf = new Buffer<int>(context, queue, transactionsCount);
			//transactionsCountBuf.Write(false);

			int[] customersCount = new int[] { customersCountInt };
			Buffer<int> customersCountBuf = new Buffer<int>(context, queue, customersCount);
			//customersCountBuf.Write(false);

			#endregion

			// the below is an anti-optimization used only to measure buffer initialization time
			//queue.Finish(); 

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
			//Zero.Write(false);

			Buffer<int> One = new Buffer<int>(context, queue, new int[] { 1 });
			//Zero.Write(false);

			//queue.Finish();

			uint localSize = 32;

			Buffer<int> tempItemsBuf = new Buffer<int>(context, queue, itemsCountUInt);
			kernelCopy.SetArgument(0, itemsBuf);
			kernelCopy.SetArgument(4, tempItemsBuf);
			kernelCopy.SetArguments(null, itemsCountInt, 0, itemsCountInt,
				null, itemsCountInt, 0, itemsCountInt);
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			//Buffer<int> transactionsTempBuf = new Buffer<int>(context, queue, (uint)items.Length);

			//int[] itemsExcluded = new int[itemsCount[0]];
			//Buffer<int> itemsExcludedBuf = new Buffer<int>(context, queue, itemsExcluded);
			////Console.Out.WriteLine(String.Join(" ", itemsExcluded));
			//itemsCountBuf.Write(false);

			//int[] newItemsExcluded = new int[itemsCount[0]];
			//Buffer<int> newItemsExcludedBuf = new Buffer<int>(context, queue, newItemsExcluded);
			//newItemsExcludedBuf.Write(false);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Initialized OpenCL in {0}ms.", watch.ElapsedMilliseconds);
				watch.Restart();
			}

			#region finding empty id

			int[] tempValue = new int[] { -1 };

			int[] step = new int[] { 1 };
			Buffer<int> stepBuf = new Buffer<int>(context, queue, step);
			//kernelConst.SetArguments(stepBuf, One, Zero, One, One);
			//kernelConst.Launch1D(queue, 1, 1);

			//queue.Finish();
			kernelMax.SetArgument(0, tempItemsBuf);
			kernelMax.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			//kernelMultiplyByTwo.SetArgument(0, stepBuf);
			//stepBuf.Value = 1;
			uint globalSize = itemsCountUInt;
			for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;
			}
			//while (stepBuf.Value < itemsCountInt)
			//{
			//	kernelMax.Launch1D(queue, itemsCountUInt, 1);
			//	//kernelMultiplyByTwo.Launch(queue);
			//	stepBuf.Value *= 2;
			//}

			//queue.Finish();
			
			//tempItemsBuf.Read(tempValue, 0, 1); // debug
			
			int emptyId = -1;
			//emptyId = tempValue[0] + 1;
			Buffer<int> emptyIdBuf = new Buffer<int>(context, queue, 1);
			//emptyIdBuf.Value = emptyId;
			//emptyIdBuf.Write(false);
			kernelIncrement.SetArgument(0, tempItemsBuf);
			kernelIncrement.SetArguments(null, 1, 0, 1);
			kernelIncrement.Launch1D(queue, 1, 1);

			kernelCopySingle.SetArgument(0, tempItemsBuf);
			kernelCopySingle.SetArgument(4, emptyIdBuf);
			kernelCopySingle.SetArguments(null, 1, 0, 1, null, 1, 0, 1);
			kernelCopySingle.Launch1D(queue, 1, 1);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("empty id @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region finding distinct item ids

			Buffer<int> itemsRemainingBuf = new Buffer<int>(context, queue, itemsCountUInt);
			//kernelCopy.SetArguments(itemsBuf, itemsCountBuf, Zero, itemsCountBuf,
			//	itemsRemainingBuf, itemsCountBuf, Zero, itemsCountBuf);
			kernelCopy.SetArgument(4, itemsRemainingBuf);
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			kernelCopy.SetArgument(4, tempItemsBuf);
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			int uniqueItemsCount = 0;
			//Buffer<int> uniqueItemsCountBuf = new Buffer<int>(context, queue, uniqueItemsCount);
			//uniqueItemsCountBuf.Write(false);

			int[] uniqueItems = new int[itemsCountInt];
			Buffer<int> uniqueItemsBuf = new Buffer<int>(context, queue, uniqueItems);
			//uniqueItemsBuf.Write(false);

			kernelZero.SetArgument(0, uniqueItemsBuf);
			kernelZero.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			//queue.Finish();

			kernelCopy.SetArgument(0, itemsRemainingBuf);
			kernelCopy.SetArgument(4, tempItemsBuf);

			kernelMin.SetArgument(0, tempItemsBuf);
			kernelMin.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(0, itemsRemainingBuf);
			kernelSubstitute.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(4, emptyIdBuf);
			kernelSubstitute.SetArgument(5, tempItemsBuf);

			while (true)
			{
				//queue.Finish();
				//kernelMin.Launch(queue, tempItemsBuf);
				//kernelConst.Launch1D(queue, 1, 0);
				//stepBuf.Value = 1;
				//while (stepBuf.Value < itemsCountInt)
				//{
				//	kernelMin.Launch1D(queue, itemsCountUInt, 1);
				//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
				//	stepBuf.Value *= 2;
				//}
				/*uint*/ globalSize = itemsCountUInt;
				for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
				{
					kernelMin.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelMin.Launch1D(queue, globalSize, localSize);
					globalSize /= localSize;
				}

				//queue.Finish();
				if (emptyId == -1)
				{
					emptyIdBuf.Read();
					emptyId = emptyIdBuf.Value;
				}
				tempItemsBuf.Read(tempValue, 0, 1);
				int foundValue = tempValue[0];
				if (tempValue[0] == emptyId)
					break; // reached end of computation

				uniqueItems[uniqueItemsCount] = tempValue[0];
				uniqueItemsCount += 1;
				uniqueItemsBuf.Write(false, 0, (uint)uniqueItemsCount);

				//uniqueItemsCountBuf.Write(false);

				kernelSubstitute.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);
				//kernelSubstitute.Launch(queue, itemsRemainingBuf, emptyIdBuf, tempItemsBuf);

				//queue.Finish(); // debug only
				//itemsRemainingBuf.Read(); // debug only
				//tempItemsBuf.Read(); // debug only

				//kernelCopy.Launch(queue, itemsRemainingBuf, tempItemsBuf);
				kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);
			}

			uint uniqueItemsCountUInt = (uint)uniqueItemsCount;

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("distinct items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region finding supports for items

			int itemsSupportsCountInt = itemsCountInt * uniqueItemsCount;
			uint itemsSupportsCountUInt = (uint)itemsSupportsCountInt;

			int[] itemsSupportsCount = new int[] { itemsSupportsCountInt };
			Buffer<int> itemsSupportsCountBuf = new Buffer<int>(context, queue, itemsSupportsCount);
			//itemsSupportsCountBuf.Write(false);

			int[] itemsSupports = new int[itemsSupportsCountInt];
			Buffer<int> itemsSupportsBuf = new Buffer<int>(context, queue, itemsSupports);

			//Abstract.Diagnostics = true;

			//queue.Finish();
			kernelZero.SetArgument(0, itemsSupportsBuf);
			kernelZero.SetArguments(null, itemsSupportsCountInt, null, itemsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);

			//Buffer<int> supportsBuf = new Buffer<int>(context, queue, itemsSupports);
			//kernelZero.Launch(queue, supportsBuf);

			//Kernel kernelInitSupports = new Kernel(queue, programSupport, "supportInitial");
			//kernelInitSupports.SetArguments(itemsBuf, itemsCountBuf, uniqueItemsBuf, uniqueItemsCountBuf,
			//	supportsBuf);

			//queue.Finish();
			//kernelInitSupports.Launch2D((uint)itemsCount[0], 1, (uint)uniqueItemsCount[0], 1);

			//Buffer<int>[] uniqueItemBufs = new Buffer<int>[uniqueItemsCount];
			Buffer<int> uniqueItemBuf = new Buffer<int>(context, queue, 1);
			Buffer<int> offsetBuf = new Buffer<int>(context, queue, 1);

			kernelCopyIfEqual.SetArgument(0, itemsBuf);
			kernelCopyIfEqual.SetArgument(4, itemsSupportsBuf);
			kernelCopyIfEqual.SetArguments(null, itemsCountInt, 0, itemsCountInt,
				null, itemsSupportsCountInt, null, itemsCountInt);
			kernelCopyIfEqual.SetArgument(8, uniqueItemBuf);
			//kernelCopy.SetArguments(itemsBuf, itemsCountBuf, Zero, itemsCountBuf,
			//	itemsSupportsBuf, itemsSupportsCountBuf, Zero, itemsCountBuf);
			//kernelCopyIfEqual.SetCopiedValue(uniqueItemBuf);

			queue.Finish(); // TODO: this is probably useless
			int uniqueItemIndex = 0;
			for (int supportsOffset = 0; supportsOffset < itemsSupports.Length; supportsOffset += items.Length)
			{
				//uniqueItemBufs[uniqueItemIndex] = new Buffer<int>(context, queue, 1);
				//uniqueItemBufs[uniqueItemIndex].BackingCollection[0] = uniqueItems[uniqueItemIndex];
				//uniqueItemBufs[uniqueItemIndex].Write(false);
				uniqueItemBuf.Value = uniqueItems[uniqueItemIndex];
				uniqueItemBuf.Write(false);
				offsetBuf.Value = supportsOffset;
				offsetBuf.Write();
				kernelCopyIfEqual.SetArgument(6, offsetBuf.Value);

				//queue.Finish();
				kernelCopyIfEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);
				//kernelCopy.Launch(queue, itemsCountUInt);

				//kernelCopyIfEqual.Launch(queue, itemsBuf, 0, (uint)items.Length,
				//	itemsSupportsBuf, (uint)supportsOffset, (uint)items.Length);

				//queue.Finish(); // debug
				//uniqueItemBuf.Read(); // debug
				//offsetBuf.Read(); // debug
				//itemsSupportsBuf.Read(); // debug
				++uniqueItemIndex;
			}

			queue.Finish(); // TODO: probably useless
			//itemsSupportsBuf.Read(); // debug
			kernelSubstituteIfNotEqual.SetArgument(0, itemsSupportsBuf);
			kernelSubstituteIfNotEqual.SetArguments(null, itemsSupportsCountInt, 0, itemsSupportsCountInt);
			kernelSubstituteIfNotEqual.SetArgument(4, One);
			kernelSubstituteIfNotEqual.SetArgument(5, Zero);
			kernelSubstituteIfNotEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);
			//kernelSubstituteIfNotEqual.Launch(queue, supportsBuf, One, Zero);

			//queue.Finish(); // debug
			//itemsSupportsBuf.Read(); // debug

			//if (progressOutput)
			//{
			//	watch.Stop();
			//	Log.WriteLine("supports array @ {0}ms", watch.ElapsedMilliseconds);
			//	watch.Start();
			//}

			#endregion

			#region finding supports for transactions

			int transactionsSupportsCountInt = transactionsCountInt * uniqueItemsCount;
			uint transactionsSupportsCountUInt = (uint)transactionsSupportsCountInt;

			int[] transactionsSupportsCount = new int[] { transactionsSupportsCountInt };
			Buffer<int> transactionsSupportsCountBuf = new Buffer<int>(context, queue, transactionsSupportsCount);
			//transactionsSupportsCountBuf.Write(false);

			int[] transactionsSupports = new int[transactionsSupportsCountInt];
			Buffer<int> transactionsSupportsBuf = new Buffer<int>(context, queue, transactionsSupports);

			//queue.Finish();
			kernelZero.SetArgument(0, transactionsSupportsBuf);
			kernelZero.SetArguments(null, transactionsSupportsCountInt, null, transactionsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsSupportsCountUInt), localSize);

			Buffer<int> itemsSupportsBufCopy = new Buffer<int>(context, queue, itemsSupports);

			kernelCopy.SetArgument(0, itemsSupportsBuf);
			kernelCopy.SetArgument(4, itemsSupportsBufCopy);
			kernelCopy.SetArguments(null, itemsSupportsCountInt, 0, itemsSupportsCountInt,
				null, itemsSupportsCountInt, 0, itemsSupportsCountInt);
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);

			Buffer<int> includedCountBuf = new Buffer<int>(context, queue, 1);
			Buffer<int> outputOffsetBuf = new Buffer<int>(context, queue, 1);

			kernelSegmentedOr.SetArgument(0, itemsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, itemsSupportsCountInt, null/*offsetBuf*/, null/*includedCountBuf*/,
				null, itemsCountInt);

			kernelCopy.SetArgument(0, itemsSupportsBufCopy);
			kernelCopy.SetArgument(4, transactionsSupportsBuf);
			kernelCopy.SetArguments(null, itemsSupportsCountInt, null/*offsetBuf*/, 1,
				null, transactionsSupportsCountInt, null/*outputOffsetBuf*/, 1);

			//queue.Finish(); // debug
			//itemsSupportsBufCopy.Read(); // debug

			for (int tn = 0; tn < transactionsCountInt; ++tn)
			{
				offsetBuf.Value = transactionsStarts[tn];
				offsetBuf.Write(false);
				kernelSegmentedOr.SetArgument(2, offsetBuf.Value);
				includedCountBuf.Value = transactionsLengths[tn];
				includedCountBuf.Write(false);
				kernelSegmentedOr.SetArgument(3, includedCountBuf.Value);

				//queue.Finish();
				//for (int i = 0; i < uniqueItemsCount; ++i)
				//	kernelOr.Launch();

				//kernelSegmentedOr.Launch(queue, (uint)transactionsLengths[tn], uniqueItemsCountUInt);
				//kernelConst.Launch1D(queue, 1, 1);
				//stepBuf.Value = 1;
				//while (stepBuf.Value < transactionsLengths[tn])
				//{
				//	kernelSegmentedOr.Launch2D(queue, (uint)transactionsLengths[tn], 1, uniqueItemsCountUInt, 1);
				//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
				//	stepBuf.Value *= 2;
				//}
				itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				globalSize = (uint)transactionsLengths[tn];
				for (int scaling = 1; scaling < transactionsLengths[tn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
					itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				}

				//queue.Finish(); // debug
				//itemsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results

				kernelCopy.SetArgument(2, offsetBuf.Value);
				outputOffsetBuf.Value = tn;
				outputOffsetBuf.Write(false);
				kernelCopy.SetArgument(6, outputOffsetBuf.Value);
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					queue.Finish(); // TODO: probably useless
					kernelCopy.Launch1D(queue, 1, 1);

					//queue.Finish(); // debug
					//transactionsSupportsBuf.Read(); // debug

					if (i == uniqueItemsCount - 1)
						break;

					offsetBuf.Value += itemsCountInt;
					offsetBuf.Write(false);
					kernelCopy.SetArgument(2, offsetBuf.Value);

					outputOffsetBuf.Value += transactionsCountInt;
					outputOffsetBuf.Write(false);
					kernelCopy.SetArgument(6, outputOffsetBuf.Value);
				}
			}

			#endregion

			#region finding supports for customers

			int customersSupportsCountInt = customersCountInt * uniqueItemsCount;
			uint customersSupportsCountUInt = (uint)customersSupportsCountInt;

			int[] customersSupportsCount = new int[] { customersSupportsCountInt };
			Buffer<int> customersSupportsCountBuf = new Buffer<int>(context, queue, customersSupportsCount);
			//customersSupportsCountBuf.Write(false);

			int[] customersSupports = new int[customersSupportsCountInt];
			Buffer<int> customersSupportsBuf = new Buffer<int>(context, queue, customersSupports);

			queue.Finish(); // TODO: probably useless
			kernelZero.SetArgument(0, customersSupportsBuf);
			kernelZero.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, customersSupportsCountUInt), localSize);

			Buffer<int> transactionsSupportsBufCopy = new Buffer<int>(context, queue, transactionsSupports);

			kernelCopy.SetArgument(0, transactionsSupportsBuf);
			kernelCopy.SetArgument(4, transactionsSupportsBufCopy);
			kernelCopy.SetArguments(null, transactionsSupportsCountInt, 0, transactionsSupportsCountInt,
				null, transactionsSupportsCountInt, 0, transactionsSupportsCountInt);
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsSupportsCountUInt), localSize);

			// TODO: after all it may be not useless?
			//kernelCopy.SetArguments(transactionsSupportsBuf, transactionsSupportsCountBuf, Zero, transactionsSupportsCountBuf,
			//	transactionsSupportsBufCopy, transactionsSupportsCountBuf, Zero, transactionsSupportsCountBuf);
			//kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsSupportsCountUInt), localSize);

			kernelSegmentedOr.SetArgument(0, transactionsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, transactionsSupportsCountInt, null/*offsetBuf*/, null/*includedCountBuf*/,
				null, transactionsCountInt);

			kernelCopy.SetArgument(0, transactionsSupportsBufCopy);
			kernelCopy.SetArgument(4, customersSupportsBuf);
			kernelCopy.SetArguments(null, transactionsSupportsCountInt, null/*offsetBuf*/, 1,
				null, customersSupportsCountInt, null/*outputOffsetBuf*/, 1);

			//queue.Finish(); // debug
			transactionsSupportsBufCopy.Read(); // debug

			for (int cn = 0; cn < customersCountInt; ++cn)
			{
				offsetBuf.Value = customersStarts[cn];
				offsetBuf.Write(false);
				kernelSegmentedOr.SetArgument(2, offsetBuf.Value);
				includedCountBuf.Value = customersLengths[cn];
				includedCountBuf.Write(false);
				kernelSegmentedOr.SetArgument(3, includedCountBuf.Value);

				//queue.Finish();
				//kernelSegmentedOr.Launch(queue, (uint)customersLengths[cn], uniqueItemsCountUInt);
				//kernelConst.Launch1D(queue, 1, 0);
				//stepBuf.Value = 1;
				//while (stepBuf.Value < customersLengths[cn])
				//{
				//	kernelSegmentedOr.Launch2D(queue, (uint)customersLengths[cn], 1, uniqueItemsCountUInt, 1);
				//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
				//	stepBuf.Value *= 2;
				//}
				globalSize = (uint)customersLengths[cn];
				for (int scaling = 1; scaling < customersLengths[cn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
					//transactionsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results
				}

				//queue.Finish(); // debug
				//transactionsSupportsBufCopy.Read(); // debug ONLY - causes incorrect results

				kernelCopy.SetArgument(2, offsetBuf.Value);
				outputOffsetBuf.Value = cn;
				outputOffsetBuf.Write(false);
				kernelCopy.SetArgument(6, outputOffsetBuf.Value);
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					queue.Finish();
					kernelCopy.Launch1D(queue, 1, 1);

					//queue.Finish(); // debug
					//customersSupportsBuf.Read(); // debug

					if (i == uniqueItemsCount - 1)
						break;

					offsetBuf.Value += transactionsCountInt;
					offsetBuf.Write(false);
					kernelCopy.SetArgument(2, offsetBuf.Value);

					outputOffsetBuf.Value += customersCountInt;
					outputOffsetBuf.Write(false);
					kernelCopy.SetArgument(6, outputOffsetBuf.Value);
				}
			}

			#endregion

			#region litemsets of size 1

			Buffer<int> customersSupportsBufCopy = new Buffer<int>(context, queue, customersSupports);

			kernelCopy.SetArgument(0, customersSupportsBuf);
			kernelCopy.SetArgument(4, customersSupportsBufCopy);
			kernelCopy.SetArguments(null, customersSupportsCountInt, 0, customersSupportsCountInt,
				null, customersSupportsCountInt, 0, customersSupportsCountInt);
			//customersSupportsBuf.Read(); // debug
			//customersSupportsBufCopy.Read(); // debug
			kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, customersSupportsCountUInt), localSize);
			customersSupportsBuf.Read(); // debug
			//customersSupportsBufCopy.Read(); // debug

			includedCountBuf.Value = customersCountInt;
			includedCountBuf.Write(false);

			//kernelSum.SetArgument(3, includedCountBuf.Value);
			kernelSum.SetArgument(0, customersSupportsBufCopy);
			kernelSum.SetArguments(null, customersSupportsCountInt, null/*offsetBuf*/, includedCountBuf.Value);

			queue.Finish(); // TODO: probably useless
			List<Litemset> litemsets = new List<Litemset>();
			List<int> supportedLocations = new List<int>();

			//queue.Finish(); // debug
			//customersSupportsBuf.Read(); // debug

			for (int un = 0; un < uniqueItemsCount; ++un)
			{
				offsetBuf.Value = un * customersCountInt;
				offsetBuf.Write(false);
				kernelSum.SetArgument(2, offsetBuf.Value);

				//queue.Finish();
				//kernelSum.Launch(queue, customersCountUInt);
				//kernelConst.Launch1D(queue, 1, 1);
				//stepBuf.Value = 1;
				//while (stepBuf.Value < customersCountUInt)
				//{
				//	kernelSum.Launch1D(queue, customersCountUInt, 1);
				//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
				//	stepBuf.Value *= 2;
				//}
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

				//queue.Finish();
				customersSupportsBufCopy.Read(tempValue, (uint)offsetBuf.Value, 1);

				//queue.Finish();
				int support = tempValue[0];
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

			#region litemsets of size 2 and above

			//int[] tempSupport = new int[] { 0 };
			bool moreCanBeFound = true;

			//queue.Finish();
			//kernelMax.Launch1D(queue, customersSupportsCountUInt);
			kernelMax.SetArgument(0, customersSupportsBufCopy);
			kernelMax.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			//kernelConst.Launch1D(queue, 1, 0);
			////stepBuf.Read(); // debug
			//stepBuf.Value = 1;
			//while (stepBuf.Value < customersSupportsCountInt)
			//{
			//	kernelMax.Launch1D(queue, customersSupportsCountUInt, 1);
			//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
			//	//stepBuf.Read(); // debug
			//	stepBuf.Value *= 2;
			//}
			globalSize = customersSupportsCountUInt;
			for (int scaling = 1; scaling < customersSupportsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;
			}

			bool[] newSupportedLocations = new bool[uniqueItemsCount];
			for (int i = 0; i < uniqueItemsCount; ++i)
				newSupportedLocations[i] = false;

			queue.Finish();
			customersSupportsBufCopy.Read(tempValue, 0, 1);

			//customersSupportsBufCopy.Read(); // debug
			//queue.Finish();
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
				//int[] currLengthArr = new int[]{ currLength };
				Buffer<int> currLengthBuf = new Buffer<int>(context, queue, 1);

				kernelMulitItemSupport.SetArguments(transactionsSupportsBufCopy, transactionsSupportsCountBuf,
					transactionsCountBuf, /*uniqueItemsBuf,*/ locationsBuf, currLengthBuf, stepBuf);

				kernelCopy.SetArgument(0, transactionsSupportsBuf);
				kernelCopy.SetArgument(4, transactionsSupportsBufCopy);
				kernelCopy.SetArguments(null, transactionsSupportsCountInt, null/*offsetBuf*/, null/*includedCountBuf*/,
					null, transactionsSupportsCountInt, null/*offsetBuf*/, null/*includedCountBuf*/);

				kernelOr.SetArgument(0, transactionsSupportsBufCopy);
				kernelOr.SetArguments(null, transactionsSupportsCountInt, null/*offsetBuf*/, null/*includedCountBuf*/);

				while (moreCanBeFound)
				{
					++currLength;
					currLengthBuf.Value = currLength;
					currLengthBuf.Write(false);

					//uint itemsLength = (uint)items.Length;
					uint litemsetOffset = (uint)litemsets.Count;

					//indices.Clear();
					for (int i = 0; i < currLength; ++i)
						indices[i] = i;
					int currIndex = currLength - 1;

					//queue.Finish(); // debug
					//supportsBuf.Read(); // debug only
					//supportsBakBuf.Read(); // debug only

					//{ // debug
					//	indices[0] = 0;
					//	indices[1] = 5;
					//	indices[2] = 6;
					//	currLength = 3;
					//	currLengthBuf.Value = currLength;
					//	currLengthBuf.Write();
					//}

					//queue.Finish(); // debug
					//transactionsSupportsBufCopy.Read(); // debug

					while (true)
					{
						for (int i = 0; i < currLength; ++i)
							locations[i] = supportedLocations[indices[i]];
						locationsBuf.Write(false, 0, (uint)currLength);

						includedCountBuf.Value = transactionsCountInt;
						includedCountBuf.Write(false);
						kernelCopy.SetArgument(3, includedCountBuf.Value);
						kernelCopy.SetArgument(7, includedCountBuf.Value);
						for (int i = 0; i < currLength; ++i)
						{
							offsetBuf.Value = supportedLocations[indices[i]] * transactionsCountInt;
							offsetBuf.Write();
							kernelCopy.SetArgument(2, offsetBuf.Value);
							kernelCopy.SetArgument(6, offsetBuf.Value);
							//queue.Finish();
							kernelCopy.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsCountUInt), localSize);
						}

						//queue.Finish(); // debug
						//transactionsSupportsBufCopy.Read(); // debug

						//queue.Finish();
						stepBuf.Value = 1;
						while (stepBuf.Value < currLength)
						{
							stepBuf.Write(queue);
							kernelMulitItemSupport.Launch(queue, transactionsCountUInt, (uint)currLength);
							stepBuf.BackingCollection[0] *= 2;

							//queue.Finish(); // debug
							//transactionsSupportsBufCopy.Read(); // debug
						}

						for (int cn = 0; cn < customersCountInt; ++cn)
						{
							offsetBuf.Value = supportedLocations[indices[0]] * transactionsCountInt + customersStarts[cn];
							offsetBuf.Write(false);
							kernelOr.SetArgument(2, offsetBuf.Value);
							includedCountBuf.Value = customersLengths[cn];
							includedCountBuf.Write();
							kernelOr.SetArgument(3, includedCountBuf.Value);

							//queue.Finish();
							//kernelOr.Launch(queue, (uint)customersLengths[cn]);
							//kernelConst.Launch1D(queue, 1, 0);
							//stepBuf.Value = 1;
							//while (stepBuf.Value < customersLengths[cn])
							//{
							//	kernelOr.Launch1D(queue, (uint)customersLengths[cn], 1);
							//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
							//	stepBuf.Value *= 2;
							//}
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

						offsetBuf.Value = supportedLocations[indices[0]] * transactionsCountInt;
						offsetBuf.Write();
						kernelSum.SetArgument(2, offsetBuf.Value);

						//queue.Finish();
						//kernelSum.Launch(queue, transactionsCountUInt);
						//kernelConst.Launch1D(queue, 1, 1);
						//stepBuf.Value = 1;
						//while (stepBuf.Value < transactionsCountInt)
						//{
						//	kernelSum.Launch1D(queue, transactionsCountUInt, 1);
						//	kernelMultiplyByTwo.Launch1D(queue, 0, 0);
						//	stepBuf.Value *= 2;
						//}
						globalSize = transactionsCountUInt;
						for (int scaling = 1; scaling < transactionsCountInt; scaling *= (int)localSize)
						{
							kernelSum.SetArgument(4, scaling);
							globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
							kernelSum.Launch1D(queue, globalSize, localSize);
							globalSize /= localSize;
						}

						//queue.Finish();
						transactionsSupportsBufCopy.Read(tempValue, (uint)offsetBuf.Value, 1);

						//int locInit = supportedLocations[indices[0]];
						//kernelCopy.Launch(queue, supportsBakBuf, (uint)locInit * itemsLength, itemsLength,
						//	itemsSupportsBuf, 0, itemsLength);

						//for (int n = 1; n < indices.Count; ++n)
						//{
						//	int locN = supportedLocations[indices[n]];
						//	queue.Finish();
						//	//supportsBuf.Read(); // debug only
						//	//supportsBakBuf.Read(); // debug only
						//	kernelPairwiseAnd.Launch(queue, supportsBakBuf, (uint)locN * itemsLength, itemsLength,
						//		itemsSupportsBuf, 0, itemsLength,
						//		itemsSupportsBuf, 0, itemsLength);
						//}

						//queue.Finish();
						//kernelSum.Launch(queue, itemsSupportsBuf, 0, itemsLength);

						//queue.Finish();
						//itemsSupportsBuf.Read(false, 0, 1);

						//queue.Finish();
						//supportsBuf.Read(); // debug only
						//supportsBakBuf.Read(); // debug only
						if (tempValue[0] >= minSupport)
						{
							Litemset l = new Litemset(new List<Item>());
							l.Support = tempValue[0];
							for (int i = 0; i < currLength; ++i)
							//foreach (int index in indices)
							{
								int index = indices[i];
								int spprtd = supportedLocations[index];
								l.Items.Add(new Item(uniqueItems[spprtd]));
								newSupportedLocations[spprtd] = true;
							}
							litemsets.Add(l);
						}

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
					}

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

					//break; // only temporarily
				}

				queue.Finish();

				currLengthBuf.Dispose();
				locationsBuf.Dispose();
			}
			#endregion

			//kernelMax.RestoreInternalArguments();
			//kernelCopy.RestoreInternalArguments();
			//kernelZero.RestoreInternalArguments();
			//kernelSubstitute.RestoreInternalArguments();
			//kernelMin.RestoreInternalArguments();
			//kernelCopyIfEqual.RestoreInternalArguments();
			//kernelSubstituteIfNotEqual.RestoreInternalArguments();
			//kernelSegmentedOr.RestoreInternalArguments();
			//kernelSum.RestoreInternalArguments();

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

			stepBuf.Dispose();
			includedCountBuf.Dispose();
			offsetBuf.Dispose();
			outputOffsetBuf.Dispose();

			// constants
			Zero.Dispose();
			One.Dispose();

			#endregion

			queue.Dispose();
			Kernels.Dispose();

			return litemsets;
			//return null;
		}

	}

	/// @}
}
