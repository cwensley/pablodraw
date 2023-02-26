using System;
using System.Linq;
using Eto.Forms;

namespace Pablo.Actions
{
	public class ZoomReset : Command
	{
		public const string ActionID = "zoomReset";

		public ViewerPane ViewerPane { get; private set; }

		public ZoomReset(ViewerPane viewerPane)
		{
			this.ViewerPane = viewerPane;
			this.ID = ActionID;
			this.MenuText = "Reset Zoom";
			this.ToolTip = "Get back to normal";
			this.Shortcut = Application.Instance.CommonModifier | Keys.D0;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			ViewerPane.ZoomInfo.Zoom = 1;
			ViewerPane.UpdateMenuItems();
			ViewerPane.UpdateSizes();
		}
	}
}

