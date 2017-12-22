using System;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Sauce.Types.Archive
{
	public enum ArchiveFileType
	{
		ZIP = 0, // (PKWare)
		ARJ, // (Robert K. Jung)
		LZH, // (Haruyasu Yoshizaki (Yoshi))
		ARC, // (SEA)
		TAR, // (Unix TAR format)
		ZOO,
		RAR,
		UC2,
		PAK,
		SQZ
	}

	public class DataTypeInfo : BaseFileType.DataTypeInfo
	{
		public override IEnumerable<SauceFileTypeInfo> FileTypes {
			get {
				yield return new SauceFileTypeInfo{ Type = 0, Name = "ZIP" };
				yield return new SauceFileTypeInfo{ Type = 1, Name = "ARJ" };
				yield return new SauceFileTypeInfo{ Type = 2, Name = "LZH" };
				yield return new SauceFileTypeInfo{ Type = 3, Name = "ARC" };
				yield return new SauceFileTypeInfo{ Type = 4, Name = "TAR" };
				yield return new SauceFileTypeInfo{ Type = 5, Name = "ZOO" };
				yield return new SauceFileTypeInfo{ Type = 6, Name = "RAR" };
				yield return new SauceFileTypeInfo{ Type = 7, Name = "UC2" };
				yield return new SauceFileTypeInfo{ Type = 8, Name = "PAK" };
				yield return new SauceFileTypeInfo{ Type = 9, Name = "SQZ" };
			}
		}

		public ArchiveFileType Type {
			get {
				return (ArchiveFileType)Sauce.ByteFileType;
			}
			set { Sauce.ByteFileType = (byte)value; }
		}
		
	}
}
