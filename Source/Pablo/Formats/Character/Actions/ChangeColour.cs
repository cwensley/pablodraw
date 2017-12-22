using Eto.Forms;
using System;

namespace Pablo.Formats.Character.Actions
{

	public class ChangeColour : ButtonAction
	{
		int? foreground;
		int? background;
		CharacterHandler handler;
		
		public ChangeColour(CharacterHandler handler, int? foreground, int? background, Key accelerator)
		{
			this.handler = handler;
			this.foreground = foreground;
			this.background = background;
			ID = string.Format("character_changeColour:{0}:{1}", foreground, background);
			Text = "Change Colour";
			Description = "Change Colour";
			Accelerator = accelerator;
		}

		
		protected override void OnActivated (EventArgs e)
		{
			Attribute attr = handler.DrawAttribute;
			
			int palHalfSize = 8;
			
			if (foreground != null) {
				var fg = handler.ColourStart + foreground.Value;
				if (attr.Foreground == fg || (attr.Foreground != fg && attr.Foreground != fg + palHalfSize && attr.Foreground >= handler.ColourStart + palHalfSize)) {
					if (fg + palHalfSize < handler.CurrentPage.Palette.Count)
						fg += palHalfSize;
				}
				if (fg < handler.CurrentPage.Palette.Count)
					attr.Foreground = fg;
			}
			if (background != null) {
				var bg = handler.ColourStart + background.Value;
				if (attr.Background == bg || (attr.Background != bg && attr.Background != bg + palHalfSize && attr.Background >= handler.ColourStart + palHalfSize)) {
					if (bg + palHalfSize < handler.CurrentPage.Palette.Count)
						bg += palHalfSize;
				}
				if (bg < handler.CurrentPage.Palette.Count)
					attr.Background = bg;
			}
			
			handler.DrawAttribute = attr;
		}
	}
}
