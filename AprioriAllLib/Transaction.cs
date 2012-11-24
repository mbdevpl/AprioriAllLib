using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	/// <summary>
	/// Single transaction of a client.
	/// </summary>
	public class Transaction {

		private List<Item> items;
		public List<Item> Items {
			get { return items; }
			set { items = value; }
		}

		// the result of apriori appplied to those items
		public List<List<Item>> FrequentItems;

		public Transaction() {
			items = new List<Item>();
		}

		public Transaction(params int[] values) {
			items = new List<Item>();
			FrequentItems = new List<List<Item>>();
			foreach (int value in values)
				items.Add(new Item(value));
		}

		public override string ToString() {
			string itemsStr = string.Join(" ",items.Select(x => x.ToString()).ToArray()); 
			return String.Format("({0})", itemsStr);
		}
    }
}
