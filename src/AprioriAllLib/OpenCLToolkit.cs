using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace AprioriAllLib
{
	/// <summary>
	/// Various tools useful in everyday OpenCL.Net usage.
	/// </summary>
	public static class OpenCLToolkit
	{
		/// <summary>
		/// Retrieves source code contained at a specific path in AprioriAllLib assembly resource file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="source"></param>
		/// <param name="lenghts"></param>
		internal static void GetSourceCodeFromLocalResource(string path, out string[] source, out IntPtr[] lenghts)
		{
			var assembly = Assembly.GetExecutingAssembly();
			Stream kernelSourceStream = assembly.GetManifestResourceStream(path);
			StreamReader kernelSourceReader = new StreamReader(kernelSourceStream);
			List<string> sourceCodeLinesList = new List<string>();
			List<IntPtr> sourceCodeLinesLenghts = new List<IntPtr>();
			while (!kernelSourceReader.EndOfStream)
			{
				sourceCodeLinesList.Add(string.Concat(kernelSourceReader.ReadLine(), "\n"));
			}
			source = sourceCodeLinesList.ToArray();
			lenghts = sourceCodeLinesLenghts.ToArray();
		}

	}

}
