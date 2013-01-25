using System;
using System.Collections.Generic;
using System.Reflection;

namespace AprioriAllLib.Test
{
	class BenchmarkParameters
	{
		public bool NewEachTime { get { return newEachTime; } }
		private bool newEachTime;

		public bool OpenCL { get { return openCL; } }
		private bool openCL;

		public bool Serialized { get { return serialized; } }
		private bool serialized;

		public bool WarmUp { get { return warmUp; } }
		private bool warmUp;

		public int Repeats { get { return repeats; } }
		private int repeats;

		public CustomerList Input { get { return input; } }
		private CustomerList input;

		public List<double> Supports { get { return supports; } }
		private List<double> supports;

		public List<int> Customers { get { return customers; } }
		private List<int> customers;

		//public List<int> Transactions { get { return transactions; } }
		//private List<int> transactions;

		//public List<int> Items { get { return items; } }
		//private List<int> items;

		public bool Output { get { return output; } }
		private bool output;

		//int custCountMin = 3;
		//int custCountMax = 3;
		//int transactions = 3;
		//int itemCount = 3;
		//int uniqueIds = 10;

		public BenchmarkParameters()
		{
			newEachTime = false;
			openCL = false;
			serialized = false;
			warmUp = false;

			repeats = 1;

			input = null;

			supports = new List<double>();
			customers = new List<int>();
			//transactions = new List<int>();
			//items = new List<int>();

			output = false;

			//custCountMin = 3;
			//custCountMax = 3;
			//transactCount = 3;
			//itemCount = 3;
			//uniqueIds = 10;
		}

		public BenchmarkParameters(string[] args)
			: this()
		{
			#region parsing string[] args

			foreach (string arg in args)
			{
				if (arg == null || arg.Length < 6)
					continue;

				else if (arg.StartsWith("repeats="))
					Int32.TryParse(arg.Substring(8), out repeats);
				else if (arg.Equals("newEachTime"))
					newEachTime = true;
				else if (arg.Equals("openCL"))
					openCL = true;
				else if (arg.Equals("serialized"))
					serialized = true;
				else if (arg.Equals("warmUp"))
					warmUp = true;
				else if (arg.StartsWith("support="))
				{
					double support;
					if (Double.TryParse(arg.Substring(8), out support))
					{
						supports.Clear();
						supports.Add(support);
					}
				}
				else if (arg.StartsWith("supports="))
				{
					string supportss = arg.Substring(9);
					int first = supportss.IndexOf(';');
					int last = supportss.LastIndexOf(';');
					if (first > 0 && last < supportss.Length - 1 && first != last)
					{
						double start, end, step;
						if (Double.TryParse(supportss.Substring(0, first), out start)
							&& Double.TryParse(supportss.Substring(last + 1), out step)
							&& Double.TryParse(supportss.Substring(first + 1, last - first - 1), out end))
						{
							supports.Clear();
							if (start < end)
								for (double d = start; d <= end; d += step)
									supports.Add(d);
							else
								for (double d = start; d >= end; d -= step)
									supports.Add(d);
						}
					}
				}
				else if (arg.StartsWith("input="))
				{
					InputData data = new InputData();

					string inputName = arg.Substring(6);

					Type type = typeof(InputData);
					FieldInfo field = type.GetField(inputName);
					object inputObj = field.GetValue(data);

					if (inputObj is CustomerList)
						input = (CustomerList)inputObj;
				}
				else if (arg.StartsWith("customers="))
				{
					string customerss = arg.Substring(10);
					int first = customerss.IndexOf(';');
					int last = customerss.LastIndexOf(';');
					if (first > 0 && last < customerss.Length - 1 && first != last)
					{
						int start, end, step;
						if (Int32.TryParse(customerss.Substring(0, first), out start)
							&& Int32.TryParse(customerss.Substring(last + 1), out step)
							&& Int32.TryParse(customerss.Substring(first + 1, last - first - 1), out end))
						{
							customers.Clear();
							if (start < end)
								for (int c = start; c <= end; c += step)
									customers.Add(c);
							else
								for (int c = start; c >= end; c -= step)
									customers.Add(c);
						}
					}
				}
				else if (arg.Equals("output"))
					output = true;
				//else if (arg.StartsWith("custCountMin="))
				//	Int32.TryParse(arg.Substring(13), out custCountMin);
				//else if (arg.StartsWith("custCountMax="))
				//	Int32.TryParse(arg.Substring(13), out custCountMax);
				//else if (arg.StartsWith("transactCount="))
				//	Int32.TryParse(arg.Substring(14), out transactCount);
				//else if (arg.StartsWith("itemCount="))
				//	Int32.TryParse(arg.Substring(10), out itemCount);
				//else if (arg.StartsWith("uniqueIds="))
				//	Int32.TryParse(arg.Substring(10), out uniqueIds);
			}

			#endregion

			if (customers.Count > 0 && customers[customers.Count - 1] > input.Customers.Count)
				throw new Exception("inconsistent benchmark scenario parameters");
			if (customers.Count == 0)
				customers.Add(input.Customers.Count);
		}
	}
}
