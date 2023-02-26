using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class FitHeight : CheckCommand, IUpdatableCommand
	{
		public const string ActionID = "zoomFitHeight";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public FitHeight (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "Fit &Height";
			this.ToolTip = "Fit the document to the height of the view";

		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ZoomInfo.FitHeight = this.Checked;
			ViewerPane.UpdateSizes ();
		}

		public void UpdateState()
		{
			Checked = ViewerPane.ZoomInfo.FitHeight;
		}
	}
}

