using System;
using Pablo.Network;
using Eto.Drawing;
using Eto;
using Pablo.Formats.Character.Undo;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class SetColour : ICommand
	{
		public CharacterHandler Handler { get; private set; }

		public Color Color { get; set; }

		public int Index { get; set; }
		
		public SetColour (CharacterHandler handler)
		{
			this.Handler = handler;
		}

		public int CommandID {
			get { return (int)NetCommands.SetColour; }
		}

		public UserLevel Level {
			get { return UserLevel.Editor; }
		}

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod {
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}
		
		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (Color);
			args.Message.WriteVariableInt32 (Index);
			return true;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			var color = args.Message.ReadColor ();
			var index = args.Message.ReadVariableInt32 ();
			
			args.Invoke (delegate {
				var pal = Handler.CurrentPage.Palette;
				if (index < pal.Count) {
					Handler.Undo.Save (new UndoColour{ Color = pal [index], Index = index });
					pal [index] = color;
					Handler.Document.IsModified = true;
				}
			});
		}
	}
}

