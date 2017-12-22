using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Net;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Pablo.Network;
using Eto;
using System.Linq;
using System.Diagnostics;

namespace Pablo.Interface.Actions
{
	public class ServerStart : ButtonAction
	{
		Main main;
		public const string ActionID = "serverStart";

		public ServerStart(Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Start Server|Start Server|Starts the pablodraw server";
			this.Accelerator = Command.CommonModifier | Key.Alt | Key.S;
			SetEnabled();
			
			//new EnabledConnector(this);
			main.ServerChanged += main_Changed;
			main.ClientChanged += main_Changed;
		}

		protected override void OnRemoved(EventArgs e)
		{
			main.ClientChanged -= main_Changed;
			main.ServerChanged -= main_Changed;
		}

		void main_Changed(object sender, EventArgs e)
		{
			SetEnabled();
		}

		void SetEnabled()
		{
			Enabled = (main.Server == null && main.Client == null);
		}

		protected override void OnActivated(EventArgs e)
		{
			if (main.Server == null)
			{
//#if !DEBUG
				try
				{
//#endif
					var dlg = new Dialogs.ServerDialog
					{
						Alias = main.Settings.Alias,
						ServerPort = main.Settings.ServerPort,
						Password = main.Settings.ServerPassword,
						UserLevel = main.Settings.UserLevel,
						UseNat = main.Settings.UseNat,
						OperatorPassword = main.Settings.ServerOperatorPassword
					};
					var result = dlg.ShowDialog(main);
					if (result == DialogResult.Ok)
					{
						main.Settings.Alias = dlg.Alias;
						main.Settings.ServerPort = dlg.ServerPort;
						main.Settings.ServerPassword = dlg.Password;
						main.Settings.UserLevel = dlg.UserLevel;
						main.Settings.UseNat = dlg.UseNat;
						main.Settings.ServerOperatorPassword = dlg.OperatorPassword;
						
						var client = new Client
						{
							Port = dlg.ServerPort,
							Password = dlg.OperatorPassword ?? dlg.Password,
							Delegate = main
						};
						client.SetCommands(main.Commands);
						client.CurrentUser.Alias = dlg.Alias;
						client.CurrentUser.Level = UserLevel.Operator;
						main.Client = client;
						
						var server = new Server
						{
							Port = dlg.ServerPort,
							Password = dlg.Password,
							OperatorPassword = dlg.OperatorPassword,
							AutoMapPort = dlg.UseNat,
							DefaultUserLevel = dlg.UserLevel,
							Delegate = main,
							Client = client
						};
						server.SetCommands(main.ServerCommands);
						main.Server = server;
						server.Start();
						
						client.Start();
					}
//#if !DEBUG
				}
				catch (Exception exception)
				{
					Debug.WriteLine("Exception: {0}", exception);
					if (main.Server != null)
					{
						main.Server.Stop();
						main.Server = null;
					}
					if (main.Client != null)
					{
						main.Client.Stop();
						main.Client = null;
					}
					MessageBox.Show(main.Generator, main, string.Format("Could not start server: {0}", exception.Message));
				}
//#endif
			}
			
		}
	}
}
