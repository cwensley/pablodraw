using System;
using System.IO;
using Pablo.BGI;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class LineStyle : RipOptionalCommand
	{
		public class Type : RipCommandType<LineStyle>
		{
			public override string OpCode { get { return "="; } }
		}

		public override bool UndoPoint { get { return false; } }
		
		public BGICanvas.LineStyle Style { get; set; }

		public uint Pattern { get; set; }

		public int Thickness { get; set; }
		
		public LineStyle()
		{
			Thickness = 1;
		}

		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Style = (BGICanvas.LineStyle)reader.ReadRipWord ();
			Pattern = reader.ReadRipInt ();
			Thickness = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord ((int)Style);
			writer.WriteInt (Pattern);
			writer.WriteWord (Thickness);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetLineStyle (Style, Pattern, Thickness);
		}
		
		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return Style != bgi.GetLineStyle ()
				|| (Style == BGICanvas.LineStyle.User && Pattern != bgi.GetLinePattern (BGICanvas.LineStyle.User))
				|| Thickness != bgi.GetLineThickness ();
		}

		public override void Set (BGICanvas bgi)
		{
			this.Style = bgi.GetLineStyle ();
			this.Pattern = bgi.GetLinePattern (BGICanvas.LineStyle.User);
			this.Thickness = bgi.GetLineThickness ();
		}

		public override void Set (RipHandler handler, bool forDrawing)
		{
			this.Style = handler.LineStyle;
			if (forDrawing) 
				this.Pattern = handler.LinePattern == 0 ? 0xAAAA : handler.LinePattern;
			else
				this.Pattern = handler.LinePattern;
			this.Thickness = handler.LineThickness;
		}
	}
}

