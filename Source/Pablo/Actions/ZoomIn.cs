using System;
using System.Linq;
using Eto.Forms;

namespace Pablo.Actions
{
	public class ZoomIn : Command
	{
		public const string ActionID = "zoomIn";

		public ViewerPane ViewerPane { get; private set; }

		public ZoomIn(ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "Zoom &In";
			this.ToolTip = "Make things bigger";
			this.Shortcut = Application.Instance.CommonModifier | Keys.Equal;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			var zoomInfo = ViewerPane.ZoomInfo;
			var nextZoom = Pablo.ViewerPane.ZOOM_LEVELS.Where(r => r > zoomInfo.Zoom).OrderBy(r => r).FirstOrDefault();
			if (nextZoom == 0)
				nextZoom = ViewerPane.Zoom + 0.5f;
			zoomInfo.Zoom = nextZoom;
			ViewerPane.UpdateMenuItems();
			ViewerPane.UpdateSizes();
		}
	}
}

