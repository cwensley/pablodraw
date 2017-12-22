using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Collections.Concurrent;
using System.Linq;
using Eto.Forms;

namespace Pablo.Network
{
	public abstract class Network
	{
		IDictionary<int, ICommand> commands;
		List<User> users = new List<User> ();
		
		protected virtual void OnCommandsUpdated (EventArgs e)
		{
		}
		
		public IDictionary<int, ICommand> Commands {
			get { 
				if (commands == null) {
					commands = new Dictionary<int, ICommand> ();
					OnCommandsUpdated (EventArgs.Empty);
				}
				return commands;
			}
		}
		
		public void SetCommands (IEnumerable<ICommand> commands)
		{
			this.commands = commands.ToDictionary (r => r.CommandID);
			OnCommandsUpdated (EventArgs.Empty);
		}
		
		public int Port { get; set; }
		
		public IClientDelegate Delegate { get; set; }
		
		protected abstract NetPeer NetPeer {
			get;
		}

		static object sync = new object();

		public void Invoke (Action action)
		{
			var app = Application.Instance;
			if (app != null)
				app.Invoke (action);
			else lock (sync)
				action ();
		}

		public string Password { get; set; }

		public List<User> Users {
			get { return users; }
			set { 
				users = value;
				OnUsersChanged (EventArgs.Empty);
			}
		}
		
		public abstract User CurrentUser {
			get;
			set;
		}
		
		public event EventHandler<EventArgs> UsersChanged;
		
		public virtual void OnUsersChanged (EventArgs e)
		{
			if (UsersChanged != null)
				UsersChanged (this, e);
		}
		
		public Network ()
		{
		}
	}
}

