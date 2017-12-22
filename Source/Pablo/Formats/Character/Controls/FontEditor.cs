using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	public class FontEditor : Dialog
	{
		readonly CharacterHandler handler;

		public FontEditor(CharacterHandler handler)
		{
			this.handler = handler;

			var layout = new DynamicLayout();

			layout.Add(null, yscale: true);

			layout.AddSeparateRow(null, CancelButton(), OkButton());
		}

		Control CancelButton()
		{
			var control = new Button { Text = "Cancel" };

			control.Click += delegate
			{
				DialogResult = DialogResult.Cancel;
				Close();
			};
			AbortButton = control;

			return control;
		}

		Control OkButton()
		{
			var control = new Button { Text = "Ok" };

			control.Click += delegate
			{
				DialogResult = DialogResult.Ok;
				Close();
			};

			DefaultButton = control;

			return control;
		}

	}
}

