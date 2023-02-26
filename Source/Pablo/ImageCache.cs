using Eto.Drawing;
using Pablo.Formats.Character;
using Pablo.Formats.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pablo
{
	public static class ImageCache
	{
		static Dictionary<string, object> cache = new Dictionary<string, object>();

		public static Icon IconFromResource(string resource, Assembly assembly = null)
		{
			assembly = assembly ?? Assembly.GetCallingAssembly();
			if (cache.TryGetValue(resource, out var val) && val is Icon icon)
				return icon;
			icon = Icon.FromResource(resource, assembly);
			cache[resource] = icon;
			return icon;
		}

		public static Bitmap BitmapFromResource(string resource, Assembly assembly = null)
		{
			assembly = assembly ?? Assembly.GetCallingAssembly();
			if (cache.TryGetValue(resource, out var val) && val is Bitmap bitmap)
				return bitmap;
			bitmap = Bitmap.FromResource(resource, assembly);
			cache[resource] = bitmap;
			return bitmap;
		}
		
		public static CharacterDocument CharacterFromResource(string resource, bool adjustPalette = true, Assembly assembly = null)
		{
			assembly = assembly ?? Assembly.GetCallingAssembly();
			if (cache.TryGetValue(resource, out var val) && val is CharacterDocument doc)
				return doc;

			var stream = assembly.GetManifestResourceStream(resource);

			var format = DocumentInfoCollection.Default.FindFormat(resource);
			doc = new CharacterDocument(format.Info);
			doc.Load(stream, format, null);
			doc.AdjustPaletteForDarkMode = adjustPalette;
			cache[resource] = doc;
			return doc;
		}
	}
}
