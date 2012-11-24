using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	public class Litemset : IComparable {

		public int Support;
        // IDs of clients that support this litemset
        public List<int> IDs;
		public List<Item> Items; // litemset

		public Litemset() {
		}

		public Litemset(List<Item> items) {
			Items = items;
            IDs = new List<int>();
		}

		public Litemset(int support, params int[] values) {
			if (values == null)
				throw new ArgumentNullException("values", "values is null.");
			Support = support;
			Items = new List<Item>();
			foreach (int value in values)
				Items.Add(new Item(value));
		}

        //-1 - greater, 1 - smaller, 0 - error(equal)
        public int CompareTo(object obj)
        {
            Litemset litemset = (Litemset)obj;
            if (Items.Count > litemset.Items.Count)
                return 1;
            else if (Items.Count < litemset.Items.Count)
                return -1;
            else
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].CompareTo(litemset.Items[i]) == 0) // equal
                        continue;
                    else 
                        return Items[i].CompareTo(litemset.Items[i]);
                }
                return 0;
            }
        }

		public override string ToString() {
			string itemsStr = string.Join(",", Items.Select(x => x.ToString()).ToArray());
			return String.Format("Lit(Supp={0};{1})", Support, itemsStr);
		}

	}

}
