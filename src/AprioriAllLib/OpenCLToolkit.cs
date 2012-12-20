using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
//using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// Various tools useful in everyday OpenCL.Net usage.
	/// </summary>
	public class OpenCLToolkit
	{

		//public static Cl.Mem CreateBuffer(Cl.Context context, Cl.MemFlags memFlags, int[] values,
		//	out IntPtr valuesLengthInBytes)
		//{
		//	Cl.ErrorCode error;

		//	valuesLengthInBytes = new IntPtr(values.Length * sizeof(int));

		//	Cl.Mem buffer = Cl.CreateBuffer(context, memFlags, valuesLengthInBytes, values, out error);
		//	if (!error.Equals(Cl.ErrorCode.Success))
		//		throw new Cl.Exception(error, "could not initialize buffer");

		//	return buffer;
		//}

		//public static void ReadBuffer(Cl.CommandQueue queue, Cl.Mem buffer,
		//	IntPtr numberOfReadBytes, int[] arrayForValues)
		//{
		//	Cl.ErrorCode error;
		//	Cl.Event eventHandler;

		//	error = Cl.EnqueueReadBuffer(queue, buffer, Cl.Bool.True, IntPtr.Zero,
		//		numberOfReadBytes, arrayForValues, 0, null, out eventHandler);
		//	if (TrueIfError(error))
		//		throw new Cl.Exception(error, "could not read results from device memory");
		//}

		//public static void WriteBuffer(Cl.CommandQueue queue, Cl.Mem buffer, IntPtr numberOfWrittenBytes, int[] valuesWritten)
		//{
		//	Cl.ErrorCode error;
		//	Cl.Event eventHandler;

		//	error = Cl.EnqueueWriteBuffer(queue, buffer, Cl.Bool.True, IntPtr.Zero,
		//		numberOfWrittenBytes, valuesWritten, 0, null, out eventHandler);
		//	if (TrueIfError(error))
		//		throw new Cl.Exception(error, "could not write to device memory");
		//}

	}
}
