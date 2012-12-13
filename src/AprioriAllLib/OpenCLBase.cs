using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCL.Net;

namespace AprioriAllLib
{
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
