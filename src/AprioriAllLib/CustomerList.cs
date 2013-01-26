﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/*!
 * \defgroup input Input data structures
 * \brief Group of classes that are used as input and output of both algorithms.
 * 
 * The format of input will be almost certainly changed in future versions.
 */

namespace AprioriAllLib
{
	/*!
	 * \addtogroup input
	 * @{
	 */

	/// <summary>
	/// Class that represents a total set of customers
	/// </summary>
	public class CustomerList
	{
		/// <summary>
		/// List of Customers 
		/// </summary>
		public List<Customer> Customers;

		/// <summary>
		/// Constructor
		/// </summary>
		public CustomerList()
		{
			Customers = new List<Customer>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public CustomerList(params int[][][] customersWithTransactionsWithItems)
		{
			Customers = new List<Customer>();
			foreach (int[][] customer in customersWithTransactionsWithItems)
				Customers.Add(new Customer(customer));
		}

		/// <summary>
		/// String representation of this class
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString()
		{
			string itemsStr = string.Join("; ", Customers.Select(x => x.ToString()).ToArray());
			return String.Format("CuLst[ {0} ]", itemsStr);
		}

		public string ToIntArrayInitializer()
		{
			StringBuilder s = new StringBuilder();

			if (Customers.Count > 0)
			{
				foreach (Customer c in Customers)
				{
					s.Append(" new int[][] {");
					foreach (Transaction t in c.Transactions)
					{
						s.Append(" new int[] {");
						foreach (Item i in t.Items)
						{
							s.AppendFormat(" {0},", i.Value);
						}
						s.Remove(s.Length - 1, 1);
						s.Append(" },");
					}
					s.Remove(s.Length - 1, 1);
					s.Append("},\n");
				}
				s.Remove(s.Length - 2, 2);
			}

			return s.ToString();
		}

	}

	/// @}
}
