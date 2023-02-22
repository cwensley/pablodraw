using System;
using Eto.Drawing;
using System.Collections.Generic;
using System.Collections;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Character.Undo
{
	public class UndoManager
	{
		static Dictionary<UndoType, Type> types = new Dictionary<UndoType, Type> {
			{ UndoType.UndoBuffer, typeof(UndoBuffer) },
			{ UndoType.UndoColour, typeof(UndoColour) },
			{ UndoType.UndoDeleteCharacter, typeof(UndoDeleteCharacter) },
			{ UndoType.UndoDeleteColumn, typeof(UndoDeleteColumn) },
			{ UndoType.UndoDeleteLine, typeof(UndoDeleteLine) },
			{ UndoType.UndoInsertCharacter, typeof(UndoInsertCharacter) },
			{ UndoType.UndoInsertColumn, typeof(UndoInsertColumn) },
			{ UndoType.UndoInsertLine, typeof(UndoInsertLine) },
			{ UndoType.UndoRect, typeof(UndoRect) }
		};
		
		public static IUndoItem CreateUndoItem (UndoType undoType)
		{
			Type type;
			if (types.TryGetValue (undoType, out type)) {
				return Activator.CreateInstance (type) as IUndoItem;
			}
			return null;
		}
		
		public CharacterHandler Handler { get; private set; }

		public bool CanUndo { get { return undo.Count > 0; } }

		public bool CanRedo { get { return redo.Count > 0; } }
		
		LimitedStack<IUndoItem> undo = new LimitedStack<IUndoItem> (1000);
		LimitedStack<IUndoItem> redo = new LimitedStack<IUndoItem> (1000);

		public UndoManager (CharacterHandler handler)
		{
			this.Handler = handler;
		}

		public void Save (IUndoItem item)
		{
			undo.Push (item);
			redo.Clear ();
		}

		public UndoBuffer Save (Point? cursorLocation, Point? nextLocation, params Rectangle[] rects)
		{
			return Save(cursorLocation, nextLocation, false, rects);
		}
		
		public UndoBuffer Save (Point? cursorLocation, Point? nextLocation, bool halfMode, params Rectangle[] rects)
		{
			var item = new UndoBuffer{ CursorLocation = cursorLocation, NextLocation = nextLocation };
			foreach (var rect in rects) {
				var r = rect;
				if (halfMode)
					r = r.FromHalfMode();
				item.Add (new UndoRect (Handler, r));
			}
			Save (item);
			return item;
		}
		
		public void Undo (Pablo.Network.ReceiveCommandArgs args)
		{
			if (args.IsServer) {
				// server will send an undo to all clients
				var server = args.Network as Network.Server;
				args.Invoke (delegate {
					var item = PopUndo ();
					if (item != null)
						server.SendCommand (item, null, null, args.User);
				});
			} else {
				// client is receiving an undo item!
				var type = (UndoType)Enum.ToObject (typeof(UndoType), args.Message.PeekInt32 ());
				var item = CreateUndoItem (type);
				if (item != null) {
					item.Receive (args);
					args.Invoke (delegate {
						item.Apply (this.Handler);
					});
				}
			}
		}

		public void Redo (Pablo.Network.ReceiveCommandArgs args)
		{
			if (args.IsServer) {
				// server will send an undo to all clients
				var server = args.Network as Network.Server;
				args.Invoke (delegate {
					var item = PopRedo ();
					if (item != null)
						server.SendCommand (item, null, null, args.User);
				});
			} else {
				// client is receiving an undo item!
				var type = (UndoType)Enum.ToObject (typeof(UndoType), args.Message.PeekInt32 ());
				var item = CreateUndoItem (type);
				if (item != null) {
					item.Receive (args);
					args.Invoke (delegate {
						item.Apply (this.Handler);
					});
				}
			}
		}
		
		IUndoItem PopUndo ()
		{
			if (undo.Count == 0)
				return null;
			var item = undo.Pop ();
			if (item != null) {
				redo.Push (item.Reciprocal (this.Handler));
				return item;
			}
			return null;
		}
		
		IUndoItem PopRedo ()
		{
			if (redo.Count == 0)
				return null;
			var item = redo.Pop ();
			if (item != null) {
				undo.Push (item.Reciprocal (this.Handler));
				return item;
			}
			return null;
		}
		
		public void Undo ()
		{
			var item = PopUndo ();
			if (item != null)
				item.Apply (this.Handler);
		}
		
		public void Redo ()
		{
			var item = PopRedo ();
			if (item != null)
				item.Apply (this.Handler);
		}
	}
}

