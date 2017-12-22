using System;
using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Character
{
	public abstract class CharacterFormat : Pablo.Formats.Animated.AnimatedFormat
	{
		protected CharacterFormat(DocumentInfo info, string id, string name, params string[] extensions) : base(info, id, name, extensions)
		{
		}
		
		protected virtual int DefaultWidth
		{
			get { return 80; }
		}
		
		protected virtual int GetWidth(Stream stream, CharacterDocument document, object state = null)
		{
			int width = DefaultWidth;
			var sauce = document.Sauce;
			if (sauce != null) {
				var charType = sauce.TypeInfo as Sauce.Types.Character.DataTypeInfo;
				if (charType != null && charType.Width > 0)
				{
					width = charType.Width;
				}
			}
			return width;
		}
		
		public abstract void Load(Stream fs, CharacterDocument document, CharacterHandler handler);
		
		public virtual void ResizeCanvasWidth(Stream stream, CharacterDocument document, Canvas canvas, object state = null)
		{
			if (document.ResizeCanvas) {
				var width = GetWidth(stream, document, state);
				canvas.ResizeCanvas(new Size(width, canvas.Height), false);
			}
		}
		
		public void ResizeCanvasHeight(CharacterDocument document, Canvas canvas, int? height = null)
		{
			if (document.ResizeCanvas) {
				if (height == null) height = canvas.FindEndY(CanvasElement.Default) + 1;
				canvas.ResizeCanvas(new Size(canvas.Width, height.Value), true);
			}
		}
		
		public virtual void Save (Stream stream, CharacterDocument document)
		{
			
		}

		public virtual void EnsureSauce(CharacterDocument document)
		{
			var sauce = document.Sauce;
			if (sauce == null && RequiresSauce(document))
			{
				sauce = new Sauce.SauceInfo();
			}
			if (sauce != null)
			{
				FillSauce(sauce, document);
			}
			document.Sauce = sauce;
		}

		protected virtual void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			sauce.DataType = GetSauceDataType(document);
		}

		protected void FillSauceSize(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			var charInfo = sauce.TypeInfo as Sauce.Types.Character.DataTypeInfo;
			if (charInfo != null)
			{
				charInfo.Width = (ushort)document.Pages[0].Canvas.Size.Width;
				charInfo.Height = (ushort)(document.Pages[0].Canvas.FindEndY(CanvasElement.Default) + 1);
			}
			/*var binaryInfo = sauce.TypeInfo as Sauce.Types.Binary.DataTypeInfo;
			if (binaryInfo != null)
			{
				binaryInfo.Width = document.Pages[0].Canvas.Size.Width;
			}*/
		}

		protected void FillFlags(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			var info = sauce.TypeInfo as Sauce.Types.BaseText.DataTypeInfo;
			if (info != null)
			{
				info.ICEColors = info.HasICEColors && document.ICEColours;
				info.AspectRatio = info.HasAspectRatio ? (bool?)document.DosAspect : null;
				info.LetterSpacing = info.HasLetterSpacing ? (bool?)(document.Pages[0].Font.Width == 9) : null;
				info.FontName = info.HasFontName ? document.Pages[0].Font.SauceID : null;
			}
		}


		public virtual Sauce.SauceDataType GetSauceDataType(CharacterDocument document)
		{
			return Sauce.SauceDataType.Character;
		}

		public virtual bool RequiresSauce(CharacterDocument document)
		{
			return false;
		}

		public virtual bool? Use9pxFont
		{
			get { return null; }
		}
	}
}
