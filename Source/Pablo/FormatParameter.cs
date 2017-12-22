using System;
using System.Collections;
using System.Collections.Generic;

namespace Pablo
{
	public class FormatParameter
	{
		public FormatParameter (string id, string name, Type valueType, object val)
		{
			this.ID = id;
			this.Name = name;
			this.Type = valueType;
			this.Value = val;
			this.Enabled = true;
		}
		
		public string ID { get; set; }

		public string Name { get; set; }

		public object Value { get; set; }
		
		public Type Type { get; set; }
		
		public bool Enabled { get; set; }
	}

	public class FormatParameterCollection : List<FormatParameter>
	{
		public FormatParameter this [string id] {
			get {
				foreach (FormatParameter fp in this) {
					if (fp.ID == id)
						return fp;
				}
				return null;
			}
		}

		public bool Contains (string id)
		{
			foreach (FormatParameter fp in this) {
				if (fp.ID == id)
					return true;
			}
			return false;
		}
	}
}

