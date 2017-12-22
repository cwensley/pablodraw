using Eto.Forms;
using System;

namespace Pablo.Formats.Character.Actions
{

	public class SwitchColour : ButtonAction
	{
		int foregroundIncrement;
		int backgroundIncrement;
		CharacterHandler handler;
		
		public SwitchColour(CharacterHandler handler, int foregroundIncrement, int backgroundIncrement, Key accelerator)
		{
			this.handler = handler;
			this.foregroundIncrement = foregroundIncrement;
			this.backgroundIncrement = backgroundIncrement;
			ID = string.Format("character_switchColour:{0}:{1}", foregroundIncrement, backgroundIncrement);
			Text = "Change Colour";
			Description = "Change Colour";
			Accelerator = accelerator;
		}


		protected override void OnActivated (EventArgs e)
		{
			Attribute attr = handler.DrawAttribute;
			
			attr.Foreground = IncrementValue(attr.Foreground, foregroundIncrement);
			attr.Background = IncrementValue(attr.Background, backgroundIncrement);
			//Console.WriteLine("Switching Colour {0} ({2}), {1} ({3})", attr.Foreground, attr.Background, foregroundIncrement, backgroundIncrement);
			handler.DrawAttribute = attr;
		}
		
		private int IncrementValue(int attr, int inc)
		{
			if (inc != 0)
			{
				int newattr = attr + inc;
				if (newattr < 0) newattr = handler.CurrentPage.Palette.Count-1;
				if (newattr >= handler.CurrentPage.Palette.Count) newattr = 0;
				return (byte)newattr;
			}
			else return attr;
		}
	}
}
