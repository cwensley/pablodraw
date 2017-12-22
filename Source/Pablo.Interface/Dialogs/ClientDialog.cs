using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Interface.Dialogs
{
	public class ClientDialog : Dialog<DialogResult>
	{
		TextBox passwordTextBox;
		TextBox aliasTextBox;
		TextBox serverIPTextBox;
		TextBox serverPortTextBox;
		
		public string Password {
			get { return passwordTextBox.Text; }
		}
		
		public string Alias {
			get { return aliasTextBox.Text; }
			set { aliasTextBox.Text = value; }
		}
		
		public string ServerIP {
			get { return serverIPTextBox.Text; }
			set { serverIPTextBox.Text = value; }
		}

		public int ServerPort {
			get { 
				int port;
				if (int.TryParse(serverPortTextBox.Text, out port))
					return port;
				else
					return Settings.DefaultServerPort;
			}
			set {
				serverPortTextBox.Text = value.ToString ();
			}
		}
		
		public ClientDialog ()
		{
			this.Title = "Connect to server";
			this.MinimumSize = new Size(300, 10);
			//this.Resizable = true;
			var layout = new TableLayout (1, 3);
			
			int row = 0;
			layout.Add (EntryControls (), 0, row++);
			layout.Add (ServerControls (), 0, row++);
			
			layout.Add (Buttons (), 0, row++);

			Content = layout;
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			if (string.IsNullOrEmpty (Alias))
			{
				aliasTextBox.Focus ();
			}
			else 
				serverIPTextBox.Focus ();
		}
		
		Control EntryControls ()
		{
			var layout = new TableLayout (2, 1);
			int row = 0;
			layout.Add (new Label{ Text = "Alias"}, 0, row);
			layout.Add (AliasControl (), 1, row++);


			return layout;
		}
		
		Control ServerControls ()
		{
			var layout = new TableLayout(2, 3);
			
			int row = 0;
			
			layout.Add (new Label{ Text = "Server IP"}, 0, row);
			layout.Add (ServerIPControl (), 1, row++);

			layout.Add (new Label{ Text = "Server Port"}, 0, row);
			layout.Add (ServerPortControl (), 1, row++);
			
			layout.Add (new Label{ Text = "Server Password"}, 0, row);
			layout.Add (PasswordControl (), 1, row++);

			return new GroupBox
			{
				Content = layout,
				Text = "Server Connection"
			};
		}

		Control ServerIPControl ()
		{
			var control = serverIPTextBox = new TextBox{
				Text = "127.0.0.1",
			};
			
			return control;
		}

		Control ServerPortControl ()
		{
			var control = serverPortTextBox = new TextBox{
			};
			
			return control;
		}
		
		Control PasswordControl ()
		{
			var control = passwordTextBox = new TextBox{
			};
			
			return control;
		}

		Control AliasControl ()
		{
			var control = aliasTextBox = new TextBox{
			};
			
			return control;
		}
		
		Control Buttons ()
		{
			var layout = new TableLayout (3, 1);
			layout.SetColumnScale (0);
			layout.Add (CancelButton (), 1, 0);
			layout.Add (OkButton (), 2, 0);
			return layout;
		}
		
		Control OkButton ()
		{
			var control = new Button{
				Text = "OK"
			};
			DefaultButton = control;
			control.Click += delegate {
				if (string.IsNullOrWhiteSpace (this.Alias)) {
					MessageBox.Show (this, "You must enter an alias (handle) to use when connecting to a server", MessageBoxButtons.OK, MessageBoxType.Error);
				}
				else if (string.IsNullOrWhiteSpace (this.ServerIP) || string.IsNullOrWhiteSpace(serverPortTextBox.Text)) {
					MessageBox.Show (this, "You must specify a server IP/name and the port of the server you are connecting to", MessageBoxButtons.OK, MessageBoxType.Error);
				}
				else {
					Result = DialogResult.Ok;
					Close ();
				}
			};
			return control;
		}

		Control CancelButton ()
		{
			var control = new Button{
				Text = "Cancel"
			};
			AbortButton = control;
			control.Click += delegate {
				Result = DialogResult.Cancel;
				Close ();
			};
			return control;
		}
	}
}

