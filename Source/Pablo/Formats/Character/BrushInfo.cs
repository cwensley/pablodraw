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
			this.characters = characters.ToArray ();
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
			this.characters = bytes.Select (r => new Character(r)).ToArray();
		}
		
		public Character[] GetCharacters(Encoding encoding = null)
		{
			encoding = encoding ?? BitFont.StandardEncoding;
			
			if (characters == null && !string.IsNullOrEmpty(Gradient) && this.encoding != encoding) {
				this.encoding = encoding;
				var bytes = encoding.GetBytes (this.Gradient);
				characters = bytes.Select(r => new Character(r)).ToArray();
			}
			
			return characters;
		}
		
		public void ReadXml (XmlElement element)
		{
			this.Gradient = element.GetAttribute ("characters");
		}
		
		public void WriteXml (XmlElement element)
		{
			element.SetAttribute ("characters", this.Gradient);
		}
	}
}

