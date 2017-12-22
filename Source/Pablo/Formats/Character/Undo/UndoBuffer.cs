using System;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;
using Pablo.Network;

namespace Pablo.Formats.Character.Undo
{
	public class UndoBuffer : UndoItem
	{
		public Point? CursorLocation { get; set; }

		public Point? NextLocation { get; set; }
		
		public List<IUndoItem> Items { get; private set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoBuffer; }
		}
		
		public UndoBuffer ()
		{
			Items = new List<IUndoItem> ();
		}

		public void Add (params IUndoItem[] items)
		{
			foreach (var item in items) {
				Items.Add (item);
			}
		}
		
		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			var buffer = new UndoBuffer{ CursorLocation = this.NextLocation, NextLocation = this.CursorLocation };
			foreach (var item in Items) {
				buffer.Add (item.Reciprocal (handler));
			}
			return buffer;
		}
		
		public override void Apply (CharacterHandler handler)
		{
			foreach (var r in Items) {
				r.Apply (handler);
			}
			if (CursorLocation != null)
				handler.SetCursorPosition (CursorLocation.Value, true);
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			
			/*var hasLocation = args.Message.ReadBoolean ();
			CursorLocation = hasLocation ? (Point?)args.Message.ReadPoint() : null;
			
			var hasnextLocation = args.Message.ReadBoolean ();
			NextLocation = hasnextLocation ? (Point?)args.Message.ReadPoint () : null;*/
			
			Items.Clear ();
			var count = args.Message.ReadInt32 ();
			for (int i = 0; i<count; i++) {
				var type = (UndoType)Enum.ToObject (typeof(UndoType), args.Message.PeekInt32 ());
				var item = UndoManager.CreateUndoItem (type);
				if (item != null) {
					item.Receive (args);
					Items.Add (item);
				}
			}
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			/*
			args.Message.Write (CursorLocation != null);
			if (CursorLocation != null) args.Message.Write (CursorLocation.Value);

			args.Message.Write (NextLocation != null);
			if (NextLocation != null) args.Message.Write (NextLocation.Value);
			*/
			
			args.Message.Write (Items.Count);
			for (int i=0; i < Items.Count; i++) {
				Items [i].Send (args);
			}
			return true;
		}
	}
}

