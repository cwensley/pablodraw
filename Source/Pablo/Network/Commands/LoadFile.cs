using System;
using Lidgren.Network;
using System.Linq;
using Eto;
using System.IO;
using Eto.Forms;

namespace Pablo.Network.Commands
{
	public class LoadFile : ICommand
	{
		public Network Network { get; private set; }
		
		public LoadFile (Network network)
		{
			this.Network = network;
		}
		
		public UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		public Stream Stream { get; set; }
		
		public string FileName { get; set; }
		
		public bool EditMode { get; set; }
		
		public Format Format { get; set; }
		
		public bool Send (SendCommandArgs args)
		{
			args.Message.Write (Path.GetFileName (FileName));
			args.Message.Write (Format.Info.ID);
			args.Message.Write (Format.ID);
			args.Message.Write (EditMode);
			args.Message.WritePadBits ();
			args.Message.WriteStream (Stream);
			return true;
		}
		
		public void Receive (ReceiveCommandArgs args)
		{
			var fileName = args.Message.ReadString ();
			var documentTypeId = args.Message.ReadString ();
			var formatId = args.Message.ReadString ();
			var editMode = args.Message.ReadBoolean ();
			args.Message.ReadPadBits ();
			var stream = args.Message.ReadStream ();

			DocumentInfo docinfo;
			if (Network.Delegate.DocumentInfos.TryGetValue (documentTypeId, out docinfo)) {
				Format format;
				if (docinfo.Formats.TryGetValue (formatId, out format)) {
					args.Network.Invoke (delegate {
						var client = args.Network as Client;
						if (client != null) {
							if (args.User != null) client.OnMessage(new ClientMessageArgs(string.Format ("{0} loaded {1}", args.User, fileName)));
							else client.OnMessage(new ClientMessageArgs(string.Format ("loaded {0}", fileName)));
						}
						Network.Delegate.LoadFile (fileName, stream, editMode, format);
					});
				}
			}
		}
		
		public int CommandID {
			get { return (int)NetCommands.LoadFile; }
		}
	}
}

