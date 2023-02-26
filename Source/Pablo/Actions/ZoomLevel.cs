using System;
using System.Linq;
using Eto.Forms;

namespace Pablo.Actions
{
	public class ZoomLevel : RadioCommand, IUpdatableCommand
	{
		public ZoomLevel(RadioCommand controller, ViewerPane viewerPane, float zoomLevel)
		{
			ViewerPane = viewerPane;
			Zoom = zoomLevel;
			Controller = controller;
			ID = "zoom" + zoomLevel;
			MenuText = string.Format("{0}%", zoomLevel * 100);
		}

		public ViewerPane ViewerPane { get; set; }
		public float Zoom { get; set; }
		public void UpdateState()
		{
			Checked = Math.Abs(ViewerPane.ZoomInfo.Zoom - Zoom) < 0.0001f;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ZoomInfo.Zoom = Zoom;
			ViewerPane.UpdateSizes();
		}
	}
}

