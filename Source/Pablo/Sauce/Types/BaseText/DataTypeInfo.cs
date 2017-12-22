using System;
using System.Collections.Generic;
using Pablo.Formats.Character;
using System.Linq;

namespace Pablo.Sauce.Types.BaseText
{
	public abstract class DataTypeInfo : Pablo.Sauce.Types.BaseFileType.DataTypeInfo
	{
		protected SauceBitFlag ICEColorsFlag { get; private set; }

		protected SauceTwoBitFlag LetterSpacingFlag { get; private set; }

		protected SauceTwoBitFlag AspectRatioFlag { get; private set; }

		protected override void Initialize()
		{
			base.Initialize();
			ICEColorsFlag = new SauceBitFlag(Sauce, "iCE Colors", 0);
			LetterSpacingFlag = new SauceTwoBitFlag(Sauce, "9px Letter Spacing", 1, 0x2, 0x1);
			AspectRatioFlag = new SauceTwoBitFlag(Sauce, "Legacy Aspect Ratio", 3);
		}

		public override Eto.Forms.Control GenerateUI()
		{
			return new Admin<DataTypeInfo>(this);
		}

		public bool ICEColors
		{
			get { return ICEColorsFlag.BoolValue; }
			set { ICEColorsFlag.BoolValue = value; }
		}

		public bool? LetterSpacing
		{
			get { return LetterSpacingFlag.BoolValue; }
			set { LetterSpacingFlag.BoolValue = value; }
		}

		public bool? AspectRatio
		{
			get { return AspectRatioFlag.BoolValue; }
			set { AspectRatioFlag.BoolValue = value; }
		}

		public string FontName
		{
			get { return Sauce.TInfoS; }
			set { Sauce.TInfoS = value; } 
		}

		public abstract bool HasICEColors { get; }

		public abstract bool HasLetterSpacing { get; }

		public abstract bool HasAspectRatio { get; }

		public abstract bool HasFontName { get; }

		public BitFont GetFont(IEnumerable<BitFontSet> availableFonts)
		{
			if (HasFontName && !string.IsNullOrEmpty(FontName))
			{
				var fontName = FontName;
				var fonts = availableFonts as BitFontSet[] ?? availableFonts.ToArray();
				var font = fonts.SelectMany(r => r.Fonts).FirstOrDefault(r => string.Equals(r.SauceID, fontName, StringComparison.OrdinalIgnoreCase));
				if (font == null && fontName.StartsWith("amiga", StringComparison.OrdinalIgnoreCase))
				{
					font = fonts.SelectMany(r => r.Fonts).FirstOrDefault(r => r.ID == BitFont.StandardAmigaFontID);
				}
				if (font == null && fontName.IndexOf("EGA43", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					var vgaFontName = fontName.Replace("EGA43", "VGA50");
					font = fonts.SelectMany(r => r.Fonts).FirstOrDefault(r => string.Equals(r.SauceID, vgaFontName, StringComparison.OrdinalIgnoreCase));
				}
				return font;
			}
			return null;
		}

		public override IEnumerable<SauceFlag> Flags
		{
			get
			{
				if (HasICEColors)
					yield return ICEColorsFlag;
				if (HasLetterSpacing)
					yield return LetterSpacingFlag;
				if (HasAspectRatio)
					yield return AspectRatioFlag;
			}
		}
	}
}

