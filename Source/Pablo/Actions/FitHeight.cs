using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class FitHeight : CheckAction
	{
		public const string ActionID = "zoomFitHeight";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public FitHeight (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.Text = "Fit &Height|Fit Height|Fit the document to the height of the view";

		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			this.Checked = !this.Checked;
			ViewerPane.ZoomInfo.FitHeight = this.Checked;
			ViewerPane.UpdateSizes ();
		}
	}
}

