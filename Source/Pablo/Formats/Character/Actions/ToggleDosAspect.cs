using System;
using Eto.Forms;
using Pablo.Network;
using Lidgren.Network;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class ToggleDosAspect : CheckAction, ICommand
	{
		readonly CharacterHandler handler;
		public const string ActionID = "character_ToggleDosAspect";

		public ToggleDosAspect(CharacterHandler handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.Text = "Emulate &Legacy Aspect|Aspect|Stretch image vertically to emulate legacy display";
			
			if (handler.Document.EditMode || handler.Client != null)
			{
				this.Checked = handler.CharacterDocument.DosAspect;
			}
			else
				this.Checked = handler.Info.DosAspect;
			
			if (handler.Client != null)
			{
				this.Enabled = handler.Client.CurrentUser.Level >= this.Level;
				
				handler.CharacterDocument.DosAspectChanged += new EventHandler<EventArgs>(delegate
				{
					this.Checked = handler.CharacterDocument.DosAspect;
				}).MakeWeak(e => handler.CharacterDocument.DosAspectChanged -= e);
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			if (handler.Client != null)
			{
				handler.Client.SendCommand(this);
			}
			else
			{
				Do(!Checked);
			}
		}

		void Do(bool val)
		{
			if (handler.Document.EditMode || handler.Client != null)
				handler.CharacterDocument.DosAspect = val;
			else
				handler.Info.DosAspect = val;
			Checked = val;
		}

		public int CommandID
		{
			get { return (int)NetCommands.ToggleDosAspect; }
		}

		public UserLevel Level
		{
			get { return UserLevel.Operator; }
		}

		public NetDeliveryMethod DeliveryMethod
		{
			get { return NetDeliveryMethod.ReliableOrdered; }
		}

		public bool Send(SendCommandArgs args)
		{
			args.Message.Write(!Checked);
			return true;
		}

		public void Receive(ReceiveCommandArgs args)
		{
			var useDosAspect = args.Message.ReadBoolean();
			args.Invoke(delegate
			{
				Do(useDosAspect);
			});
		}
	}
}

