using System;
using Lidgren.Network;
using System.Linq;
using Eto;
using Eto.Forms;

namespace Pablo.Network.Commands
{
	public class LoadDocument : ICommand
	{
		public LoadDocument ()
		{
		}
		
		public Document Document { get; set; }
		
		public UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		
		public bool Send (SendCommandArgs args)
		{
			args.Message.Write(Document != null);
			if (Document != null) {
				args.Message.Write (Document.Info.ID);
				Document.Send (args);
			}
			return true;
		}
		
		public void Receive (ReceiveCommandArgs args)
		{
			var hasdoc = args.Message.ReadBoolean ();
			if (hasdoc) {
				
				var typeId = args.Message.ReadString ();
				DocumentInfo type;
				if (args.Network.Delegate.DocumentInfos.TryGetValue (typeId, out type)) {
					var doc = type.Create ();
					doc.Receive (args);
					args.Network.Invoke (delegate {
						var client = args.Network as Client;
						if (client != null) {
							if (args.User != null) client.OnMessage(new ClientMessageArgs(string.Format ("{0} loaded {1}", args.User, doc.FileName)));
							else client.OnMessage(new ClientMessageArgs(string.Format ("loaded {0}", doc.FileName)));
						}
						args.Network.Delegate.SetDocument(doc);
					});
				}
			}
			else {
				args.Invoke (delegate {
					args.Network.Delegate.SetDocument(null);
				});
			}
		}
		
		public int CommandID {
			get { return (int)NetCommands.LoadDocument; }
		}
	}
}

