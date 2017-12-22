using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Sauce.Types.Bitmap
{
	public enum BitmapFileType
	{
		Gif = 0,
		// (CompuServe Graphics Interchange format)
		Pcx = 1,
		// (ZSoft Paintbrush PCX format)
		LbmIff = 2,
		// (DeluxePaint LBM/IFF format)
		Tga = 3,
		// (Targa Truecolor)
		Fli = 4,
		// (Autodesk FLI animation file)
		Flc = 5,
		// (Autodesk FLC animation file)
		Bmp = 6,
		// (Windows or OS/2 Bitmap)
		Gl = 7,
		// (Grasp GL Animation)
		Dl = 8,
		// (DL Animation)
		Wpg = 9,
		// (Wordperfect Bitmap)
		Png = 10,
		// (Portable Graphics)
		Jpeg = 11,
		// (JPeg compressed File)
		Mpeg = 12,
		// (MPeg compressed animation/video)
		Avi = 13,
		// (Audio Visual Interlace)
	}

	public class DataTypeInfo : BaseFileType.DataTypeInfo
	{
		public override IEnumerable<SauceFileTypeInfo> FileTypes
		{
			get
			{
				yield return new SauceFileTypeInfo{ Type = 0, Name = "GIF" };
				yield return new SauceFileTypeInfo{ Type = 1, Name = "PCX" };
				yield return new SauceFileTypeInfo{ Type = 2, Name = "LBM/IFF" };
				yield return new SauceFileTypeInfo{ Type = 3, Name = "TGA" };
				yield return new SauceFileTypeInfo{ Type = 4, Name = "FLI" };
				yield return new SauceFileTypeInfo{ Type = 5, Name = "FLC" };
				yield return new SauceFileTypeInfo{ Type = 6, Name = "BMP" };
				yield return new SauceFileTypeInfo{ Type = 7, Name = "GL" };
				yield return new SauceFileTypeInfo{ Type = 8, Name = "DL" };
				yield return new SauceFileTypeInfo{ Type = 9, Name = "WPG" };
				yield return new SauceFileTypeInfo{ Type = 10, Name = "PNG" };
				yield return new SauceFileTypeInfo{ Type = 11, Name = "JPG" };
				yield return new SauceFileTypeInfo{ Type = 12, Name = "MPG" };
				yield return new SauceFileTypeInfo{ Type = 13, Name = "AVI" };
			}
		}

		public BitmapFileType Type
		{
			get { return (BitmapFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}
	}
}
