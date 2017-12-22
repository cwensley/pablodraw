using System;
using System.IO;
using System.Text;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.BGI
{
	/// <summary>
	/// Summary description for BGIFont.
	/// </summary>
	public class BGIFont : IBGIFont
	{
		// font header
		UInt16 headerSize;
		string fontName;
		UInt16 fontSize;
		byte fontMajor;
		byte fontMinor;
		byte minMajor;
		byte minMinor;
		// header
		byte sig;			/* SIGNATURE byte			*/
		Int16 characterCount;		/* number of characters in file 	*/
		byte first;		/* first character in file		*/
		Int16 characterOffset;		/* offset to char definitions		*/
		byte scanFlag;		/* True if set is scanable		*/
		sbyte org_to_cap;		/* Height from origin to top of capitol */
		sbyte org_to_base;		/* Height from origin to baseline	*/
		sbyte org_to_dec;		/* Height from origin to bot of decender*/
		string shortFontName;		/* Four character name of font		*/

		int capitalHeight;
		int baseHeight;
		int descenderHeight;
		int lowerCaseHeight;


		BGICharacter[] characters;

		bool loaded;

		public Int16 CharacterOffset
		{
			get { return characterOffset; }
		}
		
		public byte ScanFlag
		{
			get { return scanFlag; }
		}
		
		public string ShortFontName
		{
			get { return shortFontName; }
		}

		public string FontName
		{
			get { return fontName; }
		}
		
		public int Height
		{
			get { return Math.Abs(org_to_cap) + Math.Abs(org_to_dec); }
		}

		public int CapitalHeight
		{
			get { return capitalHeight; }
		}
		public int BaseHeight
		{
			get { return baseHeight; }
		}

		public int DescenderHeight
		{
			get { return descenderHeight; }
		}
		public int LowerCaseHeight
		{
			get { return lowerCaseHeight; }
		}

		
		public int OriginToCapital
		{
			get { return org_to_cap; }
		}
		public int OriginToBaseline
		{
			get { return org_to_base; }
		}

		public int OriginToDescender
		{
			get { return org_to_dec; }
		}
		
		public UInt16 FontSize
		{
			get { return fontSize; }
		}
		public byte FontMajor
		{
			get { return fontMajor; }
		}
		public byte FontMinor
		{
			get { return fontMinor; }
		}
		public byte MinMajor
		{
			get { return minMajor; }
		}
		public byte MinMinor
		{
			get { return minMinor; }
		}
		
		public byte Sig
		{
			get { return sig; }
		}

		
		public BGIFont()
		{
			loaded = false;
		}

		public bool IsLoaded
		{
			get { return loaded; }
		}

		public BGICharacter this[int index]
		{
			get { return characters[index]; }
			set { characters[index] = value; }
		}


		public void Load(Stream stream)
		{
			BinaryReader br = new BinaryReader(stream);
			byte b;
			do
			{
				b = br.ReadByte();
			} while (b != 0x1a);
			
			// fheader
			headerSize = br.ReadUInt16();
			byte[] name = br.ReadBytes(4);
			fontName = Encoding.ASCII.GetString(name);
			fontSize = br.ReadUInt16();
			fontMajor = br.ReadByte();
			fontMinor = br.ReadByte();
			minMajor = br.ReadByte();
			minMinor = br.ReadByte();

			stream.Position = headerSize;

			// header
			sig = br.ReadByte();
			characterCount = br.ReadInt16();
			br.ReadByte(); // unused byte
			first = br.ReadByte();
			characterOffset = br.ReadInt16();
			scanFlag = br.ReadByte();
			org_to_cap = br.ReadSByte();
			org_to_base = br.ReadSByte();
			org_to_dec = br.ReadSByte();
			byte[] shortName = br.ReadBytes(4);
			shortFontName = Encoding.ASCII.GetString(shortName);
			br.ReadByte(); // unused byte

			// read offset table
			Int16[] fontOffsets = new Int16[256];
			for (int i=first; i<first+characterCount; i++)
			{
				fontOffsets[i] = br.ReadInt16();
			}

			// read character width table
			byte[] charWidths = new byte[256];
			for (int i=first; i<first+characterCount; i++)
			{
				charWidths[i] = br.ReadByte();
			}

			characters = new BGICharacter[256];

			long start = stream.Position;

			for (int i=first; i<first+characterCount; i++)
			{
				stream.Seek(start + fontOffsets[i], SeekOrigin.Begin);
				characters[i] = new BGICharacter(this, stream, charWidths[i]);
			}

			capitalHeight = 40;
			baseHeight = 0;
			descenderHeight = -7;
			BGICharacter bc;
			bc = characters[(int)'E'];
			if (bc == null) bc = characters[(int)'M'];
			if (bc != null)
			{
				bool isFirst = true;
				int min = 0;
				int max = 0;
				foreach (BGICharacter.Stroke s in bc.Strokes)
				{
					if (isFirst || max < s.y) max = s.y;
					if (isFirst || min > s.y) min = s.y;
					isFirst = false;
				}
				capitalHeight = Math.Abs(max);
				baseHeight = Math.Abs(min);
			}
			lowerCaseHeight = capitalHeight/2;

			bc = characters[(int)'q'];
			if (bc != null)
			{
				bool isFirst = true;
				int min = 0;
				int max = 0;
				foreach (BGICharacter.Stroke s in bc.Strokes)
				{
					if (isFirst || max < s.y) max = s.y;
					if (isFirst || min > s.y) min = s.y;
					isFirst = false;
				}
				descenderHeight = Math.Abs(min);
			}

			bc = characters[(int)'x'];
			if (bc != null)
			{
				bool isFirst = true;
				int min = 0;
				int max = 0;
				foreach (BGICharacter.Stroke s in bc.Strokes)
				{
					if (isFirst || max < s.y) max = s.y;
					if (isFirst || min > s.y) min = s.y;
					isFirst = false;
				}
				lowerCaseHeight = Math.Abs(max);
			}
			
			//Console.WriteLine("Font {0}", this);
			loaded = true;
		}
		#region IBGIFont Members

		public float DrawCharacter(BGICanvas bgi, float x, float y, BGICanvas.Direction dir, int size, byte character, IList<Rectangle> updates)
		{
			BGICharacter ch = this[character];
			if (ch != null)
			{
				var drawUpdates = updates ?? new List<Rectangle>();
				ch.Draw(bgi, x, y, dir, size, updates);
				if (updates == null) bgi.UpdateRegion (drawUpdates);
				return ch.GetWidth(size);
			}
			return 0;
		}
		
		public Size GetRealTextSize (string str, BGICanvas.Direction dir, int size)
		{
			float width = 0;
			foreach (byte c in BGICanvas.Encoding.GetBytes(str))
			{
				BGICharacter ch = this[c];
				if (ch != null) width += ch.GetWidth (size);
			}
			if (dir == BGICanvas.Direction.Horizontal) return new Size(BGICanvas.Round (width ), (int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]));
			else return new Size((int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]), BGICanvas.Round (width));
		}

		public Size GetTextSize(string str, BGICanvas.Direction dir, int size)
		{
			float width = 0;
			foreach (byte c in BGICanvas.Encoding.GetBytes(str))
			{
				BGICharacter ch = this[c];
				if (ch != null) width += ch.Width;
			}
			if (dir == BGICanvas.Direction.Horizontal) return new Size(BGICanvas.Trunc (width * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]), (int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]));
			else return new Size((int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]), BGICanvas.Trunc (width * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]));
		}
		
		public Size GetMaxCharacterSize (BGICanvas.Direction dir, int size)
		{
			float width = 0;
			foreach (var ch in characters) {
				if (ch != null)
					width = Math.Max (width, ch.GetWidth(size));
			}
			if (dir == BGICanvas.Direction.Horizontal) return new Size(BGICanvas.Round (width), (int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]));
			else return new Size((int)((Height + Math.Abs(OriginToDescender) + 1) * BGICharacter.scaleup[size] / BGICharacter.scaledown[size]), BGICanvas.Round (width));
		}

		#endregion
		
		public override string ToString ()
		{
			return string.Format ("[BGIFont: CharacterOffset={0}, ScanFlag={1}, FontName={3}, Height={4}, CapitalHeight={5}, BaseHeight={6}, DescenderHeight={7}, LowerCaseHeight={8}, OriginToCapital={9}, OriginToBaseline={10}, OriginToDescender={11}, FontSize={12}, FontMajor={13}, FontMinor={14}, MinMajor={15}, MinMinor={16}, Sig={17}]", CharacterOffset, ScanFlag, ShortFontName, FontName, Height, CapitalHeight, BaseHeight, DescenderHeight, LowerCaseHeight, OriginToCapital, OriginToBaseline, OriginToDescender, FontSize, FontMajor, FontMinor, MinMajor, MinMinor, Sig);
		}
	}
}
