using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class FitWidth : CheckAction
	{
		public const string ActionID = "zoomFitWidth";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public FitWidth (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.Text = "Fit &Width|Fit Width|Fit the document to the width of the view";

		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			this.Checked = !this.Checked;
			ViewerPane.ZoomInfo.FitWidth = this.Checked;
			ViewerPane.UpdateSizes ();
		}
	}
}

