using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib
{

	/// <summary>
	/// Single item of a transaction.
	/// </summary>
	public class Item : IComparable
	{

		/// <summary>
		/// Integer value (id) of the item
		/// </summary>
		public int Value;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Value of the item</param>
		public Item(int value)
		{
			this.Value = value;
		}

		public int CompareTo(object obj)
		{
			return ((Item)obj).Value == this.Value ? 0 : (((Item)obj).Value > this.Value ? -1 : 1);
		}

		/// <summary>
		/// String representation of this class
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString()
		{
			return String.Format("{0}", Value);
		}

		public override bool Equals(object obj)
		{
			if (typeof(Item).Equals(obj.GetType()) && ((Item)obj).Value == Value)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

	}

}
