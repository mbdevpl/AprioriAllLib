using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

    /// <summary>
    /// Class that represents a litemset (1-itemset)
    /// </summary>
	public class Litemset : IComparable {

        /// <summary>
        /// Support of this litemset
        /// </summary>
		public int Support;
         
        /// <summary>
        /// List of IDs of clients that support this litemset
        /// </summary>
        public List<int> IDs;

        /// <summary>
        /// List of items that this litemset contains
        /// </summary>
		public List<Item> Items; // litemset

        /// <summary>
        /// Constructor
        /// </summary>
		public Litemset() {
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">List of items</param>
		public Litemset(List<Item> items) {
			Items = items;
            IDs = new List<int>();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="support">Support</param>
        /// <param name="values">Values of items</param>
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

        /// <summary>
        /// String representation of this class
        /// </summary>
        /// <returns>String representation</returns>
		public override string ToString() {
			string itemsStr = string.Join(",", Items.Select(x => x.ToString()).ToArray());
			return String.Format("Lit(Supp={0};{1})", Support, itemsStr);
		}

	}

}
