using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// Base class for all OpenCL.Net object-oriented abstraction classes. Contains very basic tools 
	/// helpful in OpenCL.Net diagnostics in case of some unpredicted errors.
	/// </summary>
	public class OpenCLBase
	{

		protected static bool TrueIfError(Cl.ErrorCode errorCode)
		{
			if (errorCode.Equals(Cl.ErrorCode.Success))
				return false;
			Console.Error.WriteLine("OpenCL.NET error: code={0}", errorCode);
			return true;
		}

	}
}
