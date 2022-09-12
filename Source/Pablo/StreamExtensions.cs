using System.IO;

namespace Pablo
{
	static class StreamExtensions
	{
		public static bool IsEOF(this Stream stream)
		{
			if (!stream.CanSeek)
				return false;
			return stream.Position >= stream.Length;
		}
	}
}

