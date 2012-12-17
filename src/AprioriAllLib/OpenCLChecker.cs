using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCL.Net;

namespace AprioriAllLib
{
	/// <summary>
	/// Utility class that is capable of basic diagnostics of presence of certain OpenCL features 
	/// via OpenCL.Net bindings.
	/// </summary>
	public class OpenCLChecker : OpenCLBase
	{

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

				StringBuilder s = new StringBuilder(" devices={");
				//String s = " devices={";
				foreach (Cl.Device d in devices)
				{
					Cl.InfoBuffer type = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Type, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer name = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Name, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer vendor = Cl.GetDeviceInfo(d, Cl.DeviceInfo.Vendor, out error);
					if (TrueIfError(error)) continue;

					s.AppendFormat("\n {0}. {1} name='{2}' vendor='{3}'",
						i, type.CastTo<Cl.DeviceType>().ToString(), name, vendor);

					Cl.InfoBuffer computeUnits = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxComputeUnits, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer clock = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxClockFrequency, out error);
					if (TrueIfError(error)) continue;

					s.AppendFormat("\n    computeUnits={0} clock={1}MHz",
						computeUnits.CastTo<uint>(), clock.CastTo<uint>());

					Cl.InfoBuffer workGroupSize = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxWorkGroupSize, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer workItemDimensions = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxWorkItemDimensions, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer workItemSizes = Cl.GetDeviceInfo(d, Cl.DeviceInfo.MaxWorkItemSizes, out error);
					if (TrueIfError(error)) continue;

					uint dims = workItemDimensions.CastTo<uint>();
					uint[] sizes = workItemSizes.CastToArray<uint>((int)dims);

					s.AppendFormat("\n    workGroupSize={0} workItemDimensions={1} workItemSizes={2}",
						workGroupSize.CastTo<uint>(), dims, String.Join("/", sizes));
					
					Cl.InfoBuffer addressBits = Cl.GetDeviceInfo(d, Cl.DeviceInfo.AddressBits, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer globalMemory = Cl.GetDeviceInfo(d, Cl.DeviceInfo.GlobalMemSize, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer globalCache = Cl.GetDeviceInfo(d, Cl.DeviceInfo.GlobalMemCacheSize, out error);
					if (TrueIfError(error)) continue;

					s.AppendFormat("\n    addressBits={0} globalMemory={1} globalCache={2}",
						addressBits.CastTo<uint>(), globalMemory.CastTo<ulong>(), globalCache.CastTo<ulong>());

					Cl.InfoBuffer localMemoryType = Cl.GetDeviceInfo(d, Cl.DeviceInfo.LocalMemType, out error);
					if (TrueIfError(error)) continue;
					Cl.InfoBuffer localMemory = Cl.GetDeviceInfo(d, Cl.DeviceInfo.LocalMemSize, out error);
					if (TrueIfError(error)) continue;

					uint memType = localMemoryType.CastTo<uint>();

					s.AppendFormat(" localMemoryType={0} localMemory={1} ",
						memType == 1 ? "LOCAL" : (memType == 2 ? "GLOBAL" : "unknown"),
						localMemory.CastTo<ulong>());
				}
				if (devices.Length > 0)
					s.Append("\n");
				s.Append("}");

				results[i] += s;
			}

			return results;
		}

	}
}
