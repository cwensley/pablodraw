using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats;
using Eto;
using Pablo.Network;
using Lidgren.Network;

namespace PabloDraw.Console.Commands
{
	public class EditFile : ICommand
	{
		IClientDelegate clientDelegate;
		
		public EditFile (IClientDelegate clientDelegate)
		{
			this.clientDelegate = clientDelegate;
		}
		
		public UserLevel Level { get { return UserLevel.Operator; } }
		
		public int CommandID { get { return (int)NetCommands.EditFile; } }
		
		public NetDeliveryMethod DeliveryMethod { get { return NetDeliveryMethod.ReliableOrdered; } }
		
		public bool Send (SendCommandArgs args)
		{
			if (!clientDelegate.Document.Info.CanEdit)
				return false;
			args.Message.Write (!clientDelegate.Document.EditMode);
			return true;
		}

		public void Receive (ReceiveCommandArgs args)
		{
			var editMode = args.Message.ReadBoolean ();
			args.Invoke (delegate {
				clientDelegate.Document.EditMode = editMode;
				var client = args.Network as Client;
				if (client != null && args.User != null) {
					if (clientDelegate.Document.EditMode)
						client.OnMessage (new ClientMessageArgs (string.Format ("{0} started editing", args.User)));
					else
						client.OnMessage (new ClientMessageArgs (string.Format ("{0} ended editing", args.User)));
				}
			});
		}
	}
}
