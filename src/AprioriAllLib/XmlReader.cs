using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AprioriAllLib
{
	/// <summary>
	/// Class that reads data from XML client database and transforms them into classes.
	/// 
	/// by Karolina Baltyn
	/// </summary>
	public static class XmlReader
	{
		/// <summary>
		/// Reads from XML client database.
		/// </summary>
		/// <param name="filename">path of the database file</param>
		/// <returns>database transformed into a CustomerList object</returns>
		public static CustomerList ReadFromXmlFile(string filename)
		{
			CustomerList list = new CustomerList();

			try
			{
				XmlTextReader reader = new XmlTextReader(filename);
				while (reader.Read())
				{
					// Once we find the Clients tag, we start a particular loop for all of them.
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "Customers")
					{
						// We'll stay in this local loop until we find the end of the Clients tag.
						while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Customers"))
						{
							if (reader.NodeType == XmlNodeType.Element && reader.Name == "Customer")
								addCustomer(reader, list);
						}
					}
				}
				reader.Close();
			}
			catch (Exception)
			{
				Console.WriteLine("Invalid XML file");
			}
			return list;
		}

		/// <summary>
		/// Adds a Customer to the CustomerList.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="list"></param>
		private static void addCustomer(XmlTextReader reader, CustomerList list)
		{
			Customer customer = new Customer();
			list.Customers.Add(customer);

			while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Customer"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "Transaction")
					addTransaction(reader, customer);
			}
		}

		/// <summary>
		/// Adds a Transaction to the Customer.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="customer"></param>
		private static void addTransaction(XmlTextReader reader, Customer customer)
		{
			Transaction transaction = new Transaction();
			customer.Transactions.Add(transaction);

			while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Transaction"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "Item")
					addItem(reader, transaction);
			}
		}

		/// <summary>
		/// Adds an Item to the Transaction.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="transaction"></param>
		private static void addItem(XmlTextReader reader, Transaction transaction)
		{
			while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement) && reader.Name != "Item")
				if (reader.NodeType == XmlNodeType.Text && reader.HasValue)
				{
					transaction.Items.Add(new Item(int.Parse(reader.Value)));
					//Console.WriteLine(reader.Value);
				}
		}

	}
}
