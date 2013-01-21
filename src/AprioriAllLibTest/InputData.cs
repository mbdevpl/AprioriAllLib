using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Defines some example inputs for Apriori and AprioriAll tests.
	/// </summary>
	public class InputData
	{
		/// <summary>
		/// Example data set.
		/// </summary>
		public readonly CustomerList Example1;

		public readonly CustomerList Example2;

		public readonly CustomerList Example3;

		/// <summary>
		/// Another data set.
		/// </summary>
		public readonly CustomerList DataSet1;

		public readonly CustomerList DataSet2;

		public readonly CustomerList DataSet3;

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

			Example2 = new CustomerList();
			Example2.Customers.Add(new Customer(new int[] { 19, 46, 39, 31, 12, 18, 17, 26 }));
			Example2.Customers.Add(new Customer(new int[] { 48, 14, 28, 21, 44, 26, 22, 46 }));
			Example2.Customers.Add(new Customer(new int[] { 25, 18 }));
			Example2.Customers.Add(new Customer(new int[] { 41, 15, 32, 13, 29, 21, 19, 12 }));
			Example2.Customers.Add(new Customer(new int[] { 27, 13, 47 }));
			Example2.Customers.Add(new Customer(new int[] { 32, 24 }));
			Example2.Customers.Add(new Customer(new int[] { 38, 28, 47, 32, 36, 13, 11 }));
			Example2.Customers.Add(new Customer(new int[] { 31, 23, 26 }));
			Example2.Customers.Add(new Customer(new int[] { 19, 44, 12, 30, 37, }));
			Example2.Customers.Add(new Customer(new int[] { 26, 37, 20, 16, 19, 38, 45 }));
			Example2.Customers.Add(new Customer(new int[] { 11, 45, 21, 10, 23, 25 }));
			Example2.Customers.Add(new Customer(new int[] { 26, 27, 42, 16, 38 }));
			Example2.Customers.Add(new Customer(new int[] { 18, 26, 35 }));
			Example2.Customers.Add(new Customer(new int[] { 37, 11, 43, 13, 19, 27, 22, 44, 23 }));
			Example2.Customers.Add(new Customer(new int[] { 38, 30, 35, 22 }));

			Example3 = new CustomerList();
			Example3.Customers.Add(new Customer(new int[] { 10001, 20000, 30001, 40000, 50000, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20005, 30003, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20005, 30002, 40012, 50003, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20000, 30006, 40000, 50000, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20005, 30005, 40008, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20001, 30003, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20000, 30004, 40008, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20002, 30002, 40006, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20005, 30001, 40000, 50000, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20005, 30005, 40008, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20000, 30003, 40008, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20002, 30004, 40008, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20005, 30002, 40000, 50000, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20000, 30001, 40003, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20005, 30007, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20002, 30005, 40005, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20001, 30003, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20000, 30002, 40013, 50003, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20004, 30005, 40000, 50000, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20005, 30003, 40014, 50003, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20000, 30002, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20001, 30005, 40010, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20003, 30004, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20004, 30007, 40004, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20005, 30001, 40007, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20001, 30004, 40004, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20003, 30003, 40010, 50002, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10002, 20003, 30005, 40002, 50001, 60002, 70004, 80004 }));
			Example3.Customers.Add(new Customer(new int[] { 10001, 20008, 30003, 40011, 50002, 60002, 70004, 80004 }));

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
