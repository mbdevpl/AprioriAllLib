using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	public class Customer {

		//public string Name;

		public List<Transaction> Transactions;

		public Customer() {
			Transactions = new List<Transaction>();
		}

		public Customer(params Transaction[] transactions) {
			Transactions = new List<Transaction>();
			foreach (Transaction t in transactions)
				Transactions.Add(t);
		}

		public Customer(params int[][] valuesArray) {
			Transactions = new List<Transaction>();
			foreach (int[] values in valuesArray)
				Transactions.Add(new Transaction(values));
		}

		public override string ToString() {
			string itemsStr = string.Join(",", Transactions.Select(x => x.ToString()).ToArray());
			return String.Format("{0}", itemsStr);
		}

	}

}
