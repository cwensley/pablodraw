using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Sauce.Types.Character
{
	public enum CharacterFileType
	{
		Ascii = 0,
		Ansi = 1,
		Ansimation = 2,
		Rip = 3,
		Pcboard = 4,
		Avatar = 5,
		Html = 6,
		Source = 7,
		TundraDraw = 8
	}

	public class DataTypeInfo : BaseText.DataTypeInfo
	{
		public override IEnumerable<SauceFileTypeInfo> FileTypes
		{
			get
			{
				yield return new SauceFileTypeInfo { Type = 0, Name = "Ascii" };
				yield return new SauceFileTypeInfo { Type = 1, Name = "Ansi" };
				yield return new SauceFileTypeInfo { Type = 2, Name = "Ansimation" };
				yield return new SauceFileTypeInfo { Type = 3, Name = "RIP" };
				yield return new SauceFileTypeInfo { Type = 4, Name = "Pcboard" };
				yield return new SauceFileTypeInfo { Type = 5, Name = "Avatar" };
				yield return new SauceFileTypeInfo { Type = 6, Name = "HTML" };
				yield return new SauceFileTypeInfo { Type = 7, Name = "Source" };
				yield return new SauceFileTypeInfo { Type = 8, Name = "Tundra Draw" };
			}
		}

		public ushort Width
		{
			get { return Sauce.TInfo1; }
			set { Sauce.TInfo1 = value; }
		}

		public ushort Height
		{
			get { return Sauce.TInfo2; }
			set { Sauce.TInfo2 = value; }
		}

		public ushort NumberOfColors
		{
			get { return Sauce.TInfo3; }
			set { Sauce.TInfo3 = value; }
		}

		public bool HasDimensions
		{
			get
			{
				switch (Type)
				{
					case CharacterFileType.Ansi:
					case CharacterFileType.Ansimation:
					case CharacterFileType.Ascii:
					case CharacterFileType.Avatar:
					case CharacterFileType.Rip:
					case CharacterFileType.Pcboard:
					case CharacterFileType.TundraDraw:
						return true;
					default:
						return false;
				}
			}
		}

		public override bool HasFontName
		{
			get { return HasAspectRatio && Type != CharacterFileType.Rip; }
		}

		public bool HasNumberOfColors
		{
			get { return Type == CharacterFileType.Rip; }
		}

		public CharacterFileType Type
		{
			get { return (CharacterFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}

		public override Control GenerateUI()
		{
			return new Admin<DataTypeInfo>(this);
		}

		public override bool HasICEColors
		{
			get { return HasAspectRatio && Type != CharacterFileType.Rip; }
		}

		public override bool HasLetterSpacing
		{
			get { return HasAspectRatio && Type != CharacterFileType.Rip; }
		}

		public override bool HasAspectRatio
		{
			get
			{
				switch (Type)
				{
					case CharacterFileType.Ansi:
					case CharacterFileType.Ansimation:
					case CharacterFileType.Ascii:
					case CharacterFileType.Avatar:
					case CharacterFileType.Rip:
					case CharacterFileType.Pcboard:
					case CharacterFileType.TundraDraw:
						return true;
					default:
						return false;
				}
			}
		}
	}
}
