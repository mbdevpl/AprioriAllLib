using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// Various tools useful in everyday OpenCL.Net usage.
	/// </summary>
	public class OpenCLToolkit : OpenCLBase
	{
		/// <summary>
		/// Write build info to the given TextWriter object.
		/// </summary>
		/// <param name="program"></param>
		/// <param name="device"></param>
		/// <param name="writer"></param>
		public static void WriteBuildInfo(Cl.Program program, Cl.Device device, TextWriter writer)
		{
			Cl.ErrorCode error;
			//String buildInfo = "build info : {";
			writer.Write("build info : {");

			Cl.InfoBuffer options = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Options, out error);
			if (TrueIfError(error))
			{
				//return buildInfo + " }";
				writer.Write(" }");
				return;
			}
			//buildInfo += String.Format("\n  options='{0}'\n", options);
			writer.Write("\n  options='{0}'\n", options);

			Cl.InfoBuffer status = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Status, out error);
			if (TrueIfError(error))
			{
				//return buildInfo + " }";
				writer.Write("}");
				return;
			}
			//buildInfo += String.Format("  status={0}\n", status.CastTo<Cl.BuildStatus>().ToString());
			writer.Write("  status={0}\n", status.CastTo<Cl.BuildStatus>().ToString());

			Cl.InfoBuffer log = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error);
			if (TrueIfError(error))
			{
				//return buildInfo + " }";
				writer.Write("}");
				return;
			}
			//buildInfo += String.Format("  log: '{0}'\n", log.ToString());
			writer.Write("  log: '{0}'\n", log.ToString());

			//return buildInfo + "}";
			writer.Write("}");
		}

		public static void WriteSourceCode(string path, string[] source, IntPtr[] lenghts, TextWriter writer)
		{
			writer.WriteLine("Source code of '{0}':", path);
			var enumeratedLenghts = lenghts.GetEnumerator();
			int i = 1;
			foreach (string line in source)
			{
				enumeratedLenghts.MoveNext();
				writer.Write("{0,3}: {1,2} => {2}", i, enumeratedLenghts.Current, line);
				++i;
			}

		}

		/// <summary>
		/// Retrieves OpenCL source code contained at a specific path in AprioriAllLib assembly resource file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="source"></param>
		/// <param name="lenghts"></param>
		internal static void GetSourceCodeFromLocalResource(string path, out string[] source, out IntPtr[] lenghts)
		{
			if (!path.EndsWith(".cl"))
				throw new ArgumentException("path must lead to .cl file", "path");
			int lastDot = path.LastIndexOf('.');
			if (lastDot >= 0)
				path = string.Concat(path.Substring(lastDot + 1), '_', path.Substring(0, lastDot));
			//var assembly = Assembly.GetExecutingAssembly();
			var resourceManager = AprioriAllLib.Properties.Resources.ResourceManager;
			var kernelSourceStream
				//= assembly.GetManifestResourceStream(path);
				= new MemoryStream((byte[])resourceManager.GetObject(path));

			//var sourceCode = resourceManager.GetObject(path).ToString();
			//var tempArray = sourceCode.Split('\n');
			//lenghts = new IntPtr[source.Length];

			StreamReader kernelSourceReader = new StreamReader(kernelSourceStream);
			List<string> sourceCodeLinesList = new List<string>();
			List<IntPtr> sourceCodeLinesLenghtsList = new List<IntPtr>();
			//foreach (string line in tempArray)
			while (!kernelSourceReader.EndOfStream)
			{
				string line = kernelSourceReader.ReadLine();
				sourceCodeLinesList.Add(string.Concat(line, "\n"));
				sourceCodeLinesLenghtsList.Add(new IntPtr(line.Length + 1));
			}
			source = sourceCodeLinesList.ToArray();
			lenghts = sourceCodeLinesLenghtsList.ToArray();
		}

		public static void GetSourceCodeFromFile(string path, out string[] source, out IntPtr[] lenghts)
		{
			if (!path.EndsWith(".cl"))
				throw new ArgumentException("path must lead to .cl file", "path");

			source = File.ReadAllLines(path);
			lenghts = new IntPtr[source.Length];
			for (int i = 0; i < source.Length; ++i)
			{
				source[i] += "\n";
				lenghts[i] = new IntPtr(source[i].Length);
			}
		}

		internal static Cl.Program GetProgramFromLocalResource(string path, Cl.Context context)
		{
			string[] source = null;
			IntPtr[] lenghts = null;
			GetSourceCodeFromLocalResource(path, out source, out lenghts);

			Cl.ErrorCode error;
			Cl.Program program = Cl.CreateProgramWithSource(context, (uint)source.Length, source, lenghts, out error);
			if (!error.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(error, "could not create program");

			return program;
		}

		public static Cl.Program GetProgramFromFile(string path, Cl.Context context)
		{
			string[] source = null;
			IntPtr[] lenghts = null;
			GetSourceCodeFromFile(path, out source, out lenghts);

			Cl.ErrorCode error;
			Cl.Program program = Cl.CreateProgramWithSource(context, (uint)source.Length, source, lenghts, out error);
			if (!error.Equals(Cl.ErrorCode.Success))
				throw new Cl.Exception(error, String.Format("could not create program using '{0}'.", path));

			return program;
		}

		internal static Cl.Program GetAndBuildProgramFromLocalResource(string path, Cl.Context context, Cl.Device device,
			TextWriter writer)
		{
			return GetAndBuildProgramFromLocalResource(path, context, device, string.Empty, writer);
		}

		internal static Cl.Program GetAndBuildProgramFromLocalResource(string path, Cl.Context context, Cl.Device device,
			string buildOptions, TextWriter writer)
		{
			Cl.Program program = GetProgramFromLocalResource(path, context);

			Cl.ErrorCode error;
			error = Cl.BuildProgram(program, 1, new Cl.Device[] { device }, buildOptions, null, IntPtr.Zero);
			if (!error.Equals(Cl.ErrorCode.Success))
			{
				if (writer != null)
				{
					writer.WriteLine("Build failed.");
					WriteBuildInfo(program, device, writer);
					writer.WriteLine();
					string[] source = null;
					IntPtr[] lenghts = null;
					GetSourceCodeFromLocalResource(path, out source, out lenghts);
					WriteSourceCode("subsets.cl", source, lenghts, writer);
					writer.WriteLine();
				}
				throw new Cl.Exception(error, "could not build program");
			}

			return program;
		}

	}

}
