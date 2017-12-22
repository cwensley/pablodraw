using System;
using Eto;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Formats.Rip
{
	/// <summary>
	/// Summary description for RipDocumentInfo.
	/// </summary>
	public class RipDocumentInfo : Animated.AnimatedDocumentInfo
	{

		public const string DocumentID = "rip";

		public RipDocumentInfo()
			: base(DocumentID, "RIPscrip Document")
		{
			Formats.Add(new FormatRip(this));
			ZoomInfo.FitWidth = true;
			ZoomInfo.FitHeight = true;
#if MOBILE
//			this.AnimationEnabled = false;
#endif
		}

		public bool DosAspect { get; set; }

		public override IEnumerable<DocumentInfoOption> Options
		{
			get { return base.Options.Concat(GetOptions()); }
		}

		IEnumerable<DocumentInfoOption> GetOptions()
		{
			yield return new DocumentInfoOption { ID = "aspect", Comment = "Scales the output to dos aspect", Values = new string[] { "dos", "none" } };
		}

		public override bool SetOption(string option, string value)
		{
			switch (option.ToLowerInvariant())
			{
				case "aspect":
					switch (value.ToLowerInvariant())
					{
						case "dos":
							this.DosAspect = true;
							break;
						case "none":
							this.DosAspect = false;
							break;
					}
					break;
			}
			return base.SetOption(option, value);
		}

		public override Format DefaultFormat
		{
			get { return Formats["rip"]; }
		}

		public override Document Create(Platform generator)
		{
			Document doc = new RipDocument(this);
			doc.Generator = generator;
			return doc;
		}

		protected override void GetCompatibleDocuments(DocumentInfoCollection documentInfos)
		{
			base.GetCompatibleDocuments(documentInfos);
			documentInfos.Add(new Image.ImageDocumentInfo());
		}

		public override void ReadXml(System.Xml.XmlElement element)
		{
			base.ReadXml(element);
			DosAspect = element.GetBoolAttribute("dosAspect") ?? false;
		}

		public override void WriteXml(System.Xml.XmlElement element)
		{
			base.WriteXml(element);
			if (DosAspect) element.SetAttribute("dosAspect", DosAspect);
		}

		public override bool CanEdit
		{
			get { return true; }
		}
	}
}
