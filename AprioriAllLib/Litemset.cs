using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	public class Litemset {

		public int Support;

		public List<Item> Items; // litemset

		public Litemset() {
		}

		public Litemset(List<Item> items) {
			Items = items;
		}

		public Litemset(int support, params int[] values) {
			if (values == null)
				throw new ArgumentNullException("values", "values is null.");
			Support = support;
			Items = new List<Item>();
			foreach (int value in values)
				Items.Add(new Item(value));
		}

		public override string ToString() {
			string itemsStr = string.Join(",", Items.Select(x => x.ToString()).ToArray());
			return String.Format("Lit(Supp={0};{1})", Support, itemsStr);
		}

	}

}
