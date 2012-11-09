using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {

	public class Customer {

		//public string Name;

		public List<Transaction> Transactions;

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

	}

}
