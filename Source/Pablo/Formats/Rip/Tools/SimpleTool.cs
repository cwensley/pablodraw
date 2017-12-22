using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.BGI;
using System.Collections;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public abstract class SimpleTool<T> : SimpleTool
		where T: RipCommand
	{
		public new T Command { 
			get { return (T)base.Command; }
			set { base.Command = value; }
		}
		
		protected override void CreateCommand ()
		{
			base.Command = RipCommands.Create <T> (Document);
		}
	}

	public abstract class SimpleTool : RipTool
	{
		protected bool Applied { get; set; }
		
		public RipCommand Command { get; protected set; }
		
		protected abstract void CreateCommand ();
		
		protected virtual bool ShouldApplyDrawing {
			get { return Command != null; }
		}

		public override void Unselected ()
		{
			RemoveDrawing ();
			Command = null;
		}
		
		protected virtual void Apply (IList<Rectangle> updates = null)
		{
			Command.Apply (updates);
		}
		
		protected virtual void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			Apply (updates);
		}
		
		public override void ApplyDrawing (IList<Rectangle> updates = null)
		{
			if (ShouldApplyDrawing && !Applied) {
				PushStyles ();
				ApplyInvertedDrawing (updates);
				Applied = true;
			}
		}

		public override void RemoveDrawing (IList<Rectangle> updates = null)
		{
			if (ShouldApplyDrawing && Applied) {
				ApplyInvertedDrawing (updates);
				PopStyles ();
				Applied = false;
			}
		}
		
		protected virtual void FinishCommand (Key modifiers, IList<Rectangle> updates = null)
		{
			ApplyStyles ();
			Handler.AddCommand (Command, updates);
			Command = null;
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (Command != null) {
				switch (e.Buttons) {
				case MouseButtons.Alternate:
					RemoveDrawing ();
					Command = null;
					break;
				default:
					base.OnMouseUp (e);
					break;
				}
			}
			else
				base.OnMouseUp (e);
		}
	}
}

