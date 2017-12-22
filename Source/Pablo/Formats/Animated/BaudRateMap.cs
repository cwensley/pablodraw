using System;
using System.Collections;
using System.Collections.Generic;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Animated
{
	public class BaudRateMap
	{
		public RadioAction action;
		public long		baud;
		public bool		isDefault;

		public BaudRateMap(BaudRateMapCollection coll, string desc, long baud)
		{
			RadioAction controller = null;
			if (coll.Count > 0) controller = coll[0].action;
			action = coll.Actions.AddRadio(controller, "baud"+baud, desc);//, coll.ActionChecked);
			action.Activated += coll.ActionChecked;
			action.Tag = this;
			this.baud = baud;
			this.isDefault = false;
		}

		public BaudRateMap(BaudRateMapCollection coll, long baud) : this(coll, baud, false)
		{
				
		}

		public BaudRateMap(BaudRateMapCollection coll, long baud, bool isDefault)
		{
			RadioAction controller = null;
			if (coll.Count > 0) controller = coll[0].action;
			action = coll.Actions.AddRadio(controller, "baud"+baud, baud.ToString());//, coll.ActionChecked);
			action.Activated += coll.ActionChecked;
			action.Tag = this;
			this.baud = baud;
			this.isDefault = isDefault;
		}

	}


	public class BaudRateMapCollection : List<BaudRateMap>
	{
		ActionCollection actions;

		public event EventHandler<EventArgs> BaudChanged;


		public BaudRateMapCollection(ActionCollection actions)
		{
			this.actions = actions;
		}

		public ActionCollection Actions
		{
			get { return actions; }
		}

		internal void CheckedChanged(object sender, EventArgs e)
		{
		}

		internal void ActionChecked(object sender, EventArgs e)
		{
			if (BaudChanged != null) BaudChanged(sender, e);
		}

		public void Add(string desc, long baud)
		{
			Add(new BaudRateMap(this, desc, baud));
		}

		public void Add(long baud)
		{
			Add(new BaudRateMap(this, baud));
		}
		
		public void AddRange(long[] values)
		{
			foreach (long val in values)
			{
				Add(val);
			}
		}

		public void Add(long baud, bool isDefault)
		{
			Add(new BaudRateMap(this, baud, isDefault));
		}
	}
}
