using System;
using System.Net;
using Lidgren.Network;
using System.Linq;
using System.Threading;
using System.IO;
using System.Reflection;

#if DEBUG
//#define SIMULATE_LATENCY
#endif
namespace Pablo.Network
{
	public class ClientMessageArgs : EventArgs
	{
		public User User { get; private set; }

		public string Message { get; private set; }

		public string DisplayMessage
		{
			get
			{
				if (User != null)
					return string.Format("[{0:t}] {1}: {2}", DateTime.Now, User.Alias, Message);
				else
					return Message;
			}
		}

		public ClientMessageArgs(string message, User user = null)
		{
			this.Message = message;
			this.User = user;
		}
	}

	public class DisconnectedArgs : EventArgs
	{
		public string Reason { get; set; }
	}

	public class Client : Network
	{
		NetClient client;
		User currentUser;

		public event EventHandler<EventArgs> CurrentUserChanged;

		protected virtual void OnCurrentUserChanged(EventArgs e)
		{
			if (CurrentUserChanged != null)
				CurrentUserChanged(this, e);
		}

		public bool Headless { get; set; }

		public string Hostname
		{
			get;
			set;
		}

		public override User CurrentUser
		{
			get { return currentUser; }
			set
			{
				currentUser = value;
				OnCurrentUserChanged(EventArgs.Empty);
			}
		}

		protected override NetPeer NetPeer
		{
			get
			{
				return client;
			}
		}

		protected override void OnCommandsUpdated(EventArgs e)
		{
			base.OnCommandsUpdated(e);
			this.Commands.Add(new Commands.UserStatusChanged());
			this.Commands.Add(new Commands.ChatMessage(this));
			this.Commands.Add(new Commands.UserList(this));
			this.Commands.Add(new Commands.LoadDocument());
			this.Commands.Add(new Commands.LoadFile(this));
		}

		public event EventHandler<ClientMessageArgs> Message;

		public virtual void OnMessage(ClientMessageArgs e)
		{
			if (Message != null)
				Message(this, e);
		}

		public event EventHandler<DisconnectedArgs> Disconnected;

		public virtual void OnDisconnected(DisconnectedArgs e)
		{
			if (Disconnected != null)
				Disconnected(this, e);
		}

		public void SetDocument(Document document)
		{
			SendCommand(new Commands.LoadDocument { Document = document });
		}

		public void SetFile(string fileName, Stream stream, Format format, bool editMode)
		{
			SendCommand(new Commands.LoadFile(this)
			{ 
				FileName = fileName,
				Stream = stream,
				Format = format,
				EditMode = editMode
			});
		}

		public void SendMessage(string message)
		{
			SendCommand(new Commands.ChatMessage(this){ Message = message });
		}

		public void SendCommand(ICommand command)
		{
			//Console.WriteLine ("User:{0}, needed:{1}", CurrentUser.Level, command.Level);
			if (CurrentUser.Level >= command.Level)
			{
				var message = client.CreateMessage();
				message.WriteVariableInt32(command.CommandID);
				var args = new SendCommandArgs(message, this);
				if (command.Send(args))
				{
					client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 1);
				}
			}
		}

		public Client()
		{
			this.CurrentUser = new User { Key = Guid.NewGuid() };
			SetCommands(Enumerable.Empty<ICommand>());
		}

		public void Start()
		{
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

			var config = new NetPeerConfiguration("PabloDraw");
			//config.Port = port;
#if SIMULATE_LATENCY
			config.SimulatedMinimumLatency = 0.2f;
			config.SimulatedRandomLatency = 0.5f;
			config.SimulatedLoss = 0.01f;
#endif
			
			#if DEBUG
			config.EnableMessageType(NetIncomingMessageType.DebugMessage);
			#endif
			client = new NetClient(config);
			
			var startupMessage = client.CreateMessage();
			startupMessage.WriteVariableInt32(Server.VERSION);
			var version = Assembly.GetEntryAssembly().GetName().Version;
			startupMessage.Write(version.ToString());
			startupMessage.Write(Password);
			CurrentUser.Send(new SendCommandArgs(startupMessage, this));

			client.RegisterReceivedCallback(ReceiveCallback);
			client.Start();
			if (!string.IsNullOrEmpty(Hostname))
				client.Connect(Hostname, Port, startupMessage);
			else
				client.Connect(new IPEndPoint(IPAddress.Loopback, Port), startupMessage);
		}

		readonly object locker = new object();
		void ReceiveCallback(object state)
		{
			var peer = (NetPeer)state;
			var message = peer.ReadMessage();
			//var connection = message.SenderConnection;
			//Console.Write ("Client: {0}", message.MessageType);
			
			switch (message.MessageType)
			{
				#if DEBUG
				case NetIncomingMessageType.VerboseDebugMessage:
				case NetIncomingMessageType.DebugMessage:
					OnMessage(new ClientMessageArgs("CLIENT:" + message.ReadString(), null));
					break;
				#endif
				case NetIncomingMessageType.WarningMessage:
				case NetIncomingMessageType.ErrorMessage:
					OnMessage(new ClientMessageArgs(message.ReadString(), null));
				//Console.Write (message.ReadString ());
					break;
				case NetIncomingMessageType.StatusChanged:
					var status = (NetConnectionStatus)message.ReadByte();
				//Console.Write (" Status: {0} ", status);
					switch (status)
					{
						case NetConnectionStatus.Connected:
							Invoke(delegate
							{
								OnMessage(new ClientMessageArgs(string.Format("Session began at {0:f}", DateTime.Now)));
							});
							break;
						case NetConnectionStatus.Disconnected:
					
							var reason = message.ReadString();
					//Console.Write (" Reason: {0}", reason);
							Invoke(delegate
							{
								OnDisconnected(new DisconnectedArgs{ Reason = reason });
							});
					
							break;
					}
					break;
				case NetIncomingMessageType.Data:
				
					if (message.DeliveryMethod == NetDeliveryMethod.ReliableOrdered)
					{
						lock (locker)
							ProcessMessage(message);
					}
					else
						ProcessMessage(message);

					break;
			}
			//Console.WriteLine ();
			client.Recycle(message);
			
		}

		void ProcessMessage(NetIncomingMessage message)
		{
			var userId = message.ReadString();
			User user = null;
			if (!string.IsNullOrEmpty(userId))
			{
				var userKey = new Guid(userId);
				user = Users.FirstOrDefault(r => r.Key == userKey);
			}
			
			var commandId = message.ReadVariableInt32();
			//Console.Write (" Message: {0}", commandId);
			ICommand command;
			if (Commands.TryGetValue(commandId, out command))
			{
				command.Receive(new ReceiveCommandArgs(message, this, user));
			}
		}

		public void Stop()
		{
			if (client != null)
			{
				//client.Disconnect ("Leaving...");
				client.Shutdown(string.Empty);
			}
		}
	}
}
