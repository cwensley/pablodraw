using System;
using Eto.Drawing;
using Pablo.Network;

namespace Pablo.Formats.Character.Undo
{
	public class UndoColour : UndoItem
	{
		public Color Color { get; set; }
		
		public int Index { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoColour; }
		}
		
		public UndoColour ()
		{
		}

		#region IUndoItem implementation
		
		public override void Apply (CharacterHandler handler)
		{
			var pal = handler.CurrentPage.Palette;
			if (Index < pal.Count)
				pal [Index] = Color;
		}

		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			return new UndoColour {
				Index = Index,
				Color = handler.CurrentPage.Palette [Index]
			};
		}
		
		#endregion

		#region INetworkReadWrite implementation
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.Write (Color);
			args.Message.WriteVariableInt32 (Index);
			return true;
		}

		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Color = args.Message.ReadColor ();
			Index = args.Message.ReadVariableInt32 ();
		}
		#endregion
	}
}

