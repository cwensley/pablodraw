using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Pablo.Interface.Dialogs;
using Eto;
using Pablo.Network;
using Lidgren.Network;

namespace Pablo.Interface.Actions
{
	public class EditFileRadio : RadioCommand, ICommand, IDisposable
	{
		Main main;
		public const string ActionID = "editFile";
		
		public EditFileRadio(Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.MenuText = "&Edit";
			this.ToolBarText = "Edit";
			this.ToolTip = "Toggles editing of this file";
			this.Image = ImageCache.IconFromResource("Pablo.Interface.Icons.edit.ico");
			this.Checked = main.EditMode;
			this.Shortcut = PabloCommand.CommonModifier | Keys.E;
			main.EditModeChanged += editModeChanged;
			this.Enabled = main.ViewHandler != null && main.ViewHandler.CanEdit && (main.Client == null || main.Client.CurrentUser.Level >= Level);
		}

		public void Dispose()
		{
			main.EditModeChanged -= editModeChanged;
		}
		
		public UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		void editModeChanged (object sender, EventArgs e)
		{
			this.Checked = main.EditMode;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (!main.EditMode || FileModifiedDialog.Show (main) == DialogResult.Ok) {
				if (main.Client != null) {
					main.Client.SendCommand (this);
				} else {
					main.EditMode = !main.EditMode;
					//if (!main.EditMode) 
					//	main.ReloadFile ();
				}
			}
		}
		
		#region ICommand implementation
		
		public int CommandID {
			get { return (int)NetCommands.EditFile; }
		}

		#endregion

		#region INetworkReadWrite implementation
		
		public bool Send (SendCommandArgs args)
		{
			if (!main.Document.Info.CanEdit)
				return false;
			args.Message.Write (!main.EditMode);
			return true;
		}

		public void Receive (ReceiveCommandArgs args)
		{
			var editMode = args.Message.ReadBoolean ();
			args.Invoke (delegate {
				main.EditMode = editMode;
				var client = args.Network as Client;
				if (client != null && args.User != null) {
					if (main.EditMode)
						client.OnMessage (new ClientMessageArgs (string.Format ("{0} started editing", args.User)));
					else
						client.OnMessage (new ClientMessageArgs (string.Format ("{0} ended editing", args.User)));
				}
			});
		}
		#endregion
	}
}
