using System;
using Eto;
using System.Linq;
using Lidgren.Network;
using Eto.Forms;

namespace Pablo.Network.Commands
{
	public class ChatMessage : ICommand
	{
		public int CommandID {
			get { return (int)NetCommands.ChatMessage; }
		}
		
		public UserLevel Level {
			get { return UserLevel.Viewer; }
		}
		
		public NetDeliveryMethod DeliveryMethod {
			get { return NetDeliveryMethod.ReliableUnordered; }
		}

		public Network Network { get; private set; }
		
		public string Message { get; set; }

		public ChatMessage (Network network)
		{
			this.Network = network;
		}

		public bool Send (SendCommandArgs args)
		{
			args.Message.Write (Message);
			return true;
		}

		public void Receive (ReceiveCommandArgs args)
		{
			Message = args.Message.ReadString ();
			var client = Network as Client;
			if (client != null)
			args.Network.Invoke (delegate {
				client.OnMessage (new ClientMessageArgs (Message, args.User));
			});
		}

		
	}
}

