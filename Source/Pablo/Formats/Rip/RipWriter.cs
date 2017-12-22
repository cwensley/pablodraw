using System;
using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Rip
{
	public class RipWriter
	{
		Stream inner;
		BinaryWriter writer;
		long lastNewLine;
		bool continuing;
		byte[] newLine;
		
		public int? MaxWidth { get; set; }
		
		public BinaryWriter Writer {
			get { return writer; }
		}

		public RipWriter (Stream inner)
		{
			this.inner = inner;
			MaxWidth = 70;
			writer = new BinaryWriter (inner);
			newLine = FormatRip.Encoding.GetBytes ("\r\n");
		}
		
		bool CheckNewline(int neededCharacters = 1, bool endCurrentLine = true)
		{
			if (MaxWidth != null) {
				if ((inner.Position - lastNewLine + neededCharacters + 1) > MaxWidth.Value) {
					if (endCurrentLine)
						writer.Write ((byte)'\\');
					writer.Write (newLine);
					continuing = endCurrentLine;
					lastNewLine = inner.Position;
					return true;
				}
			}
			return false;
		}
		
		public void WriteNewLine ()
		{
			writer.Write (newLine);
			continuing = false;
		}
		
		public void WriteNewCommand (string op)
		{
			CheckNewline (op.Length + 1, false);
			if (!continuing && lastNewLine == inner.Position)
				Write ("!");
			Write("|" + op);
		}
		
		public void WriteNumber (byte value)
		{
			CheckNewline();
			if (value > 36)
				throw new ArgumentOutOfRangeException ("value", "value must be less than 36");
			if (value < 10)
				writer.Write ((byte)(value + (byte)'0'));
			else
				writer.Write ((byte)(value - 10 + (byte)'A'));
		}

		public void WriteWord (int value)
		{
			WriteWord ((UInt16)value);
		}
		
		public void WriteWord (UInt16 value)
		{
			WriteNumber ((byte)(value / 36));
			WriteNumber ((byte)(value % 36));
		}
		
		public void WriteInt (UInt32 value)
		{
			WriteWord ((UInt16)(value / 1296));
			WriteWord ((UInt16)(value % 1296));
		}
		
		public void Write (Point value)
		{
			WriteWord (value.X);
			WriteWord (value.Y);
		}

		public void Write (Size value)
		{
			WriteWord (value.Width);
			WriteWord (value.Height);
		}

		public void Write (Rectangle value)
		{
			WriteWord (value.X);
			WriteWord (value.Y);
			WriteWord (value.X + value.Width - 1);
			WriteWord (value.Y + value.Height - 1);
		}
		
		void Write (byte[] bytes)
		{
			if (MaxWidth != null) {
				int left = bytes.Length;
				int pos = 0;
				while (left > 0) {
					CheckNewline ();
					var len = (int)Math.Min (left, MaxWidth.Value - (inner.Position - lastNewLine) - 1);
					writer.Write (bytes, pos, len);
					pos += len;
					left -= len;
				}
			}
			else
				writer.Write (bytes);
		}
		
		public void Write (string value)
		{
			Write (FormatRip.Encoding.GetBytes (value));
		}
		
	}
}

