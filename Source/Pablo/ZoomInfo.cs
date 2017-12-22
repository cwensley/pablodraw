using System;
using System.Xml;
using Eto;

namespace Pablo
{
	public class ZoomInfo
	{
		public float Zoom { get; set; }

		public bool FitWidth { get; set; }

		public bool FitHeight { get; set; }

		public bool AllowGrow { get; set; }

		public ZoomInfo()
		{
			Zoom = 1;
		}

		public virtual void ReadXml(XmlElement element)
		{
			Zoom = element.GetFloatAttribute("zoom") ?? 1;
			FitWidth = element.GetBoolAttribute("fitWidth") ?? false;
			FitHeight = element.GetBoolAttribute("fitHeight") ?? false;
			AllowGrow = element.GetBoolAttribute("allowGrow") ?? false;
		}

		public virtual void WriteXml(XmlElement element)
		{
			element.SetAttribute("zoom", Zoom);
			element.SetAttribute("fitWidth", FitWidth);
			element.SetAttribute("fitHeight", FitHeight);
			element.SetAttribute("allowGrow", AllowGrow);
		}
	}
}

