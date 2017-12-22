using System;
using System.IO;
using System.Collections;
using Eto;
using Eto.Drawing;
using Pablo.BGI;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace Pablo.Formats.Character
{
	public class BitFont : IBGIFont
	{
		public const int StartCopy9 = 192;
		public const int EndCopy9 = 223;
		public const int StandardCodePage = 437;
		public const string StandardAmigaFontID = "amiga-topaz-500";
		public static Encoding StandardEncoding = Encoding.GetEncoding(StandardCodePage);
		FontCharacter[] chars;
		Encoding encoding;
		readonly string resourceName;
		readonly Assembly resourceAssembly;
		readonly int resourceNumChars;

		public int[] FallbackCodePages { get; set; }

		public int CodePage { get; set; }

		public string SauceID { get; set; }

		public float LegacyRatio { get; set; }

		public bool Is9xFont { get; private set; }

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
							info = encodings.FirstOrDefault(r => r.CodePage == FallbackCodePages[i]);
						}
					}
					encoding = info != null ? info.GetEncoding() : BitFont.StandardEncoding;
				}
				return encoding;
			}
		}

		public string ID { get; set; }

		public string Name { get; set; }

		public BitFontSet FontSet { get; set; }

		public string DisplayName
		{
			get
			{
				return FontSet != null ? string.Format("{0} - {1}", FontSet.Name, Name) : Name;
			}
		}

		public float Aspect
		{
			get { return (float)Width / (float)Height; }
		}

		public static BitFont FromResource(string resourceName, int numChars, int codePage, int width, int height)
		{
			var font = new BitFont(numChars, width, height, codePage);
			font.Load(new BinaryReader(Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName)));
			return font;
		}

		public static BitFont FromResource(string resourceName, int numChars, int codePage, int width, int height, string id, string name, BitFontSet set, bool isSystemFont, string sauceID = null, float? ratio = null, Assembly assembly = null)
		{
			var font = new BitFont(resourceName, numChars, width, height, codePage, assembly ?? Assembly.GetCallingAssembly());
			font.ID = id;
			font.Name = name;
			font.FontSet = set;
			font.IsSystemFont = isSystemFont;
			font.SauceID = sauceID;
			if (ratio != null)
				font.LegacyRatio = ratio.Value;
			return font;
		}

		BitFont(string resourceName, int numChars, int width, int height, int codePage, Assembly assembly = null)
		{
			this.resourceNumChars = numChars;
			this.resourceName = resourceName;
			this.resourceAssembly = assembly ?? Assembly.GetCallingAssembly();
			CodePage = codePage;
			Width = width;
			Height = height;
			LegacyRatio = GetLegacyRatio(height);
		}

		public BitFont(Stream stream, int numChars, int width, int height, int codePage)
			: this(numChars, width, height, codePage)
		{
			var br = new BinaryReader(stream);
			Load(br);
			LegacyRatio = GetLegacyRatio(height);
		}

		public BitFont(Stream stream, int codePage)
		{
			CodePage = codePage;
			var br = new BinaryReader(stream);
			Width = br.ReadInt16();
			Height = br.ReadInt16();
			LegacyRatio = GetLegacyRatio(Height);
			int nc = br.ReadInt16();
			chars = new FontCharacter[nc];
			for (int i = 0; i < nc; i++)
			{
				chars[i] = new FontCharacter(this);
			}
			Load(br);
		}

		public BitFont(BitFont source)
		{
			if (source.chars == null)
				source.EnsureLoaded();
			ID = source.ID;
			Name = source.Name;
			FontSet = source.FontSet;
			CodePage = source.CodePage;
			chars = new FontCharacter[source.chars.Length];
			Width = source.Width;
			Height = source.Height;
			LegacyRatio = source.LegacyRatio;
			Is9xFont = source.Is9xFont;
			SauceID = source.SauceID;
			IsStandardFont = source.IsStandardFont;
			IsSystemFont = source.IsSystemFont;

			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = new FontCharacter(source.chars[i], this);
			}
		}

		public BitFont(int numChars, int width, int height, int codePage)
		{
			CodePage = codePage;
			chars = new FontCharacter[numChars];
			Width = width;
			Height = height;
			LegacyRatio = GetLegacyRatio(height);

			for (int i = 0; i < numChars; i++)
			{
				chars[i] = new FontCharacter(this);
			}
		}

		public int NumChars
		{
			get
			{ 
				return chars != null ? chars.Length : resourceNumChars;
			}
		}

		public Size Size
		{
			get { return new Size(Width, Height); }
		}

		public void Resize(int width, int height, bool scale, bool copy9 = false)
		{
			if (chars == null)
				EnsureLoaded();
			Is9xFont = Width == 8 && width == 9 && copy9;
			for (int ch = 0; ch < chars.Length; ch++)
			{
				chars[ch].Resize(width, height, scale, IsCopy9(ch));
			}
			LegacyRatio = LegacyRatio * width / Width; // adjust ratio
			Width = width;
			Height = height;
		}

		public static float GetLegacyRatio(int height)
		{
			switch (height)
			{
				case 8:
					return 480f / 400f;
				case 14:
					return  480f / 350f;
				case 16:
					return 480f / 400f;
				default:
					return 1f;
			}
		}

		bool IsCopy9(int character)
		{
			return Is9xFont && character >= BitFont.StartCopy9 && character <= BitFont.EndCopy9;
		}

		public event EventHandler<EventArgs> Changed;

		static BitFont standard8x8;

		public static BitFont GetStandard8x8()
		{
			if (standard8x8 == null)
			{
				standard8x8 = BitFont.FromResource("Pablo.Formats.Character.Fonts.DOS.CP437.F08", 256, 437, 8, 8);
				standard8x8.ID = "CP 437 8x8";
				standard8x8.LegacyRatio = 1.2f;
				standard8x8.SauceID = "IBM VGA50";
				standard8x8.Name = "IBM PC - " + standard8x8.ID;
				standard8x8.IsSystemFont = true;
				standard8x8.IsStandardFont = true;
			}
			return standard8x8;
		}

		static BitFont standard8x16;

		public static BitFont GetStandard8x16()
		{
			if (standard8x16 == null)
			{
				standard8x16 = BitFont.FromResource("Pablo.Formats.Character.Fonts.DOS.CP437.F16", 256, 437, 8, 16);
				standard8x16.ID = "CP 437 8x16";
				standard8x16.SauceID = "IBM VGA";
				standard8x16.LegacyRatio = 1.2f;
				standard8x16.Name = "IBM PC - " + standard8x16.ID;
				standard8x16.IsSystemFont = true;
				standard8x16.IsStandardFont = true;
			}
			return standard8x16;
		}

		public bool IsStandardFont { get; private set; }

		public bool IsSystemFont { get; set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public FontCharacter this [int index]
		{
			get
			{
				if (chars == null)
					EnsureLoaded();
				return chars[index];
			}
			set
			{
				if (chars == null)
					EnsureLoaded();
				chars[index] = value;
				if (Changed != null)
					Changed(this, EventArgs.Empty);
			}
		}

		void EnsureLoaded()
		{
			chars = new FontCharacter[resourceNumChars];
			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = new FontCharacter(this);
			}
			using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
			{
				Load(new BinaryReader(stream));
			}
		}

		public bool Equals(BitFont font)
		{
			if (IsSystemFont && font.IsSystemFont)
				return ID == font.ID;
			if (chars == null)
				EnsureLoaded();
			if (font.chars == null)
				font.EnsureLoaded();
			if (font.chars.Length != chars.Length || font.Width != Width || font.Height != Height)
				return false;
			for (int i = 0; i < chars.Length; i++)
			{
				if (!chars[i].Equals(font.chars[i]))
					return false;
			}
			return true;
		}

		public void FullSave(BinaryWriter bw)
		{
			bw.Write((Int16)Width);
			bw.Write((Int16)Height);
			bw.Write((Int16)NumChars);

			Save(bw);
		}

		public void Save(BinaryWriter bw)
		{
			if (chars == null)
				EnsureLoaded();
			for (int i = 0; i < chars.Length; i++)
			{
				chars[i].Write(bw);
			}
		}

		public void Load(BinaryReader br)
		{
			if (chars == null)
				throw new InvalidOperationException("Cannot load into a font that has not already been loaded");
			for (int i = 0; i < chars.Length; i++)
			{
				chars[i].Read(br);
			}
			if (Changed != null)
				Changed(this, EventArgs.Empty);
		}

		#region IBGIFont Members

		public float DrawCharacter(BGICanvas bgi, float x, float y, BGICanvas.Direction dir, int size, byte character, IList<Rectangle> updates)
		{
			byte foreCol = bgi.GetColor();
			FontCharacter fc = this[character];
			if (dir == BGICanvas.Direction.Horizontal)
			{
				int starty = (int)y;
				for (int cy = 0; cy < Height; cy++)
				{
					int startx = (int)x;
					for (int cx = 0; cx < Width; cx++)
					{
						if (fc[cx, cy])
						{
							if (size == 1)
							{
								bgi.PutPixelInternal(startx, starty, foreCol);
							}
							else
							{
								for (int ccy = 0; ccy < size; ccy++)
								{
									for (int ccx = 0; ccx < size; ccx++)
									{
										bgi.PutPixelInternal(startx + ccx, starty + ccy, foreCol);
									}
								}
							}
						}
						startx += size;
					}
					starty += size;
				}
				var updateRect = new Rectangle((int)x, (int)y, Width * size, Height * size);
				if (updates != null)
					updates.Add(updateRect);
				else
					bgi.UpdateRegion(updateRect);
			}
			else
			{
				int startx = (int)x;
				for (int cy = 0; cy < Height; cy++)
				{
					int starty = (int)y;
					for (int cx = 0; cx < Width; cx++)
					{
						if (fc[cx, cy])
						{
							if (size == 1)
							{
								bgi.PutPixelInternal(startx, starty, foreCol);
							}
							else
							{
								for (int ccy = 0; ccy < size; ccy++)
								{
									for (int ccx = 0; ccx < size; ccx++)
									{
										bgi.PutPixelInternal(startx + ccx, starty - ccy, foreCol);
									}
								}
							}
						}
						starty -= size;
					}
					startx += size;
				}
				var updateRect = new Rectangle((int)x, (int)y, Height * size, Width * size);
				if (updates != null)
					updates.Add(updateRect);
				else
					bgi.UpdateRegion(updateRect);
			}
			return Width * size;
		}

		public Size GetTextSize(string str, BGICanvas.Direction dir, int size)
		{
			if (dir == BGICanvas.Direction.Horizontal)
				return new Size(str.Length * Width * size, Height * size);
			else
				return new Size(Height * size, str.Length * Width * size);
		}

		public Size GetRealTextSize(string str, BGICanvas.Direction dir, int size)
		{
			return GetTextSize(str, dir, size);
		}

		public Size GetMaxCharacterSize(BGICanvas.Direction dir, int size)
		{
			if (dir == BGICanvas.Direction.Horizontal)
				return new Size(Width * size, Height * size);
			else
				return new Size(Height * size, Width * size);
		}

		#endregion

	}

	public class FontCharacter
	{
		int[] data;
		BitFont font;

		void Set(int index, bool value)
		{
			if (value)
			{
				data[index >> 5] |= 1 << index;
			}
			else
			{
				data[index >> 5] &= ~(1 << index);
			}
		}

		void Set(int[] data, int index, bool value)
		{
			if (value)
			{
				data[index >> 5] |= 1 << index;
			}
			else
			{
				data[index >> 5] &= ~(1 << index);
			}
		}

		public int GetData(int index)
		{
			return data[index];
		}

		internal void Resize(int newWidth, int newHeight, bool scale, bool copy9)
		{
			int oldWidth = font.Width;
			int oldHeight = font.Height;
			var newdata = new int[(newWidth * newHeight + 31) / 32];
			for (int y = 0; y < newHeight; y++)
			{
				var oldrow = (y * oldHeight / newHeight) * font.Width;
				var newrow = y * newWidth;
				for (int x = 0; x < newWidth; x++)
				{
					if (scale)
					{
						Set(newdata, newrow + x, this[oldrow + x * oldWidth / newWidth]);
					}
					else
					{
						if (x < oldWidth)
							Set(newdata, newrow + x, this[oldrow + x]);
						else if (copy9 && x == newWidth - 1)
							Set(newdata, newrow + x, this[oldrow + x - 1]);
					}
				}
			}
			data = newdata;
		}

		internal FontCharacter(FontCharacter fc, BitFont font)
		{
			data = (int[])fc.data.Clone();
			this.font = font;
		}

		public FontCharacter(BitFont font)
		{
			this.font = font;
			data = new int[(font.Width * font.Height + 31) / 32];
		}

		public bool this [int x, int y]
		{
			get { return this[y * font.Width + x]; }
			set { Set(y * font.Width + x, value); }
		}

		public bool this [int index]
		{
			get { return (data[index / 32] & 1 << index) != 0; }
			set { Set(index, value); }
		}

		byte[] GetByteRow(int index)
		{
			var width = font.Is9xFont ? 8 : font.Width;
			var ba = new BitArray(width);
			var row = index * font.Width;
			for (int x = 0; x < width; x++)
			{
				ba[width - x - 1] = this[row + x];
			}
			var bytes = new byte[(ba.Length + 7) / 8];
			ba.CopyTo(bytes, 0);
			return bytes;
		}

		public bool Equals(FontCharacter character)
		{
			return data.Equals(character.data);
		}

		public static byte ReverseBitsWith7Operations(byte b)
		{
			return (byte)(((b * 0x0802u & 0x22110u) | (b * 0x8020u & 0x88440u)) * 0x10101u >> 16);
		}

		public unsafe void Read(BinaryReader br)
		{
			if (font.Width == 8)
			{
				fixed (int* ptr = data)
				{
					byte* bptr = (byte*)ptr;
					for (int y = 0; y < font.Height; y++)
					{
						*bptr = ReverseBitsWith7Operations(br.ReadByte());
						bptr++;
					}
				}
			}
			else
			{
				for (int y = 0; y < font.Height; y++)
				{
					var row = y * font.Width;
					byte currentRow = br.ReadByte();
					int currentBit = 128;
					for (int x = 0; x < font.Width; x++)
					{
						if (currentBit == 0)
							currentRow = br.ReadByte();
						this[row + x] = (currentRow & currentBit) != 0;
						currentBit >>= 1;
					}
				}
			}
		}

		public void Write(BinaryWriter bw)
		{
			for (int i = 0; i < font.Height; i++)
			{
				bw.Write(GetByteRow(i));
			}
		}
	}
}
