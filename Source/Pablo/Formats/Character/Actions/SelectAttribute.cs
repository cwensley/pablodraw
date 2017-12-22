using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Controls;

namespace Pablo.Formats.Character.Actions
{
	public class SelectAttribute : ButtonAction
	{
		CharacterHandler handler;
		
		public SelectAttribute(CharacterHandler handler)
		{
			this.handler = handler;
			ID = "character_selectAttribute";
			Text = "Attribute|Attribute|Select Attribute|Shows a dialog to select the current attribute";
			Accelerators = new Key[] { Key.Escape, Key.Alt | Key.A };
		}
		
		public override bool Enabled {
			get { return handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}
		
		protected override void OnActivated (EventArgs e)
		{
			var dialog = new AttributeDialog(handler, handler.CurrentPage.Palette, handler.DrawAttribute, handler.CharacterDocument.ICEColours);
			var result = dialog.ShowDialog(handler.Viewer as Control);
			if (result == DialogResult.Ok)
			{
				handler.DrawAttribute = dialog.Attribute;
			}
		}
	}
}
