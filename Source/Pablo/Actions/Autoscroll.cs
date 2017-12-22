using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class Autoscroll : CheckCommand
	{
		public const string ActionID = "autoscroll";
		public ViewerPane ViewerPane { get; private set; }

		public Autoscroll (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			base.ID = ActionID;
			this.MenuText = "Auto&scroll";
			this.ToolTip = "Automaticaly scroll when viewing";
			this.Checked = ViewerPane.ViewHandler.Document.Info.AutoScroll;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ViewHandler.Document.Info.AutoScroll = this.Checked;
		}
	}
}

