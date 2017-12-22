using System;
using System.IO;
using Eto.Drawing;
using System.Text;

namespace Pablo.Formats.Rip
{
	public static class RipExtensions
	{
		public static byte ReadRipByte(this BinaryReader reader)
		{
			byte b = reader.ReadByte ();
			if (b == (byte)'\\') {
				b = (byte)reader.ReadByte ();
				while (b == (byte)'\n' || b == (byte)'\r') {
					b = reader.ReadByte ();
				}
			}
			return b;
		}
		
		public static byte ReadRipNumber (this BinaryReader reader)
		{
			byte b = ReadRipByte (reader);

			if (b >= 48 && b <= 57)
				return (byte)(b - 48);
			if (b >= 65 && b <= 90)
				return (byte)(b - 55);
			return 0;
		}

		public static UInt16 ReadRipWord (this BinaryReader reader)
		{
			return (UInt16)(reader.ReadRipNumber () * 36 + reader.ReadRipNumber ());
		}
		
		public static UInt32 ReadRipInt (this BinaryReader reader)
		{
			return (UInt32)reader.ReadRipWord () * (UInt32)1296 + (UInt32)reader.ReadRipWord ();
		}
		
		public static Point ReadRipPoint (this BinaryReader reader)
		{
			return new Point (
				reader.ReadRipWord (),
				reader.ReadRipWord ()
				);
		}

		public static Size ReadRipSize (this BinaryReader reader)
		{
			return new Size (
				reader.ReadRipWord (),
				reader.ReadRipWord ()
				);
		}
		
		public static Rectangle ReadRipRectangle (this BinaryReader reader)
		{
			var start = reader.ReadRipPoint ();
			var end = reader.ReadRipPoint ();
			return new Rectangle (start, end);
		}
		
		public static string ReadRipString (this BinaryReader reader)
		{
			var sb = new StringBuilder ();
			do {
				var next = reader.PeekChar ();
				if (next == -1 || (byte)next == 13 || (byte)next == 10 || (byte)next == (byte)'|')
					break;
				var cur = ReadRipByte (reader);
				sb.Append (FormatRip.Encoding.GetString (new byte[] {cur}));
			} while (true);

			var b = reader.PeekChar ();
			while ((byte)b == 13 || (byte)b == 10) {
				reader.ReadByte ();
				b = (byte)reader.PeekChar ();
			}
			return sb.ToString ();
		}
		
	}
}

