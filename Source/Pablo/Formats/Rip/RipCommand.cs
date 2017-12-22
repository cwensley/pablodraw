using System;
using Pablo.BGI;
using System.IO;
using System.Text;
using Eto.Forms;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip
{
	public abstract class RipOptionalCommand : RipCommand
	{
		public virtual bool ShouldApply (BGICanvas bgi)
		{
			var contains = this.Document.OptionalApplied.Contains (OpCode);
			if (InternalShouldApply (bgi) || !contains) {
				return true;
			}
			return false;
		}
		
		public abstract bool InternalShouldApply (BGICanvas bgi);
		
		public abstract void Set (BGICanvas bgi);
		
		public abstract void Set (RipHandler handler, bool forDrawing);
	}
	
	public abstract class RipCommand
	{
		public RipCommandType CommandType { get; set; }
		
		public virtual bool Store { get { return true; } }
		
		public virtual bool UndoPoint { get { return true; } }
		
		public RipDocument Document { get; set; }
		
		public BGICanvas BGI { get { return Document.BGI; } }
		
		public string OpCode {
			get { return CommandType.OpCode; }
		}
		
		public virtual void Read (BinaryReader reader)
		{
		}
		
		public virtual void Write (RipWriter writer)
		{
			writer.WriteNewCommand (OpCode);
		}
		
		public abstract void Apply (IList<Rectangle> updates = null);
		
	}
}

