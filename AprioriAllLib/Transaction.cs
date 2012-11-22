using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {

	/// <summary>
	/// Single transaction of a client.
	/// </summary>
	public class Transaction {

		public List<Item> Items;
        //public List<Item> Items {
        //    get { return items; }
		//}

        // the result of apriori appplied to those items
        public List<List<Item>> FrequentItems;

		public Transaction(params int[] values) {
			Items = new List<Item>();
            FrequentItems = new List<List<Item>>();
			foreach(int value in values)
				Items.Add(new Item(value));
		}

        public Transaction()
        {
            Items = new List<Item>();
        }

	}

}
