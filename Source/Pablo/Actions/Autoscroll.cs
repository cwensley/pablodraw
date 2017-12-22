using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class Autoscroll : CheckAction
	{
		public const string ActionID = "autoscroll";
		public ViewerPane ViewerPane { get; private set; }

		public Autoscroll (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			base.ID = ActionID;
			this.Text = "Auto&scroll|Autoscroll|Automaticaly scroll when viewing";
			this.Checked = ViewerPane.ViewHandler.Document.Info.AutoScroll;
		}

		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			this.Checked = !this.Checked;
			ViewerPane.ViewHandler.Document.Info.AutoScroll = this.Checked;
		}
	}
}

