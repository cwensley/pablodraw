using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Pablo.Network;

namespace Pablo.Interface.Actions
{
	public class ServerStop : ButtonAction
	{
		Main main;
		public const string ActionID = "serverStop";
		
		public ServerStop (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Disconnect|Disconnect|Disconnects from the pablodraw server or stops local server";
			this.Accelerator = Command.CommonModifier | Key.Alt | Key.D;
			SetEnabled ();
			
			//new EnabledConnector(this);
			main.ServerChanged += main_Changed;
			main.ClientChanged += main_Changed;
		}
		
		protected override void OnRemoved (EventArgs e)
		{
			main.ClientChanged -= main_Changed;
			main.ServerChanged -= main_Changed;
		}
		
		private void main_Changed (object sender, EventArgs e)
		{
			SetEnabled ();
		}
		
		private void SetEnabled ()
		{
			this.Enabled = (main.Server != null) || (main.Client != null);
		}
		
		protected override void OnActivated (EventArgs e)
		{
			if (main.Client != null) {
				try {
					main.Client.Stop ();
				} catch (Exception exception) {
					MessageBox.Show (main.Generator, main, string.Format ("Error disconnecting: {0}", exception.Message));
				} finally {
					main.Client = null;
				}
			}
			
			if (main.Server != null) {
				try {
					main.Server.Stop ();
				} catch (Exception exception) {
					MessageBox.Show (main.Generator, main, string.Format ("Error stopping server: {0}", exception.Message));
				} finally {
					main.Server = null;
				}
			}
		}
	}
}
