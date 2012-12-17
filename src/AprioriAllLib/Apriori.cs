using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenCL.Net;

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
	public class Apriori
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

		private Cl.Platform platform;

		private Cl.Device[] devices;

		private Cl.Device device;

		private Cl.Context context;

		/// <summary>
		/// Inditator whether source code of OpenCL kernels was already read by this instance.
		/// </summary>
		private bool clProgramsInitialized;

		private Cl.Program programBasicFunctions;

		/// <summary>
		/// Program created from file 'distinct.cl'.
		/// </summary>
		private Cl.Program programDistinct;

		/// <summary>
		/// Program created from file 'separation.cl'.
		/// </summary>
		private Cl.Program programSeparation;

		/// <summary>
		/// Program created from file 'support.cl'.
		/// </summary>
		private Cl.Program programSupport;

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

			Cl.ErrorCode err;

			Cl.Platform[] platforms = Cl.GetPlatformIDs(out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not get list of platforms");

			platform = platforms[0];

			devices = Cl.GetDeviceIDs(platform, Cl.DeviceType.All, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not get list of devices");

			device = devices[0];

			//Cl.ContextProperty[] contextProperties = new Cl.ContextProperty[1];
			//contextProperties[0] = Cl.ContextProperties.Platform;

			context = Cl.CreateContext(null, 1, devices, null, IntPtr.Zero, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create context");

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

			programBasicFunctions = OpenCLToolkit.GetAndBuildProgramFromLocalResource("basicFunctions.cl", context, device,
				progressOutput ? Console.Out : null);

			programDistinct = OpenCLToolkit.GetAndBuildProgramFromLocalResource("distinct.cl", context, device,
				progressOutput ? Console.Out : null);

			programSeparation = OpenCLToolkit.GetAndBuildProgramFromLocalResource("separation.cl", context, device,
				progressOutput ? Console.Out : null);

			programSupport = OpenCLToolkit.GetAndBuildProgramFromLocalResource("support.cl", context, device,
				progressOutput ? Console.Out : null);

			buildWatch.Stop();

			if (progressOutput)
				Trace.WriteLine(String.Format("Built OpenCL programs in {0}ms.", buildWatch.ElapsedMilliseconds));

			clProgramsInitialized = true;
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
			if (OpenCLChecker.PlatformsCount() == 0)
				return RunApriori(minimalSupport, progressOutput);

			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			minimalSupport *= customerList.Customers.Count;
			List<Litemset> litemsets = new List<Litemset>();

			InitOpenCL(progressOutput);
			InitOpenCLPrograms(progressOutput);

			Cl.ErrorCode err;

			Cl.CommandQueue queue = Cl.CreateCommandQueue(context, device, Cl.CommandQueueProperties.None, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create command queue");

			List<List<int>> allTransactions = new List<List<int>>();
			List<int> flatTransactions = new List<int>();
			List<int> transactionsLimits = new List<int>();
			{
				int limit = 0;
				foreach (Customer c in customerList.Customers)
					foreach (Transaction t in c.Transactions)
					{
						List<int> transaction = new List<int>();
						transactionsLimits.Add(limit);
						foreach (Item item in t.Items)
						{
							transaction.Add(item.Value);
							flatTransactions.Add(item.Value);
							++limit;
						}
						allTransactions.Add(transaction);
					}
			}

			//int transactionsCount = allTransactions.Count;
			//Cl.CreateImage2D(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,

			#region buffers initialization

			int[] items = flatTransactions.ToArray();
			IntPtr itemsBytes;
			Cl.Mem itemsBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				items, out itemsBytes);

			int[] itemsCount = new int[] { items.Length };
			IntPtr itemsCountBytes;
			Cl.Mem itemsCountBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				itemsCount, out itemsCountBytes);

			int[] itemsExcluded = new int[itemsCount[0]];
			IntPtr itemsExcludedBytes;
			Cl.Mem itemsExcludedBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				itemsExcluded, out itemsExcludedBytes);

			int[] newItemsExcluded = new int[itemsCount[0]];
			IntPtr newItemsExcludedBytes;
			Cl.Mem newItemsExcludedBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				newItemsExcluded, out newItemsExcludedBytes);

			int[] uniqueItems = new int[itemsCount[0]];
			IntPtr uniqueItemsBytes;
			Cl.Mem uniqueItemsBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				uniqueItems, out uniqueItemsBytes);

			int[] uniqueItemsCount = new int[] { 0 };
			IntPtr uniqueItemsCountBytes;
			Cl.Mem uniqueItemsCountBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				uniqueItemsCount, out uniqueItemsCountBytes);

			int[] discovered = new int[itemsCount[0]];
			IntPtr discoveredBytes;
			Cl.Mem discoveredBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				discovered, out discoveredBytes);

			int[] step = new int[] { 1 };
			IntPtr stepBytes;
			Cl.Mem stepBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				step, out stepBytes);

			#endregion

			#region kernels of basicFunctions initialization

			Cl.Kernel kernelZero = Cl.CreateKernel(programBasicFunctions, "assignZero", out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create kernel");

			Cl.Kernel kernelOr = Cl.CreateKernel(programBasicFunctions, "logicOr", out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create kernel");

			//OpenCLToolkit.UnsetKernelArgs(kernelZero);

			//OpenCLToolkit.SetKernelArgs(kernelZero, itemsBuf, itemsCountBuf, itemsExcludedBuf,
			//	uniqueItemsBuf, uniqueItemsCountBuf, discoveredBuf, stepBuf);

			#endregion

			#region kernelDistinct initialization

			Cl.Kernel kernelDistinct = Cl.CreateKernel(programDistinct, "findNewDistinctItem", out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create kernel");

			OpenCLToolkit.SetKernelArgs(kernelDistinct, itemsBuf, itemsCountBuf, itemsExcludedBuf, newItemsExcludedBuf,
				/*uniqueItemsBuf, uniqueItemsCountBuf,*/ discoveredBuf, stepBuf);

			#endregion

			#region kernelExcludeDistinct initialization

			Cl.Kernel kernelExcludeDistinct = Cl.CreateKernel(programDistinct, "excludeLatestDistinctItem", out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not create kernel");

			OpenCLToolkit.SetKernelArgs(kernelExcludeDistinct, itemsBuf, itemsCountBuf, itemsExcludedBuf,
				uniqueItemsBuf, uniqueItemsCountBuf);

			#endregion

			Cl.Event ev;
			IntPtr[] globalWorkSize = new IntPtr[] { new IntPtr(items.Length) };
			IntPtr[] localWorkSize = new IntPtr[] { new IntPtr(1) };

			if (progressOutput)
				Trace.WriteLine("Looking for unique items in all transactions.");

			while (true)
			{
				#region kernelDistinct launch

				if (progressOutput)
					Trace.Write(" launching kernelDistinct");

				step[0] = 1;
				OpenCLToolkit.WriteBuffer(queue, stepBuf, stepBytes, step);
				while (step[0] <= itemsCount[0])
				{
					OpenCLToolkit.LaunchKernel1D(queue, kernelDistinct, globalWorkSize, localWorkSize);

					OpenCLToolkit.ReadBuffer(queue, newItemsExcludedBuf, newItemsExcludedBytes, newItemsExcluded);
					OpenCLToolkit.ReadBuffer(queue, discoveredBuf, discoveredBytes, discovered);

					if (progressOutput)
						Trace.Write(String.Format(", step {0}", step[0]));

					step[0] *= 2;
					OpenCLToolkit.WriteBuffer(queue, stepBuf, stepBytes, step);
				}

				if (progressOutput)
					Trace.WriteLine("");

				#endregion

				// TODO: read 1 byte in a proper way, i.e. not using 'itemsCountBytes'
				err = Cl.EnqueueReadBuffer(queue, discoveredBuf, Cl.Bool.True, IntPtr.Zero,
					itemsBytes, discovered, 0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not read results from device memory");

				if (discovered[0] == 0)
					break;

				uniqueItems[uniqueItemsCount[0]] = discovered[0];
				err = Cl.EnqueueWriteBuffer(queue, uniqueItemsBuf, Cl.Bool.True, IntPtr.Zero,
					uniqueItemsBytes, uniqueItems, 0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not write to device memory");

				uniqueItemsCount[0] += 1;
				err = Cl.EnqueueWriteBuffer(queue, uniqueItemsCountBuf, Cl.Bool.True, IntPtr.Zero,
					uniqueItemsCountBytes, uniqueItemsCount, 0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not write to device memory");

				#region kernel 'zero'

				OpenCLToolkit.SetKernelArgs(kernelZero, discoveredBuf, itemsCountBuf);

				if (progressOutput)
					Trace.Write(" launching kernelZero");

				OpenCLToolkit.LaunchKernel1D(queue, kernelZero, globalWorkSize, localWorkSize);

				//err = Cl.EnqueueReadBuffer(queue, discoveredBuf, Cl.Bool.True, IntPtr.Zero,
				//	itemsBytes, discovered, 0, null, out ev);
				//if (!err.Equals(Cl.ErrorCode.Success))
				//	throw new Cl.Exception(err, "could not read results from device memory");

				#endregion

				#region kernel exclude launch

				if (progressOutput)
					Trace.WriteLine(" launching kernelExcludeDistinct");

				OpenCLToolkit.LaunchKernel1D(queue, kernelExcludeDistinct, globalWorkSize, localWorkSize);

				#endregion

				//err = Cl.EnqueueReadBuffer(queue, uniqueItemsBuf, Cl.Bool.True, IntPtr.Zero,
				//	uniqueItemsBytes, uniqueItems, 0, null, out ev);
				//if (!err.Equals(Cl.ErrorCode.Success))
				//	throw new Cl.Exception(err, "could not read results from device memory");

				OpenCLToolkit.ReadBuffer(queue, itemsExcludedBuf, itemsExcludedBytes, itemsExcluded);

				for (int i = 0; i < itemsCount[0]; ++i)
					newItemsExcluded[i] = itemsExcluded[i];

				OpenCLToolkit.WriteBuffer(queue, newItemsExcludedBuf, newItemsExcludedBytes, newItemsExcluded);

				//OpenCLToolkit.ReadBuffer(queue, newItemsExcludedBuf, newItemsExcludedBytes, newItemsExcluded);

				// add new excluded to those already existing
				//OpenCLToolkit.SetKernelArgs(kernelOr, newItemsExcludedBuf, itemsExcludedBuf, itemsCountBuf);
				//OpenCLToolkit.LaunchKernel1D(queue, kernelOr, globalWorkSize, localWorkSize);

			}

			kernelDistinct.Dispose();

			kernelExcludeDistinct.Dispose();

			if (progressOutput)
			{
				Trace.WriteLine(String.Format("Found all unique items: {0}.", 
					String.Join(", ", uniqueItems)));
			}

			if (progressOutput)
				Trace.WriteLine("Calculating support.");

			//if (progressOutput)
			//	Trace.WriteLine("Finished subset generation.");

			//Cl.InfoBuffer buf3 = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.HostPtr, out err);
			//Cl.InfoBuffer buf = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.Size, out err);
			//Cl.InfoBuffer buf2 = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.Type, out err);

			//int x = buf.CastTo<int>();

			#region more buffers initialization

			int[] supports = new int[itemsCount[0] * uniqueItemsCount[0]];
			IntPtr supportsBytes;
			Cl.Mem supportsBuf = OpenCLToolkit.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				supports, out supportsBytes);

			#endregion

			err = Cl.EnqueueReadBuffer(queue, supportsBuf, Cl.Bool.True, IntPtr.Zero,
				supportsBytes, supports, 0, null, out ev);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not read results from device memory");

			for (int i = 0; i < supports.Length; ++i)
			{
				if (supports[i] >= minimalSupport)
					litemsets.Add(new Litemset(supports[i], allTransactions.ElementAt(i).ToArray()));
			}

			if (progressOutput)
				Trace.WriteLine(String.Format("Generated all litemsets, found {0}.", litemsets.Count));

			return litemsets;
		}

	}

	/// @}
}

