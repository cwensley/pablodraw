using System;
using Eto.Forms;
using Eto.Drawing;
using Eto;

namespace Pablo.Formats.Character.Controls
{
	public class FlagsPad : Panel
	{
		Label insertLabel;
		Label fontLabel;

		public new CharacterHandler Handler { get; private set; }

		public FlagsPad(CharacterHandler handler)
		{
			this.Handler = handler;
#if DESKTOP
			//this.MinimumSize = new Size (200, 0);
#endif
			
			handler.InsertModeChanged += new EventHandler<EventArgs>(delegate
			{
				Update();
			}).MakeWeak(e => handler.InsertModeChanged -= e);
			
			handler.CurrentPage.FontChanged += new EventHandler<EventArgs>(delegate
			{
				Update();
			}).MakeWeak(e => handler.CurrentPage.FontChanged -= e);
			
			
			var layout = new DynamicLayout { Padding = new Padding(0, 5) };
			
			layout.AddRow(FontLabel(), InsertLabel());

			Content = layout;
			
			Update();
			
		}

		Control InsertLabel()
		{
			insertLabel = new Label
			{ 
				TextAlignment = TextAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			return insertLabel;
		}

		Control FontLabel()
		{
			fontLabel = new Label
			{
				Wrap = WrapMode.None,
				TextAlignment = TextAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center
			};
#if DESKTOP
			fontLabel.MouseDown += delegate
			{
				var menu = new ContextMenu();
				Handler.Info.GetFontMenu(Handler, subMenu: menu);
				menu.Show(fontLabel);
			};
#endif
			
			return fontLabel;
		}

		void Update()
		{
			var font = Handler.CurrentPage.Font;
			if (font.IsSystemFont)
				fontLabel.Text = Handler.CurrentPage.Font.DisplayName;
			else
				fontLabel.Text = "(Custom Font)";
			insertLabel.Text = Handler.InsertMode ? "(ins)" : string.Empty;
		}
	}
}

