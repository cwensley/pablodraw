using System;

namespace Pablo.Formats.Character
{
	public struct Attribute
	{
		public static implicit operator Attribute(byte b)
		{
			return new Attribute(b);
		}

		int foreground;
		int background;
		//bool blink;
		public Attribute(byte b)
		{
			foreground = (byte)(b & 0xF);
			background = (byte)((b >> 4) & 0xF);
			//blink = (b & 0x80) != 0;
		}

		public Attribute(int foreground, int background)
		{
			this.foreground = foreground;
			this.background = background;
			//blink = false;
		}

		public Attribute(byte foreground, byte background, bool bold, bool blink)
		{
			this.foreground = (int)(foreground & 0x07);
			this.background = (int)(background & 0x07);
			//this.blink = blink;
			if (bold)
				this.foreground |= 0x08;
			if (blink)
				this.background |= 0x08;
		}

		public static explicit operator byte(Attribute a)
		{
			return (byte)((a.foreground & 0xF) + ((a.background & 0xF) << 4));
		}

		public void Set(int foreground, int background)
		{
			Foreground = foreground;
			Background = background;
		}

		public int ForegroundOnly
		{
			get { return foreground < 16 ? (foreground & (byte)0x7) : foreground; }
			set { foreground = value < 16 ? ((foreground & (byte)0x8) + (value & (byte)0x7)) : value; }
		}

		public int BackgroundOnly
		{
			get { return background < 16 ? (byte)(background & (byte)0x7) : background; }
			set { background = value < 16 ? (byte)((background & (byte)0x8) + (value & (byte)0x7)) : value; }
		}

		public int Foreground
		{
			get { return foreground; }
			set { foreground = value; }
		}

		public int Background
		{
			get { return background; }
			set { background = value; }
		}

		public bool Bold
		{
			get { return foreground < 16 && (foreground & (byte)0x8) != 0; }
			set
			{
				if (foreground < 16)
				{
					foreground &= 0xF;
					if (value)
						foreground |= (byte)0x8;
					else
						foreground &= (byte)0x7; 
				}
			}
		}

		public bool Blink
		{
			get { return background < 16 && (background & (int)0x8) != 0; }
			set
			{
				if (background < 16)
				{
					background &= 0xF;
					if (value)
						background |= (byte)0x8;
					else
						background &= (byte)0x7;
				}
			}
		}

		public override int GetHashCode()
		{
			return foreground.GetHashCode() ^ background.GetHashCode();
		}

		public override bool Equals(object obj)
		{
		
			if (!(obj is Attribute))
				return false;
			var a = (Attribute)obj;
			return a.foreground == foreground && a.background == background;
		}

		public static bool operator ==(Attribute val1, Attribute val2)
		{
			return val1.Equals(val2);
		}

		public static bool operator !=(Attribute val1, Attribute val2)
		{
			return !val1.Equals(val2);
		}

		public override string ToString()
		{
			return string.Format("[Attribute: Foreground={0}, Background={1}]", Foreground, Background);
		}
	}
}
