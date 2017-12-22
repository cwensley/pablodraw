using System;
using Eto.Forms;

namespace Pablo.Actions
{
	public class AllowGrow : CheckAction
	{
		public const string ActionID = "zoomAllowGrow";
		
		public ViewerPane ViewerPane { get; private set; }
		
		public AllowGrow (ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.Text = "&Expand to Fit|Expand to Fit|Make the document expand to fit the view if it is smaller";

		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			this.Checked = !this.Checked;
			ViewerPane.ZoomInfo.AllowGrow = this.Checked;
			ViewerPane.UpdateSizes ();
		}
	}
}

