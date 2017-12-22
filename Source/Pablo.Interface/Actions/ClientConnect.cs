using System;
using Eto.Forms;
using Pablo.Network;

namespace Pablo.Interface.Actions
{
	public class ClientConnect : Command, IDisposable
	{
		readonly Main main;
		public const string ActionID = "clientConnect";
		
		public ClientConnect (Main main)
		{
			this.main = main;
			ID = ActionID;
			MenuText = "&Connect to server...";
			ToolTip = "Connects to a pablodraw server";
			Shortcut = PabloCommand.CommonModifier | Keys.Alt | Keys.C;
			SetEnabled ();
			
			main.ClientChanged += main_Changed;
		}

		public void Dispose()
		{
			main.ClientChanged -= main_Changed;
		}

		void main_Changed (object sender, EventArgs e)
		{
			SetEnabled ();
		}
		
		void SetEnabled ()
		{
			Enabled = (main.Client == null);
		}

		protected override void OnExecuted(EventArgs e)
		{
			try {
				var dlg = new Dialogs.ClientDialog{
					Alias = main.Settings.Alias,
					ServerIP = main.Settings.ServerIP,
					ServerPort = main.Settings.ServerPort
				};
				var result = dlg.ShowModal (main);
				if (result == DialogResult.Ok) {
					main.Settings.Alias = dlg.Alias;
					main.Settings.ServerIP = dlg.ServerIP;
					main.Settings.ServerPort = dlg.ServerPort;
					var client = new Client{
						Hostname = dlg.ServerIP,
						Port = dlg.ServerPort,
						Password = dlg.Password,
						Delegate = main
					};
					client.SetCommands(main.Commands);
					client.CurrentUser.Alias = dlg.Alias;
					client.Start ();
					main.Client = client;
				}
			} catch (Exception exception) {
				MessageBox.Show (main, string.Format ("Error connecting to server: {0}", exception.Message));
			}
		}
	}
}
