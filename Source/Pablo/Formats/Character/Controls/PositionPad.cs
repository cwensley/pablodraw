using System;
using Eto.Forms;
using Eto.Drawing;
using Eto;

namespace Pablo.Formats.Character.Controls
{
	public class PositionPad : Panel
	{
		public new CharacterHandler Handler { get; private set; }
		
		Label label;
		public PositionPad (CharacterHandler handler)
		{
			this.Handler = handler;
#if DESKTOP
			//this.MinimumSize = new Size(100, 10);
#endif
			
			Handler.CursorPositionChanged += new EventHandler<EventArgs>(delegate{
				Update();
			}).MakeWeak(e => Handler.CursorPositionChanged -= e);
			
			label = new Label{ 
				TextAlignment = TextAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};

			var layout = new DynamicLayout { Padding = new Padding(0, 5, 5, 5) };
			layout.AddRow(label);
			this.Content = layout;
			
			Update ();
		}

		void Update()
		{
			var p = Handler.CursorPosition;
			label.Text = string.Format("({0}, {1})", p.X + 1, p.Y + 1);
		}
	}
}

