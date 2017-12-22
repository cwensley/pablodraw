using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Pablo.Sauce;
using System.Collections.Generic;
using Eto;

namespace Pablo
{
    public abstract class Format : IXmlReadable
    {
     
        public Format (DocumentInfo info, string id, string name, params string[] extensions)
        {
            this.Info = info;
            this.ID = id;
            this.Name = name;
            this.Extensions = extensions;
        }

        public override string ToString ()
        {
            return Name;
        }
     
        public DocumentInfo Info { get; private set; }
     
        public string ID { get; private set; }
     
        public string Name { get; private set; }
     
        public string[] Extensions { get; private set; }
		
        public virtual bool CanLoad
		{
			get { return true; }
		}
     
        public virtual bool CanSave
		{
			get { return false; }
		}
     
        public bool TypeMatches (string extension)
        {
            foreach (string s in Extensions) {
                if (string.Compare (s, extension, true) == 0)
                    return true;
            }
            return false;
        }
     
        public virtual void ReadXml (XmlElement element)
        {
            string extensionsString = element.GetAttribute("extensions");
            this.Extensions = extensionsString.Split(';', '|', ',');
        }
     
        public virtual void WriteXml (XmlElement element)
        {
            element.SetAttribute ("extensions", String.Join (",", Extensions));
        }
     
        public virtual IEnumerable<FormatParameter> GetParameters (SauceInfo sauce)
        {
            yield break;
        }
    }

    public class FormatCollection : Dictionary<string, Format>, IXmlReadable
    {
     
        public void AddRange (IEnumerable<Format> formats)
        {
            foreach (var format in formats) {
                Add (format);
            }
        }
     
        public void Add (Format value)
        {
            this.Add (value.ID, value);
        }
     
        public Format Find (string fileName, string defaultInfo)
        {
            Format info = Find (fileName);
			if (info == null && defaultInfo != null) {
				if (this.TryGetValue (defaultInfo, out info))
					return info;
			}
            return info;
        }
     
        public Format Find (string fileName)
        {
            string extension = Path.GetExtension (fileName);
            extension = extension.Trim ('.');
			if (!string.IsNullOrEmpty (extension)) {
            	foreach (Format format in this.Values) {
                	if (format.TypeMatches (extension))
	                    return format;
    	        }
			}
            return null;
        }
     
        public void ReadXml (XmlElement element)
        {
            XmlNodeList types = element.SelectNodes ("formatInfo");
            foreach (XmlElement infoElement in types) {
                string id = infoElement.GetAttribute ("infoId");
                if (!string.IsNullOrEmpty (id)) {
                    Format info;
					if (this.TryGetValue (id, out info))
                        info.ReadXml (infoElement);
                }
            }
         
        }
     
        public void WriteXml (XmlElement element)
        {
            foreach (Format info in this.Values) {
                XmlElement infoElement = element.OwnerDocument.CreateElement ("formatInfo");
                infoElement.SetAttribute ("infoId", info.ID);
                info.WriteXml (infoElement);
             
                element.AppendChild (infoElement);
            }
        }
    }
}


