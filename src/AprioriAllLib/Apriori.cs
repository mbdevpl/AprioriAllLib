using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
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

		private Program programBasicFunctions;

		/// <summary>
		/// Program created from file 'distinct.cl'.
		/// </summary>
		private Program programDistinct;

		/// <summary>
		/// Program created from file 'separation.cl'.
		/// </summary>
		private Program programSeparation;

		/// <summary>
		/// Program created from file 'support.cl'.
		/// </summary>
		private Program programSupport;

		//private bool clKernelsInitialized;

		//private Cl.Kernel kernel;

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

			initWatch.Stop();

			if (progressOutput)
				Trace.WriteLine(String.Format("Initialized OpenCL in {0}ms.", initWatch.ElapsedMilliseconds));

			clInitialized = true;
		}

		protected void InitOpenCLPrograms(bool progressOutput)
		{
			if (clProgramsInitialized)
				return;

			Stopwatch buildWatch = new Stopwatch();
			buildWatch.Start();

			ResourceManager manager = AprioriAllLib.Properties.Resources.ResourceManager;

			programBasicFunctions = new Program(context, new SourceCode(manager, "cl_basicFunctions"));
			programBasicFunctions.Build(device);
			programDistinct = new Program(context, new SourceCode(manager, "cl_distinct"));
			programDistinct.Build(device);
			programSeparation = new Program(context, new SourceCode(manager, "cl_separation"));
			programSeparation.Build(device);
			programSupport = new Program(context, new SourceCode(manager, "cl_support"));
			programSupport.Build(device);

			buildWatch.Stop();

			if (progressOutput)
				Trace.WriteLine(String.Format("Built OpenCL programs in {0}ms.", buildWatch.ElapsedMilliseconds));

			clProgramsInitialized = true;
		}

		public void Dispose()
		{
			if (clProgramsInitialized)
			{
				programBasicFunctions.Dispose();
				programDistinct.Dispose();
				programSeparation.Dispose();
				programSupport.Dispose();
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
				Trace.WriteLine(String.Format("Finished subset generation, found {0}.", litemsets.Count));

			// rewrite the litemsets with support >= minimum to a new list
			List<Litemset> properLitemsets = new List<Litemset>();
			foreach (Litemset litemset in litemsets)
				if (litemset.Support >= minimalSupport)
					properLitemsets.Add(litemset);

			if (progressOutput)
				Trace.WriteLine(String.Format("Purged unsupported litemsets, {0} remain.", properLitemsets.Count));

			properLitemsets.Sort();

			if (progressOutput)
				Trace.WriteLine("Sorted output.");

			return properLitemsets;
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

			//Abstract.Diagnostics = false;

			InitOpenCL(progressOutput);
			InitOpenCLPrograms(progressOutput);

			CommandQueue queue = new CommandQueue(device, context);

			#region conversion of input data to regular int arrays

			//List<List<int>> allTransactions = new List<List<int>>();
			List<int> flatTransactions = new List<int>();
			List<int> flatTransactionsIds = new List<int>();
			List<int> flatUsersIds = new List<int>();
			//List<int> transactionsLimits = new List<int>();
			{
				int currentUserId = 0;
				int currentTransactionId;
				foreach (Customer c in customerList.Customers)
				{
					currentTransactionId = 0;
					foreach (Transaction t in c.Transactions)
					{
						//List<int> transaction = new List<int>();
						//transactionsLimits.Add(limit);
						foreach (Item item in t.Items)
						{
							//transaction.Add(item.Value);
							flatTransactions.Add(item.Value);
							flatTransactionsIds.Add(currentTransactionId);
							flatUsersIds.Add(currentUserId);
							//++limit;
						}
						//allTransactions.Add(transaction);
						++currentTransactionId;
					}
					++currentUserId;
				}
			}

			#endregion

			#region buffers initialization

			int[] items = flatTransactions.ToArray();
			Buffer<int> itemsBuf = new Buffer<int>(context, queue, items);
			itemsBuf.Write();

			int[] itemsCount = new int[] { items.Length };
			Buffer<int> itemsCountBuf = new Buffer<int>(context, queue, itemsCount);
			itemsCountBuf.Write();

			int[] itemsExcluded = new int[itemsCount[0]];
			Buffer<int> itemsExcludedBuf = new Buffer<int>(context, queue, itemsExcluded);
			Console.Out.WriteLine(String.Join(" ", itemsExcluded));
			itemsCountBuf.Write(); // TODO: kernelZero here

			int[] newItemsExcluded = new int[itemsCount[0]];
			Buffer<int> newItemsExcludedBuf = new Buffer<int>(context, queue, newItemsExcluded);
			newItemsExcludedBuf.Write(); // TODO: kernelZero here

			int[] uniqueItems = new int[itemsCount[0]];
			Buffer<int> uniqueItemsBuf = new Buffer<int>(context, queue, uniqueItems);
			uniqueItemsBuf.Write(); // TODO: kernelZero here

			int[] uniqueItemsCount = new int[] { 0 };
			Buffer<int> uniqueItemsCountBuf = new Buffer<int>(context, queue, uniqueItemsCount);
			uniqueItemsCountBuf.Write(); // TODO: kernelZero here

			int[] discovered = new int[itemsCount[0]];
			Buffer<int> discoveredBuf = new Buffer<int>(context, queue, discovered);
			discoveredBuf.Write(); // TODO: kernelZero here

			int[] step = new int[] { 1 };
			Buffer<int> stepBuf = new Buffer<int>(context, queue, step);

			#endregion

			#region distinct items finding

			Kernel kernelZero = new Kernel(programBasicFunctions, "assignZero");
			Kernel kernelOr = new Kernel(programBasicFunctions, "logicOr");

			//Abstract.Diagnostics = true;

			Kernel kernelDistinct = new Kernel(programDistinct, "findNewDistinctItem");
			kernelDistinct.SetArguments(itemsBuf, itemsCountBuf, itemsExcludedBuf, newItemsExcludedBuf,
				discoveredBuf, stepBuf);

			Kernel kernelExcludeDistinct = new Kernel(programDistinct, "excludeLatestDistinctItem");
			kernelExcludeDistinct.SetArguments(itemsBuf, itemsCountBuf, itemsExcludedBuf,
				uniqueItemsBuf, uniqueItemsCountBuf);

			if (progressOutput)
				Trace.WriteLine("Looking for unique items in all transactions.");

			while (true)
			{
				#region kernelDistinct launch

				if (progressOutput)
					Trace.Write(" launching kernelDistinct");

				step[0] = 1;
				while (step[0] <= itemsCount[0])
				{
					stepBuf.Write();
					kernelDistinct.Launch1D(queue, (uint)items.Length, (uint)1);

					//queue.Finish();

					//newItemsExcludedBuf.Read(); // only for debugging
					//discoveredBuf.Read(); // only for debugging

					if (progressOutput)
						Trace.Write(String.Format(", step {0}", step[0]));

					step[0] *= 2;
				}

				if (progressOutput)
					Trace.WriteLine("");

				#endregion

				queue.Finish();

				discoveredBuf.Read(0, 1);

				if (discovered[0] == 0)
					break;

				uniqueItems[uniqueItemsCount[0]] = discovered[0];
				uniqueItemsBuf.Write();

				uniqueItemsCount[0] += 1;
				uniqueItemsCountBuf.Write();

				kernelZero.SetArguments(discoveredBuf, itemsCountBuf);

				if (progressOutput)
					Trace.Write(" launching kernelZero");

				kernelZero.Launch1D(queue, (uint)itemsCount[0], 1);

				if (progressOutput)
					Trace.WriteLine(" launching kernelExcludeDistinct");

				kernelExcludeDistinct.Launch1D(queue, (uint)itemsCount[0], 1);

				queue.Finish();

				itemsExcludedBuf.Read();

				for (int i = 0; i < itemsCount[0]; ++i)
					newItemsExcluded[i] = itemsExcluded[i];

				newItemsExcludedBuf.Write();
			}

			queue.Finish();

			if (progressOutput)
				Trace.WriteLine(String.Format("Found all unique items: {0}.",
					String.Join(", ", uniqueItems)));

			kernelDistinct.Dispose();
			kernelExcludeDistinct.Dispose();

			kernelOr.Dispose();
			kernelZero.Dispose();

			#endregion

			if (progressOutput)
				Trace.WriteLine("Calculating support of each unique item.");

			//if (progressOutput)
			//	Trace.WriteLine("Finished subset generation.");

			#region more buffers initialization

			int[] itemsTransactions = flatUsersIds.ToArray();
			Buffer<int> itemsTransactionsBuf = new Buffer<int>(context, queue, itemsTransactions);

			int[] supports = new int[itemsCount[0] * uniqueItemsCount[0]];
			Buffer<int> supportsBuf = new Buffer<int>(context, queue, supports);

			#endregion

			#region finding support for single items

			Kernel kernelInitSupports = new Kernel(queue, programSupport, "supportInitial");
			kernelInitSupports.SetArguments(itemsBuf, itemsCountBuf, uniqueItemsBuf, uniqueItemsCountBuf,
				supportsBuf);

			kernelInitSupports.Launch2D((uint)itemsCount[0], 1, (uint)uniqueItemsCount[0], 1);

			//supportsBuf.Read(); // only for debugging

			Kernel kernelSupports = new Kernel(queue, programSupport, "supportDuplicatesRemoval");
			kernelSupports.SetArguments(itemsBuf, itemsCountBuf, itemsTransactionsBuf,
				uniqueItemsBuf, uniqueItemsCountBuf, stepBuf, supportsBuf);

			step[0] = 1;
			while (step[0] < itemsCount[0])
			{
				stepBuf.Write();

				kernelSupports.Launch2D((uint)itemsCount[0], 1, (uint)uniqueItemsCount[0], 1);

				//supportsBuf.Read(); // only for debugging
				step[0] *= 2;
			}

			queue.Finish();

			kernelInitSupports.Dispose();
			kernelSupports.Dispose();

			Kernels.Device = device;
			Kernels.Context = context;
			Kernels.Sum sumSupports = new Kernels.Sum();
			uint itemsCountUint = (uint)itemsCount[0];
			for (uint offset = 0; offset < supports.Length; offset += itemsCountUint)
				sumSupports.Launch(queue, supportsBuf, offset, itemsCountUint);
			supportsBuf.Read();

			queue.Finish();

			sumSupports.Dispose();

			#endregion

			List<Litemset> litemsets = new List<Litemset>();
			int index = 0;
			for (int i = 0; i < uniqueItemsCount[0]; ++i)
			{
				supportsBuf.Read((uint)index, 1);
				if (supports[index] >= minSupport)
					litemsets.Add(new Litemset(supports[index], uniqueItems[i]));
				index += itemsCount[0];
			}

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
				Trace.WriteLine(String.Format("Generated all litemsets, found {0}.", litemsets.Count));

			queue.Finish();

			itemsBuf.Dispose();
			itemsCountBuf.Dispose();
			itemsExcludedBuf.Dispose();
			newItemsExcludedBuf.Dispose();
			uniqueItemsBuf.Dispose();
			uniqueItemsCountBuf.Dispose();
			discoveredBuf.Dispose();
			stepBuf.Dispose();

			itemsTransactionsBuf.Dispose();
			supportsBuf.Dispose();

			Kernels.Dispose();
			queue.Dispose();

			return litemsets;
		}

	}

	/// @}
}

