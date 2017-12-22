using System;
using Eto.Forms;
using Eto.Misc;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class Redo : Command
	{
		public const string ActionID = "character_redo";
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public Redo (CharacterHandler handler)
			: base(handler)
		{
			this.ID = ActionID;
			this.Text = "&Redo|Redo|Redo the last action";
			if (EtoEnvironment.Platform.IsMac)
				this.Accelerators = new Key[] { CommonModifier | Key.Shift | Key.Z, CommonModifier | Key.Y };
			else
				this.Accelerators = new Key[] { CommonModifier | Key.Y, CommonModifier | Key.Shift | Key.Z };
		}
		
		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}
		
		public override int CommandID {
			get { return (int)NetCommands.Redo; }
		}
		
		public override Pablo.Network.UserLevel Level {
			get { return Pablo.Network.UserLevel.Operator; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			Handler.Undo.Redo ();
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Handler.Undo.Redo (args);
		}
	}
}

