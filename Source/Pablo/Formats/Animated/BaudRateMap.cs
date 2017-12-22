using System;
using System.Collections;
using System.Collections.Generic;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Animated
{
	public class BaudRateMap
	{
		public RadioCommand Command { get; set; }

		public long Baud { get; set; }

		public BaudRateMap(BaudRateMapCollection coll, string desc, long baud)
		{
			Command = new RadioCommand { ID = "baud" + baud, MenuText = desc, Tag = this };
			if (coll.Count > 0)
				Command.Controller = coll[0].Command;
			Command.Executed += coll.ActionChecked;
			Baud = baud;
		}
	}

	public class BaudRateMapCollection : List<BaudRateMap>
	{
		public event EventHandler<EventArgs> BaudChanged;

		internal void ActionChecked(object sender, EventArgs e)
		{
			if (BaudChanged != null)
				BaudChanged(sender, e);
		}

		public void Add(string desc, long baud)
		{
			Add(new BaudRateMap(this, desc, baud));
		}

		public void Add(long baud)
		{
			Add(new BaudRateMap(this, baud.ToString(), baud));
		}

		public void AddRange(long[] values)
		{
			foreach (long val in values)
			{
				Add(val);
			}
		}
	}
}
