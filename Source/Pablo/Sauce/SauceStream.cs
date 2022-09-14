using System;
using System.IO;

namespace Pablo.Sauce
{
	public class SauceStream : Stream
	{
		Stream stream;

		public SauceStream(SauceInfo sauce, Stream stream)
		{
			this.Sauce = sauce;
			this.stream = stream;
		}

		public SauceStream(Stream stream)
		{
			this.stream = stream;
			if (SauceInfo.HasSauce(stream))
			{
				Sauce = new SauceInfo(stream);
			}
			else Sauce = null;
		}
		public override bool CanRead => stream.CanRead;

		public override bool CanSeek => stream.CanSeek;

		public override bool CanWrite => false;

		public override void Flush() { stream.Flush(); }

		public override long Length { get { return (Sauce != null) ? Sauce.FileSize : stream.Length; } }
		public override long Position 
		{
			get { return stream.Position; } 
			set { stream.Position = value; }
		}

		public override int ReadByte()
		{
			if (Sauce != null && stream.Position >= Sauce.FileSize) return -1;
			return stream.ReadByte();
		}


		public override int Read(byte[] array, int offset, int count)
		{
			if (Sauce != null)
			{
				if (stream.Position > Sauce.FileSize) return 0;
				if (stream.Position+count > Sauce.FileSize) count = (int)(Sauce.FileSize - stream.Position);
			}
			return stream.Read(array, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			throw new Exception("Cannot set length on a sauce stream");
		}

		public override void Write(byte[] array, int offset, int count)
		{
			stream.Write(array, offset, count);
		}
		
		public SauceInfo Sauce
		{
			get; set;
		}

	}
}
