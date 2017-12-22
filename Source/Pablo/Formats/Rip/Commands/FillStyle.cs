using System;
using System.IO;
using Pablo.BGI;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class FillStyle : RipOptionalCommand
	{
		public class Type : RipCommandType<FillStyle>
		{
			public override string OpCode { get { return "S"; } }
		}

		public override bool UndoPoint { get { return false; } }

		public BGICanvas.FillStyle Style { get; set; }

		public int Color { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Style = (BGICanvas.FillStyle)reader.ReadRipWord ();
			Color = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord ((int)Style);
			writer.WriteWord (Color);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetFillStyle (Style, (byte)Color);
		}
		
		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return this.Style != BGICanvas.FillStyle.User
				&& (
					bgi.GetFillColor() != this.Color
					|| bgi.GetFillStyle() != this.Style
				);
		}

		public override void Set (BGICanvas bgi)
		{
			this.Style = bgi.GetFillStyle ();
			this.Color = bgi.GetFillColor ();
		}

		public override void Set (RipHandler handler, bool forDrawing)
		{
			this.Color = handler.Background;
			this.Style = handler.FillStyle;
		}
	}
}

