using System;
using Eto.Drawing;
using Pablo.Network;
using Lidgren.Network;

namespace Pablo.Formats.Character.Undo
{
	public interface IUndoItem : INetworkReadWrite, ICommand
	{
		int UndoID { get; }
		
		void Apply (CharacterHandler handler);
		
		IUndoItem Reciprocal (CharacterHandler handler);
	}
	
	public abstract class UndoItem : IUndoItem
	{
		public abstract void Apply (CharacterHandler handler);

		public abstract IUndoItem Reciprocal (CharacterHandler handler);

		public abstract int UndoID { get; }

		public virtual bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (UndoID);
			return true;
		}

		public virtual void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			var id = args.Message.ReadInt32 ();
			if (id != UndoID) throw new Eto.EtoException("invalid undo type");
			
		}

		#region ICommand implementation
		
		int ICommand.CommandID {
			get { return (int)NetCommands.Undo; }
		}

		UserLevel ICommand.Level {
			get { return UserLevel.Operator; }
		}
		
		NetDeliveryMethod ICommand.DeliveryMethod {
			get { return NetDeliveryMethod.ReliableOrdered; }
		}
		
		#endregion
	}
}

