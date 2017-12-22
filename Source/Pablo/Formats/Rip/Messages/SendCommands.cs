using System;
using Pablo.Network;
using System.Collections.Generic;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Formats.Rip.Messages
{
	public class SendCommands : ICommand
	{
		List<RipCommand> commands = new List<RipCommand>();
		RipHandler handler;
		
		public List<RipCommand> Commands
		{
			get { return commands; }
		}
		
		public int CommandID {
			get { return (int)NetCommands.SendCommands; }
		}

		public UserLevel Level {
			get { return UserLevel.Editor; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		public SendCommands (RipHandler handler)
		{
			this.handler = handler;
		}

		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (Commands);
			return true;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			var commands = new List<RipCommand>();
			args.Message.ReadCommands(handler.RipDocument, commands);
			handler.Document.IsModified = true;
			args.Invoke (delegate { 
				var updates = new List<Rectangle>();
				if (handler.SelectedTool != null)
					handler.SelectedTool.RemoveDrawing(updates);
				foreach (var cmd in commands) {
					var optional = cmd as RipOptionalCommand;
					if (optional != null && !optional.ShouldApply (handler.BGI))
						continue;
					handler.RipDocument.Commands.Add (cmd);
					cmd.Apply (updates);
				}
				handler.RedoBuffer.Clear ();
				if (handler.SelectedTool != null)
					handler.SelectedTool.ApplyDrawing(updates);
				handler.BGI.UpdateRegion (updates);
			});
		}
	}
}

