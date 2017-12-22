using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Sauce.Types.Vector
{
	public enum VectorFileType
	{
		DXF = 0,
		// (CAD Data eXchange File)
		DWG = 1,
		// (AutoCAD Drawing file)
		WPG = 2,
		// (WordPerfect/DrawPerfect vector graphics)
		ThreeDS = 3
		// (3D Studio file).
	}

	public class DataTypeInfo : BaseFileType.DataTypeInfo
	{
		public override IEnumerable<SauceFileTypeInfo> FileTypes
		{
			get
			{
				yield return new SauceFileTypeInfo{ Type = 0, Name = "DXF (CAD)" };
				yield return new SauceFileTypeInfo{ Type = 1, Name = "DWD (AutoCAD)" };
				yield return new SauceFileTypeInfo{ Type = 2, Name = "WPG (WordPerfect)" };
				yield return new SauceFileTypeInfo{ Type = 3, Name = "3DS (3D Studio)" };
			}
		}

		public VectorFileType Type
		{
			get { return (VectorFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}
	}
}
