using System;
using Eto.Forms;
using Lidgren.Network;
using System.Reflection;
using Eto.Drawing;
using System.Net;

namespace Pablo.Network
{
	public enum UserLevel
	{
		Viewer = 0,
		Editor,
		Operator
	}
	
	public class User : IImageListItem, INetworkReadWrite
	{
		public string Alias { get; set; }
		
		public string HostName { get; set; }
		
		public IPAddress IPAddress { get; set; }

		public Guid Key { get; set; }

		public bool Connected { get; set; }
		
		public User KickedBy { get; set; }

		public UserLevel Level { get; set; }

		#region IListItem implementation
		string IListItem.Text
		{
			get { return Alias; }
			set { }
		}

		string IListItem.Key {
			get {
				return Key.ToString ();
			}
		}

		#endregion
		
		public User()
		{
		}
		
		public User(User user)
		{
			this.Alias = user.Alias;
			this.Key = user.Key;
			this.IPAddress = user.IPAddress;
			this.HostName = user.HostName;
			this.Connected = user.Connected;
			this.Level = user.Level;
		}
		
		public bool Send (SendCommandArgs args)
		{
			args.Message.Write (Key.ToString ());
			args.Message.Write (Alias);
			args.Message.Write (HostName);
			//args.Message.Write (IPAddress);
			args.Message.WriteVariableInt32 ((int)Level);
			return true;
		}
		
		public void Receive (ReceiveCommandArgs args)
		{
			Key = new Guid (args.Message.ReadString ());
			Alias = args.Message.ReadString ();
			HostName = args.Message.ReadString ();
			//IPAddress = args.Message.ReadIPAddress();
			Level = (UserLevel)args.Message.ReadVariableInt32 ();
		}
		
		public override string ToString ()
		{
			return this.Alias;
		}
		
		public override int GetHashCode ()
		{
			return this.Alias.GetHashCode ();
		}
		
		public override bool Equals (object obj)
		{
			var user = obj as User;
			if ((object)user == null) return false;
			return (user.Key == this.Key);
		}
		
		public static bool operator==(User user1, User user2)
		{
			if ((object)user1 == null) return false;
			return user1.Equals (user2);
		}
		
		public static bool operator!=(User user1, User user2)
		{
			if ((object)user1 == null) return false;
			return !user1.Equals (user2);
		}
		
		static Image operatorImage;
		public static Image OperatorImage 
		{
			get {
				if (operatorImage == null)
					operatorImage = Bitmap.FromResource ("Pablo.Icons.operator.png");
				return operatorImage;
			}
		}

		static Image editorImage;
		public static Image EditorImage 
		{
			get {
				if (editorImage == null)
					editorImage = Bitmap.FromResource ("Pablo.Icons.editor.png");
				return editorImage;
			}
		}

		static Image viewerImage;
		public static Image ViewerImage 
		{
			get {
				if (viewerImage == null)
					viewerImage = Bitmap.FromResource ("Pablo.Icons.viewer.png");
				return viewerImage;
			}
		}
		
		public Image Image {
			get {
				switch (Level) {
				case UserLevel.Operator:
					return OperatorImage;
				case UserLevel.Editor:
					return EditorImage;
				default:
				case UserLevel.Viewer:
					return ViewerImage;
				}
			}
		}
	}
}

