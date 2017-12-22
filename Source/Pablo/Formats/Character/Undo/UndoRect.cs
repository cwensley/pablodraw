using System;
using Eto.Drawing;
using Pablo.Network;

namespace Pablo.Formats.Character.Undo
{
	public class UndoRect : UndoItem
	{
		public Point Location { get; set; }
		
		public Canvas Canvas { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoRect; }
		}
		
		
		public Rectangle Rectangle {
			get { return new Rectangle (Location, Canvas.Size); }
		}
		
		public UndoRect()
		{
		}
		
		public UndoRect(CharacterHandler handler, Rectangle rect)
		{
			var nrect = rect;
			nrect.Normalize ();
			nrect.Restrict (new Rectangle (handler.CurrentPage.Canvas.Size));
			Location = nrect.TopLeft;
			Canvas = handler.CurrentPage.Canvas.Copy (nrect);
		}
		
		public override IUndoItem Reciprocal(CharacterHandler handler)
		{
			return new UndoRect{ 
				Location = this.Location, 
				Canvas = handler.CurrentPage.Canvas.Copy (Rectangle)
			};
		}
		
		public override void Apply (CharacterHandler handler)
		{
			handler.CurrentPage.Canvas.Set (Location, Canvas);
			handler.InvalidateCharacterRegion (this.Rectangle, true);
		}

		#region INetworkReadWrite implementation
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.Write (Location);
			var characterDocument = args.Network.Delegate.Document as CharacterDocument;
			args.Message.Write (Canvas, characterDocument.Pages[0].Palette);
			return true;
		}

		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Location = args.Message.ReadPoint();
			var characterDocument = args.Document as CharacterDocument;
			Canvas = args.Message.ReadCanvas (characterDocument.Pages[0].Palette);
		}
		
		#endregion
	}
}

