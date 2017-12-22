using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Eto;
using Pablo.Interface.Dialogs;
using Pablo.Network;

namespace Pablo.Interface.Actions
{
	public class ViewFile : CheckAction, ICommand
	{
		Main main;
		public const string ActionID = "viewFile";
		
		public ViewFile (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&View|View|Views the document";
			this.Image = Icon.FromResource ("Pablo.Interface.Icons.view.ico");
			this.Checked = !main.EditMode;
			this.Accelerator = Command.CommonModifier | Key.V;
			main.EditModeChanged += editModeChanged;
			this.Enabled = (main.Client == null || main.Client.CurrentUser.Level >= Level);
		}
		
		protected override void OnRemoved (EventArgs e)
		{
			base.OnRemoved (e);
			main.EditModeChanged -= editModeChanged;
		}
		
		void editModeChanged (object sender, EventArgs e)
		{
			this.Checked = !main.EditMode;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			if (main.Document == null)
				return;

			if (FileModifiedDialog.Show (main) == DialogResult.Ok) {
				if (main.Client != null) {
					main.Client.SendCommand (this);
				} else {
					main.EditMode = !main.EditMode;
					//if (!main.EditMode) 
					//	main.ReloadFile ();
				}
			}
		}
		
		public int CommandID {
			get { return (int)NetCommands.EditFile; }
		}
		
		public UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		#region INetworkReadWrite implementation
		
		public bool Send (SendCommandArgs args)
		{
			args.Message.Write (false);
			return true;
		}

		public void Receive (ReceiveCommandArgs args)
		{
			throw new NotImplementedException (); // should not get here
		}
		#endregion
	}
}
