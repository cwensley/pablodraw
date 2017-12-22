using System;
using Eto.Forms;
using Eto;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;

namespace Pablo.Formats.Rip.Actions
{
	public class Redo : PabloCommand
	{
		RipHandler handler;
		public const string ActionID = "rip_redo";
		
		public Redo (RipHandler handler)
			: base(handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.MenuText = "Redo";
			this.ToolBarText = "Redo";
			this.ToolTip = "re-applies the last command removed via Undo";
			if (EtoEnvironment.Platform.IsMac)
				this.Shortcut = PabloCommand.CommonModifier | Keys.Shift | Keys.Z; // TODO: Shortcut , PabloCommand.CommonModifier | Keys.Y };
			else
				this.Shortcut = PabloCommand.CommonModifier | Keys.Y; //, PabloCommand.CommonModifier | Keys.Shift | Keys.Z };
		}
		
		IEnumerable<RipCommand> commandsToSend;

		protected override void Execute (CommandExecuteArgs args)
		{
			Do (RedoCommands());
		}
		

		IEnumerable<RipCommand> RedoCommands()
		{
			do {
				var command = handler.RedoBuffer.Pop ();
				
				if (command != null) {
					yield return command;
					//if (command.UndoPoint)
					//	break;
				} else
					break;
				
				command = handler.RedoBuffer.Peek ();
				if (command == null || command.UndoPoint)
					break;
		
			} while (true);
		}
		
		void Do (IEnumerable<RipCommand> redoCommands)
		{
			var updates = new List<Rectangle> ();
			foreach (var command in redoCommands) {
				handler.AddCommand (command, updates, true);
				
			}
			handler.FlushCommands (updates);
		}

		public override int CommandID {
			get { return (int)NetCommands.Redo; }
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			if (commandsToSend != null)
				args.Message.Write (commandsToSend);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			args.Invoke (delegate {
				if (args.IsServer) {
					// server will send an undo to all clients
					var server = args.Network as Network.Server;
					commandsToSend = RedoCommands ().ToList();
					server.SendCommand (this, null, null, args.User);
				} else {
					Do (args.Message.ReadCommands (handler.RipDocument));
				}
			});
			
		}
		
	}
}

