using System;
using System.IO;
using Pablo.BGI;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class WriteMode : RipOptionalCommand
	{
		public class Type : RipCommandType<WriteMode>
		{
			public override string OpCode { get { return "W"; } }
		}

		public override bool UndoPoint { get { return false; } }

		public BGICanvas.WriteMode Mode { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			var i = reader.ReadRipWord ();
			Mode = i == 1 ? BGICanvas.WriteMode.Xor : BGICanvas.WriteMode.Copy;
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			
			writer.WriteWord (Mode == BGICanvas.WriteMode.Xor ? 1 : 0);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetWriteMode (Mode);
		}
		
		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return bgi.GetWriteMode () != Mode;
		}

		public override void Set (BGICanvas bgi)
		{
			Mode = bgi.GetWriteMode ();
		}

		public override void Set (RipHandler handler, bool forDrawing)
		{
			if (forDrawing) Mode = BGICanvas.WriteMode.Xor;
			else Mode = BGICanvas.WriteMode.Copy;
		}
	}
}

