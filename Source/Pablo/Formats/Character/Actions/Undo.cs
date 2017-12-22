using System;
using Eto.Forms;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class Undo : PabloCommand
	{
		public const string ActionID = "character_undo";
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public Undo (CharacterHandler handler)
			: base(handler)
		{
			this.ID = ActionID;
			this.MenuText = "&Undo";
			this.ToolTip = "Undo the last action";
			this.Shortcut = CommonModifier | Keys.Z;
		}
		
		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing && Handler.Undo.CanUndo && base.Enabled; }
			set { base.Enabled = value; }
		}
		
		public override int CommandID {
			get { return (int)NetCommands.Undo; }
		}
		
		public override Pablo.Network.UserLevel Level {
			get { return Pablo.Network.UserLevel.Operator; }
		}

		protected override void Execute (CommandExecuteArgs args)
		{
			Handler.Undo.Undo ();
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Handler.Undo.Undo(args);
		}
	}
}

