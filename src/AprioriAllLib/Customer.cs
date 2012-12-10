using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib
{

	/// <summary>
	/// Class which represents a customer from database having a list of transactions
	/// </summary>
	public class Customer
	{

		//public string Name;

		/// <summary>
		/// List of transactions
		/// </summary>
		public List<Transaction> Transactions;

		/// <summary>
		/// Constructor
		/// </summary>
		public Customer()
		{
			Transactions = new List<Transaction>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="transactions">Array of transactions of this client</param>
		public Customer(params Transaction[] transactions)
		{
			Transactions = new List<Transaction>();
			foreach (Transaction t in transactions)
				Transactions.Add(t);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="valuesArray">Two-dimensional array of integer values of items in transactions of this client</param>
		public Customer(params int[][] valuesArray)
		{
			Transactions = new List<Transaction>();
			foreach (int[] values in valuesArray)
				Transactions.Add(new Transaction(values));
		}

		/// <summary>
		/// Prints the content of client transactions
		/// </summary>
		/// <returns>String of values of transaction items</returns>
		public override string ToString()
		{
			string itemsStr = string.Join(",", Transactions.Select(x => x.ToString()).ToArray());
			return String.Format("({0})", itemsStr);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType().Equals(typeof(Customer)) && Transactions.Count == ((Customer)obj).Transactions.Count
					&& Enumerable.SequenceEqual(Transactions, ((Customer)obj).Transactions))
				return true;
			return false;
			//return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void AddTransaction(Transaction t)
		{
			Transactions.Add(t);
		}

	}

}
