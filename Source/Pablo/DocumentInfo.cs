using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using Eto;
using Eto.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Pablo
{
	public class DocumentInfoOption
	{
		public string ID { get; set; }
		public string[] Values { get; set; }
		public string Comment { get; set; }
	}

	public abstract class DocumentInfo
	{
		FormatCollection formats = new FormatCollection();
		Hashtable properties = new Hashtable();
		ZoomInfo zoomInfo = new ZoomInfo();
		
		public DocumentInfo(string id, string description)
		{
			this.ID = id;
			this.Description = description;
		}

		public string OptionID { get; set; }

		public string ID { get; set; }
		
		public string Description { get; set; }
		
		public abstract bool CanEdit
		{
			get;
		}

		public abstract Format DefaultFormat
		{
			get;
		}
	
		public Hashtable Properties
		{
			get { return properties; }
		}
		
		public ZoomInfo ZoomInfo
		{
			get { return zoomInfo; }
		}
		
		public bool AutoScroll
		{
			get; set;
		}
		
		public Document Create()
		{
			return this.Create(Generator.Current);
		}
		
		public abstract Document Create(Generator generator);

		public FormatCollection Formats
		{
			get { return formats; }
		}
		
		public virtual void GenerateActions(GenerateActionArgs args)
		{
		}

		public virtual IEnumerable<DocumentInfoOption> Options
		{
			get { yield break; }
		}

		public virtual bool SetOption(string option, string value)
		{
			return false;
		}
		
		public DocumentInfoCollection GetCompatibleDocuments()
		{
			DocumentInfoCollection documentInfos = new DocumentInfoCollection();
			GetCompatibleDocuments(documentInfos);
			return documentInfos;
		}
		
		protected virtual void GetCompatibleDocuments(DocumentInfoCollection documentInfos)
		{
			documentInfos.Add(this);
		}
		
		public virtual void ReadXml(XmlElement element)
		{
			AutoScroll = element.GetBoolAttribute ("autoscroll") ?? true;
			XmlElement formats = (XmlElement)element.SelectSingleNode("formats");
			if (formats != null) this.formats.ReadXml(formats);
			XmlElement zoomInfoElement = (XmlElement)element.SelectSingleNode("zoomInfo");
			if (zoomInfoElement != null) this.zoomInfo.ReadXml(zoomInfoElement);
		}
		
		public virtual void WriteXml(XmlElement element)
		{
			if (!AutoScroll)
				element.SetAttribute ("autoscroll", AutoScroll);
			XmlElement formats = element.OwnerDocument.CreateElement("formats");
			this.formats.WriteXml(formats);
			element.AppendChild(formats);
			XmlElement zoomInfoElement = element.OwnerDocument.CreateElement("zoomInfo");
			zoomInfo.WriteXml(zoomInfoElement);
			element.AppendChild(zoomInfoElement);
		}
		
		public override string ToString()
		{
			return Description;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public class DocumentInfoCollection : Dictionary<string, DocumentInfo>, IXmlReadable
	{
		public Format DefaultFormat { get; set; }

		public static DocumentInfoCollection Default
		{
			get
			{
				var info = new DocumentInfoCollection();
				info.Add(new Pablo.Formats.Character.CharacterDocumentInfo());
				info.Add(new Pablo.Formats.Rip.RipDocumentInfo());
				info.Add(new Pablo.Formats.Image.ImageDocumentInfo());
				info.DefaultFormat = info[Pablo.Formats.Character.CharacterDocumentInfo.DocumentID].Formats["ansi"];
				return info;
			}
		}
		
		public DocumentInfoCollection()
		{
			// initialize default values
		}
		
		public void Add(DocumentInfo value)
		{
			this.Add(value.ID, value);
		}
		
		public DocumentInfo Find(string fileName, string defaultInfo)
		{
			DocumentInfo info = Find(fileName);
			if (info == null && defaultInfo != null) info = this[defaultInfo];
			return info;
		}
		
		public DocumentInfo Find(string fileName)
		{
			foreach (DocumentInfo info in this.Values)
			{
				Format format = info.Formats.Find(fileName);
				if (format != null) return info;
			}
			// return default format
			return null;
		}
		
		public Format FindFormat(string fileName)
		{
			DocumentInfo documentInfo = Find(fileName, null);
			if (documentInfo != null) return documentInfo.Formats.Find(fileName, null);
			else return null;
		}

		public Format FindFormat(string fileName, string defaultInfo, string defaultFormat)
		{
			DocumentInfo documentInfo = Find(fileName, defaultInfo);
			return documentInfo.Formats.Find(fileName, defaultFormat);
		}
		
		public FormatCollection GetFormats()
		{
			var formats = new FormatCollection();
			foreach (var info in this.Values)
			{
				formats.AddRange(info.Formats.Values);
			}
			return formats;
		}
		
		public void ReadXml(XmlElement element)
		{
			XmlNodeList types = element.SelectNodes("documentInfo");
			foreach (XmlElement infoElement in types)
			{
				string id = infoElement.GetAttribute("infoId");
				if (!string.IsNullOrEmpty (id))
				{
					DocumentInfo info;
					if (this.TryGetValue (id, out info))
						info.ReadXml(infoElement);
				}
			}
		}
		
		public void WriteXml(XmlElement element)
		{
			foreach (DocumentInfo info in this.Values)
			{
				XmlElement infoElement = element.OwnerDocument.CreateElement("documentInfo");
				infoElement.SetAttribute("infoId", info.ID);
				info.WriteXml(infoElement);
				
				element.AppendChild(infoElement);
			}
		}
	}
}


