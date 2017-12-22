using System;
using System.Collections;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.BGI
{
	/// <summary>
	/// Summary description for BGICharacter.
	/// </summary>
	public class BGICharacter
	{
		internal enum StrokeType
		{
			End,
			MoveTo,
			LineTo
		}

		internal struct Stroke
		{
			public StrokeType type;
			public int x;
			public int y;
		}

		Stroke[] strokes;
		int width;
		BGIFont font;
		public static int[] scaleup = new int[] {    1,  6,  2, 3,  1,  4,  5,  2,  5,  3,  4 };
		public static int[] scaledown = new int[] {  1, 10,  3, 4,  1,  3,  3,  1,  2,  1,  1 };
		
		public int Width
		{
			get { return width; }
		}

		public float GetWidth (int scaleFactor)
		{
			return width * scaleup [scaleFactor] / scaledown [scaleFactor];
		}

		internal Stroke[] Strokes {
			get { return strokes; }
		}

		public void Draw (BGICanvas bgi, float x, float y, BGICanvas.Direction dir, int size, IList<Rectangle> updates)
		{
			var drawUpdates = updates ?? new List<Rectangle> ();
			
			var height = font.Height * scaleup [size] / scaledown [size];
			if (dir == BGICanvas.Direction.Horizontal) {
				foreach (var stroke in strokes) {
					int curx = (int)x + (stroke.x * scaleup [size] / scaledown [size]);
					int cury = (int)y + height - (stroke.y * scaleup [size] / scaledown [size]);

					if (stroke.type == StrokeType.MoveTo)
						bgi.MoveTo (curx, cury);
					else if (stroke.type == StrokeType.LineTo)
						bgi.LineTo (curx, cury, drawUpdates);
				}
			} else {
				foreach (var stroke in strokes) {
					int curx = (int)x + height - (stroke.y * scaleup [size] / scaledown [size]);
					int cury = (int)y - (stroke.x * scaleup [size] / scaledown [size]);
					
					if (stroke.type == StrokeType.MoveTo)
						bgi.MoveTo (curx, cury);
					else if (stroke.type == StrokeType.LineTo)
						bgi.LineTo (curx, cury, drawUpdates);
				}
			}
			if (updates == null)
				bgi.UpdateRegion (drawUpdates);
		}

		private Stroke ReadStroke (BinaryReader br)
		{
			byte byte1 = br.ReadByte ();
			byte byte2 = br.ReadByte ();
			bool flag1 = ((byte1 & 0x80) != 0);
			bool flag2 = ((byte2 & 0x80) != 0);
			var stroke = new Stroke ();
			if ((byte1 & 0x40) != 0)
				stroke.x = -(~byte1 & 0x3F) - 1;
			else
				stroke.x = (byte1 & 0x3F);
			if ((byte2 & 0x40) != 0)
				stroke.y = -(~byte2 & 0x3F) - 1;
			else
				stroke.y = (byte2 & 0x3F);
			if (flag1 && flag2)
				stroke.type = StrokeType.LineTo;
			else if (flag1 && !flag2)
				stroke.type = StrokeType.MoveTo;
			else
				stroke.type = StrokeType.End;
			return stroke;
		}

		public BGICharacter (BGIFont font, Stream stream, int width)
		{
			this.font = font;
			var br = new BinaryReader (stream);
			var al = new List<Stroke> ();
			this.width = width;

			var stroke = new Stroke ();
			do {
				stroke = ReadStroke (br);
				if (stroke.type == StrokeType.End)
					break;
				al.Add (stroke);
			} while (true);

			strokes = al.ToArray ();
			
			/*
			int firstx = 0;
			bool first = true;
			foreach (Stroke stroke in strokes)
			{
				if (stroke.y >= 0 && (stroke.x < firstx || first))
				{
					firstx = stroke.x;
					first = false;
				}
			}

			this.width -= firstx;
			for (int i=0; i<strokes.Length; i++)
			{
				strokes[i].x -= firstx;
			}*/
		}
	}
}
