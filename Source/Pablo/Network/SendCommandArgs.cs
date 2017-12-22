using System;
using Lidgren.Network;
using Eto.Forms;

namespace Pablo.Network
{
	public class SendCommandArgs
	{
		public NetOutgoingMessage Message { get; private set; }
		
		public Network Network { get; private set; }

		public User CurrentUser
		{
			get { return Network.CurrentUser; }
		}

		static object sync = new object();
		
		public void Invoke (Action action)
		{
			if (Network != null)
				Network.Invoke (action);
			else if (Application.Instance != null)
				Application.Instance.Invoke (action);
			else lock (sync)
				action ();
		}

		public SendCommandArgs (NetOutgoingMessage message, Network network)
		{
			this.Message = message;
			this.Network = network;
		}
	}
}

