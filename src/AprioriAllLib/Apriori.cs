using System;
using System.Collections.Generic;
using System.Linq;

using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// This class is used for calculating litemsets from a set of customers' transactions
	/// 
	/// Serialized version of algorithm: (C) 2012 by Karolina Baltyn
	/// </summary>
	public class Apriori
	{
		private CustomerList customerList;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="customerList">CustomerList object containing a list of Customers from a database</param>
		public Apriori(CustomerList customerList)
		{
			this.customerList = customerList;
		}

		/// <summary>
		/// Produces all subsets of a given list of items in a transaction
		/// </summary>
		/// <param name="items">List of items contained in one transaction</param>
		/// <returns>List of Litemsets</returns>
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
			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			//common part - initialization
			minimalSupport *= customerList.Customers.Count;
			List<Litemset> litemsets = new List<Litemset>();

			//parallel version of the algorithm
			if (parallel)
			{
				Cl.ErrorCode err;
				Cl.Platform platform;
				Cl.Device device;
				Cl.Event ev;

				Cl.Platform[] platforms = Cl.GetPlatformIDs(out err);
				platform = platforms[0];

				Cl.Device[] devices = Cl.GetDeviceIDs(platform, Cl.DeviceType.All, out err);
				device = devices[0];

				//Cl.CommandQueue a = new Cl.CommandQueue();

				//Cl.ContextProperty[] contextProperties = new Cl.ContextProperty[1];
				//contextProperties[0] = Cl.ContextProperties.Platform;

				Cl.Context context = Cl.CreateContext(null, 1, devices, null, IntPtr.Zero, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create context");

				Cl.CommandQueue queue = Cl.CreateCommandQueue(context, device, Cl.CommandQueueProperties.None, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create command queue");

				string[] sourceCode = System.IO.File.ReadAllLines("subsets.cl");
				IntPtr[] lenghts = new IntPtr[sourceCode.Length];

				for (int i = 0; i < sourceCode.Length; ++i)
				{
					sourceCode[i] += "\n";
					string s = sourceCode[i];
					lenghts[i] = new IntPtr(s.Length);
				}

				Cl.Program program = Cl.CreateProgramWithSource(context, (uint)sourceCode.Length, sourceCode, lenghts, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create program");

				err = Cl.BuildProgram(program, 1, devices, "", null, IntPtr.Zero);
				if (!err.Equals(Cl.ErrorCode.Success))
				{
					Console.Out.WriteLine(OpenCLChecker.GetBuildInfo(program, device));
					throw new Cl.Exception(err, "could not build program");
				}

				Cl.Kernel kernel = Cl.CreateKernel(program, "countSubsetsSupport", out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not create kernel");

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
				Cl.Mem setsCountBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
					new IntPtr((int)1), setsCount, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not initialize buffer");

				int[] sets = flatTransactions.ToArray();
				Cl.Mem setsBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
					new IntPtr(flatTransactions.Count), sets, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not initialize buffer");

				int[] setSizes = transactionsLimits.ToArray();
				Cl.Mem setSizesBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
					new IntPtr(transactionsLimits.Count), setSizes, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not initialize buffer");

				int[] subsetSize = new int[] { 1 };
				Cl.Mem subsetSizeBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadOnly | Cl.MemFlags.UseHostPtr,
					new IntPtr((int)1), subsetSize, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not initialize buffer");

				int[] supports = new int[allTransactions.Count];
				Cl.Mem supportsBuf = Cl.CreateBuffer(context, Cl.MemFlags.ReadWrite | Cl.MemFlags.UseHostPtr,
					new IntPtr(allTransactions.Count), supports, out err);
				if (!err.Equals(Cl.ErrorCode.Success))
					throw new Cl.Exception(err, "could not initialize buffer");

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

				IntPtr[] globalWorkSize = new IntPtr[] { new IntPtr(flatTransactions.Count) };
				IntPtr[] localWorkSize = new IntPtr[] { new IntPtr((int)1) };
				err = Cl.EnqueueNDRangeKernel(queue, kernel, 1, null, globalWorkSize, localWorkSize, 0, null, out ev);
				if (!err.Equals(Cl.ErrorCode.Success))
				{
					throw new Cl.Exception(err, "error while launching kernel");
				}

				IntPtr supportsLen = new IntPtr(supports.Length);
				err = Cl.EnqueueReadBuffer(queue, supportsBuf, Cl.Bool.True, IntPtr.Zero, supportsLen, supports, 
					0, null, out ev);

				Console.Out.WriteLine("Fin.");

				for (int i = 0; i < supports.Length; ++i)
				{
					if(supports[i] >= minimalSupport)
						litemsets.Add(new Litemset(supports[i], allTransactions.ElementAt(i).ToArray()));
				}

				return litemsets;
			}

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
	}
}
