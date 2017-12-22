using System;

namespace Pablo.Sauce
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
	public class XBinDataTypeInfo : SauceDataTypeInfo
	{
		public XBinFileType Type
		{
			get { return (XBinFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}
	}
}
