using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.BGI;
using System.Collections;
using System.Collections.Generic;

namespace Pablo.Formats.Character.Tools
{
	public abstract class SimpleTool : CharacterTool, IGenerateRegion
	{
		public virtual void Cancel()
		{
		}

		protected virtual void Finish()
		{
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			switch (e.Buttons)
			{
				case MouseButtons.Alternate:
					Cancel();
					break;
				default:
					base.OnMouseUp(e);
					break;
			}
		}

		public override IGenerateRegion GetGenerator()
		{
			return this;
		}

		public virtual CanvasElement? GetElement(Point point, Canvas canvas)
		{
			return canvas[point];
		}

		public virtual void TranslateColour(Point point, ref CanvasElement ce, ref int foreColour, ref int backColour)
		{
		}

	}
}

