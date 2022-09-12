using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace PabloDraw.Handlers.Pix
{
	public class FormatPix : Format
	{

		public static Format Create() { return new FormatPix(); }

		public override bool Use9pxFont
		{
			get
			{
				return false;
			}
		}

		protected override Document CreateDocument()
		{
			return new PixDocument();
		}



		public override void Load(Stream fs, Document document)
		{
			PixDocument doc = (PixDocument)document;
			BinaryReader br = new BinaryReader(fs);

			byte b;
			Point p = new Point(0,0);
			
			try
			{
				while (!fs.IsEOF())
				{
					b = br.ReadByte();
					switch ((char)b)
					{
						case (char)13:
							p.X = 0;
							break;
						case (char)10:
							p.Y++;
							break;
						case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G':
						case 'H': case 'I': case 'J': case 'K': case 'L': case 'M': case 'N':
						case 'O': case 'P': case 'Q': case 'R': case 'S': case 'T': case 'U':
						case 'V': case 'W': case 'X': case 'Y': case 'Z': case '1': case '2':
						case '3': case '4': case '5': case '6': case '7': case '8': case '9':
						case '0': case '-': case '$': case '!': case '&': case '#': case (char)39 : //single quote
						case '(': case ')': case (char)34 : case '/': case ':': case ';': case '?': // quote
						case ',': case '.': case ' ':
							if (p.X < doc.Size.Width) doc.Canvas.Rows[p.Y].AddChar(p.X, (char)b);
							p.X++;
							break;
					}
				}
			}
			catch (EndOfStreamException)
			{
			
			}

		}
	}
}
