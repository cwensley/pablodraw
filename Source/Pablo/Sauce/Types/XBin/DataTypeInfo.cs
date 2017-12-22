using System;

namespace Pablo.Sauce.Types.XBin
{
	public enum XBinFileType
	{
		XBin = 0,
		/* XBin format file
						 * TInfo1 is used for the width of the file. 
						 * TInfo2 is used to hold the number of lines in the file.
						 */
	}

	/// <summary>
	/// Summary description for XBinDataTypeInfo.
	/// </summary>
	public class DataTypeInfo : BaseText.DataTypeInfo
	{
		public override bool HasFileType { get { return false; } }

		public override bool HasFontName { get { return false; } }

		public override bool HasICEColors { get { return false; } }

		public override bool HasAspectRatio { get { return true; } }

		public override bool HasLetterSpacing { get { return true; } }

		public XBinFileType Type
		{
			get { return (XBinFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}
	}
}
