using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class Homepage : Command
	{
		public const string ActionID = "homepage";
		
		public Homepage ()
		{
			this.MenuText = "PabloDraw Website";
			base.ID = ActionID;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			PabloApplication.Instance.Open("https://github.com/cwensley/pablodraw");
		}
	}
}

