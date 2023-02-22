using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Controls;
using System.Reflection;
using System.Linq;
using Eto;

namespace Pablo.Formats.Character.Controls
{
	public class ToolboxPad : Panel
	{
		Panel toolPad;

		public new CharacterHandler Handler { get; private set; }

		public ToolboxPad(CharacterHandler handler)
		{
			this.Handler = handler;

			this.Handler.ToolChanged += new EventHandler<EventArgs>(HandleToolChanged).MakeWeak(e => this.Handler.ToolChanged -= HandleToolChanged);

			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.DefaultPadding = Padding.Empty;
			layout.DefaultSpacing = new Size(5, 5);

			layout.BeginCentered(spacing: Size.Empty);
			layout.BeginVertical(spacing: new Size(1, 1));
			layout.BeginHorizontal();
			// layout.Add(null, true);
			var tools = Handler.Tools.ToList();
			for (int i = 0; i < tools.Count; i++)
			{
				var tool = tools[i];
				var button = new ImageButton
				{
					Image = tool.Image,
					Persistent = true,
					Tag = tool,
					Size = new Size(20, 20),
					Pressed = handler.SelectedTool == tool
				};
				if (tool.Accelerator != Keys.None)
					button.ToolTip = string.Format("{0} ({1})", tool.Description, tool.Accelerator.ToShortcutString());
				else
					button.ToolTip = tool.Description;
				tool.Tag = button;
				button.Click += delegate
				{
					Handler.SelectedTool = tool;
				};
				layout.Add(button);
				if ((i % 2) == 1)
				{
					// layout.Add(null, true);
					layout.EndHorizontal();
					layout.BeginHorizontal();
					// layout.Add(null, true);
				}
			}
			layout.EndHorizontal();
			layout.EndVertical();
			layout.EndCentered();

			layout.Add(toolPad = new Panel());

			layout.Add(null);
			SetSelectedTool(Handler.SelectedTool);

			Content = layout;
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			SetSelectedTool(Handler.SelectedTool);
		}

		protected Control Separator()
		{
			var control = new Drawable
			{
				Size = new Size(1, 2)
			};

			control.Paint += (sender, pe) =>
			{
				pe.Graphics.FillRectangle(SystemColors.ControlBackground, 0, 0, control.Size.Width, 0);
				pe.Graphics.DrawInsetRectangle(Colors.Gray, SystemColors.ControlBackground, new Rectangle(0, 0, control.Size.Width, 2));
			};
			return control;
		}

		void SetSelectedTool(CharacterTool tool)
		{
			if (tool != null)
			{
				var pad = tool.GeneratePad();
				if (pad != null)
					pad = new TableLayout(Separator(), pad) { Spacing = new Size(5, 5) };
				toolPad.Content = pad;
			}
			else
				toolPad.Content = null;
		}

		void HandleToolChanged(object sender, EventArgs e)
		{
			var tool = Handler.SelectedTool;
			var toolbutton = tool != null ? tool.Tag as ImageButton : null;
			foreach (var control in Children.OfType<ImageButton>().Where(r => r.Pressed && r.Tag is CharacterTool && r != toolbutton))
			{
				control.Pressed = false;
			}
			if (toolbutton != null)
			{
				toolbutton.Pressed = true;
			}
			SetSelectedTool(tool);
		}
	}
}

