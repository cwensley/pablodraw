using System;
using Pablo.Network;

namespace Pablo.Formats.Character
{
	/// <summary>
	/// Summary description for Character.
	/// </summary>
	public struct Character : INetworkReadWrite
	{
		public int character;
		
		public Character(int character)
		{
			this.character = character;
		}

		public Character(byte character)
		{
			this.character = character;
		}


		public static implicit operator Character(byte b)
		{
			return new Character(b);
		}
		public static implicit operator Character(int i)
		{
			return new Character(i);
		}
		
		public static implicit operator byte(Character c)
		{
			return (byte)c.character;
		}

		public static implicit operator int(Character c)
		{
			return c.character;
		}
		
		public override int GetHashCode ()
		{
			return character;
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is Character)) return false;
			var c = (Character)obj;
			return c.character == this.character;
		}
		
		public override string ToString ()
		{
			return character.ToString ();
		}

		#region INetworkReadWrite implementation
		
		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			args.Message.Write (character);
			return true;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			character = args.Message.ReadInt32 ();
		}
		
		#endregion
	}
}
