using System;
using Eto.Drawing;
using System.Runtime.InteropServices;

namespace Pablo.Formats.Character
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CanvasElement
	{
		Character character;
		Attribute attribute;

		public static readonly CanvasElement Default = new CanvasElement (32, 7);

		public CanvasElement (Character character, Attribute attribute)
		{
			this.character = character;
			this.attribute = attribute;
		}

		public Character Character { get { return character; } set { character = value; } }

		public Attribute Attribute { get { return attribute; } set { attribute = value; } }

		public ushort Value {
			get { return (ushort)((ushort)(Character + (byte)Attribute) << 8); }
			set {
				Character = (byte)(value & 0xFF);
				Attribute = (byte)(value >> 8);
			}
		}
		
		public bool IsTransparent {
			get {
				return (Character == 32 || Character == 0) && Attribute.Background == 0;
			}
		}

		public int Foreground {
			get { return Attribute.Foreground; }
		}

		public int Background {
			get { return Attribute.Background; }
		}
		
		public static bool operator ==(CanvasElement val1, CanvasElement val2)
		{
			return val1.Character == val2.Character && val1.Attribute == val2.Attribute;
		}

		public static bool operator !=(CanvasElement val1, CanvasElement val2)
		{
			return !(val1.Character == val2.Character && val1.Attribute == val2.Attribute);
		}

		public override int GetHashCode ()
		{
			return Character.GetHashCode () ^ Attribute.GetHashCode ();
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is CanvasElement)) return false;
			var val = (CanvasElement)obj;
			return val.Character == this.Character && val.Attribute == this.Attribute; 
		}
		
		public override string ToString ()
		{
			return string.Format ("[CanvasElement: Character={0}, Attribute={1}]", Character, Attribute);
		}
	}
}
