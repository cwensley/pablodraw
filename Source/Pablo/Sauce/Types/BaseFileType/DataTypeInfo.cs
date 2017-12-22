using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Sauce.Types.BaseFileType
{
	public class DataTypeInfo : SauceDataTypeInfo
	{
		public SauceFileTypeInfo FileType
		{
			get { return FileTypes.FirstOrDefault(r => r.Type == Sauce.ByteFileType); }
			set
			{
				Sauce.ByteFileType = value.Type;
			}
		}

		public virtual IEnumerable<SauceFileTypeInfo> FileTypes
		{
			get
			{
				yield break;	
			}
		}

		public override Control GenerateUI()
		{
			return new Admin<DataTypeInfo>(this);
		}
	}
}

