using Eto.Drawing;
using Eto.Forms;
using System;

namespace Pablo.Formats.Character.Tools
{
	public abstract class SizeTool : CharacterTool
	{
		public int Size { get; set; }

		protected SizeTool()
		{
			Size = 3;
		}

		Point? lastPoint;

		protected abstract void Draw(Point location, MouseEventArgs e);

		protected override Point GetLocation(Point location)
		{
			location = base.GetLocation(location);
			location.X -= Size / 2;
			location.Y -= Size / 2;
			return location;
		}

		void Do(MouseEventArgs e)
		{
			var location = GetLocation((Point)e.Location);
			if (lastPoint == null || lastPoint.Value != location)
			{
				Draw(location, e);
				lastPoint = location;
			}
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			
			lastPoint = null;
			Do(e);
			
		}

		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Do(e);
		}

		public override Control GeneratePad()
		{
			return new Controls.SizePad(this);
		}
	}
}

