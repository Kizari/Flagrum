using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ZLibNet
{
	/// <summary>Provides methods and properties used to compress and decompress streams.</summary>
	public class DeflateStream : Stream
	{
		//		private const int BufferSize = 16384;

		long pBytesIn = 0;
		long pBytesOut = 0;
		bool pSuccess;
		//		uint pCrcValue = 0;
		const int WORK_DATA_SIZE = 0x1000;
		byte[] pWorkData = new byte[WORK_DATA_SIZE];
		int pWorkDataPos = 0;

		private Stream pStream;
		private CompressionMode pCompMode;
		private z_stream pZstream = new z_stream();
		bool pLeaveOpen;

		public DeflateStream(Stream stream, CompressionMode mode)
			: this(stream, mode, CompressionLevel.Default)
		{
		}

		public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen) :
			this(stream, mode, CompressionLevel.Default, leaveOpen)
		{
		}

		public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level) :
			this(stream, mode, level, false)
		{
		}

		public DeflateStream(Stream stream, CompressionMode compMode, CompressionLevel level, bool leaveOpen)
		{
			this.pLeaveOpen = leaveOpen;
			this.pStream = stream;
			this.pCompMode = compMode;

			int ret;
			if (this.pCompMode == CompressionMode.Compress)
				ret = ZLib.deflateInit(ref pZstream, level, this.WriteType);
			else
				ret = ZLib.inflateInit(ref pZstream, this.OpenType);

			if (ret != ZLibReturnCode.Ok)
				throw new ZLibException(ret, pZstream.lasterrormsg);

			pSuccess = true;
		}

		/// <summary>GZipStream destructor. Cleans all allocated resources.</summary>
		~DeflateStream()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Stream.Close() ->   this.Dispose(true); + GC.SuppressFinalize(this);
		/// Stream.Dispose() ->  this.Close();
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				try
				{
					if (disposing) //managed stuff
					{
						if (this.pStream != null)
						{
							//managed stuff
							if (this.pCompMode == CompressionMode.Compress && pSuccess)
							{
								Flush();
								//								this.pStream.Flush();
							}
							if (!pLeaveOpen)
								this.pStream.Close();
							this.pStream = null;
						}
					}
				}
				finally
				{
					//unmanaged stuff
					FreeUnmanagedResources();
				}

			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Finished, free the resources used.
		private void FreeUnmanagedResources()
		{
			if (this.pCompMode == CompressionMode.Compress)
				ZLib.deflateEnd(ref pZstream);
			else
				ZLib.inflateEnd(ref pZstream);
		}

		protected virtual ZLibOpenType OpenType
		{
			get { return ZLibOpenType.Deflate; }
		}
		protected virtual ZLibWriteType WriteType
		{
			get { return ZLibWriteType.Deflate; }
		}

		

		/// <summary>Reads a number of decompressed bytes into the specified byte array.</summary>
		/// <param name="array">The array used to store decompressed bytes.</param>
		/// <param name="offset">The location in the array to begin reading.</param>
		/// <param name="count">The number of bytes decompressed.</param>
		/// <returns>The number of bytes that were decompressed into the byte array. If the end of the stream has been reached, zero or the number of bytes read is returned.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (pCompMode == CompressionMode.Compress)
				throw new NotSupportedException("Can't read on a compress stream!");

			int readLen = 0;
			if (pWorkDataPos != -1)
			{
				using (FixedArray workDataPtr = new FixedArray(pWorkData))
				using (FixedArray bufferPtr = new FixedArray(buffer))
				{
					pZstream.next_in = workDataPtr[pWorkDataPos];
					pZstream.next_out = bufferPtr[offset];
					pZstream.avail_out = (uint)count;

					while (pZstream.avail_out != 0)
					{
						if (pZstream.avail_in == 0)
						{
							pWorkDataPos = 0;
							pZstream.next_in = workDataPtr;
							pZstream.avail_in = (uint)pStream.Read(pWorkData, 0, WORK_DATA_SIZE);
							pBytesIn += pZstream.avail_in;
						}

						uint inCount = pZstream.avail_in;
						uint outCount = pZstream.avail_out;

						int zlibError = ZLib.inflate(ref pZstream, ZLibFlush.NoFlush); // flush method for inflate has no effect

						pWorkDataPos += (int)(inCount - pZstream.avail_in);
						readLen += (int)(outCount - pZstream.avail_out);

						if (zlibError == ZLibReturnCode.StreamEnd)
						{
							pWorkDataPos = -1; // magic for StreamEnd
							break;
						}
						else if (zlibError != ZLibReturnCode.Ok)
						{
							pSuccess = false;
							throw new ZLibException(zlibError, pZstream.lasterrormsg);
						}
					}

					//					pCrcValue = crc32(pCrcValue, &bufferPtr[offset], (uint)readLen);
					pBytesOut += readLen;
				}

			}
			return readLen;
		}


		/// <summary>This property is not supported and always throws a NotSupportedException.</summary>
		/// <param name="array">The array used to store compressed bytes.</param>
		/// <param name="offset">The location in the array to begin reading.</param>
		/// <param name="count">The number of bytes compressed.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (pCompMode == CompressionMode.Decompress)
				throw new NotSupportedException("Can't write on a decompression stream!");

			pBytesIn += count;

			using (FixedArray writePtr = new FixedArray(pWorkData))
			using (FixedArray bufferPtr = new FixedArray(buffer))
			{
				pZstream.next_in = bufferPtr[offset];
				pZstream.avail_in = (uint)count;
				pZstream.next_out = writePtr[pWorkDataPos];
				pZstream.avail_out = (uint)(WORK_DATA_SIZE - pWorkDataPos);

				//				pCrcValue = crc32(pCrcValue, &bufferPtr[offset], (uint)count);

				while (pZstream.avail_in != 0)
				{
					if (pZstream.avail_out == 0)
					{
						//rar logikk, men det betyr vel bare at den kun skriver hvis buffer ble fyllt helt,
						//dvs halvfyllt buffer vil kun skrives ved flush
						pStream.Write(pWorkData, 0, (int)WORK_DATA_SIZE);
						pBytesOut += WORK_DATA_SIZE;
						pWorkDataPos = 0;
						pZstream.next_out = writePtr;
						pZstream.avail_out = WORK_DATA_SIZE;
					}

					uint outCount = pZstream.avail_out;

					int zlibError = ZLib.deflate(ref pZstream, ZLibFlush.NoFlush);

					pWorkDataPos += (int)(outCount - pZstream.avail_out);

					if (zlibError != ZLibReturnCode.Ok)
					{
						pSuccess = false;
						throw new ZLibException(zlibError, pZstream.lasterrormsg);
					}

				}
			}
		}

		/// <summary>Flushes the contents of the internal buffer of the current GZipStream object to the underlying stream.</summary>
		public override void Flush()
		{
			if (pCompMode == CompressionMode.Decompress)
				throw new NotSupportedException("Can't flush a decompression stream.");

			using (FixedArray workDataPtr = new FixedArray(pWorkData))
			{
				pZstream.next_in = IntPtr.Zero;
				pZstream.avail_in = 0;
				pZstream.next_out = workDataPtr[pWorkDataPos];
				pZstream.avail_out = (uint)(WORK_DATA_SIZE - pWorkDataPos);

				int zlibError = ZLibReturnCode.Ok;
				while (zlibError != ZLibReturnCode.StreamEnd)
				{
					if (pZstream.avail_out != 0)
					{
						uint outCount = pZstream.avail_out;
						zlibError = ZLib.deflate(ref pZstream, ZLibFlush.Finish);

						pWorkDataPos += (int)(outCount - pZstream.avail_out);
						if (zlibError == ZLibReturnCode.StreamEnd)
						{
							//ok. will break loop
						}
						else if (zlibError != ZLibReturnCode.Ok)
						{
							pSuccess = false;
							throw new ZLibException(zlibError, pZstream.lasterrormsg);
						}
					}

					pStream.Write(pWorkData, 0, pWorkDataPos);
					pBytesOut += pWorkDataPos;
					pWorkDataPos = 0;
					pZstream.next_out = workDataPtr;
					pZstream.avail_out = WORK_DATA_SIZE;
				}
			}

			this.pStream.Flush();
		}


		//public uint CRC32
		//{
		//    get
		//    {
		//        return pCrcValue;
		//    }
		//}

		public long TotalIn
		{
			get { return this.pBytesIn; }
		}

		public long TotalOut
		{
			get { return this.pBytesOut; }
		}

		// The compression ratio obtained (same for compression/decompression).
		public double CompressionRatio
		{
			get
			{
				if (pCompMode == CompressionMode.Compress)
					return ((pBytesIn == 0) ? 0.0 : (100.0 - ((double)pBytesOut * 100.0 / (double)pBytesIn)));
				else
					return ((pBytesOut == 0) ? 0.0 : (100.0 - ((double)pBytesIn * 100.0 / (double)pBytesOut)));
			}
		}

		/// <summary>Gets a value indicating whether the stream supports reading while decompressing a file.</summary>
		public override bool CanRead
		{
			get
			{
				return pCompMode == CompressionMode.Decompress && pStream.CanRead;
			}
		}

		/// <summary>Gets a value indicating whether the stream supports writing.</summary>
		public override bool CanWrite
		{
			get
			{
				return pCompMode == CompressionMode.Compress && pStream.CanWrite;
			}
		}

		/// <summary>Gets a value indicating whether the stream supports seeking.</summary>
		public override bool CanSeek
		{
			get { return (false); }
		}

		/// <summary>Gets a reference to the underlying stream.</summary>
		public Stream BaseStream
		{
			get { return (this.pStream); }
		}

		/// <summary>This property is not supported and always throws a NotSupportedException.</summary>
		/// <param name="offset">The location in the stream.</param>
		/// <param name="origin">One of the SeekOrigin values.</param>
		/// <returns>A long value.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
		}

		/// <summary>This property is not supported and always throws a NotSupportedException.</summary>
		/// <param name="value">The length of the stream.</param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength not supported");
		}

		/// <summary>This property is not supported and always throws a NotSupportedException.</summary>
		public override long Length
		{
			get
			{
				throw new NotSupportedException("Length not supported.");
			}
		}

		/// <summary>This property is not supported and always throws a NotSupportedException.</summary>
		public override long Position
		{
			get
			{
				throw new NotSupportedException("Position not supported.");
			}
			set
			{
				throw new NotSupportedException("Position not supported.");
			}
		}
	}

	/// <summary>
	/// hdr(?) + adler32 et end.
	/// wraps a deflate stream
	/// </summary>
	public class ZLibStream : DeflateStream
	{
		public ZLibStream(Stream stream, CompressionMode mode)
			: base(stream, mode)
		{
		}
		public ZLibStream(Stream stream, CompressionMode mode, bool leaveOpen) :
			base(stream, mode, leaveOpen)
		{
		}
		public ZLibStream(Stream stream, CompressionMode mode, CompressionLevel level) :
			base(stream, mode, level)
		{
		}
		public ZLibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen) :
			base(stream, mode, level, leaveOpen)
		{
		}

		protected override ZLibOpenType OpenType
		{
			get { return ZLibOpenType.ZLib; }
		}
		protected override ZLibWriteType WriteType
		{
			get { return ZLibWriteType.ZLib; }
		}
	}

	/// <summary>
	/// Saved to file (.gz) can be opened with zip utils.
	/// Have hdr + crc32 at end.
	/// Wraps a deflate stream
	/// </summary>
	public class GZipStream : DeflateStream
	{
		public GZipStream(Stream stream, CompressionMode mode)
			: base(stream, mode)
		{
		}
		public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen) :
			base(stream, mode, leaveOpen)
		{
		}
		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level) :
			base(stream, mode, level)
		{
		}
		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen) :
			base(stream, mode, level, leaveOpen)
		{
		}

		protected override ZLibOpenType OpenType
		{
			get { return ZLibOpenType.GZip; }
		}
		protected override ZLibWriteType WriteType
		{
			get { return ZLibWriteType.GZip; }
		}
	}

}
