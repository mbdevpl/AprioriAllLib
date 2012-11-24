using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib {

	/// <summary>
	/// Single item of a transaction.
	/// </summary>
	public class Item : IComparable {

		public int Value;

		public Item(int value) {
			this.Value = value;
		}

		public int CompareTo(object obj) {
			return ((Item)obj).Value == this.Value ? 1 : 0;
		}

		public override string ToString() {
			return String.Format("Item({0})", Value);
		}

	}

}
