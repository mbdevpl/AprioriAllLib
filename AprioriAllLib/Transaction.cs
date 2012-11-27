using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	/// <summary>
	/// Single client's transaction having a list of items
	/// </summary>
	public class Transaction {

		private List<Item> items;

		/// <summary>
		/// A list of items
		/// </summary>
		public List<Item> Items {
			get { return items; }
			set { items = value; }
		}

		// the result of apriori appplied to those items
		public List<List<Item>> FrequentItems;

		/// <summary>
		/// Constructor
		/// </summary>
		public Transaction() {
			items = new List<Item>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="values">Array of integer values of items contained in this transaction</param>
		public Transaction(params int[] values) {
			items = new List<Item>();
			FrequentItems = new List<List<Item>>();
			foreach (int value in values)
				items.Add(new Item(value));
		}

		/// <summary>
		/// Prints the content of transaction
		/// </summary>
		/// <returns>String of values of transaction items</returns>
		public override string ToString() {
			string itemsStr = string.Join(" ", items.Select(x => x.ToString()).ToArray());
			return String.Format("({0})", itemsStr);
		}
	}
}
