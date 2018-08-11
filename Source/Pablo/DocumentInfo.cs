using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using Eto;
using Eto.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

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
        readonly FormatCollection formats = new FormatCollection();
        readonly Hashtable properties = new Hashtable();
        readonly ZoomInfo zoomInfo = new ZoomInfo();
        private readonly ZoomInfo previewZoomInfo = new ZoomInfo();

        protected DocumentInfo(string id, string description)
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

        public ZoomInfo PreviewZoomInfo
        {
            get { return previewZoomInfo; }
        }

        public bool AutoScroll
        {
            get; set;
        }

        public Document Create()
        {
            return Create(Platform.Instance);
        }

        public abstract Document Create(Platform generator);

        public FormatCollection Formats
        {
            get { return formats; }
        }

        public virtual void GenerateCommands(GenerateCommandArgs args)
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
            var documentInfos = new DocumentInfoCollection();
            GetCompatibleDocuments(documentInfos);
            return documentInfos;
        }

        protected virtual void GetCompatibleDocuments(DocumentInfoCollection documentInfos)
        {
            documentInfos.Add(this);
        }

        public virtual void ReadXml(XmlElement element)
        {
            AutoScroll = element.GetBoolAttribute("autoscroll") ?? true;
            var formatsElement = (XmlElement)element.SelectSingleNode("formats");
            if (formatsElement != null) formats.ReadXml(formatsElement);
            var zoomInfoElement = (XmlElement)element.SelectSingleNode("zoomInfo");
            if (zoomInfoElement != null) zoomInfo.ReadXml(zoomInfoElement);
            var previewZoomInfoElement = (XmlElement)element.SelectSingleNode("previewZoomInfo");
            if (previewZoomInfoElement != null) previewZoomInfo.ReadXml(previewZoomInfoElement);

            // TODO - Add: previewVisibleRip
            // TODO - Add: previewVisibleTextModeNonEdit


        }

        public virtual void WriteXml(XmlElement element)
        {
            if (!AutoScroll)
                element.SetAttribute("autoscroll", AutoScroll);
            XmlElement formatsElement = element.OwnerDocument.CreateElement("formats");
            formats.WriteXml(formatsElement);
            element.AppendChild(formatsElement);
            XmlElement zoomInfoElement = element.OwnerDocument.CreateElement("zoomInfo");
            zoomInfo.WriteXml(zoomInfoElement);
            element.AppendChild(zoomInfoElement);
            XmlElement previewZoomInfoElement = element.OwnerDocument.CreateElement("previewZoomInfo");
            previewZoomInfo.WriteXml(previewZoomInfoElement);
            element.AppendChild(previewZoomInfoElement);

            // TODO - Add: previewVisibleRip
            // TODO - Add: previewVisibleTextModeNonEdit
        }

        public override string ToString()
        {
            return Description;
        }
    }

    public class DocumentInfoCollection : Dictionary<string, DocumentInfo>, IXmlReadable
    {
        public Format DefaultFormat { get; set; }

        static readonly object locker = new object();
        static DocumentInfoCollection defaultInfos;
        public static DocumentInfoCollection Default
        {
            get
            {
                if (defaultInfos == null)
                {
                    lock (locker)
                    {
                        if (defaultInfos == null)
                        {
                            defaultInfos = new DocumentInfoCollection();
                            defaultInfos.Add(new Pablo.Formats.Character.CharacterDocumentInfo());
                            defaultInfos.Add(new Pablo.Formats.Rip.RipDocumentInfo());
                            defaultInfos.Add(new Pablo.Formats.Image.ImageDocumentInfo());
                            defaultInfos.DefaultFormat = defaultInfos[Pablo.Formats.Character.CharacterDocumentInfo.DocumentID].Formats["ansi"];
                        }
                    }
                }
                return defaultInfos;
            }
        }

        public void Add(DocumentInfo value)
        {
            Add(value.ID, value);
        }

        public DocumentInfo Find(string fileName, string defaultInfo)
        {
            DocumentInfo info = Find(fileName);
            if (info == null && defaultInfo != null) info = this[defaultInfo];
            return info;
        }

        public DocumentInfo Find(string fileName)
        {
            foreach (DocumentInfo info in Values)
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
                if (!string.IsNullOrEmpty(id))
                {
                    DocumentInfo info;
                    if (TryGetValue(id, out info))
                        info.ReadXml(infoElement);
                }
            }
        }

        public void WriteXml(XmlElement element)
        {
            foreach (DocumentInfo info in Values)
            {
                XmlElement infoElement = element.OwnerDocument.CreateElement("documentInfo");
                infoElement.SetAttribute("infoId", info.ID);
                info.WriteXml(infoElement);

                element.AppendChild(infoElement);
            }
        }
    }
}


