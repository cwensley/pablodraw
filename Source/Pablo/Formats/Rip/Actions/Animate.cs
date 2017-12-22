using Eto.Forms;
using System;

namespace Pablo.Formats.Rip.Actions
{
	public class Animate : Command
	{
		//RipHandler handler;
		
		public const string ActionID = "rip_Animate";
		
		public Animate(RipHandler handler)
		{
			//this.handler = handler;
			ID = ActionID;
			MenuText = "Animate";
			Shortcut = Keys.Alt | Keys.A;
		}


		protected override void OnExecuted(EventArgs e)
		{
			// start thread to animate!
		}
	}
}
