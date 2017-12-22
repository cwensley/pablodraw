using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	public partial class Ansi : CharacterFormat
	{
		bool animateView;

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ansi;
			FillSauceSize(sauce, document);
			FillFlags(sauce, document);
		}

		public Ansi (DocumentInfo info) : base(info, "ansi", "Ansi", "ans", "diz", "mem", "cia", "drk", "ice")
		{
			LineWrap = true;
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != DefaultWidth || document.ICEColours || !document.IsUsingStandard8x16Font;
		}

		public override bool CanSave {
			get { return true; }
		}

		protected Ansi (DocumentInfo info, string id, string name, params string[] extensions)
		: base(info, id, name, extensions)
		{
			LineWrap = true;
		}

		public override bool DetectAnimation (Stream stream)
		{
			var sauce = Sauce.SauceInfo.GetSauce (stream);
			if (sauce != null) {
				var charType = sauce.TypeInfo as Sauce.Types.Character.DataTypeInfo;
				if (charType != null)
					return charType.Type == Sauce.Types.Character.CharacterFileType.Ansimation;
			}
			bool isAnsimated = false;

			long pos = stream.Position;
			try {
				// should read from sauce if available, if not, then do the following:
				var bs = new BufferedStream (stream, 1024);
				var br = new BinaryReader (bs);
				int read = 0;
				while (read < 2048 && true) {
					byte curByte = br.ReadByte ();
					read++;
					if (curByte == 27) {
						curByte = br.ReadByte ();
						if (curByte == 91) {
							char curChar = (char)br.ReadByte ();
							while (!char.IsLetter(curChar)) {
								curChar = (char)br.ReadByte ();
							}
							if ("Hf".IndexOf (curChar) != -1) {
								isAnsimated = true;
								break; // finished
							}
						}
					}
				}
			} catch (EndOfStreamException) {
				// reached end of file, so we're okay
			} finally {
				stream.Position = pos;
			}
			return isAnsimated;
		}

		
		class SaveArgs
		{
			Stream stream;
			int x;
			int spacecount;
			int linelength;
			int xposition;
			CanvasElement element;
			Attribute lastAttr;
			Attribute lastRgbAttr;
			Rectangle rect;
			Canvas canvas;
			Palette palette;
			int[] closestPalette;
			Palette standardPalette = Palette.GetDosPalette ();
			static readonly string[] ansifore = { "30", "34", "32", "36", "31", "35", "33", "37" };
			static readonly string[] ansiback = { "40", "44", "42", "46", "41", "45", "43", "47" };
			
			public bool iCEColours { get; set; }

			public SaveArgs (Canvas canvas, Palette palette, Stream stream)
			{
				this.canvas = canvas;
				this.palette = palette;
				this.stream = stream;
				x = 0;
				spacecount = 0;
				linelength = -1;
				xposition = 0;
				element = new CanvasElement ();
				element.Attribute.Set (7, 0);
				lastAttr = new Attribute (0, 0);
			}
			
			bool GetReset (StringBuilder sb)
			{
				if (sb.Length > 0)
					sb.Append (";");
				sb.Append ("0");
				lastAttr.Set (7, 0);
				lastRgbAttr = new Attribute(-1, -1);
				return true;
			}

			void GetFlagChanges (StringBuilder sb, bool force, Attribute attr)
			{
				bool reset = false;
				if (force)
					reset = GetReset (sb);
				
				if (!lastAttr.Blink && attr.Blink && !(lastAttr.Bold && !attr.Bold)) {
					if (sb.Length > 0)
						sb.Append (";");
					sb.Append ("5");
					lastAttr.Blink = true;
					lastRgbAttr.Background = -1; 

				} else if (lastAttr.Blink && !attr.Blink) {
					if (!reset)
						reset = GetReset (sb);
				}

				if (!lastAttr.Bold && attr.Bold) {
					if (sb.Length > 0)	
						sb.Append (";");
					sb.Append ("1");
					lastRgbAttr.Foreground = -1; 
					lastAttr.Bold = true;
				} else if (lastAttr.Bold && !attr.Bold) {
					if (!reset)
						reset = GetReset (sb);
					
					if (attr.Blink) {
						if (sb.Length > 0)
							sb.Append (";");
						sb.Append ("5");
						lastRgbAttr.Background = -1; 
						lastAttr.Blink = true;
					}
				}
			}

			void GetColourChanges (StringBuilder sb, bool force, Attribute attr)
			{
				if (force || lastAttr.Background != attr.Background) {
					if (sb.Length > 0)
						sb.Append (";");
					sb.Append (ansiback [attr.BackgroundOnly]);
					lastAttr.Background = attr.Background;
					lastRgbAttr.Background = -1;

				}
				if (force || lastAttr.Foreground != attr.Foreground) {
					if (sb.Length > 0)
						sb.Append (";");
					sb.Append (ansifore [attr.ForegroundOnly]);
					lastAttr.Foreground = attr.Foreground;
					lastRgbAttr.Foreground = -1;
				}
			}

			void GetRgbColourChanges (StringBuilder sb, bool force, Attribute attr, Attribute standardAttr)
			{
				if (force || lastRgbAttr.Background != attr.Background) {
					var col = palette [attr.Background];
					var standardCol = standardPalette [standardAttr.Background];
					if (col != standardCol)
						sb.AppendFormat ("\x1b[0;{0};{1};{2}t", (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255));
					lastRgbAttr.Background = attr.Background;
				}
				if (force || lastRgbAttr.Foreground != attr.Foreground) {
					var col = palette [attr.Foreground];
					var standardCol = standardPalette [standardAttr.Foreground];
					if (col != standardCol)
						sb.AppendFormat ("\x1b[1;{0};{1};{2}t", (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255));
					lastRgbAttr.Foreground = attr.Foreground;
				}
			}
			
			public void WriteColour (bool force = false)
			{
				WriteColour (element.Attribute, force);
			}

			public void WriteColour (Attribute attr, bool force = false)
			{
				attr.Background = closestPalette[attr.Background];
				attr.Foreground = closestPalette[attr.Foreground];
				
				// write 16-color attribute
				var sb = new StringBuilder ();
				GetFlagChanges (sb, force, attr);
				GetColourChanges (sb, force, attr);
				if (sb.Length > 0) {
					sb.Insert (0, "\x1b[");
					sb.Append ("m");
					WriteToAnsi (sb.ToString (), false);
				}
				
				// write RGB attribute
				var sbRGB = new StringBuilder ();
				GetRgbColourChanges (sbRGB, force, element.Attribute, attr);
				WriteToAnsi (sbRGB.ToString (), false);
			}

			void TrimLine (Attribute attribute)
			{
				if (lastAttr.Background > 0) {
					var attr = lastAttr;
					attr.Background = 0;
					WriteColour (attr);
				}
				
				WriteToAnsi ("\r\n");
				WriteToAnsi ("\x1b[A");
				if (x > rect.Left)
					WriteToAnsi (string.Format ("\x1b[{0}C", x));

				if (lastAttr.Background > 0)
					WriteColour (lastAttr);
			}
			
			public void WriteToAnsi (string str, bool compress = false)
			{
				if (string.IsNullOrEmpty (str))
					return;
				byte[] bytes = BitFont.StandardEncoding.GetBytes (str);
				WriteToAnsi (bytes, compress);
			}

			public void WriteToAnsi (byte[] bytes, bool compress)
			{
				var startAttribute = lastAttr;
				
				if (bytes [0] == 32 && compress) {
					spacecount++;
				} else {
					if (spacecount > 0) {
						string strTemp = string.Empty;
						if (spacecount > 4) {
							strTemp = string.Format ("\x1b[{0}C", spacecount);
						} else if (spacecount > 0) {
							for (int e = 0; e<spacecount; e++)
								strTemp += " ";
						}
						spacecount = 0;
						WriteToAnsi (strTemp, false);
					}


					if (bytes [0] == 10 && bytes [1] == 13) { // "\r\n"
						xposition = 0;
						spacecount = 0;
					} else {
						if (linelength != -1 && ((xposition + bytes.Length) >= linelength)) {
							TrimLine (startAttribute);
						}
						xposition += bytes.Length;
					}
			        
					if (bytes.Length == 0)
						stream.WriteByte (0);
					else {
						stream.Write (bytes, 0, bytes.Length);
					}
				}
			}

			public void Save ()
			{
				closestPalette = new int[palette.Count];
				for (int i = 0; i < palette.Count; i++)
				{
					closestPalette[i] = standardPalette.FindClosest(palette[i]);
				}
				//WriteToAnsi ("\x1b[?7h", false);
				element = canvas [0, 0];
				WriteColour (true); // write colour of first character
				
				/*
				if (iCEColours)
					WriteToAnsi("\x1b[?33h");
				else
					WriteToAnsi("\x1b[?33l");
				 */
				
				rect = new Rectangle (0, 0, canvas.Width, canvas.FindEndY (CanvasElement.Default) + 1);
				for (int y=rect.Top; y<=rect.InnerBottom; y++) {
					int iXEnd = canvas.FindEndX (y, rect.Left, rect.InnerRight, CanvasElement.Default);

					for (int x=rect.Left; x<=iXEnd; x++) {
						element = canvas [x, y];
			            
						WriteColour ();
						//			if (currchar == 0) currchar = 32;

						WriteToAnsi (new byte[] { element.Character }, (element.Background == 0));
					}

					if (iXEnd < rect.InnerRight) {
						if (lastAttr.Background > 0) {
							WriteToAnsi ("\x1b[0m", false);
							lastAttr.Set (7, 0);
							lastRgbAttr = new Attribute(-1, -1);
						}
						WriteToAnsi ("\r\n", false);
					}
				}
			
			}
		}
		
		public override void Save (Stream stream, CharacterDocument document)
		{
			var doc = document;
			if (doc != null) {
				var page = doc.Pages [0];
				var args = new SaveArgs (page.Canvas, page.Palette, stream);
				args.iCEColours = document.ICEColours;
				args.Save ();
			}
		}

		public virtual void Save (Stream stream, Canvas canvas, Palette palette)
		{
			var args = new SaveArgs (canvas, palette, stream);
			args.Save ();
		}
		
		public Stream Save (Canvas canvas, Palette palette)
		{
			var stream = new MemoryStream ();
			Save (stream, canvas, palette);
			stream.Seek (0, SeekOrigin.Begin);
			return stream;
		}
	}
}

