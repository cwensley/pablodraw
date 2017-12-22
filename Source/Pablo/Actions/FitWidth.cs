using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class FitWidth : CheckCommand
	{
		public const string ActionID = "zoomFitWidth";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public FitWidth (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "Fit &Width";
			this.ToolTip = "Fit the document to the width of the view";

		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ZoomInfo.FitWidth = this.Checked;
			ViewerPane.UpdateSizes ();
		}
	}
}

