using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZLibNet
{
	/// <summary>
	/// Classes that simplify a common use of compression streams
	/// </summary>

	delegate DeflateStream CreateStreamDelegate(Stream s, CompressionMode cm, bool leaveOpen);

	public static class DeflateCompressor
	{
		public static MemoryStream Compress(Stream source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static MemoryStream DeCompress(Stream source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		public static byte[] Compress(byte[] source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static byte[] DeCompress(byte[] source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		private static DeflateStream CreateStream(Stream s, CompressionMode cm, bool leaveOpen)
		{
			return new DeflateStream(s, cm, leaveOpen);
		}
	}

	public static class GZipCompressor
	{
		public static MemoryStream Compress(Stream source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static MemoryStream DeCompress(Stream source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		public static byte[] Compress(byte[] source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static byte[] DeCompress(byte[] source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		private static DeflateStream CreateStream(Stream s, CompressionMode cm, bool leaveOpen)
		{
			return new GZipStream(s, cm, leaveOpen);
		}
	}

	public static class ZLibCompressor
	{
		public static MemoryStream Compress(Stream source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static MemoryStream DeCompress(Stream source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		public static byte[] Compress(byte[] source)
		{
			return CommonCompressor.Compress(CreateStream, source);
		}
		public static byte[] DeCompress(byte[] source)
		{
			return CommonCompressor.DeCompress(CreateStream, source);
		}
		private static DeflateStream CreateStream(Stream s, CompressionMode cm, bool leaveOpen)
		{
			return new ZLibStream(s, cm, leaveOpen);
		}
	}

	public static class DynazipCompressor
	{
		const int DZ_DEFLATE_POS = 46;

		public static bool IsDynazip(byte[] source)
		{
			return source.Length >= 4 && BitConverter.ToInt32(source, 0) == 0x02014b50;
		}

		public static byte[] DeCompress(byte[] source)
		{
			if (!IsDynazip(source))
				throw new InvalidDataException("not dynazip header");
			using (MemoryStream srcStream = new MemoryStream(source, DZ_DEFLATE_POS, source.Length - DZ_DEFLATE_POS))
			using (MemoryStream dstStream = DeCompress(srcStream))
				return dstStream.ToArray();
		}

		private static MemoryStream DeCompress(Stream source)
		{
			MemoryStream dest = new MemoryStream();
			DeCompress(source, dest);
			dest.Position = 0;
			return dest;
		}

		private static void DeCompress(Stream source, Stream dest)
		{
			using (DeflateStream zsSource = new DeflateStream(source, CompressionMode.Decompress, true))
			{
				zsSource.CopyTo(dest);
			}
		}
	}

	class CommonCompressor
	{
		private static void Compress(CreateStreamDelegate sc, Stream source, Stream dest)
		{
			using (DeflateStream zsDest = sc(dest, CompressionMode.Compress, true))
			{
				source.CopyTo(zsDest);
			}
		}

		private static void DeCompress(CreateStreamDelegate sc, Stream source, Stream dest)
		{
			using (DeflateStream zsSource = sc(source, CompressionMode.Decompress, true))
			{
				zsSource.CopyTo(dest);
			}
		}

		public static MemoryStream Compress(CreateStreamDelegate sc, Stream source)
		{
			MemoryStream result = new MemoryStream();
			Compress(sc, source, result);
			result.Position = 0;
			return result;
		}

		public static MemoryStream DeCompress(CreateStreamDelegate sc, Stream source)
		{
			MemoryStream result = new MemoryStream();
			DeCompress(sc, source, result);
			result.Position = 0;
			return result;
		}

		public static byte[] Compress(CreateStreamDelegate sc, byte[] source)
		{
			using (MemoryStream srcStream = new MemoryStream(source))
			using (MemoryStream dstStream = Compress(sc, srcStream))
				return dstStream.ToArray();
		}

		public static byte[] DeCompress(CreateStreamDelegate sc, byte[] source)
		{
			using (MemoryStream srcStream = new MemoryStream(source))
			using (MemoryStream dstStream = DeCompress(sc, srcStream))
				return dstStream.ToArray();
		}
	}
}
