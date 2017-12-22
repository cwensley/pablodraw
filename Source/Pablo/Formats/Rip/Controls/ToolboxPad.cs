using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Controls;
using System.Reflection;
using System.Linq;
using Eto;

namespace Pablo.Formats.Rip.Controls
{
	public class ToolboxPad : Panel
	{
		Panel toolPad;
		
		public new RipHandler Handler { get; private set; }
		
		public ToolboxPad (RipHandler handler)
		{
			this.Handler = handler;
			
			this.Handler.ToolChanged += new EventHandler<EventArgs> (HandleToolChanged).MakeWeak (e => this.Handler.ToolChanged -= HandleToolChanged);

			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.DefaultPadding = Padding.Empty;
			layout.DefaultSpacing = new Size(5, 5);
			
			
			layout.BeginVertical (spacing: Size.Empty);
			layout.BeginHorizontal ();
			layout.Add (null, true);
			var tools = Handler.Tools.ToList ();
			for (int i=0; i<tools.Count; i++) {
				var tool = tools [i];
				var button = new ImageButton{ 
					Image = tool.Image, 
					Persistent = true,
#if DESKTOP
					ToolTip = tool.Description,
#endif
					Tag = tool, 
					Size = new Size(20, 18),
					Pressed = handler.SelectedTool == tool
				};
				tool.Tag = button;
				button.Click += delegate {
					Handler.SelectedTool = tool;
				};
				layout.Add (button);
				if ((i % 2) == 1) {
					layout.Add (null, true);
					layout.EndHorizontal ();
					layout.BeginHorizontal ();
					layout.Add (null, true);
				}
			}
			layout.EndHorizontal ();
			layout.EndVertical ();

			layout.Add (toolPad = new Panel ());
			
			layout.Add (null);

			Content = layout;
			
		}
		
		protected override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			if (Handler.SelectedTool != null) {
				toolPad.Content = Handler.SelectedTool.GeneratePad ();
				//this.ParentLayout.Update ();
			}
		}

		void HandleToolChanged (object sender, EventArgs e)
		{
			var tool = Handler.SelectedTool;
			var toolbutton = tool != null ? tool.Tag as ImageButton : null;
			foreach (var control in Children.OfType<ImageButton>().Where (r => r.Pressed && r != toolbutton)) {
				control.Pressed = false;
			}
			if (toolbutton != null) {
				toolbutton.Pressed = true;
				toolPad.Content = tool.GeneratePad ();
			} else
				toolPad.Content = null;
			//this.ParentLayout.Update ();
		}
	}
}

