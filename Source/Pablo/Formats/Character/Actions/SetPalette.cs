using System;
using Pablo.Network;
using Eto.Drawing;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class SetPalette : ICommand
	{
		public CharacterHandler Handler { get; private set; }

		public Palette Palette { get; set; }
		
		public SetPalette (CharacterHandler handler)
		{
			this.Handler = handler;
		}

		public int CommandID {
			get { return (int)NetCommands.SetPalette; }
		}

		public UserLevel Level {
			get { return UserLevel.Editor; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (Palette);
			return true;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			Palette = args.Message.ReadPalette ();

			args.Invoke (delegate {
				Handler.CurrentPage.Palette = Palette;
				Handler.Document.IsModified = true;
			});
		}
	}
}

