using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Controls;

namespace Pablo.Formats.Character.Actions
{
	public class SelectAttribute : Command
	{
		CharacterHandler handler;
		
		public SelectAttribute(CharacterHandler handler)
		{
			this.handler = handler;
			ID = "character_selectAttribute";
			MenuText = ToolBarText = "Attribute";
			ToolTip = "Shows a dialog to select the current attribute";
			Shortcut = Keys.Alt | Keys.A; // TODO: new [] { Keys.Escape, Keys.Alt | Keys.A };
		}
		
		public override bool Enabled {
			get { return handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			var dialog = new AttributeDialog(handler, handler.CurrentPage.Palette, handler.DrawAttribute, handler.CharacterDocument.ICEColours);
			if (dialog.ShowModal(handler.Viewer as Control))
			{
				handler.DrawAttribute = dialog.Attribute;
			}
		}
	}
}
