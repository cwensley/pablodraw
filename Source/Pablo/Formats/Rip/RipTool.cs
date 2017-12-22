using Eto.Forms;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;

namespace Pablo.Formats.Rip
{
	public abstract class RipTool
	{
		public abstract Eto.Drawing.Image Image { get; }

		public RipHandler Handler { get; set; }

		public virtual Keys Accelerator { get { return Keys.None; } }

		public virtual bool AllowToolShortcuts { get { return true; } }

		public abstract string Description { get; }

		public object Tag { get; set; }

		public RipDocument Document
		{
			get { return Handler.RipDocument; }
		}

		public BGI.BGICanvas BGI
		{
			get { return Handler.BGI; }
		}

		protected virtual bool InvertDrawing
		{
			get { return true; }
		}

		public virtual IEnumerable<RipOptionalCommand> Styles
		{
			get
			{
				if (InvertDrawing)
					yield return Document.Create<Commands.WriteMode>();
			}
		}

		protected virtual void ApplyStyles()
		{
			foreach (var style in Styles)
			{
				Handler.ApplyIfNeeded(style);
			}
		}

		IList<RipOptionalCommand> savedStyles;

		protected void PushStyles()
		{
			savedStyles = Styles.ToList();

			foreach (var style in savedStyles)
				style.Set(BGI);

			foreach (var style in Styles)
			{
				style.Set(Handler, true);
				if (style.ShouldApply(BGI))
					style.Apply();
			}
		}

		protected void PopStyles()
		{
			if (savedStyles != null)
			{
				foreach (var style in savedStyles)
				{
					if (style.ShouldApply(BGI))
						style.Apply();
				}

				savedStyles = null;
			}
		}

		public virtual void OnMouseDown(MouseEventArgs e)
		{
		}

		public virtual void OnMouseUp(MouseEventArgs e)
		{
		}

		public virtual void OnMouseMove(MouseEventArgs e)
		{
		}

		public virtual void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Escape)
			{
				Unselected();
				e.Handled = true;
			}
		}

		public abstract void ApplyDrawing(IList<Rectangle> updates = null);

		public abstract void RemoveDrawing(IList<Rectangle> updates = null);

		public void Redraw()
		{
			var updates = new List<Rectangle>();
			RemoveDrawing(updates);
			ApplyDrawing(updates);
			BGI.UpdateRegion(updates);
		}

		public virtual void Unselected()
		{
		}

		public virtual void Selecting()
		{
		}

		protected Control TopSeparator()
		{
			return Separator(new Padding(0, 0, 0, 5));
		}

		protected Control Separator(Padding? padding = null)
		{
			var control = new Drawable
			{
				Size = new Size(-1, 2)
			};

			control.Paint += (sender, pe) =>
			{
				pe.Graphics.FillRectangle(Colors.White, 0, 0, control.Size.Width, 0);
				pe.Graphics.DrawInsetRectangle(Colors.Gray, Colors.White, new Rectangle(0, 0, control.Size.Width, 2));
			};
			return padding == null ? (Control)control : new Panel
			{
				Content = control,
				Padding = padding.Value
			};
		}

		public virtual Control GeneratePad()
		{
			return null;
		}

	}
}

