using Pablo;

namespace Pablo.Mobile
{
	public class Settings
	{
		DocumentInfoCollection info = new DocumentInfoCollection();
	
		public Settings()
		{
			info.Add(new Pablo.Formats.Character.CharacterDocumentInfo());
			//info.Add(new Pablo.Formats.Character.Character24DocumentInfo());
			info.Add(new Pablo.Formats.Rip.RipDocumentInfo());
			info.Add(new Pablo.Formats.Image.ImageDocumentInfo());
			info.DefaultFormat = info["character"].Formats["ansi"];
			
		}
		
		public DocumentInfoCollection Infos
		{
			get { return info; }
		}
	}
}
