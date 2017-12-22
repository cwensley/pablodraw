using System;
using Eto.Forms;

namespace Pablo.Sauce.Types.Binary
{
	public class Admin<T> : BaseText.Admin<T>
		where T: DataTypeInfo
	{
		public Admin (T dataType)
			: base(dataType, false)
		{
		}

		protected override void CreateControls()
		{
			base.CreateControls();
			Layout.BeginHorizontal ();
			Layout.Add (new Label{ Text = "Binary Width", VerticalAlign = VerticalAlign.Middle });
			Layout.Add (WidthControl ());
			Layout.EndHorizontal ();
		}
		
		Control WidthControl ()
		{
			var control = new TextBox{
				Text = DataType.Width.ToString ()
			};
			control.TextChanged += delegate {
				int val;
				if (int.TryParse (control.Text, out val)) {
					if (val > 0 && val <= 510 && val % 2 == 0)
						DataType.Width = val;
					else {
						DataType.Width = 160;
					}
				} else 
					DataType.Width = 160;
			};
			return control;
		}
	}
}

