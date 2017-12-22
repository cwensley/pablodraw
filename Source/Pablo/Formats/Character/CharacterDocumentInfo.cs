using System;
using System.Xml;
using System.Linq;
using Eto;
using Eto.Forms;
using Eto.Misc;
using System.Collections.Generic;
using Eto.Drawing;
using System.Text;

namespace Pablo.Formats.Character
{
	/// <summary>
	/// Summary description for DocumentInfo.
	/// </summary>
	public class CharacterDocumentInfo : Animated.AnimatedDocumentInfo
	{
		public const int MAX_CHARACTER_SETS = 20;
		public const int MAX_BRUSHES = 10;
		public const int MAX_BRUSH_SIZE = 12;
		public static readonly int[,] DefaultCharacterSets = new int[,]
		{
			{0xda,0xbf,0xc0,0xd9,0xc4,0xb3,0xc3,0xb4,0xc1,0xc2,0x20,0x20},
			{0xc9,0xbb,0xc8,0xbc,0xcd,0xba,0xcc,0xb9,0xca,0xcb,0x20,0x20},
			{0xd5,0xb8,0xd4,0xbe,0xcd,0xb3,0xc6,0xb5,0xcf,0xd1,0x20,0x20},
			{0xd6,0xb7,0xd3,0xbd,0xc4,0xba,0xc7,0xb6,0xd0,0xd2,0x20,0x20},
			{0xc5,0xce,0xd8,0xd7,0xe8,0xe8,0x9b,0x9c,0x99,0xef,0x20,0x20},
			{0xb0,0xb1,0xb2,0xdb,0xdf,0xdc,0xdd,0xde,0xfe,0xfa,0x20,0x20},
			{0x01,0x02,0x03,0x04,0x05,0x06,0xf0,0x0e,0x0f,0x20,0x20,0x20},
			{0x18,0x19,0x1e,0x1f,0x10,0x11,0x12,0x1d,0x14,0x15,0x20,0x20},
			{0xae,0xaf,0xf2,0xf3,0xa9,0xaa,0xfd,0xf6,0xab,0xac,0x20,0x20},
			{0xe3,0xf1,0xf4,0xf5,0xea,0x9d,0xe4,0xf8,0xfb,0xfc,0x20,0x20},
			{0xe0,0xe1,0xe2,0xe5,0xe6,0xe7,0xeb,0xec,0xed,0xee,0x20,0x20},
			{0x80,0x87,0xa5,0xa4,0x98,0x9f,0xf7,0xf9,0xad,0xa8,0x20,0x20},
			{0x83,0x84,0x85,0xa0,0xa6,0x86,0x8e,0x8f,0x91,0x92,0x20,0x20},
			{0x88,0x89,0x8a,0x82,0x90,0x8c,0x8b,0x8d,0xa1,0x9e,0x20,0x20},
			{0x93,0x94,0x95,0xa2,0xa7,0x96,0x81,0x97,0xa3,0x9a,0x20,0x20},
			{0x2f,0x5c,0x28,0x29,0x7b,0x7d,0x5b,0x5d,0x60,0x27,0x20,0x20},
			{0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20},
			{0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20},
			{0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20},
			{0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20}
		};
		public static readonly BrushInfo[] DefaultBrushes = new BrushInfo[MAX_BRUSHES] {
			new BrushInfo (BitFont.StandardEncoding, 176, 177, 178, 219),
			new BrushInfo (".,;sS$"),
			new BrushInfo (".,;%!&#@"),
			null,
			null,
			null,
			null,
			null,
			null,
			null
		};
		BrushInfo[] brushes = DefaultBrushes.Clone() as BrushInfo[];
		bool use9x = false;
		bool dosAspect = false;
		bool iceColours = true;
		bool insertMode = false;
		bool shiftSelect = true;
		int[,] characterSets = (int[,])DefaultCharacterSets.Clone();
		Dictionary<int, int> flipxdictionary;
		static int[,] flipx = { 
			{16, 17}, {17, 16}, {26, 27},  {27, 26},
			{'\'', '`'}, {'(', ')'}, {')', '('}, {'/', '\\'},
			{'<', '>'}, {'>', '<'}, {'[', ']'}, {'\\', '/'},
			{']', '['}, {'`', '\''}, {'{', '}'}, {'}', '{'},
			{'©', 'ª'}, {'ª', '©'}, {'®', '¯'}, {'¯', '®'},
			{'´', 'Ã'}, {'µ', 'Æ'}, {'¶', 'Ç'}, {'·', 'Ö'},
			{'¸', 'Õ'}, {'¹', 'Ì'}, {'»', 'É'}, {'¼', 'È'},
			{'½', 'Ó'}, {'¾', 'Ô'}, {'¿', 'Ú'}, {'À', 'Ù'},
			{'Ã', '´'}, {'Æ', 'µ'}, {'È', '¼'}, {'É', '»'},
			{'Ì', '¹'}, {'Ó', '½'}, {'Ô', '¾'}, {'Õ', '¸'},
			{'Ö', '·'}, {'Ù', 'À'}, {'Ú', '¿'}, {'Ý', 'Þ'},
			{'Þ', 'Ý'}, {'ò', 'ó'}, {'ó', 'ò'}
		};

		public int FlipX(int ch)
		{
			int ret;
			if (flipxdictionary == null)
				flipxdictionary = flipx.ToDictionary();
			if (flipxdictionary.TryGetValue(ch, out ret))
				return ret;
			return ch;
		}

		Dictionary<int, int> flipydictionary;
		static int[,] flipy = { 
			{24, 25}, {25, 24}, {30, 31}, {31, 30},
			{'/', '\\'}, {'\\', '/'}, {'·', '½'}, {'¸', '¾'},
			{'»', '¼'}, {'¼', '»'}, {'½', '·'}, {'¾', '¸'},
			{'¿', 'Ù'}, {'À', 'Ú'}, {'Á', 'Â'}, {'Â', 'Á'},
			{'È', 'É'}, {'É', 'È'}, {'Ê', 'Ë'}, {'Ë', 'Ê'},
			{'Ï', 'Ñ'}, {'Ð', 'Ò'}, {'Ñ', 'Ï'}, {'Ò', 'Ð'},
			{'Ó', 'Ö'}, {'Ô', 'Õ'}, {'Õ', 'Ô'}, {'Ö', 'Ó'},
			{'Ù', '¿'}, {'Ú', 'À'}, {'Ü', 'ß'}, {'ß', 'Ü'}
		};

		public int FlipY(int ch)
		{
			int ret;
			if (flipydictionary == null)
				flipydictionary = flipy.ToDictionary();
			if (flipydictionary.TryGetValue(ch, out ret))
				return ret;
			return ch;
		}

		Dictionary<int, int> fliprotdictionary;
		static int[,] fliprot = { 
			{16, 31}, {17, 30}, {23, 29}, {24, 26},
			{25, 27}, {26, 25}, {27, 24}, {29, 23},
			{30, 16}, {31, 17}, {'³', 'Ä'}, {'´', 'Á'},
			{'µ', 'Ð'}, {'¶', 'Ï'}, {'·', '¾'}, {'¸', '½'},
			{'¹', 'Ê'}, {'º', 'Í'}, {'»', '¼'}, {'¼', 'È'},
			{'½', 'Ô'}, {'¾', 'Ó'}, {'¿', 'Ù'}, {'À', 'Ú'},
			{'Á', 'Ã'}, {'Â', '´'}, {'Ã', 'Â'}, {'Ä', '³'},
			{'Æ', 'Ò'}, {'Ç', 'Ñ'}, {'È', 'É'}, {'É', '»'},
			{'Ê', 'Ì'}, {'Ë', '¹'}, {'Ì', 'Ë'}, {'Í', 'º'},
			{'Ï', 'Ç'}, {'Ð', 'Æ'}, {'Ñ', '¶'}, {'Ò', 'µ'},
			{'Ó', 'Õ'}, {'Ô', 'Ö'}, {'Õ', '·'}, {'Ö', '¸'},
			{'×', 'Ø'}, {'Ø', '×'}, {'Ù', 'À'}, {'Ú', '¿'},
			{'Ü', 'Ý'}, {'Ý', 'ß'}, {'Þ', 'Ü'}, {'ß', 'Þ'}
		};

		public int FlipRotate(int ch)
		{
			int ret;
			if (fliprotdictionary == null)
				fliprotdictionary = fliprot.ToDictionary();
			if (fliprotdictionary.TryGetValue(ch, out ret))
				return ret;
			return ch;
		}

		public const string DocumentID = "character";

		public CharacterDocumentInfo()
			: base(DocumentID, "Text Document")
		{
			OptionID = "text";
			Formats.Add(new Types.Ansi(this));
			Formats.Add(new Types.Ascii(this));
			Formats.Add(new Types.Avatar(this));
			Formats.Add(new Types.XBin(this));
			Formats.Add(new Types.Idf(this));
			Formats.Add(new Types.Adf(this));
			Formats.Add(new Types.Binary(this));
			Formats.Add(new Types.CG(this));
			Formats.Add(new Types.Tundra(this));
			Formats.Add(new Types.CtrlA(this));
			Formats.Add(new Types.Atascii(this));
			Formats.Add(new Types.Font(this));
			//Formats.Add(new Types.Pablo(this));
			ZoomInfo.FitWidth = true;
		}

		protected CharacterDocumentInfo(string documentID, string name)
			: base(documentID, name)
		{
			ZoomInfo.FitWidth = true;
		}

		public override Format DefaultFormat
		{
			get { return Formats["ansi"]; }
		}

		public override bool CanEdit
		{
			get { return true; }
		}

		BitFont selectedFont = BitFont.GetStandard8x16();

		public event EventHandler<EventArgs> SelectedFontChanged;

		public BitFont SelectedFont
		{
			get { return selectedFont; }
			set
			{
				if (selectedFont != value)
				{
					selectedFont = value;
					if (SelectedFontChanged != null)
						SelectedFontChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> Use9xChanged;

		public bool Use9x
		{
			get { return use9x; }
			set
			{
				if (value != use9x)
				{
					use9x = value;
					if (Use9xChanged != null)
						Use9xChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> iCEColoursChanged;

		public bool iCEColours
		{
			get { return iceColours; }
			set
			{
				if (value != iceColours)
				{
					iceColours = value;
					if (iCEColoursChanged != null)
						iCEColoursChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> BrushesChanged;

		public BrushInfo[] Brushes
		{
			get { return brushes; }
			set
			{
				brushes = value;
				if (BrushesChanged != null)
					BrushesChanged(this, EventArgs.Empty);
			}
		}

		public Size? AttributeDialogSize
		{
			get;
			set;
		}

		public event EventHandler<EventArgs> DosAspectChanged;

		public bool DosAspect
		{
			get { return dosAspect; }
			set
			{
				dosAspect = value;
				if (DosAspectChanged != null)
					DosAspectChanged(this, EventArgs.Empty);
			}
		}

		public bool ShiftSelect
		{
			get { return shiftSelect; }
			set { shiftSelect = value; }
		}

		public override IEnumerable<DocumentInfoOption> Options
		{
			get { return base.Options.Concat(GetOptions()); }
		}

		IEnumerable<DocumentInfoOption> GetOptions()
		{
			yield return new DocumentInfoOption { ID = "aspect", Comment = "Scales the output to dos aspect", Values = new string[] { "dos", "none" } };
			yield return new DocumentInfoOption { ID = "use9x", Comment = "Adds a 9th pixel to each character", Values = new string[] { "true", "false" } };
			yield return new DocumentInfoOption { ID = "ice", Comment = "iCE Color mode (16 background colors)", Values = new string[] { "true", "false" } };
		}

		public override bool SetOption(string option, string value)
		{
			switch (option.ToLowerInvariant())
			{
				case "aspect":
					switch (value.ToLowerInvariant())
					{
						case "dos":
							this.DosAspect = true;
							break;
						case "none":
							this.DosAspect = false;
							break;
					}
					break;
				case "use9x":
					bool use9x;
					if (bool.TryParse(value, out use9x))
					{
						this.Use9x = use9x;
						return true;
					}
					break;
				case "ice":
					bool ice;
					if (bool.TryParse(value, out ice))
					{
						this.iCEColours = ice;
					}
					break;
			}
			return base.SetOption(option, value);
		}

		public override Document Create(Generator generator)
		{
			Document doc = new CharacterDocument(this);
			doc.Generator = generator;
			return doc;
		}

		public ActionItemSubMenu GetFontMenu(CharacterHandler handler, ActionCollection actions = null, int order = 600, Action<BitFont> selectFont = null, Func<BitFont, bool> fontSelected = null)
		{
			Actions.ChangeFont mainChangeFont = null;
			actions = actions ?? new ActionCollection();
			var aiChangeFonts = new ActionItemSubMenu(actions, "Change Font");
			aiChangeFonts.Order = order;
			foreach (var fontSet in GetFonts())
			{
				var aiFontSet = aiChangeFonts.Actions.GetSubmenu(fontSet.Name);
				foreach (var font in fontSet.Fonts)
				{
					var chfont = new Actions.ChangeFont(mainChangeFont, handler, font);
					if (fontSelected != null)
						chfont.Checked = fontSelected(font);
					if (mainChangeFont == null)
						mainChangeFont = chfont;
					if (selectFont != null)
					{
						chfont.Activated += (sender, e) => selectFont(((Actions.ChangeFont)sender).Font);
					}
					actions.Add(chfont);
					aiFontSet.Actions.Add(Actions.ChangeFont.ActionID + font.ID);
				}
			}
			return aiChangeFonts;
		}

		public override void GenerateActions(GenerateActionArgs args)
		{
			bool editMode = (bool)args.GetArgument("editMode", false);
			string area = (string)args.GetArgument("area", string.Empty);

			if (area == "main")
			{


				var edit = args.Menu.GetSubmenu("&Edit");

				if (editMode)
				{
					var actionShiftSelect = args.Actions.AddCheck("shiftSelect", "Use Shift+Movement to select|Shift Select|Use Shift+Movement keys for selection", actionShiftSelect_CheckedChanged);
					actionShiftSelect.Checked = ShiftSelect;
					edit.Actions.Add(actionShiftSelect.ID);
				}



			}

			base.GenerateActions(args);
		}

		private void actionShiftSelect_CheckedChanged(Object sender, EventArgs e)
		{
			CheckAction action = (CheckAction)sender;
			action.Checked = !action.Checked;
			ShiftSelect = action.Checked;
		}

		protected override void GetCompatibleDocuments(DocumentInfoCollection documentInfos)
		{
			base.GetCompatibleDocuments(documentInfos);
			documentInfos.Add(new Image.ImageDocumentInfo());
		}

		public int GetCharacter(int characterSet, int character)
		{
			return characterSets[characterSet, character];
		}

		public IEnumerable<int> GetCharacterSet(int characterSet)
		{
			for (int i = 0; i < characterSets.GetLength(1); i++)
			{
				yield return characterSets[characterSet, i];
			}
		}

		public void SetCharacterSet(int characterSet, int[] characters)
		{
			if (characterSets.GetLength(1) != characters.Length)
				throw new ArgumentOutOfRangeException("characters", "Characters must match number of items in character set");
			for (int i = 0; i < characterSets.GetLength(1); i++)
			{
				characterSets[characterSet, i] = characters[i];
			}
		}

		private void CreateCharacterSets()
		{
			characterSets = new int[20, 12];

			for (int set = 0; set < characterSets.GetLength(0); set++)
			{
				for (int ch = 0; ch < 12; ch++)
				{
					characterSets[set, ch] = 0x20; // space!
				}
			}
		}

		List<BitFontSet> fonts;

		public IEnumerable<BitFontSet> GetFonts()
		{
			if (fonts == null)
			{
				fonts = new List<BitFontSet>();
				fonts.AddRange(BitFontSet.DosFonts());
				fonts.Add(BitFontSet.AmigaFont());
				fonts.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
			}

			return fonts;
		}

		public override void ReadXml(System.Xml.XmlElement element)
		{
			base.ReadXml(element);

			use9x = element.GetBoolAttribute("use9x") ?? use9x;
			dosAspect = element.GetBoolAttribute("dosAspect") ?? dosAspect;
			insertMode = element.GetBoolAttribute("insertMode") ?? insertMode;
			shiftSelect = element.GetBoolAttribute("shiftSelect") ?? shiftSelect;
			iceColours = element.GetBoolAttribute("iceColours") ?? true;
			AttributeDialogSize = element.ReadChildSizeXml("attribute-dialog");
			if (AttributeDialogSize != null && AttributeDialogSize.Value == Size.Empty)
				AttributeDialogSize = null;
			//element.ReadChildListXml(brushes, "brushes", "brush");

			XmlNodeList charElements = element.SelectNodes("characterSets/characterSet");
			if (charElements != null)
			{
				if (charElements.Count > 0)
					CreateCharacterSets();

				foreach (XmlElement charElement in charElements)
				{
					int set = Convert.ToInt32(charElement.GetAttribute("set"));
					if (set >= 0 && set < characterSets.GetLength(0))
					{
						for (int i = 0; i < characterSets.GetLength(1); i++)
						{
							characterSets[set, i] = charElement.GetIntAttribute("f" + Convert.ToString(i + 1)) ?? 32;
						}
					}
				}
			}
		}

		public override void WriteXml(System.Xml.XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttribute("use9x", use9x);
			element.SetAttribute("dosAspect", dosAspect);
			element.SetAttribute("iceColours", iceColours);
			element.SetAttribute("insertMode", insertMode);
			element.SetAttribute("shiftSelect", shiftSelect);
			element.WriteChildSizeXml("attribute-dialog", AttributeDialogSize);

			element.WriteChildListXml(brushes, "brushes", "brush");

			XmlElement charElement = element.OwnerDocument.CreateElement("characterSets");
			for (int set = 0; set < characterSets.GetLength(0); set++)
			{
				XmlElement charSet = charElement.OwnerDocument.CreateElement("characterSet");
				charSet.SetAttribute("set", set.ToString());

				for (int i = 0; i < characterSets.GetLength(1); i++)
				{
					charSet.SetAttribute("f" + Convert.ToString(i + 1), characterSets[set, i].ToString());
				}

				charElement.AppendChild(charSet);
			}
			element.AppendChild(charElement);
		}
	}
}
