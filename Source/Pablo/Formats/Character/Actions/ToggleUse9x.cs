using System;
using Eto.Forms;
using Pablo.Network;
using Lidgren.Network;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class ToggleUse9x : CheckCommand, ICommand, IDisposable
	{
		CharacterHandler handler;
		public const string ActionID = "character_ToggleUse9x";

		public ToggleUse9x(CharacterHandler handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.MenuText = "Use &9px Font";
			this.ToolBarText = "9px";
			this.ToolTip = "Toggle the 9th pixel to emulate text mode";
			
			if (handler.Document.EditMode || handler.Client != null)
			{
				this.Checked = handler.CharacterDocument.Use9x;
			}
			else
				this.Checked = handler.Info.Use9x;
			
			if (handler.Client != null)
			{
				this.Enabled = handler.Client.CurrentUser.Level >= this.Level;
				
				handler.CharacterDocument.Use9xChanged += CharacterDocument_Use9xChanged;
			}
		}

		void CharacterDocument_Use9xChanged(object sender, EventArgs e)
		{
			Checked = handler.CharacterDocument.Use9x;
		}

		public void Dispose()
		{
			handler.CharacterDocument.Use9xChanged -= CharacterDocument_Use9xChanged;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (handler.Client != null)
			{
				handler.Client.SendCommand(this);
			}
			else
			{
				Do(Checked);
			}
		}

		void Do(bool val)
		{
			if (handler.Document.EditMode || handler.Client != null)
				handler.CharacterDocument.Use9x = val;
			else
				handler.Info.Use9x = val;
			Checked = val;
		}

		public int CommandID
		{
			get { return (int)NetCommands.Toggle9x; }
		}

		public UserLevel Level
		{
			get { return UserLevel.Operator; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod
		{
			get { return NetDeliveryMethod.ReliableOrdered; }
		}

		public bool Send(Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write(this.Checked);
			return true;
		}

		public void Receive(Pablo.Network.ReceiveCommandArgs args)
		{
			var use9x = args.Message.ReadBoolean();
			args.Invoke(delegate
			{
				Do(use9x);
			});
		}
	}
}

