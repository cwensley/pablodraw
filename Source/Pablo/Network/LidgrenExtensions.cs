using System;
using Lidgren.Network;
using System.IO;
using Eto.Drawing;
using System.Net;

namespace Pablo.Network
{
	public static class LidgrenExtensions
	{
		public const int BufferSize = 1024 * 8;

		public static void WriteDate (this NetOutgoingMessage message, DateTime? date)
		{
			message.Write (date == null);
			if (date != null) {
				message.WriteVariableInt32 (date.Value.Day);
				message.WriteVariableInt32 (date.Value.Month);
				message.WriteVariableInt32 (date.Value.Year);
			}
		}
		
		public static void WriteDate (this NetOutgoingMessage message, DateTime date)
		{
			message.WriteVariableInt32 (date.Day);
			message.WriteVariableInt32 (date.Month);
			message.WriteVariableInt32 (date.Year);
		}
		
		public static DateTime ReadDate (this NetIncomingMessage message)
		{
			var day = message.ReadVariableInt32 ();
			var month = message.ReadVariableInt32 ();
			var year = message.ReadVariableInt32 ();
			return new DateTime (year, month, day);
		}
		
		public static void Write<T> (this NetOutgoingMessage message, T val)
			where T: INetworkReadWrite
		{
			message.Write (val == null);
			if (val != null) {
				val.Send (new SendCommandArgs (message, null));
			}
		}
		
		public static T Read<T> (this NetIncomingMessage message)
			where T: INetworkReadWrite, new()
		{
			var isnull = message.ReadBoolean ();
			if (isnull)
				return default(T);
			var val = new T ();
			val.Receive (new ReceiveCommandArgs (message, null, null));
			return val;
		}

		public static void Write<T> (this NetOutgoingMessage message, T[] val)
			where T: INetworkReadWrite
		{
			if (val == null) {
				message.WriteVariableInt32(0);
				return;
			}
			message.WriteVariableInt32(val.Length);
			for (int i=0; i<val.Length; i++) {
				Write<T>(message, val[i]);
			}
		}
		
		
		public static T[] ReadArray<T> (this NetIncomingMessage message)
			where T: INetworkReadWrite, new()
		{
			var count = message.ReadVariableInt32 ();
			var result = new T[count];
			for (int i=0; i<count; i++) {
				result[i] = Read<T>(message);
			}
			return result;
		}
		
		public static DateTime? ReadNullableDate (this NetIncomingMessage message)
		{
			var isnull = message.ReadBoolean ();
			if (isnull)
				return null;
			var day = message.ReadVariableInt32 ();
			var month = message.ReadVariableInt32 ();
			var year = message.ReadVariableInt32 ();
			return new DateTime (year, month, day);
		}
		
		public static void WriteStream (this NetOutgoingMessage message, Stream stream)
		{
			int size = (int)stream.Length;
			message.Write (size);
			var left = size;
			var buffer = new byte[Math.Min (size, BufferSize)];
			while (left > 0) {
				var bufsize = Math.Min (left, buffer.Length);
				stream.Read (buffer, 0, bufsize);
				message.Write (buffer, 0, bufsize);
				left -= bufsize;
			}
		}

		public static Stream ReadStream (this NetIncomingMessage message)
		{
			var size = message.ReadInt32 ();
			
			var stream = new MemoryStream (size);
			var buffer = new byte[Math.Min (size, BufferSize)];
			var left = size;
			while (left > 0) {
				var bufsize = Math.Min (left, buffer.Length);
				message.ReadBytes (buffer, 0, bufsize);
				stream.Write (buffer, 0, bufsize);
				left -= bufsize;
			}
			stream.Seek (0, SeekOrigin.Begin);
			return stream;
		}
		
		public static T ReadEnum<T> (this NetIncomingMessage message)
			where T: struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException ("T must be an enumerated type");
			return (T)Enum.ToObject (typeof(T), message.ReadVariableInt32 ());
		}
		
		public static void WriteEnum<T> (this NetOutgoingMessage message, T value)
			where T: struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException ("T must be an enumerated type");
			message.WriteVariableInt32 (Convert.ToInt32 (value));
		}
		
		public static Size ReadSize (this NetIncomingMessage message)
		{
			return new Size (message.ReadVariableInt32 (), message.ReadVariableInt32 ());
		}
		
		public static void Write (this NetOutgoingMessage message, Size size)
		{
			message.WriteVariableInt32 (size.Width);
			message.WriteVariableInt32 (size.Height);
		}

		public static Point ReadPoint (this NetIncomingMessage message)
		{
			return new Point (message.ReadVariableInt32 (), message.ReadVariableInt32 ());
		}
		
		public static void Write (this NetOutgoingMessage message, Point point)
		{
			message.WriteVariableInt32 (point.X);
			message.WriteVariableInt32 (point.Y);
		}

		public static Rectangle ReadRectangle (this NetIncomingMessage message)
		{
			return new Rectangle (message.ReadPoint (), message.ReadSize ());
		}
		
		public static void Write (this NetOutgoingMessage message, Rectangle rectangle)
		{
			message.Write (rectangle.Location);
			message.Write (rectangle.Size);
		}
		
		public static Color ReadColor (this NetIncomingMessage message)
		{
			var r = message.ReadFloat ();
			var g = message.ReadFloat ();
			var b = message.ReadFloat ();
			var a = message.ReadFloat ();
			return new Color (r, g, b, a);
		}
		
		public static void Write (this NetOutgoingMessage message, Color color)
		{
			message.Write (color.R);
			message.Write (color.G);
			message.Write (color.B);
			message.Write (color.A);
		}
		
		public static Palette ReadPalette (this NetIncomingMessage message)
		{
			var count = message.ReadInt32 ();
			var palette = new Palette ();
			for (int i=0; i<count; i++) {
				palette.Add (message.ReadColor ());
			}
			return palette;
		}
		
		public static void Write (this NetOutgoingMessage message, Palette palette)
		{
			message.Write (palette.Count);
			for (int i=0; i<palette.Count; i++) {
				message.Write (palette [i]);
			}
		}
		
		public static void Write (this NetOutgoingMessage message, IPAddress address)
		{
			if (address == null) {
				message.Write ((byte)0);
				return;
			}
			
			byte[] bytes = address.GetAddressBytes();
			message.Write((byte)bytes.Length);
			if (bytes.Length > 0)
				message.Write(bytes);
		}
		
		public static IPAddress ReadIPAddress (this NetIncomingMessage message)
		{
			byte len = message.ReadByte();
			if (len == 0)
				return null;
			byte[] addressBytes = message.ReadBytes(len);
			return new IPAddress(addressBytes);
		}
	}
}

