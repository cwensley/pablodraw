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
	public class ViewFile : RadioCommand, ICommand, IDisposable
	{
		Main main;
		public const string ActionID = "viewFile";

		public ViewFile(Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.MenuText = "&View";
			this.ToolBarText = "View";
			this.ToolTip = "Views the document";
			this.Image = ImageCache.IconFromResource("Pablo.Interface.Icons.view.ico");
			this.Checked = !main.EditMode;
			this.Shortcut = PabloCommand.CommonModifier | Keys.V;
			main.EditModeChanged += editModeChanged;
			this.Enabled = (main.Client == null || main.Client.CurrentUser.Level >= Level);
		}

		public void Dispose()
		{
			main.EditModeChanged -= editModeChanged;
		}


		void editModeChanged(object sender, EventArgs e)
		{
			this.Checked = !main.EditMode;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (main.Document == null)
				return;

			if (FileModifiedDialog.Show(main) == DialogResult.Ok)
			{
				if (main.Client != null)
				{
					main.Client.SendCommand(this);
				}
				else
				{
					main.EditMode = !main.EditMode;
					//if (!main.EditMode) 
					//	main.ReloadFile ();
				}
			}
		}

		public int CommandID
		{
			get { return (int)NetCommands.EditFile; }
		}

		public UserLevel Level
		{
			get { return UserLevel.Operator; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod
		{
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}

		#region INetworkReadWrite implementation

		public bool Send(SendCommandArgs args)
		{
			args.Message.Write(false);
			return true;
		}

		public void Receive(ReceiveCommandArgs args)
		{
			throw new NotImplementedException(); // should not get here
		}
		#endregion
	}
}
