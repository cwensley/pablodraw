using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;

namespace Pablo.Sauce
{
	public class SauceFileTypeInfo : IListItem
	{
		public byte Type { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}

		#region IListItem implementation

		string IListItem.Text
		{
			get { return Name; }
			set { }
		}

		string IListItem.Key
		{
			get { return Convert.ToString(Type); }
		}

		#endregion

	}

	public class SauceDataTypeInfo
	{
		SauceInfo sauce;

		public SauceInfo Sauce
		{
			get { return sauce; }
			internal set
			{
				sauce = value;
				Initialize();
			}
		}

		public virtual bool IsValid
		{
			get { return true; }
		}

		protected virtual void Initialize()
		{
		}

		public virtual IEnumerable<SauceFlag> Flags
		{
			get
			{
				yield break;
			}
		}

		public virtual Control GenerateUI()
		{
			// no ui!
			return null;
		}
	}
}
