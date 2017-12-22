using Eto.Forms;
using System;

namespace Pablo.Formats.Rip.Actions
{
	public class Animate : ButtonAction
	{
		//RipHandler handler;
		
		public const string ActionID = "rip_Animate";
		
		public Animate(RipHandler handler)
		{
			//this.handler = handler;
			ID = ActionID;
			Text = "Animate";
			Description = "Animate";
			Accelerator = Key.Alt | Key.A;
		}

		
		protected override void OnActivated (EventArgs e)
		{
			// start thread to animate!
		}
	}
}
