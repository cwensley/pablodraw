using System;
using System.IO;
using Pablo.BGI;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class FontStyle : RipOptionalCommand
	{
		public class Type : RipCommandType<FontStyle>
		{
			public override string OpCode { get { return "Y"; } }
		}

		public override bool UndoPoint { get { return false; } }

		public BGICanvas.FontType Font { get; set; }

		public BGICanvas.Direction Direction { get; set; }

		public int CharacterSize { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Font = (BGICanvas.FontType)reader.ReadRipWord ();
			Direction = (BGICanvas.Direction)reader.ReadRipWord ();
			CharacterSize = reader.ReadRipWord ();
			reader.ReadRipWord (); // reserved
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord ((int)Font);
			writer.WriteWord ((int)Direction);
			writer.WriteWord (CharacterSize);
			writer.WriteWord (0); // reserved
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetTextStyle (Font, Direction, CharacterSize);
		}
		
		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return bgi.GetFontType () != this.Font
				|| bgi.GetTextDirection () != this.Direction
				|| bgi.GetFontSize () != this.CharacterSize;
		}

		public override void Set (BGICanvas bgi)
		{
			this.Font = bgi.GetFontType ();
			this.Direction = bgi.GetTextDirection ();
			this.CharacterSize = bgi.GetFontSize ();
		}

		public override void Set (RipHandler handler, bool forDrawing)
		{
			this.Font = handler.FontType;
			this.Direction = handler.TextDirection;
			this.CharacterSize = handler.FontSize;
		}
	}
}

