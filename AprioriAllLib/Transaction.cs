using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {

	/// <summary>
	/// Single transaction of a client.
	/// </summary>
	public class Transaction {

		public List<Item> items; //sorry
		public List<Item> Items {
			get { return items; }
			//set { items = value; }
		}

		public Transaction(params int[] values) {
			items = new List<Item>();
			foreach(int value in values)
				items.Add(new Item(value));
		}

        public Transaction()
        {
            items = new List<Item>();
        }

	}

}
