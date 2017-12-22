using System;
using Eto.Forms;
using Pablo.Network;

namespace Pablo.Interface.Actions
{
	public class EnableBackups : RadioCommand
	{
		readonly IClientDelegate clientDelegate;
		public const string ActionID = "EnableBackups";

		public EnableBackups(IClientDelegate clientDelegate)
			: base(null)
		{
			this.clientDelegate = clientDelegate;
			ID = ActionID;
			MenuText = "Enable Backups";
			Checked = clientDelegate.EnableBackups;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			clientDelegate.EnableBackups = Checked;
		}
	}
}

