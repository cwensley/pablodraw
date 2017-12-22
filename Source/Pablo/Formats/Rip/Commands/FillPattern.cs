using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Pablo.BGI;

namespace Pablo.Formats.Rip.Commands
{
	public class FillPattern : RipOptionalCommand
	{
		public class Type : RipCommandType<FillPattern>
		{
			public override string OpCode { get { return "s"; } }
		}

		public override bool UndoPoint { get { return false; } }
		
		bool isUserStyle;
		
		public byte[] Pattern { get; set; }

		public int Color { get; set; }
		
		public FillPattern()
		{
			Pattern = new byte[8];
		}
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			for (int i=0; i<8; i++) {
				Pattern [i] = (byte)reader.ReadRipWord ();
			}
			Color = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			for (int i=0; i<8; i++) {
				writer.WriteWord (Pattern [i]);
			}
			writer.WriteWord (Color);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			if (Pattern != null)
				BGI.SetFillPattern (Pattern, (byte)Color);
		}
		
		public override bool ShouldApply (BGICanvas bgi)
		{
			return isUserStyle && base.ShouldApply (bgi);
		}
		
		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return 
				isUserStyle
				&& (
					bgi.GetFillStyle () != BGICanvas.FillStyle.User
					|| bgi.GetFillColor () != Color
					|| !bgi.GetFillPattern (BGICanvas.FillStyle.User).SequenceEqual(Pattern)
				);
		}

		public override void Set (Pablo.BGI.BGICanvas bgi)
		{
			isUserStyle = bgi.GetFillStyle() == BGICanvas.FillStyle.User;
			Pattern = bgi.GetFillPattern (BGICanvas.FillStyle.User);
			Color = bgi.GetFillColor ();
		}

		public override void Set (RipHandler handler, bool forDrawing)
		{
			isUserStyle = handler.FillStyle == BGICanvas.FillStyle.User;
			Pattern = handler.FillPattern;
			Color = handler.Background;
		}
	}
}

