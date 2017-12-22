using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class AllowGrow : CheckCommand
	{
		public const string ActionID = "zoomAllowGrow";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public AllowGrow (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "&Expand to Fit";
			this.ToolTip = "Make the document expand to fit the view if it is smaller";

		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ZoomInfo.AllowGrow = this.Checked;
			ViewerPane.UpdateSizes ();
		}
	}
}

