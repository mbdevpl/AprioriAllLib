using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Defines some example inputs for Apriori and AprioriAll tests.
	/// </summary>
	class InputData
	{
		/// <summary>
		/// Example data set.
		/// </summary>
		public CustomerList Example1;

		/// <summary>
		/// Another data set.
		/// </summary>
		public CustomerList DataSet1;

		public CustomerList DataSet2;

		public CustomerList DataSet3;

		/// <summary>
		/// Constructs a new instance of input data.
		/// </summary>
		public InputData()
		{
			Example1 = new CustomerList();
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }));
			Example1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 10, 60, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			Example1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 40 }));
			Example1.Customers.Add(new Customer(new int[] { 90 }));

			DataSet1 = new CustomerList();
			DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 90 }));
			DataSet1.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			DataSet1.Customers.Add(new Customer(new int[] { 30, 50, 70 }));
			DataSet1.Customers.Add(new Customer(new int[] { 30 }, new int[] { 40, 70 }, new int[] { 90 }));
			DataSet1.Customers.Add(new Customer(new int[] { 90 }));

			DataSet2 = new CustomerList();
			DataSet2.Customers.Add(new Customer(new int[] { 30 }, new int[] { 30, 40, 50 }, new int[] { 80 }, new int[] { 90 }));
			DataSet2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			DataSet2.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30, 50, 70 }));
			DataSet2.Customers.Add(new Customer(new int[] { 30, 40 }, new int[] { 50, 60 }, new int[] { 70, 80, 90 }));
			DataSet2.Customers.Add(new Customer(new int[] { 80 }, new int[] { 90 }));
			DataSet2.Customers.Add(new Customer(new int[] { 10, 50 }, new int[] { 80 }));

			DataSet3 = new CustomerList();
			DataSet3.Customers.Add(new Customer(new int[] { 30 }, new int[] { 80 }, new int[] { 30, 40, 50 }, new int[] { 90 }));
			DataSet3.Customers.Add(new Customer(new int[] { 10, 20 }, new int[] { 30 }, new int[] { 40, 60, 70 }));
			DataSet3.Customers.Add(new Customer(new int[] { 30, 50, 70 }, new int[] { 10, 20 }));
			DataSet3.Customers.Add(new Customer(new int[] { 30, 80 }, new int[] { 40, 70 }, new int[] { 90, 30, 40, 50 }));
			DataSet3.Customers.Add(new Customer(new int[] { 90 }, new int[] { 80 }));
			DataSet3.Customers.Add(new Customer(new int[] { 50, 10 }, new int[] { 80 }));

			// future sets may be read from xml files?
			//XmlReader reader = new XmlReader();
			//CustomerList cList = reader.ReadFromXmlFile(/* full path or whatever else works*/);
		}

	}
}
