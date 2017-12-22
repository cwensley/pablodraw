using System;
using Lidgren.Network;
using Eto.Forms;

namespace Pablo.Network
{
	public class ReceiveCommandArgs
	{
		public NetIncomingMessage Message { get; private set; }
		
		public Network Network { get; private set; }
		
		public User User { get; private set; }
		
		public Document Document { get { return Network.Delegate.Document; } }

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
		
		public bool IsMe {
			get {
				return User != null && CurrentUser != null && User.Key == CurrentUser.Key;
			}
		}
		
		public bool IsServer {
			get { return Network is Server; }
		}
		
		public ReceiveCommandArgs (NetIncomingMessage message, Network network, User user)
		{
			this.Message = message;
			this.Network = network;
			this.User = user;
		}
	}
}

