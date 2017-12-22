using System;
using Eto.Forms;

namespace Pablo.Interface.Dialogs
{
	public class NewDocument : Dialog
	{
		public NewDocument ()
		{
			var layout = new DynamicLayout ();
			
			layout.BeginVertical();
			layout.AddRow (null, OkButton());
			layout.EndVertical ();

			Content = layout;
		}
		
		
		Control OkButton()
		{
			var control = new Button{
				Text = "Create"
			};
			return control;
		}
	}
}

