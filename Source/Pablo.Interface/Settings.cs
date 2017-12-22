using Pablo;
using Eto;
using System.Xml;
using Pablo.Network;

namespace Pablo.Interface
{
	public class Settings : IXmlReadable
	{
		readonly DocumentInfoCollection info = DocumentInfoCollection.Default;
		public const int DefaultServerPort = 14400;
		
		public string Alias { get; set; }

		public string ServerIP { get; set; }

		public int ServerPort { get; set; }

		public string ServerPassword { get; set; }

		public string ServerOperatorPassword { get; set; }
		
		public UserLevel UserLevel { get; set; }
		
		public bool UseNat { get; set; }
		
		public int EditFileSplit { get; set; }

		public int FileSplit { get; set; }

		public bool EnableBackups { get; set; }
	
		public Settings ()
		{
			FileSplit = 200;
			EditFileSplit = 0;
			ServerPort = DefaultServerPort;
			UserLevel = UserLevel.Viewer;
		}
		
		public DocumentInfoCollection Infos {
			get { return info; }
		}

		public void ReadXml (XmlElement element)
		{
			Alias = element.GetAttribute ("alias");
			ServerIP = element.GetAttribute ("serverIP");
			ServerPort = element.GetIntAttribute ("serverPort") ?? DefaultServerPort;
			ServerPassword = element.GetAttribute ("serverPassword");
			EnableBackups = element.GetBoolAttribute("enableBackups") ?? false;
			ServerOperatorPassword = element.GetAttribute ("serverOperatorPassword");
			UserLevel = element.GetEnumAttribute<UserLevel> ("userLevel") ?? UserLevel.Viewer;
			element.ReadChildXml ("documentTypes", Infos);
			UseNat = element.GetBoolAttribute ("useNat") ?? false;
			EditFileSplit = element.GetIntAttribute ("editFileSplit") ?? 0;
			FileSplit = element.GetIntAttribute ("fileSplit") ?? 200;
		}

		public void WriteXml (XmlElement element)
		{
			if (!string.IsNullOrEmpty (this.Alias))
				element.SetAttribute ("alias", this.Alias);
			if (!string.IsNullOrEmpty (this.ServerIP))
				element.SetAttribute ("serverIP", this.ServerIP);
			if (this.ServerPort != DefaultServerPort)
				element.SetAttribute ("serverPort", this.ServerPort);
			if (!string.IsNullOrEmpty (this.ServerPassword))
				element.SetAttribute ("serverPassword", this.ServerPassword);
			if (!string.IsNullOrEmpty (this.ServerOperatorPassword))
				element.SetAttribute ("serverOperatorPassword", this.ServerOperatorPassword);
			if (UseNat)
				element.SetAttribute ("useNat", this.UseNat);
			if (EnableBackups)
				element.SetAttribute("enableBackups", EnableBackups);

			element.SetAttribute ("editFileSplit", EditFileSplit);
			element.SetAttribute ("fileSplit", FileSplit);
			
			element.SetAttribute ("userLevel", UserLevel);
			
			element.WriteChildXml ("documentTypes", Infos);

		}

	}
}
