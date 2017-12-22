using System;
using Eto.Forms;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class Redo : PabloCommand
	{
		public const string ActionID = "character_redo";

		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }

		public Redo(CharacterHandler handler)
			: base(handler)
		{
			this.ID = ActionID;
			this.MenuText = "&Redo";
			this.ToolTip = "Redo the last action";
			if (EtoEnvironment.Platform.IsMac)
				this.Shortcut = CommonModifier | Keys.Shift | Keys.Z; // new [] { CommonModifier | Keys.Shift | Keys.Z, CommonModifier | Keys.Y };
			else
				this.Shortcut = CommonModifier | Keys.Y; // TODO: Shortcut new [] { CommonModifier | Keys.Y, CommonModifier | Keys.Shift | Keys.Z };
		}

		public override bool Enabled
		{
			get { return Handler.AllowKeyboardEditing && Handler.Undo.CanRedo && base.Enabled; }
			set { base.Enabled = value; }
		}

		public override int CommandID
		{
			get { return (int)NetCommands.Redo; }
		}

		public override Pablo.Network.UserLevel Level
		{
			get { return Pablo.Network.UserLevel.Operator; }
		}

		protected override void Execute(CommandExecuteArgs args)
		{
			Handler.Undo.Redo();
		}

		public override bool Send(Pablo.Network.SendCommandArgs args)
		{
			base.Send(args);
			return true;
		}

		public override void Receive(Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive(args);
			Handler.Undo.Redo(args);
		}
	}
}

