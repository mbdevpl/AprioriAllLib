using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib {

	public class CustomerList {

		public List<Customer> Customers;

		public CustomerList() {
			Customers = new List<Customer>();
		}

		public override string ToString() {
			string itemsStr = string.Join("; ", Customers.Select(x => x.ToString()).ToArray());
			return String.Format("CuLst[ {0} ]", itemsStr);
		}

	}

}
