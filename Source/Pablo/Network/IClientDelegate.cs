using System;
using System.IO;

namespace Pablo.Network
{
	public interface IClientDelegate
	{
		DocumentInfoCollection DocumentInfos { get; }
		
		void SetDocument(Document document);
		
		void LoadFile(string fileName, Stream stream, bool editMode, Format format);
		
		Document Document { get; }

		bool EnableBackups { get; set; }

		bool EditMode { get; }
	}
}

