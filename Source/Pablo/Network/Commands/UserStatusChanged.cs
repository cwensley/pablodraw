using System;
using Eto;
using System.Linq;
using Eto.Forms;

namespace Pablo.Network.Commands
{
	public enum UserStatus
	{
		Connected,
		Disconnected,
		LevelChange,
		AliasChange,
		Kicked,
		Initialize
	}
	
	public class UserStatusChanged : ICommand
	{
		
		public int CommandID {
			get { return (int)NetCommands.UserStatusChanged; }
		}
		
		public UserLevel Level {
			get { return UserLevel.Viewer; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		public User User { get; set; }
		
		public UserStatus Status { get; set; }
		
		public bool Send (SendCommandArgs args)
		{
			User.Send (args);
			args.Message.WriteVariableInt32 ((int)Status);
			return true;
		}
		
		public void Receive (ReceiveCommandArgs args)
		{
			var user = new User ();
			user.Receive (args);
			var status = (UserStatus)args.Message.ReadVariableInt32 ();
			if (args.Network != null) {
				var forMe = args.CurrentUser != null && user.Key == args.CurrentUser.Key;
				var server = args.Network as Server;
				var client = /*server != null ? server.Client :*/ args.Network as Client;
				args.Network.Invoke (delegate {
					string statusMessage = null;
					switch (status) {
					case UserStatus.Initialize:
						if (client != null)
							client.CurrentUser = user;
						break;
					case UserStatus.Connected:
						statusMessage = string.Format ("{0} ({1}) joined the session", user.Alias, user.HostName);
						args.Network.Users.Add (user);
						break;
					case UserStatus.Kicked:
						if (args.User.Level >= UserLevel.Operator) {
							statusMessage = string.Format ("{0} was kicked by {1}", user, args.User);
							if (server != null)
								server.KickUser (user, args.User);
							args.Network.Users.RemoveAll (r => r.Key == user.Key);
						}
						break;
					case UserStatus.Disconnected:
						if (args.Network.Users.RemoveAll (r => r.Key == user.Key) > 0)
							statusMessage = string.Format ("{0} left", user.Alias);
						break;
					case UserStatus.LevelChange:
						if (args.User.Level >= UserLevel.Operator) {
							switch (user.Level) {
							default:
							case UserLevel.Editor:
							case UserLevel.Operator:
								if (forMe) 
									statusMessage = string.Format ("you have been made an {1} by {2}", user.Alias, user.Level, args.User);
								else
									statusMessage = string.Format ("{0} has been made an {1} by {2}", user.Alias, user.Level, args.User);
								break;
							case UserLevel.Viewer:
								if (forMe) 
									statusMessage = string.Format ("you have been made a {1} by {2}", user.Alias, user.Level, args.User);
								else
									statusMessage = string.Format ("{0} has been made a {1} by {2}", user.Alias, user.Level, args.User);
								break;
							}
							args.Network.Users.RemoveAll (r => r.Key == user.Key);
							args.Network.Users.Add (user);
							if (forMe && client != null)
								client.CurrentUser = user;
							if (server != null)
								server.ResendMessage (args.Message, args.User);
						}
						break;
					case UserStatus.AliasChange:
						var olduser = args.Network.Users.FirstOrDefault (r => r.Key == user.Key);
						statusMessage = string.Format ("{0} is now known as {1}", olduser.Alias, user.Alias);
						args.Network.Users.Remove (olduser);
						args.Network.Users.Add (user);
						break;
					default:
						throw new ArgumentOutOfRangeException ();
					}
					args.Network.OnUsersChanged (EventArgs.Empty);
					if (client != null && !string.IsNullOrEmpty (statusMessage))
						client.OnMessage (new ClientMessageArgs (statusMessage));
					
				});
			}
			//Console.WriteLine ("User Connected: {0}", this.User);
		}
	}
}

