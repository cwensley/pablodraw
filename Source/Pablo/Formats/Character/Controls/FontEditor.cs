using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	public class FontEditor : Dialog<DialogResult>
	{
		public FontEditor(CharacterHandler handler)
		{
			var layout = new DynamicLayout();

			layout.Add(null, yscale: true);

			layout.AddSeparateRow(null, CancelButton(), OkButton());
		}

		Control CancelButton()
		{
			var control = new Button { Text = "Cancel" };

			control.Click += delegate
			{
				Result = DialogResult.Cancel;
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
				Result = DialogResult.Ok;
				Close();
			};

			DefaultButton = control;

			return control;
		}

	}
}

