using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	/// <summary>
	/// Class that represents a total set of customers
	/// </summary>
	public class CustomerList {

		/// <summary>
		/// List of Customers 
		/// </summary>
		public List<Customer> Customers;

		/// <summary>
		/// Constructor
		/// </summary>
		public CustomerList() {
			Customers = new List<Customer>();
		}

		/// <summary>
		/// String representation of this class
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString() {
			string itemsStr = string.Join("; ", Customers.Select(x => x.ToString()).ToArray());
			return String.Format("CuLst[ {0} ]", itemsStr);
		}

	}

}
