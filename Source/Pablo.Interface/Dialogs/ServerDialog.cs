using System;
using Eto.Forms;
using Pablo.Network;
using Eto.Drawing;

namespace Pablo.Interface.Dialogs
{
	public class ServerDialog : Dialog<DialogResult>
	{
		TextBox passwordTextBox;
		TextBox operatorPassword;
		TextBox aliasTextBox;
		CheckBox useNat;
		TextBox serverPortTextBox;
		EnumDropDown<UserLevel> userLevel;

		public string Password
		{
			get { return passwordTextBox.Text; }
			set { passwordTextBox.Text = value; }
		}

		public string OperatorPassword
		{
			get { return operatorPassword.Text; }
			set { operatorPassword.Text = value; }
		}

		public string Alias
		{
			get { return aliasTextBox.Text; }
			set { aliasTextBox.Text = value; }
		}

		public int ServerPort
		{
			get
			{ 
				int port;
				if (int.TryParse(serverPortTextBox.Text, out port))
					return port;
				else
					return Settings.DefaultServerPort;
			}
			set
			{
				serverPortTextBox.Text = value.ToString();
			}
		}

		public bool UseNat
		{
			get { return useNat.Checked ?? false; }
			set { useNat.Checked = value; }
		}

		public UserLevel UserLevel
		{
			get { return userLevel.SelectedValue; }
			set { userLevel.SelectedValue = value; }
		}

		public ServerDialog()
		{
			this.Title = "Server Options";
			//this.MinimumSize = new Size(300, 10);
			var layout = new TableLayout(1, 3) { Spacing = new Size(4, 4), Padding = 6 };
			
			int row = 0;
			
			layout.Add(EntryControls(), 0, row++);
			layout.Add(ServerControls(), 0, row++);
			layout.SetRowScale(1);
			layout.Add(Buttons(), 0, row++);

			Content = layout;
		}

		Control EntryControls()
		{
			var layout = new TableLayout(2, 1) { Spacing = new Size(4, 4) };
			int row = 0;
			layout.Add(new Label { Text = "Alias" }, 0, row);
			layout.Add(AliasControl(), 1, row++);

			return layout;
		}

		Control ServerControls()
		{
			var layout = new DynamicLayout { DefaultSpacing = new Size(4, 4), Padding = 6 };
			
			layout.AddRow(new Label { Text = "Server Port" }, ServerPortControl());

			//layout.Add (new Label{ Text = "Use NAT/UPNP"}, 0, row);
			layout.AddRow(new Panel(), UseNATControl());
			layout.AddRow(new Panel(), new Label { Text = "Uses either NAT-PMP or UPnP", Font = new Font(SystemFont.Label, 8), TextColor = Colors.Gray });

			layout.AddRow(new Label { Text = "Password" }, PasswordControl());
			layout.AddRow(new Label { Text = "Operator Password" }, AdminPasswordControl());
			layout.AddRow(new Label { Text = "Default User Level" }, UserLevelControl());

			return new GroupBox
			{
				Content = layout,
				Text = "Server Details"
			};
		}

		Control UseNATControl()
		{
			var control = useNat = new CheckBox
			{
				Text = "Automatically map port"
			};
			
			return control;
		}

		Control PasswordControl()
		{
			var control = passwordTextBox = new TextBox
			{
			};
			
			return control;
		}

		Control AdminPasswordControl()
		{
			return operatorPassword = new TextBox();
		}

		Control ServerPortControl()
		{
			var control = serverPortTextBox = new TextBox
			{
			};
			
			return control;
		}

		Control AliasControl()
		{
			var control = aliasTextBox = new TextBox
			{
			};
			
			return control;
		}

		Control Buttons()
		{
			var layout = new TableLayout(3, 1) { Spacing = new Size(4, 4) };
			layout.SetColumnScale(0);
			layout.Add(CancelButton(), 1, 0);
			layout.Add(OkButton(), 2, 0);
			return layout;
		}

		bool Validate()
		{
			if (string.IsNullOrEmpty(Alias))
			{
				MessageBox.Show(this, "Please enter an alias to use", MessageBoxType.Error);
				return false;
			}
			int port;
			if (string.IsNullOrEmpty(serverPortTextBox.Text) || !Int32.TryParse(serverPortTextBox.Text, out port))
			{
				MessageBox.Show(this, "Port is invalid", MessageBoxType.Error);
				return false;
			}
			return true;
		}

		Control OkButton()
		{
			var control = new Button
			{
				Text = "OK"
			};
			DefaultButton = control;
			control.Click += delegate
			{
				if (Validate())
				{
					Result = DialogResult.Ok;
					Close();
				}
			};
			return control;
		}

		Control CancelButton()
		{
			var control = new Button
			{
				Text = "Cancel"
			};
			AbortButton = control;
			control.Click += delegate
			{
				Result = DialogResult.Cancel;
				Close();
			};
			return control;
		}

		Control UserLevelControl()
		{
			var control = userLevel = new EnumDropDown<UserLevel>
			{
			};
			
			return control;
		}
	}
}

