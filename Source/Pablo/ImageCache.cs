using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pablo
{
	public static class ImageCache
	{
		static Dictionary<string, Image> cache = new Dictionary<string, Image>();

		public static Icon IconFromResource(string resource, Assembly assembly = null)
		{
			assembly = assembly ?? Assembly.GetCallingAssembly();
			Image img;
			if (cache.TryGetValue(resource, out img))
				return (Icon)img;
			var icon = Icon.FromResource(resource, assembly);
			cache[resource] = icon;
			return icon;
		}

		public static Bitmap BitmapFromResource(string resource, Assembly assembly = null)
		{
			assembly = assembly ?? Assembly.GetCallingAssembly();
			Image img;
			if (cache.TryGetValue(resource, out img))
				return (Bitmap)img;
			var bitmap = Bitmap.FromResource(resource, assembly);
			cache[resource] = bitmap;
			return bitmap;
		}
	}
}
