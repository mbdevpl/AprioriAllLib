using System;
using System.Collections.Generic;
using System.Linq;

using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// This class is used for calculating litemsets from a set of customers' transactions
	/// 
	/// Authors of the implementation:
	/// Serialized version by Karolina Baltyn
	/// Parallel version by Mateusz Bysiek
	/// </summary>
	public class Apriori
	{
		private CustomerList customerList;

		private bool clInitialized;

		private Cl.Platform platform;

		private Cl.Device[] devices;

		private Cl.Device device;

		private Cl.Context context;

		private bool clProgramsInitialized;

		private Cl.Program program;

		private bool clKernelsInitialized;

		private Cl.Kernel kernel;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="customerList">CustomerList object containing a list of Customers from a database</param>
		public Apriori(CustomerList customerList)
		{
			this.customerList = customerList;

			clInitialized = false;
			clProgramsInitialized = false;
			clKernelsInitialized = false;
		}

		private void InitOpenCL(bool progressOutput)
		{
			if (clInitialized)
				return; // already initialized

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

			if (progressOutput)
				Console.Out.WriteLine("Initialized OpenCL.");

			clInitialized = true;
		}

		private void InitOpenCLKernels(bool progressOutput)
		{
			Cl.ErrorCode err;

			if (!clProgramsInitialized)
			{

				string[] sourceCode = System.IO.File.ReadAllLines("subsets.cl");
				IntPtr[] lenghts = new IntPtr[sourceCode.Length];

				for (int i = 0; i < sourceCode.Length; ++i)
				{
					sourceCode[i] += "\n";
					lenghts[i] = new IntPtr(sourceCode[i].Length);
				}

				program = Cl.CreateProgramWithSource(context, (uint)sourceCode.Length, sourceCode, lenghts, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create program");

				clProgramsInitialized = true;
			}
			if (!clKernelsInitialized)
			{
				err = Cl.BuildProgram(program, 1, devices, "", null, IntPtr.Zero);
				if (!err.Equals(Cl.ErrorCode.Success))
				{
					if (progressOutput)
					{
						Console.Out.WriteLine("Build failed.");
						Console.Out.WriteLine(OpenCLChecker.GetBuildInfo(program, device));
					}
					throw new Cl.Exception(err, "could not build program");
				}

				kernel = Cl.CreateKernel(program, "countSubsetsSupport", out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create kernel");

				if (progressOutput)
					Console.Out.WriteLine("Compiled kernels.");

				clKernelsInitialized = true;
			}
		}

		/// <summary>
		/// Produces all subsets of a given list of items in a transaction.
		/// </summary>
		/// <param name="items">list of items contained in one transaction</param>
		/// <returns>list of candidates for litemsets (large itemsets)</returns>
		private List<Litemset> generateCandidates(List<Item> items)
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
		/// Finds all litemsets that have the minimal support
		/// </summary>
		/// <param name="minimalSupport">Minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> FindOneLitemsets(double minimalSupport)
		{
			return FindOneLitemsets(minimalSupport, false);
		}

		/// <summary>
		/// Finds all litemsets that have the minimal support
		/// </summary>
		/// <param name="minimalSupport">Minimal support</param>
		/// <param name="parallel">if true, executed</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> FindOneLitemsets(double minimalSupport, bool parallel)
		{
			if (parallel)
				return ExecuteParallel(minimalSupport);
			else
				return Execute(minimalSupport);
		}

		/// <summary>
		/// Finds all litemsets that have the minimal support.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> Execute(double minimalSupport)
		{
			return Execute(minimalSupport, false);
		}

		/// <summary>
		/// Finds all litemsets that have the minimal support.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> Execute(double minimalSupport, bool progressOutput)
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
					List<Litemset> candidateLitemsets = generateCandidates(t.Items);

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

			// rewrite the litemsets with support >= minimum to a new list
			List<Litemset> properLitemsets = new List<Litemset>();
			foreach (Litemset litemset in litemsets)
				if (litemset.Support >= minimalSupport)
					properLitemsets.Add(litemset);

			properLitemsets.Sort();

			return properLitemsets;
		}

		/// <summary>
		/// Executes parallel version of the Apriori algorithm.
		/// </summary>
		/// <param name="minimalSupport">minimal support</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> ExecuteParallel(double minimalSupport)
		{
			return ExecuteParallel(minimalSupport, false);
		}

		/// <summary>
		/// Executes parallel version of the Apriori algorithm.
		/// </summary>
		/// <param name="minimalSupport"></param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>A list of Litemsets with support >= minimalSupport</returns>
		public List<Litemset> ExecuteParallel(double minimalSupport, bool progressOutput)
		{
			if (OpenCLChecker.PlatformsCount() == 0)
				return Execute(minimalSupport, progressOutput);

			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			minimalSupport *= customerList.Customers.Count;
			List<Litemset> litemsets = new List<Litemset>();

			InitOpenCL(progressOutput);

			InitOpenCLKernels(progressOutput);

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


			int[] setsCount = new int[] { allTransactions.Count };
			IntPtr setsCountLen = new IntPtr(setsCount.Length * sizeof(int));
			Cl.Mem setsCountBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				setsCountLen, setsCount, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not initialize buffer");

			int[] sets = flatTransactions.ToArray();
			IntPtr setsLen = new IntPtr(sets.Length * sizeof(int));
			Cl.Mem setsBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				setsLen, sets, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not initialize buffer");

			int[] setSizes = transactionsLimits.ToArray();
			IntPtr setSizesLen = new IntPtr(setSizes.Length * sizeof(int));
			Cl.Mem setSizesBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				new IntPtr(transactionsLimits.Count * sizeof(int)), setSizes, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not initialize buffer");

			int[] subsetSize = new int[] { 1 };
			IntPtr subsetSizeLen = new IntPtr(subsetSize.Length * sizeof(int));
			Cl.Mem subsetSizeBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
				subsetSizeLen, subsetSize, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not initialize buffer");

			int[] supports = new int[allTransactions.Count];
			IntPtr supportsLen = new IntPtr(supports.Length * sizeof(int));
			Cl.Mem supportsBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
				supportsLen, supports, out err);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not initialize buffer");

			if (progressOutput)
				Console.Out.WriteLine("Copied input data to device memory.");

			err = Cl.SetKernelArg(kernel, 0, setsCountBuf); //setsCount
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not set kernel argument");
			err = Cl.SetKernelArg(kernel, 1, setsBuf); //sets
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not set kernel argument");
			err = Cl.SetKernelArg(kernel, 2, setSizesBuf); //setSizes
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not set kernel argument");
			err = Cl.SetKernelArg(kernel, 3, subsetSizeBuf); //subsetSize
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not set kernel argument");
			err = Cl.SetKernelArg(kernel, 4, supportsBuf); //supports
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not set kernel argument");

			Cl.Event ev;
			IntPtr[] globalWorkSize = new IntPtr[] { setsLen };
			IntPtr[] localWorkSize = new IntPtr[] { new IntPtr(1) };

			while (subsetSize[0] <= 5)
			{
				err = Cl.EnqueueNDRangeKernel(queue, kernel, 1, null, globalWorkSize, localWorkSize,
					0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "error while launching kernel");

				if (progressOutput)
					Console.Out.WriteLine("launched kernel for subset size n-{0}.", subsetSize[0]);

				subsetSize[0] += 1;
				err = Cl.EnqueueWriteBuffer(queue, subsetSizeBuf, Cl.Bool.True, IntPtr.Zero, subsetSizeLen, subsetSize,
					0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not write to device memory");
			}

			if (progressOutput)
				Console.Out.WriteLine("Finished subset generation.");

			//Cl.InfoBuffer buf3 = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.HostPtr, out err);
			//Cl.InfoBuffer buf = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.Size, out err);
			//Cl.InfoBuffer buf2 = Cl.GetMemObjectInfo(supportsBuf, Cl.MemInfo.Type, out err);

			//int x = buf.CastTo<int>();

			err = Cl.EnqueueReadBuffer(queue, supportsBuf, Cl.Bool.True, IntPtr.Zero, supportsLen, supports,
				0, null, out ev);
			if (!err.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(err, "could not read results from device memory");

			for (int i = 0; i < supports.Length; ++i)
			{
				if (supports[i] >= minimalSupport)
					litemsets.Add(new Litemset(supports[i], allTransactions.ElementAt(i).ToArray()));
			}

			if (progressOutput)
				Console.Out.WriteLine("Generated all litemsets.");

			return litemsets;
		}

	}
}
