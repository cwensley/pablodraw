using System;
using System.IO;
using System.Reflection;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class Readme : Command
	{
        Main main;
		public const string ActionID = "readme";
		
		public Readme(Main main)
		{
            this.main = main;
			this.MenuText = "View Readme";
			base.ID = ActionID;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			using (var stream = typeof(Readme).Assembly.GetManifestResourceStream("Pablo.Interface.README.ans"))
			{
				main.LoadFile("README.ans", stream, hasSavePermissions: false, setFileList: false, editMode: false);
			}
		}
	}
}

