using System;
using Eto.Forms;
using System.IO;
using Eto.Drawing;

namespace Pablo.Interface.Dialogs
{
	public class RenameDialog : Dialog<DialogResult>
	{
		string name;
		string path;
		
		public string NewName {
			get { return Path.Combine (path, name); }
		}
		
		Control FileNameControl ()
		{
			var control = new TextBox{
				Text = name
			};
			control.KeyDown += (sender, e) => {
				if (e.Key == Keys.Escape)
					Close ();
			};
			control.TextChanged += delegate {
				name = control.Text;
			};
			return control;
		}
		
		Control OkButton ()
		{
			var control = new Button{
				Text = "Rename"
			};
			control.Click += delegate {
				Result = DialogResult.Ok;
				Close ();
			};
			this.DefaultButton = control;
			return control;
		}
		
		Control CancelButton ()
		{
			var control = new Button{
				Text = "Cancel"
			};
			control.Click += delegate {
				Result = DialogResult.Cancel;
				Close ();
			};
			this.AbortButton = control;
			return control;
		}
		
		public RenameDialog (string fileName)
		{
			this.DisplayMode = DialogDisplayMode.Attached;
			this.Title = "Rename File / Directory";
			this.MinimumSize = new Size(320, 10);
			this.Resizable = true;
			this.name = Path.GetFileName (fileName);
			this.path = Path.GetDirectoryName (fileName);
			
			var layout = new DynamicLayout { Padding = new Padding(20, 10) };
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			layout.BeginHorizontal ();
			layout.Add (new Label{ Text = "File Name"});
			layout.Add (FileNameControl (), true);
			layout.EndHorizontal ();
			layout.EndVertical ();
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			layout.BeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CancelButton ());
			layout.Add (OkButton ());
			layout.EndHorizontal ();
			layout.EndVertical ();

			Content = layout;
		}
	}
}

