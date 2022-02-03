using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace ZLibNet
{
	internal class FixedArray : IDisposable
	{
		GCHandle pHandle;
		Array pArray;

		public FixedArray(Array array)
		{
			pArray = array;
			pHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		}

		~FixedArray()
		{
			pHandle.Free();
		}

		#region IDisposable Members

		public void Dispose()
		{
			pHandle.Free();
			GC.SuppressFinalize(this);
		}

		public IntPtr this[int idx]
		{
			get
			{
				return Marshal.UnsafeAddrOfPinnedArrayElement(pArray, idx);
			}
		}
		public static implicit operator IntPtr(FixedArray fixedArray)
		{
			return fixedArray[0];
		}
		#endregion
	}

	public static class ListHelper
	{
		public static void Add<T>(this List<T> list, params T[] items)
		{
			foreach (T i in items)
				list.Add(i);
		}
		public static void AddRange<T>(this List<T> list, IEnumerable<T> items)
		{
			foreach (T i in items)
				list.Add(i);
		}
	}


	internal static class BitFlag
	{
		internal static bool IsSet(int bits, int flag)
		{
			return (bits & flag) == flag;
		}
		internal static bool IsSet(uint bits, uint flag)
		{
			return (bits & flag) == flag;
		}
		//internal static uint Set(uint bits, uint flag)
		//{
		//    return bits | flag;
		//}
		//internal static int Set(int bits, int flag)
		//{
		//    return bits | flag;
		//}
	}

	public static class DllLoader
	{
		
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr LoadLibrary(string lpFileName);

		// http://stackoverflow.com/questions/666799/embedding-unmanaged-dll-into-a-managed-c-sharp-dll
		public static void Load()
		{
			var thisAss = Assembly.GetExecutingAssembly();

			// Get a temporary directory in which we can store the unmanaged DLL, with
			// this assembly's version number in the path in order to avoid version
			// conflicts in case two applications are running at once with different versions
			string dirName = Path.Combine(Path.GetTempPath(), "zlibnet-zlib" + ZLibDll.ZLibDllFileVersion);

			try
			{
				if (!Directory.Exists(dirName))
					Directory.CreateDirectory(dirName);
			}
			catch
			{
				// raced?
				if (!Directory.Exists(dirName))
					throw;
			}

			string dllName = ZLibDll.GetDllName();
			string dllFullName = Path.Combine(dirName, dllName);

			// Get the embedded resource stream that holds the Internal DLL in this assembly.
			// The name looks funny because it must be the default namespace of this project
			// (MyAssembly.) plus the name of the Properties subdirectory where the
			// embedded resource resides (Properties.) plus the name of the file.
			if (!File.Exists(dllFullName))
			{
				// Copy the assembly to the temporary file
				string tempFile = Path.GetTempFileName();
				using (Stream stm = thisAss.GetManifestResourceStream("ZLibNet." + dllName))
				{
					using (Stream outFile = File.Create(tempFile))
					{
						stm.CopyTo(outFile);
					}
				}

				try
				{
					File.Move(tempFile, dllFullName);
				}
				catch
				{
					// clean up tempfile
					try
					{
						File.Delete(tempFile);
					}
					catch
					{
						// eat
					}

					// raced?
					if (!File.Exists(dllFullName))
						throw;
				}

			}

			// We must explicitly load the DLL here because the temporary directory is not in the PATH.
			// Once it is loaded, the DllImport directives that use the DLL will use the one that is already loaded into the process.
			IntPtr hFile = LoadLibrary(dllFullName);
			if (hFile == IntPtr.Zero)
				throw new Exception("Can't load " + dllFullName);
		}
	}
}
