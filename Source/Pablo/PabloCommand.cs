using System;
using Eto.Forms;
using Pablo.Network;
using Lidgren.Network;
using System.Collections.Generic;

namespace Pablo
{
	public class CommandExecuteArgs : EventArgs
	{
	}

	public interface IClientSource
	{
		Client Client { get; }
	}

	public class GenerateCommandArgs
	{
		public Control Control { get; set; }
		public bool EditMode { get; set; }
		public string Area { get; set; }
		public MenuBar Menu { get; set; }
		public ToolBar ToolBar { get; set; }
		public List<Command> KeyboardCommands { get; private set; }

		public GenerateCommandArgs(GenerateCommandArgs args)
		{
			Control = args.Control;
			EditMode = args.EditMode;
			Area = args.Area;
			Menu = args.Menu;
			ToolBar = args.ToolBar;
			KeyboardCommands = args.KeyboardCommands;
		}

		public GenerateCommandArgs()
		{
			Menu = new MenuBar();
			ToolBar = new ToolBar();
			KeyboardCommands = new List<Command>();
		}
	}

	public abstract class PabloCommand : Command, ICommand
	{
		readonly IClientSource handler;

		public override bool Enabled
		{
			get
			{
				return base.Enabled && handler != null && (handler.Client == null || handler.Client.CurrentUser.Level >= Level);
			}
			set
			{
				base.Enabled = value;
			}
		}

		public static Keys CommonModifier
		{
			get
			{
				return Application.Instance != null ? Application.Instance.CommonModifier : Keys.Control;
			}
		}

		public static Keys AlternateModifier
		{
			get
			{
				return Application.Instance != null ? Application.Instance.AlternateModifier : Keys.Alt;
			}
		}

		protected PabloCommand(IClientSource handler)
		{
			this.handler = handler;
		}

		public string Name { get; set; }

		public IClientSource Handler
		{
			get { return handler; }
		}

		protected abstract void Execute(CommandExecuteArgs args);

		protected sealed override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (Handler != null && Handler.Client != null)
			{
				Handler.Client.SendCommand(this);
			}
			else
				Execute(new CommandExecuteArgs());
		}

		public virtual UserLevel Level
		{
			get { return UserLevel.Operator; }
		}

		#region ICommand implementation

		public abstract int CommandID { get; }

		public virtual NetDeliveryMethod DeliveryMethod
		{
			get { return NetDeliveryMethod.ReliableOrdered; }
		}

		#endregion

		#region INetworkReadWrite implementation

		public virtual bool Send(SendCommandArgs args)
		{
			return false;
		}

		public virtual void Receive(ReceiveCommandArgs args)
		{
		}

		#endregion

	}
}

