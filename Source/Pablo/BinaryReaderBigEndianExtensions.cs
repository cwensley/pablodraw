using System;
using System.IO;

namespace Pablo
{
	public static class BinaryReaderBigEndianExtensions
	{
		public static void WriteBigEndian(this BinaryWriter writer, Int16 value)
		{
			WriteBigEndian(writer, BitConverter.GetBytes (value));
		}

		public static void WriteBigEndian(this BinaryWriter writer, UInt16 value)
		{
			WriteBigEndian(writer, BitConverter.GetBytes (value));
		}

		public static void WriteBigEndian(this BinaryWriter writer, Int32 value)
		{
			WriteBigEndian(writer, BitConverter.GetBytes (value));
		}

		public static void WriteBigEndian(this BinaryWriter writer, UInt32 value)
		{
			WriteBigEndian(writer, BitConverter.GetBytes (value));
		}
		
		public static void WriteBigEndian(this BinaryWriter writer, byte[] bytes)
		{
			for (int i = bytes.Length - 1; i >= 0; i--)
				writer.Write (bytes[i]);
		}
		
		public static void WriteBytes(this BinaryWriter writer, byte @byte, int count)
		{
			for (int i=0; i<count; i++)
			{
				writer.Write (@byte);
			}
		}
		
		
		public static short ReadBigEndianInt16 (this BinaryReader reader)
		{
			return BitConverter.ToInt16 (ReadBigEndianBytes (reader, 2), 0);
		}

		public static ushort ReadBigEndianUInt16 (this BinaryReader reader)
		{
			return BitConverter.ToUInt16 (ReadBigEndianBytes (reader, 2), 0);
		}

		public static int ReadBigEndianInt32 (this BinaryReader reader)
		{
			return BitConverter.ToInt32 (ReadBigEndianBytes (reader, 4), 0);
		}

		public static uint ReadBigEndianUInt32 (this BinaryReader reader)
		{
			return BitConverter.ToUInt32 (ReadBigEndianBytes (reader, 4), 0);
		}
		
		public static byte[] ReadBigEndianBytes (this BinaryReader reader, int count)
		{
			byte[] bytes = new byte[count];
			for (int i = count - 1; i >= 0; i--)
				bytes [i] = reader.ReadByte ();

			return bytes;
		}
	}
}

