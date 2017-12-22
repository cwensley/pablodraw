using System;
using Eto.Forms;
using Pablo.Network;
using Lidgren.Network;

namespace Pablo
{
	public class CommandExecuteArgs : EventArgs
	{
		
	}

	public interface IClientSource
	{
		Client Client { get; }
	}

	public abstract class Command : ButtonAction, ICommand
	{
		IClientSource handler;
		
		public override bool Enabled {
			get {
				return base.Enabled && handler != null && (handler.Client == null || handler.Client.CurrentUser.Level >= Level);
			}
			set {
				base.Enabled = value;
			}
		}

		public static Key CommonModifier {
			get {
				if (Application.Instance != null)
					return Application.Instance.CommonModifier;
				else
					return Key.Control;
			}
		}

		public static Key AlternateModifier {
			get {
				if (Application.Instance != null)
					return Application.Instance.AlternateModifier;
				else
					return Key.Alt;
			}
		}

		public Command (IClientSource handler)
		{
			this.handler = handler;
		}
				
		public string Name { get; set; }
		
		public IClientSource Handler {
			get { return handler; }
		}
		
		protected abstract void Execute (CommandExecuteArgs args);

		protected sealed override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			
			if (Handler.Client != null) {
				Handler.Client.SendCommand(this);
			}
			else
				Execute (new CommandExecuteArgs ());
		}
		
		public virtual UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		#region ICommand implementation
		public abstract int CommandID { get; }
		
		public virtual NetDeliveryMethod DeliveryMethod {
			get { return NetDeliveryMethod.ReliableOrdered; }
		}
		#endregion

		#region INetworkReadWrite implementation
		public virtual bool Send (SendCommandArgs args)
		{
			return false;
		}

		public virtual void Receive (ReceiveCommandArgs args)
		{
		}
		#endregion
	}
}

