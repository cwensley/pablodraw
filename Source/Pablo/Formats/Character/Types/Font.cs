using Eto.Drawing;
using System.IO;
using System.Reflection;

namespace Pablo.Formats.Character.Types
{
	public class Font : CharacterFormat
	{
		public Font(DocumentInfo info)
			: base(info, "font", "Font (fnt)", "fnt")
		{
		}

		public override bool CanLoad
		{
			get { return false; }
		}

		public override bool CanSave
		{
			get { return true; }
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
		}

		public override void Save(Stream stream, CharacterDocument document)
		{
			var page = document.Pages[0];
			page.Font.Save(new BinaryWriter(stream));
		}
	}
}
