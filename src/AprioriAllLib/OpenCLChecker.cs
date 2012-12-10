using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// Utility class that is capable of basic diagnostics of presence of certain OpenCL features 
	/// via OpenCL.NET bindings.
	/// </summary>
	public static class OpenCLChecker
	{
		private static bool TrueIfError(Cl.ErrorCode errorCode)
		{
			if (errorCode.Equals(Cl.ErrorCode.Success))
				return false;

			Console.Error.WriteLine("OpenCL.NET error: code={0}", errorCode);

			return true;
		}

		/// <summary>
		/// Determines and returns number of available OpenCL platforms.
		/// </summary>
		/// <returns>number of available OpenCL platforms, 0 in case of any errors</returns>
		public static uint PlatformsCount()
		{
			Cl.ErrorCode error;
			uint platformsCount = 0;

			error = Cl.GetPlatformIDs(0, null, out platformsCount);

			if (error.Equals(Cl.ErrorCode.Success))
				return platformsCount;

			return 0;
		}

		/// <summary>
		/// Lists details of all available OpenCL platforms.
		/// </summary>
		/// <returns>list of details of all platforms</returns>
		public static List<String> AvailablePlatfroms()
		{
			var results = new List<String>();

			Cl.ErrorCode error;
			Cl.Platform[] platforms = Cl.GetPlatformIDs(out error);

			if (TrueIfError(error))
				return results;
			// Console.Out.WriteLine("Cannot find any OpenCL platforms, error={0}", error.ToString());

			foreach (Cl.Platform p in platforms)
			{
				Cl.InfoBuffer name = Cl.GetPlatformInfo(p, Cl.PlatformInfo.Name, out error);
				if (TrueIfError(error))
					continue;
				Cl.InfoBuffer vendor = Cl.GetPlatformInfo(p, Cl.PlatformInfo.Vendor, out error);
				if (TrueIfError(error))
					continue;
				Cl.InfoBuffer version = Cl.GetPlatformInfo(p, Cl.PlatformInfo.Version, out error);
				if (TrueIfError(error))
					continue;
				Cl.InfoBuffer profile = Cl.GetPlatformInfo(p, Cl.PlatformInfo.Profile, out error);
				if (TrueIfError(error))
					continue;
				Cl.InfoBuffer extensions = Cl.GetPlatformInfo(p, Cl.PlatformInfo.Extensions, out error);
				if (TrueIfError(error))
					continue;
				results.Add(String.Format("name='{0}' vendor='{1}' version='{2}' profile='{3}' extensions='{4}'",
					name, vendor, version, profile, extensions));
			}

			return results;
		}

		/// <summary>
		/// Lists details and devices of all available OpenCL platforms.
		/// </summary>
		/// <returns>list of details and devices of all platforms</returns>
		public static List<String> AvailablePlatformsWithDevices()
		{
			var results = AvailablePlatfroms();

			Cl.ErrorCode error;
			Cl.Platform[] platforms = Cl.GetPlatformIDs(out error);

			if (TrueIfError(error))
				return results;

			System.Collections.IEnumerator platformsEnumerator = platforms.GetEnumerator();
			List<String>.Enumerator resultsEnumerator = results.GetEnumerator();
			for (int i = 0; i < platforms.Length; ++i)
			{
				platformsEnumerator.MoveNext();
				resultsEnumerator.MoveNext();
				Cl.Platform p = (Cl.Platform)platformsEnumerator.Current;
				String r = resultsEnumerator.Current;

				Cl.Device[] devices = Cl.GetDeviceIDs(p, Cl.DeviceType.All, out error);
				if (TrueIfError(error)) continue;

				String s = " devices={";
				foreach (Cl.Device d in devices)
				{
					Cl.InfoBuffer type = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Type, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer name = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Name, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer vendor = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Vendor, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer cores = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxComputeUnits, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer clock = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxClockFrequency, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer memoryType = Cl.GetDeviceInfo(d, Cl.DeviceInfo.LocalMemType, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer memory = Cl.GetDeviceInfo(d, Cl.DeviceInfo.LocalMemSize, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer workGroupSize = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxWorkGroupSize, out error);
					if (TrueIfError(error)) continue;

					uint memType = memoryType.CastTo<uint>();

					s += String.Format("\n {0}. {1} name='{2}' vendor='{3}' cores={4} clock={5}",
						i, type.CastTo<Cl.DeviceType>().ToString(), name, vendor, cores.CastTo<uint>(),
						clock.CastTo<uint>());

					s += String.Format("\n    memoryType={0} memory={1} workGroupSize={2}",
						memType == 1 ? "LOCAL" : (memType == 2 ? "GLOBAL" : "unknown"),
						memory.CastTo<ulong>(), workGroupSize.CastTo<uint>());
				}
				if (devices.Length > 0)
					s += "\n";
				s += "}";

				results[i] += s;
			}

			return results;
		}


		public static String GetBuildInfo(Cl.Program program, Cl.Device device)
		{
			Cl.ErrorCode error;
			String buildInfo = "build info : {";

			Cl.InfoBuffer options = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Options, out error);
			if (TrueIfError(error))
				return buildInfo + " }";
			buildInfo += String.Format("\n  options='{0}'\n", options);

			Cl.InfoBuffer status = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Status, out error);
			if (TrueIfError(error))
				return buildInfo + "}";
			buildInfo += String.Format("  status={0}\n", status.CastTo<Cl.BuildStatus>().ToString());

			Cl.InfoBuffer log = Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error);
			if (TrueIfError(error))
				return buildInfo + "}";
			buildInfo += String.Format("  log: '{0}'\n", log.ToString());

			return buildInfo + "}";
		}
	}
}
