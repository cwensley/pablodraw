using System;
using Eto.Forms;

namespace Pablo.Interface.Dialogs
{
	public class FileModifiedDialog
	{
		Main main;

		public FileModifiedDialog (Main main)
		{
			this.main = main;
		}
		
		public static DialogResult Show (Main main)
		{
			var dlg = new FileModifiedDialog (main);
			return dlg.ShowDialog ();
		}

		public DialogResult ShowDialog ()
		{
			if (main.Document != null && (main.Document.IsModified || (main.EditMode && string.IsNullOrEmpty (main.Document.FileName)))) {
				var result = MessageBox.Show (main, "Do you wish to save?", "There are changes to this document", MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Yes || result == DialogResult.No) {
					if (result == DialogResult.Yes) {
						var action = new Pablo.Actions.SaveFile(main.ViewHandler);
						action.Execute();
					}
					return DialogResult.Ok;
				} else
					return DialogResult.Cancel;
			}
			return DialogResult.Ok;
			
		}
	}
}

