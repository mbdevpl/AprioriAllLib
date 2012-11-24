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

		public override string ToString() {
			string itemsStr = string.Join(",", Items.Select(x => x.ToString()).ToArray());
			return String.Format("Litemset(Support={0};{1})", Support, itemsStr);
		}

	}

}
