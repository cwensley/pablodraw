using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Eto;
using System.Reflection;

namespace Pablo.Formats.Character
{
	public class BitFontSet
	{
		readonly List<BitFont> fonts = new List<BitFont>();
		Encoding encoding;
		public int[] FallbackCodePages { get; set; }

		public int CodePage { get; set; }

		public string Name { get; set; }

		public Func<Stream> GetStream { get; set; }

		public Encoding Encoding
		{
			get
			{
				if (encoding == null)
				{
					var encodings = Encoding.GetEncodings();
					var info = encodings.FirstOrDefault(r => r.CodePage == CodePage);
					if (info == null && FallbackCodePages != null)
					{
						for (int i = 0; i < FallbackCodePages.Length && info == null; i++)
						{
							var ii = i;
							info = encodings.FirstOrDefault(r => r.CodePage == FallbackCodePages[ii]);
						}
					}
					encoding = info != null ? info.GetEncoding() : BitFont.StandardEncoding;
				}
				return encoding;
			}
		}

		public IList<BitFont> Fonts
		{
			get
			{
				return fonts;
			}
		}

		public static BitFontSet FromResource(string resource, string name, int[] forceCodePage = null, bool isSystemFont = true, Assembly asm = null)
		{
			asm = asm ?? Assembly.GetExecutingAssembly();
			return FromStream(asm.GetManifestResourceStream(resource), name, forceCodePage, isSystemFont);
		}
		public static BitFontSet FromFontResources(string resource, string name, int codePage, int[] heights, bool isSystemFont = true, int[] fallbackCodePages = null, int numChars = 256, Assembly asm = null)
		{
			asm = asm ?? Assembly.GetExecutingAssembly();
			var fontSet = new BitFontSet { Name = name };
			foreach (var height in heights)
			{
				var id = string.Format("CP {0} {1}x{2}", codePage, 8, height);
				string sauceID;
				float ratio;
				const int width = 8;
				switch (height)
				{
					case 8:
						sauceID = "IBM VGA50";
						ratio = 480f / 400f;
						break;
					case 14:
						sauceID = "IBM EGA";
						ratio = 480f / 350f;
						break;
					case 16:
						sauceID = "IBM VGA";
						ratio = 480f / 400f;
						break;
					case 19:
						sauceID = "IBM VGA25G";
						ratio = 1f;
						break;
					default:
						throw new NotSupportedException("Font height is not supported using this method");
				}
				if (codePage != 437)
					sauceID += " " + codePage;
				var resourceName = string.Format("{0}.F{1:00}", resource, height);
				var bitFont = BitFont.FromResource(resourceName, numChars, codePage, width, height, id, id, fontSet, isSystemFont);
				bitFont.SauceID = sauceID;
				bitFont.LegacyRatio = ratio;
				fontSet.Fonts.Add(bitFont);
			}

			return fontSet;
		}

		public static BitFontSet FromStream(Stream stream, string name, int[] forceCodePage = null, bool isSystemFont = true)
		{
			var val = new BitFontSet { Name = name };
			val.Load(stream, forceCodePage, isSystemFont);
			return val;
		}

		public static IEnumerable<BitFontSet> DosFonts()
		{
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP437", "IBM PC", 437, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP737", "Greek", 737, new [] { 8, 14, 16 }, fallbackCodePages: new [] { 869 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP851", "Greek", 851, new [] { 8, 14, 16, 19 }, fallbackCodePages: new [] { 869 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP869", "Greek", 869, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP775", "Baltic Rim", 775, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP850", "Latin-1 Western European", 850, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP852", "Latin-2 Central European", 852, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP853", "Latin-3 Multilingual", 853, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP855", "Cyrillic", 855, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP866", "Cyrillic", 866, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP857", "Turkish", 857, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP860", "Portuguese", 860, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP861", "Icelandic", 861, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP862", "Hebrew", 862, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP863", "French Canadian", 863, new [] { 8, 14, 16, 19 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP864", "Arabic", 864, new [] { 8, 14, 16 });
			yield return BitFontSet.FromFontResources("Pablo.Formats.Character.Fonts.DOS.CP865", "Nordic", 865, new [] { 8, 14, 16, 19 });
		}

		public static BitFontSet AmigaFont()
		{
			var fontSet = new BitFontSet { Name = "Amiga" };
			const int codePage = 1252;
			const int numChars = 256;
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.MicroKnight.F16", numChars, codePage, 8, 16, "amiga-microknight", "MicroKnight", fontSet, true, "Amiga MicroKnight", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.MicroKnightPlus.F16", numChars, codePage, 8, 16, "amiga-microknight+", "MicroKnight+", fontSet, true, "Amiga MicroKnight+", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.mO'sOul.F16", numChars, codePage, 8, 16, "amiga-mOsOul", "mO'sOul", fontSet, true, "Amiga mOsOul", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.P0T-NOoDLE.F16", numChars, codePage, 8, 16, "amiga-pot-noodle", "P0T-NOoDLE", fontSet, true, "Amiga P0T-NOoDLE", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.Topaz_a1200.F16", numChars, codePage, 8, 16, "amiga-topaz-1200", "Topaz 1200", fontSet, true, "Amiga Topaz 2", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.Topaz_a500.F16", numChars, codePage, 8, 16, BitFont.StandardAmigaFontID, "Topaz 500", fontSet, true, "Amiga Topaz 1", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.TopazPlus_a1200.F16", numChars, codePage, 8, 16, "amiga-topaz-plus-1200", "Topaz+ 1200", fontSet, true, "Amiga Topaz 2+", 1.2f));
			fontSet.Fonts.Add(BitFont.FromResource("Pablo.Formats.Character.Fonts.Amiga.TopazPlus_a500.F16", numChars, codePage, 8, 16, "amiga-topaz-plus-500", "Topaz+ 500", fontSet, true, "Amiga Topaz 1+", 1.2f));
			return fontSet;
		}

		public BitFontSet()
		{
		}

		public BitFontSet(BitFont font)
		{
			this.CodePage = font.CodePage;
			this.fonts.Add(font);
		}

		public void Load(Stream stream, int[] forceCodePage = null, bool isSystemFont = true)
		{
			FallbackCodePages = forceCodePage;
			var br = new BinaryReader(stream);
			var headerSize = br.ReadInt16();
			/*var nextOffset =*/
			br.ReadInt32();
			var deviceType = br.ReadInt16();
			// must be 1
			FallbackCodePages = forceCodePage;

			/*var deviceName =*/
			br.ReadBytes(8);
			var codePage = br.ReadInt16();
			CodePage = codePage;
			/*var reserved =*/
			br.ReadBytes(6);
			/*var offset =*/
			br.ReadInt32();

			if (headerSize > 0x1C)
				br.ReadBytes(0x1C - headerSize);

			// CodePageInfoHeader
			//stream.Position = offset;
			/*var version =*/
			br.ReadInt16();
			var numFonts = br.ReadInt16();
			/*var size =*/
			br.ReadInt16();
			for (int i = 0; i < numFonts; i++)
			{
				if (deviceType == 1)
				{
					// screen
					var height = br.ReadByte();
					var width = br.ReadByte();
					// should be 8
					/*var yaspect =*/
					br.ReadByte();
					/*var xaspect =*/
					br.ReadByte();
					var numChars = br.ReadInt16();
					var font = new BitFont(numChars, width, height, CodePage);
					font.ID = string.Format("{0} {1}x{2}", codePage, width, height);
					font.Name = string.Format("CP {0} {1}x{2}", codePage, width, height);
					font.IsSystemFont = isSystemFont;
					font.FallbackCodePages = forceCodePage;
					font.FontSet = this;
					font.Load(br);
					fonts.Add(font);
				}
				else if (deviceType == 2)
				{
					// printer
					throw new NotImplementedException();
				}
			}
			fonts.Sort((x, y) => x.Height.CompareTo(y.Height));

		}
	}
}

