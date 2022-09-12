using Eto;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Reflection;

namespace Pablo.Interface.Dialogs
{
	public class About : Dialog
	{
		const string CopyRightText = @"PabloDraw is © 2006-2022 by Curtis Wensley aka Eto

Amiga raw font versions are © dMG/t!s^dS!

Topaz is © AmigaInc
P0T-NOoDLE is © Leo ""Nudel"" Davidson
MicroKnight is © Unknown author
mO'sOul is © Desoto/Mo'Soul";

		public About()
		{
			this.Title = "About PabloDraw";
			this.Resizable = true;

			var layout = new DynamicLayout();
			layout.Padding = Padding.Empty;

			layout.BeginHorizontal();
			layout.BeginVertical(new Padding(15, 20));
			layout.Add(new ImageView
			{
				Image = ImageCache.IconFromResource("Pablo.Interface.Icons.PabloDraw.ico"),
				Size = new Size(128, 128)
			}, yscale: true);

			layout.Add(new Label
			{
				Text = "PabloDraw",
				Font = Fonts.Sans(16, FontStyle.Bold),
				TextAlignment = TextAlignment.Center
			});

			var version = Assembly.GetEntryAssembly().GetName().Version;
			layout.Add(new Label
			{
				Text = string.Format("Version {0}", version),
				TextAlignment = TextAlignment.Center
			});

			if (!Platform.IsMac)
			{
				var b = new Button
				{
					Text = "Close",
				};
				this.AbortButton = b;
				b.Click += delegate
				{
					Close();
				};
				layout.AddCentered(b);
			}

			layout.EndVertical();

			layout.Add(new Panel
			{
				Padding = new Padding(10),
				Content = new Scrollable
				{
					Size = new Size(-1, 140),
					Padding = new Padding(4),
					Border = BorderType.Line,
					Content = new Label
					{
						Text = CopyRightText,
						TextAlignment = TextAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center

					},
					BackgroundColor = SystemColors.ControlBackground
				}
			});

			layout.EndHorizontal();


			Content = layout;
		}
	}
}

