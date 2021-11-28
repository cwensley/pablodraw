using System;
using Eto.Drawing;
using System.Collections.Generic;
using Eto;
using System.Text;
using System.Linq;
using System.Xml;
using Pablo.Network;

namespace Pablo.Formats.Character
{
	public class BrushInfo : IXmlReadable
	{
		public string Gradient { get; set; }
		Encoding encoding;
		Character[] characters;

		public BrushInfo(IEnumerable<Character> characters)
		{
			this.characters = characters.ToArray();
		}

		public BrushInfo()
		{
		}

		public BrushInfo(string gradient)
		{
			this.Gradient = gradient;
		}

		public BrushInfo(Encoding encoding, params byte[] bytes)
		{
			this.encoding = encoding ?? BitFont.StandardEncoding;
			this.characters = bytes.Select(r => new Character(r)).ToArray();
		}

		public Character[] GetCharacters(Encoding encoding = null)
		{
			encoding = encoding ?? BitFont.StandardEncoding;

			if (characters == null && !string.IsNullOrEmpty(Gradient) && this.encoding != encoding)
			{
				this.encoding = encoding;
				var bytes = encoding.GetBytes(this.Gradient);
				characters = bytes.Select(r => new Character(r)).ToArray();
			}

			return characters;
		}

		public void ReadXml(XmlElement element)
		{
			var codePageString = element.GetAttribute("codepage");
			if (!string.IsNullOrEmpty(codePageString) && int.TryParse(codePageString, out var codePage))
			{
				encoding = Encoding.GetEncoding(codePage);
			}
			var chars = new List<Character>();
			foreach (XmlElement childElement in element.SelectNodes("char"))
			{
				var value = childElement.GetAttribute("value");
				if (int.TryParse(value, out var ch))
				{
					chars.Add(new Character(ch));
				}
			}
			characters = chars.ToArray();
		}

		public void WriteXml(XmlElement element)
		{
			if (encoding != null)
				element.SetAttribute("codepage", encoding.CodePage);
			if (characters != null)
			{
				foreach (var ch in characters)
				{
					var childElement = element.OwnerDocument.CreateElement("char");
					childElement.SetAttribute("value", (int)ch.character);
					element.AppendChild(childElement);
				}
			}
		}
	}
}

