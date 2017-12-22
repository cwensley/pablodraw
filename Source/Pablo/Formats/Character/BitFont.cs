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

		public static BitFont FromResource(string resourceName, int numChars, int codePage, int width, int height, string id, string name, BitFontSet set, bool isSystemFont, string sauceID = null, float ratio = 1f)
		{
			var font = new BitFont(resourceName, numChars, width, height, codePage, Assembly.GetCallingAssembly());
			font.ID = id;
			font.Name = name;
			font.FontSet = set;
			font.IsSystemFont = isSystemFont;
			font.SauceID = sauceID;
			font.LegacyRatio = ratio;
			return font;
		}

		public BitFont(string resourceName, int numChars, int width, int height, int codePage, Assembly assembly = null)
		{
			this.resourceNumChars = numChars;
			this.resourceName = resourceName;
			this.resourceAssembly = assembly ?? Assembly.GetCallingAssembly();
			CodePage = codePage;
			Width = width;
			Height = height;
			LegacyRatio = 1f;
		}

		public BitFont(Stream stream, int numChars, int width, int height, int codePage)
			: this(numChars, width, height, codePage)
		{
			var br = new BinaryReader(stream);
			Load(br);
		}

		public BitFont(Stream stream, int codePage)
		{
			CodePage = codePage;
			var br = new BinaryReader(stream);
			Width = br.ReadInt16();
			Height = br.ReadInt16();
			LegacyRatio = 1f;
			int nc = br.ReadInt16();
			chars = new FontCharacter[nc];
			for (int i = 0; i < nc; i++)
			{
				chars[i] = new FontCharacter(Width, Height);
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
			SauceID = source.SauceID;
			IsStandardFont = source.IsStandardFont;
			IsSystemFont = source.IsSystemFont;

			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = new FontCharacter(source.chars[i]);
			}
		}

		public BitFont(int numChars, int width, int height, int codePage)
		{
			CodePage = codePage;
			chars = new FontCharacter[numChars];
			Width = width;
			Height = height;
			LegacyRatio = 1f;

			for (int i = 0; i < numChars; i++)
			{
				chars[i] = new FontCharacter(width, height);
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

		public void Resize(int width, int height, bool scale)
		{
			LegacyRatio = LegacyRatio * width / Width; // adjust ratio
			Width = width;
			Height = height;
			if (chars == null)
				EnsureLoaded();

			for (int ch = 0; ch < chars.Length; ch++)
			{
				chars[ch].Resize(Width, Height, scale, IsCopy9(ch));
			}
		}

		public bool IsCopy9(int character)
		{
			return Width == 9 && character >= BitFont.StartCopy9 && character <= BitFont.EndCopy9;
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
				chars[i] = new FontCharacter(Width, Height);
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
				chars[i].Read(br, IsCopy9(i));
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
		int[] _data;
		int _width;
		int _height;

		void Set(int index, bool value)
		{
			if (value)
			{
				_data[index >> 5] |= 1 << index;
			}
			else
			{
				_data[index >> 5] &= ~(1 << index);
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
			return _data[index];
		}

		internal void Resize(int width, int height, bool scale, bool copy9)
		{
			var newdata = new int[(width * height + 31) / 32];
			for (int y = 0; y < height; y++)
			{
				var oldrow = (y * _height / height) * _width;
				var newrow = y * width;
				for (int x = 0; x < width; x++)
				{
					if (scale)
						Set(newdata, newrow + x, this[oldrow + x * _width / width]);
					else
					{
						if (x < _width)
							Set(newdata, newrow + x, this[oldrow + x]);
						else if (copy9 && x == width - 1)
							Set(newdata, newrow + x, this[oldrow + x - 1]);
					}
				}
			}
			_width = width;
			_height = height;
			_data = newdata;
		}

		internal FontCharacter(FontCharacter fc)
		{
			_data = (int[])fc._data.Clone();
			_width = fc._width;
			_height = fc._height;
		}

		internal FontCharacter(Size size, FontCharacter fc, Size oldSize)
		{
			_data = new int[(size.Width * size.Height + 31) / 32];
			_width = size.Width;
			_height = size.Height;
			for (int y = 0; y < size.Height; y++)
			{
				var row = y * size.Width;
				for (int x = 0; x < size.Width; x++)
				{
					int xold = x * oldSize.Width / size.Width;
					int yold = y * oldSize.Height / size.Height;
					Set(row + x, fc[yold, xold]);
				}
			}
		}

		public FontCharacter(int width, int height)
		{
			_data = new int[(width * height + 31) / 32];
			_width = width;
			_height = height;
		}

		public bool this [int x, int y]
		{
			get { return this[y * _width + x]; }
			set { Set(y * _width + x, value); }
		}

		public bool this [int index]
		{
			get { return (_data[index / 32] & 1 << index) != 0; }
			set { Set(index, value); }
		}

		byte[] GetByteRow(int index)
		{
			var ba = new BitArray(_width);
			var row = index * _width;
			for (int x = 0; x < _width; x++)
			{
				ba[x] = this[row + x];
			}
			Reverse(ba);
			var bytes = new byte[(ba.Length + 7) / 8];
			ba.CopyTo(bytes, 0);
			return bytes;
		}

		void Reverse(BitArray array)
		{
			int length = array.Length;
			int mid = (length / 2);

			for (int i = 0; i < mid; i++)
			{
				bool bit = array[i];
				array[i] = array[length - i - 1];
				array[length - i - 1] = bit;
			}
		}

		public bool Equals(FontCharacter character)
		{
			if (_height != character._height)
				return false;
			if (_width != character._width)
				return false;
			if (!_data.Equals(character._data))
				return false;
			return true;
		}

		public static byte ReverseBitsWith7Operations(byte b)
		{
			return (byte)(((b * 0x0802u & 0x22110u) | (b * 0x8020u & 0x88440u)) * 0x10101u >> 16);
		}

		public unsafe void Read(BinaryReader br, bool copy9)
		{
			if (_width == 8)
			{
				fixed (int* ptr = _data)
				{
					byte* bptr = (byte*)ptr;
					for (int y = 0; y < _height; y++)
					{
						*bptr = ReverseBitsWith7Operations(br.ReadByte());
						bptr++;
					}
				}
			}
			else
			{
				for (int y = 0; y < _height; y++)
				{
					var row = y * _width;
					byte currentRow = br.ReadByte();
					int currentBit = 128;
					for (int x = 0; x < _width; x++)
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
			for (int i = 0; i < _height; i++)
			{
				bw.Write(GetByteRow(i));
			}
		}
	}
}
