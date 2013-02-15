using System;
using System.Collections.Generic;
using System.Reflection;

namespace AprioriAllLib.Test
{
	/// <summary>
	/// Set of parameters for the benchmark.
	/// </summary>
	public class BenchmarkParameters
	{
		#region benchmarked algorithms

		/// <summary>
		/// If true, benchmark includes Apriori algorithm.
		/// </summary>
		public bool Apriori { get { return apriori; } }
		private bool apriori;

		/// <summary>
		/// If true, benchmark includes AprioriAll algorithm.
		/// </summary>
		public bool AprioriAll { get { return aprioriAll; } }
		private bool aprioriAll;

		/// <summary>
		/// If true, benchmark includes serialized version.
		/// </summary>
		public bool Serialized { get { return serialized; } }
		private bool serialized;

		/// <summary>
		/// If true, benchmark includes OpenCL version.
		/// </summary>
		public bool OpenCL { get { return openCL; } }
		private bool openCL;

		#endregion

		#region repetition settings

		/// <summary>
		/// Sets number of repetitions of each benchmark.
		/// </summary>
		public int Repeats { get { return repeats; } }
		private int repeats;

		/// <summary>
		/// If true, each benchmark has an extra repetition, for which the time is not measured.
		/// </summary>
		public bool WarmUp { get { return warmUp; } }
		private bool warmUp;

		/// <summary>
		/// If true, each repetition is performed on new instance of solver.
		/// </summary>
		public bool NewEachTime { get { return newEachTime; } }
		private bool newEachTime;

		#endregion

		#region algorithms' parameters

		/// <summary>
		/// List of customers used in the benchmark.
		/// </summary>
		public List<ICustomer> Input { get { return input; } }
		private List<ICustomer> input;

		/// <summary>
		/// Sequence of supports checked by the benchmark.
		/// </summary>
		public List<double> Supports { get { return supports; } }
		private List<double> supports;

		/// <summary>
		/// Sequence of customers' counts from Input that are included in benchmark.
		/// </summary>
		public List<int> Customers { get { return customers; } }
		private List<int> customers;

		//public List<int> Transactions { get { return transactions; } }
		//private List<int> transactions;

		//public List<int> Items { get { return items; } }
		//private List<int> items;

		#endregion

		#region output settings

		public bool PrintInput { get { return printInput; } }
		private bool printInput;

		public bool SaveInput { get { return saveInput; } }
		private bool saveInput;

		public bool PrintProgress { get { return printProgress; } }
		private bool printProgress;

		public bool PrintOutput { get { return printOutput; } }
		private bool printOutput;

		public bool SaveLatex { get { return saveLatex; } }
		private bool saveLatex;

		#endregion

		public BenchmarkParameters()
		{
			apriori = false;
			aprioriAll = false;
			serialized = false;
			openCL = false;

			repeats = 0;
			warmUp = false;
			newEachTime = false;

			input = null;
			supports = new List<double>();
			customers = new List<int>();
			//transactions = new List<int>();
			//items = new List<int>();

			printInput = false;
			printProgress = false;
			printOutput = false;
		}

		public BenchmarkParameters(
			bool apriori, bool aprioriAll, bool serialized, bool openCL,
			int repeats, bool warmUp, bool newEachTime,
			List<ICustomer> input, List<double> supports, List<int> customers,
			bool printInput, bool printProgress, bool printOutput)
		{
			this.apriori = apriori;
			this.aprioriAll = aprioriAll;
			this.serialized = serialized;
			this.openCL = openCL;

			this.repeats = repeats;
			this.warmUp = warmUp;
			this.newEachTime = newEachTime;

			this.input = input;
			this.supports = supports;
			this.customers = customers;

			this.printInput = printInput;
			this.printProgress = printProgress;
			this.printOutput = printOutput;
		}

		/// <summary>
		/// Converts a sequence of command line options into set of benchmark parameters.
		/// </summary>
		/// <param name="args">arguments usually given as command line options 
		/// to a console application that runs benchmark</param>
		public BenchmarkParameters(string[] args)
			: this()
		{
			foreach (string arg in args)
			{
				if (arg == null || arg.Length < 6)
					continue;
				#region benchmarked algorithms
				else if (arg.Equals("apriori"))
					apriori = true;
				else if (arg.Equals("aprioriAll"))
					aprioriAll = true;
				else if (arg.Equals("serialized"))
					serialized = true;
				else if (arg.Equals("openCL"))
					openCL = true;
				#endregion
				#region repetition settings
				else if (arg.StartsWith("repeats="))
					Int32.TryParse(arg.Substring(8), out repeats);
				else if (arg.Equals("warmUp"))
					warmUp = true;
				else if (arg.Equals("newEachTime"))
					newEachTime = true;
				#endregion
				#region algorithms' parameters
				else if (arg.StartsWith("input="))
				{
					string inputName = arg.Substring(6);

					if (inputName.StartsWith("Generate(") && inputName.EndsWith(")"))
					{
						string generatorParams = inputName.Substring(9, inputName.Length - 9 - 1);

						int[] genArgs = new int[4];
						bool genArgsOk = false;

						for (int i = 0; i < genArgs.Length; ++i)
						{
							int end = generatorParams.IndexOf(',');
							if (end < 1)
								end = generatorParams.Length;

							int argValue;
							if (!Int32.TryParse(generatorParams.Substring(0, end), out argValue))
								break;

							genArgs[i] = argValue;
							if (i == genArgs.Length - 1)
							{
								genArgsOk = true;
								break;
							}
							generatorParams = generatorParams.Substring(end + 1);
						}

						if (genArgsOk)
							input = InputGenerator.GenerateRandomList(genArgs[0], genArgs[1], genArgs[2], genArgs[3]);
					}
					else
					{
						InputData data = new InputData();

						Type type = typeof(InputData);
						FieldInfo field = type.GetField(inputName);
						object inputObj = field.GetValue(data);

						if (inputObj is List<ICustomer>)
							input = (List<ICustomer>)inputObj;
					}

				}
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
					else
					{
						int number;
						if (Int32.TryParse(customerss, out number))
						{
							customers.Clear();
							customers.Add(number);
						}
					}
				}
				#endregion
				#region output settings
				else if (arg.Equals("printInput"))
					printInput = true;
				else if (arg.Equals("saveInput"))
					saveInput = true;
				else if (arg.Equals("printProgress"))
					printProgress = true;
				else if (arg.Equals("printOutput"))
					printOutput = true;
				else if (arg.Equals("saveLatex"))
					saveLatex = true;
				else if (arg.Equals("printAll"))
				{
					printInput = true;
					printProgress = true;
					printOutput = true;
				}
				#endregion
			}

			if (customers.Count > 0 && customers[customers.Count - 1] > input.Count)
				throw new Exception("inconsistent benchmark scenario parameters");
			if (customers.Count == 0)
				customers.Add(input.Count);
		}

	}
}
