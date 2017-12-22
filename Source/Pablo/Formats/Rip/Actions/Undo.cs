using System;
using Eto.Forms;
using Eto;
using System.Linq;

namespace Pablo.Formats.Rip.Actions
{
	public class Undo : Command
	{
		RipHandler handler;
		public const string ActionID = "rip_undo";
		
		public Undo (RipHandler handler)
			: base(handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.MenuText = "Undo";
			this.ToolBarText = "Undo";
			this.TooltipText = "removes the last command and redraws the screen";
			this.Accelerator = Command.CommonModifier | Key.Z;
		}

		protected override void Execute (CommandExecuteArgs args)
		{
			Do ();
		}
		
		void Do ()
		{
			var commands = handler.RipDocument.Commands;
			do {
				var command = commands.LastOrDefault ();
				if (command != null) {
					handler.RedoBuffer.Push (command);
					commands.Remove (command);
					if (command.UndoPoint)
						break;
				} else
					break;
				
			} while (true);
			handler.Redraw ();
		}

		public override int CommandID {
			get { return (int)NetCommands.Undo; }
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			args.Invoke (delegate {
				if (args.IsServer) {
					// server will send an undo to all clients
					var server = args.Network as Network.Server;
					server.SendCommand (this, null, null, args.User);
				} else {
					Do ();
				}
			});
			
		}
		
	}
}

