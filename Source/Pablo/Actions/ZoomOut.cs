using System;
using System.Linq;
using Eto.Forms;

namespace Pablo.Actions
{
	public class ZoomOut : Command
	{
		public const string ActionID = "zoomOut";

		public ViewerPane ViewerPane { get; private set; }

		public ZoomOut(ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "Zoom &Out";
			this.ToolTip = "Make things smaller";
			this.Shortcut = Application.Instance.CommonModifier | Keys.Minus;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			var zoomInfo = ViewerPane.ZoomInfo;
			var nextZoom = Pablo.ViewerPane.ZOOM_LEVELS.Where(r => r < zoomInfo.Zoom).OrderByDescending(r => r).FirstOrDefault();
			if (nextZoom == 0)
				nextZoom = ViewerPane.Zoom * 0.5f;
			if (nextZoom > 0.02)
			{
				zoomInfo.Zoom = nextZoom;
				ViewerPane.UpdateMenuItems();
				ViewerPane.UpdateSizes();
			}
		}
	}
}

