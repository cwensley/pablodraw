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
	public class EditFile : CheckAction, ICommand
	{
		Main main;
		public const string ActionID = "editFile";
		
		public EditFile (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Edit|Edit|Toggles editing of this file";
			this.Image = Icon.FromResource ("Pablo.Interface.Icons.edit.ico");
			this.Checked = main.EditMode;
			this.Accelerator = Command.CommonModifier | Key.E;
			main.EditModeChanged += editModeChanged;
			this.Enabled = main.ViewHandler != null && main.ViewHandler.CanEdit && (main.Client == null || main.Client.CurrentUser.Level >= Level);
		}
		
		public UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		protected override void OnRemoved (EventArgs e)
		{
			base.OnRemoved (e);
			main.EditModeChanged -= editModeChanged;
		}
		
		void editModeChanged (object sender, EventArgs e)
		{
			this.Checked = main.EditMode;
		}
		
		protected override void OnActivated (EventArgs e)
		{
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
