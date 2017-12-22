using System;
using Eto.Forms;
using Pablo.Network;
using Lidgren.Network;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class ToggleIceMode : CheckAction, ICommand
	{
		CharacterHandler handler;
		
		public const string ActionID = "character_ToggleIceMode";
		
		public ToggleIceMode (CharacterHandler handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.Text = "Use iCE Colors|iCE|Toggle iCE colors (bright backgrounds) vs. Blinking Foreground characters";
			if (handler.Document.EditMode || handler.Client != null)
				this.Checked = handler.CharacterDocument.ICEColours;
			else
				this.Checked = handler.Info.iCEColours;
			
			if (handler.Client != null) {
				handler.CharacterDocument.ICEColoursChanged += new EventHandler<EventArgs>(delegate {
					this.Checked = handler.CharacterDocument.ICEColours;
				}).MakeWeak(e => handler.CharacterDocument.ICEColoursChanged -= e);
				this.Enabled = handler.Client.CurrentUser.Level >= this.Level;
			}
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			if (handler.Client != null)
				handler.Client.SendCommand (this);
			else
				Do (!this.Checked);
		}
		
		void Do(bool useIce)
		{
			if (handler.Document.EditMode || handler.Client != null)
				handler.CharacterDocument.ICEColours = useIce;
			else
				handler.Info.iCEColours = useIce;
			this.Checked = useIce;
		}
		

		public int CommandID {
			get { return (int)NetCommands.ToggleIceMode; }
		}

		public UserLevel Level {
			get { return UserLevel.Operator; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return NetDeliveryMethod.ReliableOrdered; }
		}

		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (!this.Checked);
			return true;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			var useIce = args.Message.ReadBoolean ();
			args.Invoke (delegate {
				Do(useIce);
			});
		}
	}
}

