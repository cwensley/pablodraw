using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Pablo.Network;
using System.Linq;

namespace Pablo.Interface.Actions
{
	public class ClientConnect : ButtonAction
	{
		Main main;
		public const string ActionID = "clientConnect";
		
		public ClientConnect (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Connect to server...|Connect|Connects to a pablodraw server";
			this.Accelerator = Command.CommonModifier | Key.Alt | Key.C;
			SetEnabled ();
			
			main.ClientChanged += main_Changed;
		}
		
		protected override void OnRemoved (EventArgs e)
		{
			base.OnRemoved (e);
			main.ClientChanged -= main_Changed;
		}
		
		void main_Changed (object sender, EventArgs e)
		{
			SetEnabled ();
		}
		
		void SetEnabled ()
		{
			this.Enabled = (main.Client == null);
		}
		
		protected override void OnActivated (EventArgs e)
		{
			try {
				var dlg = new Dialogs.ClientDialog{
					Alias = main.Settings.Alias,
					ServerIP = main.Settings.ServerIP,
					ServerPort = main.Settings.ServerPort
				};
				var result = dlg.ShowDialog (main);
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
				MessageBox.Show (main.Generator, main, string.Format ("Error connecting to server: {0}", exception.Message));
			}
		}
	}
}
