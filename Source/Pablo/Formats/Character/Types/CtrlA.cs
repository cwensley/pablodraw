using System;
using System.IO;
using Eto.Drawing;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Formats.Character.Types
{
	public class CtrlA : CharacterFormat
	{
		public CtrlA (DocumentInfo info)
			: base(info, "ctrla", "Ctrl-A", "msg", "asc")
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ascii;
			FillSauceSize(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != DefaultWidth || document.ICEColours || !document.IsUsingStandard8x16Font;
		}

		static readonly byte[] background = Encoding.ASCII.GetBytes ("04261537");
		static readonly byte[] foreground = Encoding.ASCII.GetBytes ("KBGCRMYWkbgcrmyw");
		
		public override bool CanSave {
			get {
				return true;
			}
		}
		
		#region Loading
		
		class Loader
		{
			Point p;
			Canvas canvas;
			Rectangle rClip;
			BinaryReader br;
			bool ended;
			Attribute attr;
			Stack<Attribute> attrStack = new Stack<Attribute> ();
			
			public CtrlA Format { get; set; }
		
			void IncrementY ()
			{
				p.Y++;
				if (p.Y > rClip.InnerBottom) {
					canvas.ShiftUp ();
					p.Y = rClip.InnerBottom;
				}
			}
		
			void IncrementX ()
			{
				p.X++;
				if (p.X > rClip.InnerRight) {
					IncrementY ();
					p.X = rClip.Left;
				}
			}
		
			void ReadCtrlA ()
			{
				var code = br.ReadByte ();
				int index;
			
				if ((index = Array.IndexOf (background, code)) >= 0) {
					attr.BackgroundOnly = index;
					return;
				}
				if ((index = Array.IndexOf (foreground, code)) >= 0) {
					attr.ForegroundOnly = index % 8;
					return;
				}
				switch (code) {
				case (byte)'A':
				case (byte)'a':
					canvas [p] = new CanvasElement (1, attr);
					IncrementX ();
					return;
				case (byte)'H':
				case (byte)'h':
					attr.Bold = true;
					return;
				case (byte)'I':
				case (byte)'i':
					attr.Blink = true;
					return;
				case (byte)'n':
				case (byte)'N':
					attr = CanvasElement.Default.Attribute;
					return;
				case (byte)'L':
				case (byte)'l':
				// clrscr
					canvas.Clear ();
					p = Point.Empty;
					return;
				case (byte)'>':
				// clreol
					canvas.Fill (new Rectangle (p, new Point (canvas.Width - 1, p.Y)), attr);
					return;
				case (byte)'<':
				// back one char (non destructive)
					if (p.X > 0)
						p.X --;
					return;
				case (byte)'[':
				// CR
					p.X = rClip.Left;
					return;
				case (byte)']':
				// LF
					p.Y++;
					if (p.Y > rClip.InnerBottom) {
						canvas.ShiftUp ();
						p.Y = rClip.InnerBottom;
					}
					return;
				case (byte)'+':
				// push attr
					attrStack.Push (attr);
					return;
				case (byte)'_':
					attr = CanvasElement.Default.Attribute;
					return;
				case (byte)'-':
				// pop attr (or default)
					if (attrStack.Count > 0)
						attr = attrStack.Pop ();
					else
						attr = CanvasElement.Default.Attribute;
					return;
				case (byte)'Z':
				case (byte)'z':
					ended = true;
					return;
				}
				if (code >= 128) {
					// move cursor!
					p.X += code - 127;
					if (p.X > rClip.InnerRight)
						p.X = rClip.InnerRight;
					return;
				}
			}

			public void Load (Stream stream, CharacterDocument doc, Handler handler)
			{
				Page page = doc.Pages [0];

				br = new BinaryReader (stream);
				canvas = page.Canvas;
				
				Format.ResizeCanvasWidth(stream, doc, canvas);
				
				rClip = new Rectangle (canvas.Size);

				p = rClip.Location;
				attr = CanvasElement.Default.Attribute;
				try {
					while (true) {
						var b = br.ReadByte ();
					
						if (b == 1) {
							ReadCtrlA ();
							if (ended)
								break;
						} else if (b == 13) {
							// do nothing?
							p.X = rClip.Left;
						} else if (b == 10) {
							// LF(+CR?)
							IncrementY ();
							p.X = rClip.Left;
						} else {
							canvas [p] = new CanvasElement (b, attr);
							IncrementX ();
						}
					
					}
				} catch (EndOfStreamException) {
				
				}
				
				Format.ResizeCanvasHeight(doc, canvas, p.Y + 1);
			}
		}
		
		public override void Load (Stream stream, CharacterDocument doc, CharacterHandler handler)
		{
			var loader = new Loader{ Format = this };
			loader.Load (stream, doc, handler);
		}
		
		#endregion
		
		#region Saving
		
		void WriteCtrlA (BinaryWriter bw, char val)
		{
			var encoding = Encoding.GetEncoding (437);
			bw.Write ((byte)1);
			bw.Write (encoding.GetBytes (val.ToString ()));
		}
		
		void WriteCtrlA (BinaryWriter bw, byte val)
		{
			bw.Write ((byte)1);
			bw.Write (val);
		}

		void WriteString (BinaryWriter bw, string str)
		{
			var encoding = Encoding.GetEncoding (437);
			byte[] bytes = encoding.GetBytes (str);
			bw.Write (bytes);
		}
		
		Attribute WriteAttribute (BinaryWriter bw, Attribute attr, Attribute next)
		{
			if (attr != next) {
				bool reset = (attr.Blink && !next.Blink);
				reset |= (attr.Bold && !next.Bold);
				if (reset) {
					WriteCtrlA (bw, 'N');
				}
				
				if ((reset && next.Blink) || (!attr.Blink && next.Blink)) {
					WriteCtrlA (bw, 'I');
				}
				if ((reset && next.Bold) || (!attr.Bold && next.Bold)) {
					WriteCtrlA (bw, 'H');
				}
				
				if ((reset && next.BackgroundOnly != 0) || (!reset && attr.BackgroundOnly != next.BackgroundOnly)) {
					WriteCtrlA (bw, background [next.BackgroundOnly]);
				}

				if ((reset && next.ForegroundOnly != 7) || (!reset && attr.ForegroundOnly != next.ForegroundOnly)) {
					WriteCtrlA (bw, foreground [next.ForegroundOnly]);
				}
			}
			return next;
		}
		
		public override void Save (Stream stream, CharacterDocument document)
		{
			var bw = new BinaryWriter (stream);
			
			var page = document.Pages [0];
			var canvas = page.Canvas;
			var endy = canvas.FindEndY (CanvasElement.Default);
			var attr = CanvasElement.Default.Attribute;
			var p = Point.Empty;
			WriteCtrlA (bw, 'N');
			byte runlength = 0;
			for (p.Y = 0; p.Y<=endy; p.Y++) {
				var endx = canvas.FindEndX (p.Y, 0, canvas.Width - 1, CanvasElement.Default);
				for (p.X=0; p.X <= endx; p.X++) {
					var ce = canvas [p];

					// optimize (black) spaces 
					if (ce.Character == 32 && ce.Attribute.BackgroundOnly == 0) {
						attr = WriteAttribute (bw, attr, ce.Attribute);
						runlength++;
						if (runlength == 128) {
							// runs can only be max 128 characters
							WriteCtrlA (bw, (byte)(runlength + 127));
							runlength = 0;
						}
						continue;
					}
					
					// write run
					if (runlength > 0) {
						if (runlength <= 2) {
							for (int i = 0; i < runlength; i++) {
								bw.Write ((byte)32);
							}
						} else 
							WriteCtrlA (bw, (byte)(runlength + 127));
						runlength = 0;
					}
					
					// write new attribute
					attr = WriteAttribute (bw, attr, ce.Attribute);
					
					// write character
					if (ce.Character == 1)
						WriteCtrlA (bw, 'A');
					else
						bw.Write ((byte)ce.Character);
				}
				if (endx < canvas.Width - 1)
					WriteString (bw, "\r\n");
			}
			WriteCtrlA (bw, 'Z'); // in case of SAUCE, end file here
		}
		
		#endregion
	}
}

