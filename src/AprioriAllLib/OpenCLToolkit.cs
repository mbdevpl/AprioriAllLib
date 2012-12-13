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

		public static void PrintBuildInfo(Cl.Program program, Cl.Device device, TextWriter writer)
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

		public static void PrintSourceCode(string path, string[] source, IntPtr[] lenghts, TextWriter writer)
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
		/// Retrieves source code contained at a specific path in AprioriAllLib assembly resource file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="source"></param>
		/// <param name="lenghts"></param>
		internal static void GetSourceCodeFromLocalResource(string path, out string[] source, out IntPtr[] lenghts)
		{
			var assembly = Assembly.GetExecutingAssembly();
			Stream kernelSourceStream
				//= assembly.GetManifestResourceStream(path);
				= new MemoryStream(AprioriAllLib.Properties.Resources.cl_subsets);
			StreamReader kernelSourceReader = new StreamReader(kernelSourceStream);
			List<string> sourceCodeLinesList = new List<string>();
			List<IntPtr> sourceCodeLinesLenghtsList = new List<IntPtr>();
			while (!kernelSourceReader.EndOfStream)
			{
				string line = kernelSourceReader.ReadLine();
				sourceCodeLinesList.Add(string.Concat(line, "\n"));
				sourceCodeLinesLenghtsList.Add(new IntPtr(line.Length + 1));
			}
			source = sourceCodeLinesList.ToArray();
			lenghts = sourceCodeLinesLenghtsList.ToArray();
		}

	}

}
