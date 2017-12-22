using System;
using Lidgren.Network;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Pablo.Network.Commands
{
	public class UserList : ICommand
	{
		public Network Network { get; private set; }
		
		public IEnumerable<User> Users { get; set; }
		
		public UserLevel Level {
			get { return UserLevel.Viewer; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableSequenced; }
		}
		
		public UserList (Network network)
		{
			this.Network = network;
		}

		public bool Send (SendCommandArgs args)
		{
			args.Message.Write (this.Users.Count());
			foreach (var user in Users)
			{
				user.Send (args);
			}
			return true;
		}

		public void Receive (ReceiveCommandArgs args)
		{
			var count = args.Message.ReadInt32 ();
			var users = new List<User>();
			while (count > 0)
			{
				var user = new User();
				user.Receive (args);
				users.Add (user);
				count--;
			}
			Network.Users = users;
		}

		public int CommandID {
			get { return (int)NetCommands.UserList; }
		}
	}
}

