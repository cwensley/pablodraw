using System;
using System.Collections.Generic;
using System.IO;

namespace Pablo
{
	public static class Extensions
	{
		public static IEnumerable<string> Chunks(this string text, int chunkSize)
		{
		    int offset = 0;
		    while (offset < text.Length)
		    {
		        int size = Math.Min(chunkSize, text.Length - offset);
		        yield return text.Substring(offset, size);
		        offset += size;
		    }
		}
		
		public static void WriteTo(this Stream source, Stream stream)
		{
			var buffer = new byte[4096];
			while (true) {
				var len = source.Read (buffer, 0, buffer.Length);
				if (len <= 0) break;
				stream.Write (buffer, 0, len);
			}
		}
		
	}
}

