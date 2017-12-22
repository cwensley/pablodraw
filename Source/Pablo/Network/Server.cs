using System;
using System.Net;
using System.Collections.Generic;
using Lidgren.Network;
using System.Threading;
using Pablo.Network.Commands;
using System.Linq;
using Mono.Nat;
using System.Reflection;

#if DEBUG
//#define SIMULATE_LATENCY
#endif
namespace Pablo.Network
{
	public class Server : Network
	{
		public const int VERSION = 7;
		bool mappedPort;
		NetServer server;
		static INatDevice natdevice;

		public Client Client { get; set; }

		public UserLevel DefaultUserLevel { get; set; }

		public string OperatorPassword { get; set; }

		public IPAddress ExternalIP { get; set; }

		public bool AutoMapPort { get; set; }

		public override User CurrentUser
		{
			get { return Client.CurrentUser; }
			set { Client.CurrentUser = value; }
		}

		public Server()
		{
			SetCommands(Enumerable.Empty<ICommand>());
		}

		protected override void OnCommandsUpdated(EventArgs e)
		{
			base.OnCommandsUpdated(e);
			Commands.Add(new UserStatusChanged());
		}

		protected override NetPeer NetPeer
		{
			get { return server; }
		}

		public void KickUser(User user, User byuser)
		{
			var conn = server.Connections.FirstOrDefault(r =>
			{
				var u = r.Tag as User;
				if (u != null)
					return u.Key == user.Key;
				return false;
			});
			
			if (conn != null)
			{
				var connuser = (User)conn.Tag;
				connuser.KickedBy = byuser;
				conn.Disconnect(string.Format("You were kicked by {0}", byuser));
			}
		}

		public void Start()
		{
			var config = new NetPeerConfiguration("PabloDraw");
			config.Port = Port;
#if SIMULATE_LATENCY
			config.SimulatedMinimumLatency = 0.2f;
			config.SimulatedRandomLatency = 0.5f;
			config.SimulatedLoss = 0.01f;
#endif

			/*if (AutoMapPort)
				config.EnableUPnP = true;
				*/
			//config.EnableUPnP = true;
			config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
#if DEBUG
			config.EnableMessageType(NetIncomingMessageType.DebugMessage);
#endif
			server = new NetServer(config);

			
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			server.RegisterReceivedCallback(ReceiveCallback);
			server.Start();

			/**/
			if (AutoMapPort)
			{
				mappedPort = false;
				try
				{
					MapDevice();
				}
				catch
				{
					// eat errors here, will try again when a new device is found
					natdevice = null;
				}
				NatUtility.DeviceFound += HandleNatUtilityDeviceFound;
				NatUtility.DeviceLost += HandleDeviceLost;
				NatUtility.UnhandledException += HandleNatUtilityUnhandledException;
				NatUtility.StartDiscovery();
			}
			/**/

			/* Lidgren only supports UPnP, not NAT-PMP
			 *
			if (!server.UPnP.ForwardPort (this.Port, "PabloDraw"))
				Console.WriteLine ("Could not forward port!!");
			else
				Console.WriteLine ("Port succesfully forwarded!");
			/**/
		}

		void HandleDeviceLost(object sender, DeviceEventArgs e)
		{
			natdevice = null;
		}

		void HandleNatUtilityUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Invoke(delegate
			{
				var message = string.Format("Could not map port {0} using NAT-PMP or UPnP", Port);
				Client.OnMessage(new ClientMessageArgs(message, null));
			});
			//SendCommand(new ChatMessage(this) { Message = message});
		}

		void HandleNatUtilityDeviceFound(object sender, DeviceEventArgs e)
		{
			natdevice = e.Device;
			MapDevice();
		}

		void MapDevice()
		{
			if (natdevice == null)
				return;
			ExternalIP = natdevice.GetExternalIP();

			try
			{
				var mapping = new Mapping(Protocol.Udp, Port, Port);
				natdevice.CreatePortMap(mapping);

				if (!mappedPort)
				{
					Invoke(delegate
					{
						var message = string.Format("Succesfully mapped port {0} from external IP address {1} to this machine", Port, ExternalIP);
						Client.OnMessage(new ClientMessageArgs(message, null));
					});
					mappedPort = true;
				}
			}
			catch (Exception ex)
			{
				Invoke(delegate
				{
					Client.OnMessage(new ClientMessageArgs(ex.Message, null));
				});
			}
		}

		public void Stop()
		{
			if (server != null)
			{
				if (AutoMapPort)
				{
					NatUtility.StopDiscovery();
					NatUtility.DeviceFound -= HandleNatUtilityDeviceFound;
					NatUtility.UnhandledException -= HandleNatUtilityUnhandledException;
					NatUtility.DeviceLost -= HandleDeviceLost;

				}
				server.Shutdown("Server shutdown by host");
			}
		}

		public void SendCommand(ICommand command, NetConnection connection = null, NetConnection except = null, User user = null)
		{
			var message = server.CreateMessage();
			message.Write(user != null ? user.Key.ToString() : string.Empty);
			message.WriteVariableInt32(command.CommandID);
			var send = command.Send(new SendCommandArgs(message, this));
			if (send)
			{
				if (connection != null)
					server.SendMessage(message, connection, NetDeliveryMethod.ReliableOrdered);
				else
					server.SendToAll(message, except, NetDeliveryMethod.ReliableOrdered, 0);
			}
		}

		public void ResendMessage(NetIncomingMessage incoming, User user)
		{
			var outmsg = server.CreateMessage();
			outmsg.Write(user != null ? user.Key.ToString() : string.Empty);
			outmsg.Write(incoming);
			server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
		}

		void ReceiveCallback(object state)
		{
			var peer = (NetPeer)state;
			var message = peer.ReadMessage();
			var connection = message.SenderConnection;
			var user = (connection != null) ? connection.Tag as User : null;
			try
			{
			
				//Console.Write ("Server {0}", message.MessageType);
			
				switch (message.MessageType)
				{
					#if DEBUG
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
						Client.OnMessage(new ClientMessageArgs("SERVER:" + message.ReadString(), null));
						break;
					#endif
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						Client.OnMessage(new ClientMessageArgs(message.ReadString(), null));
						break;
					case NetIncomingMessageType.StatusChanged:
					//Console.Write (" Status: Peer:{0}, Connection:{1}", peer.Status, message.SenderConnection.Status);
						switch (message.SenderConnection.Status)
						{
					
							case NetConnectionStatus.Connected:
								if (user != null)
								{
									bool isCurrentUser = user.Key == Client.CurrentUser.Key;
									User[] usersArray;
							
									lock (Users)
									{
										if (!isCurrentUser || !Client.Headless)
											Users.Add(user);
										usersArray = Users.ToArray();
									}
									if (!isCurrentUser)
									{
										SendCommand(new UserStatusChanged { User = user, Status = UserStatus.Initialize }, connection);
										SendCommand(new LoadDocument { Document = Delegate != null ? Delegate.Document : null }, connection);
									}
									SendCommand(new UserList(this) { Users = usersArray }, connection);
									SendCommand(new UserStatusChanged { User = user, Status = UserStatus.Connected }, null, connection);
								}
								break;
							case NetConnectionStatus.Disconnected:
								if (user != null)
								{
									var users = Users;
									lock (users)
										users.Remove(user);
									if (user.KickedBy != null)
									{
										SendCommand(new UserStatusChanged{ User = user, Status = UserStatus.Kicked }, null, connection, user.KickedBy);
									}
									else
										SendCommand(new UserStatusChanged{ User = user, Status = UserStatus.Disconnected }, null, connection);
								}
								break;
						}
						break;
					case NetIncomingMessageType.ConnectionApproval:
						var version = message.ReadVariableInt32();
						var clientVersion = message.ReadString();
						var password = message.ReadString();
						if (version != Server.VERSION)
						{
							var serverVersion = Assembly.GetEntryAssembly().GetName().Version;
							connection.Deny(string.Format("Server is running version {0} (protocol v{1}), which is not the same as your client version {2} (protocol v{3})", serverVersion, Server.VERSION, clientVersion, version));
						}
						else
						{
							try
							{
								var requiresPassword = !string.IsNullOrEmpty(Password);
								var hasUserPassword = (!string.IsNullOrEmpty(Password) && password == Password);
								var hasAdminPassword = (!string.IsNullOrEmpty(OperatorPassword) && password == OperatorPassword);
								var hasPassword = hasUserPassword || hasAdminPassword;
								if (requiresPassword && !hasPassword)
								{
									if (string.IsNullOrEmpty(password))
										connection.Deny("A password is required to join this server");
									else
										connection.Deny("Password is incorrect");
								}
								else
								{

									IPHostEntry host = null;
									try
									{
										host = Dns.GetHostEntry(message.SenderEndPoint.Address);
									}
									catch
									{
									}
					
									user = new User();
									user.Connected = true;
									user.Receive(new ReceiveCommandArgs(message, this, null));
									user.IPAddress = message.SenderEndPoint.Address;
									if (host != null)
										user.HostName = host.HostName;
									user.Level = hasAdminPassword ? UserLevel.Operator : DefaultUserLevel;
									bool isCurrentUser = user.Key == Client.CurrentUser.Key;
									if (isCurrentUser)
										user = Client.CurrentUser;
									connection.Tag = user;

									if (!isCurrentUser)
									{
										var users = Users;

										lock (users)
										{
											if (users.Any(r => r.Alias == user.Alias))
											{
												connection.Deny("Alias is currently in use");
												return;
											}
										}
									}
									connection.Approve();
								}
							}
							catch
							{
								connection.Deny("There was a problem connecting to this server");
#if DEBUG
							throw;
#endif
							}
						}
						break;
					case NetIncomingMessageType.Data:

						int messageId = message.ReadVariableInt32();
						ICommand command;
						if (Commands.TryGetValue(messageId, out command))
						{
							command.Receive(new ReceiveCommandArgs(message, this, user));
						}
						else
						{
							// send to all clients
							ResendMessage(message, user);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Client.OnMessage(new ClientMessageArgs(string.Format("Server Error: {0}", ex), null));
#if DEBUG
				throw;
#endif
			}
			finally
			{
				server.Recycle(message);
			}
			//Console.WriteLine ();
		}
	}
}
