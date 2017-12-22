using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Controls
{
	public class CustomButton : Drawable
	{
		bool pressed;
		bool hover;
		bool mouseDown;
		public static Color DisabledColor = Color.FromGrayscale(0.4f, 0.3f);
		public static Color EnabledColor = Colors.Black;

		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if (base.Enabled != value)
				{
					base.Enabled = value;
					if (Loaded)
						Invalidate();
				}
			}
		}

		public bool Pressed
		{
			get { return pressed; }
			set
			{ 
				if (pressed != value)
				{
					pressed = value;
					mouseDown = false;
					if (Loaded)
						Invalidate();
				}
			}
		}

		public Color DrawColor
		{
			get { return Enabled ? EnabledColor : DisabledColor; }
		}

		public bool Toggle { get; set; }

		public bool Persistent { get; set; }

		public event EventHandler<EventArgs> Click;

		protected virtual void OnClick(EventArgs e)
		{
			if (Click != null)
				Click(this, e);
		}

		public CustomButton()
		{
			Enabled = true;
		}

		public override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (Loaded)
				Invalidate();
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (Enabled)
			{
				mouseDown = true;
				Invalidate();
			}
		}

		public override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			hover = true;
			Invalidate();
		}

		public override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			hover = false;
			Invalidate();
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			var rect = new Rectangle(this.Size);
			if (mouseDown && rect.Contains((Point)e.Location))
			{
				if (Toggle)
					pressed = !pressed;
				else if (Persistent)
					pressed = true;
				else
					pressed = false;
				mouseDown = false;

				this.Invalidate();
				if (Enabled)
					OnClick(EventArgs.Empty);
			}
			else
			{
				mouseDown = false;
				this.Invalidate();
			}
		}

		public override void OnPaint(PaintEventArgs pe)
		{
			var rect = new Rectangle(this.Size);
			var col = Color.FromGrayscale(hover && Enabled ? 0.95f : 0.8f);
			if (Enabled && (pressed || mouseDown))
			{
				pe.Graphics.FillRectangle(col, rect);
				pe.Graphics.DrawInsetRectangle(Colors.Gray, Colors.White, rect);
			}
			else if (hover && Enabled)
			{
				pe.Graphics.FillRectangle(col, rect);
				pe.Graphics.DrawInsetRectangle(Colors.White, Colors.Gray, rect);
			}
			
			base.OnPaint(pe);
		}
	}
}

