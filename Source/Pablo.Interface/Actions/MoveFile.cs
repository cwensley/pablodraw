using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Pablo.Interface.Dialogs;
using Eto.IO;
using Eto;

namespace Pablo.Interface.Actions
{
	public class MoveFile : Command, IDisposable
	{
		Main main;
		
		public const string ActionID = "moveFile";
		
		public MoveFile (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.MenuText = "&Move...";
			this.ToolTip = "Move the selected file to another folder";
			this.Enabled = main.FileList.SelectedFile != null;
			this.Shortcut = PabloCommand.CommonModifier | Keys.Shift | Keys.M;
			main.FileList.SelectedIndexChanged += fileList_Changed;
		}

		public void Dispose()
		{
			main.FileList.SelectedIndexChanged -= fileList_Changed;
		}
		
		void fileList_Changed(object sender, EventArgs e)
		{
			this.Enabled = main.FileList.SelectedFile != null;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			var file = main.FileList.SelectedFile;
			if (file != null && File.Exists(file.FullName)) {
				var dialog = new SelectFolderDialog();
				var result = dialog.ShowDialog(null);
				if (result == DialogResult.Ok)
				{
					File.Move(file.FullName, Path.Combine(dialog.Directory, Path.GetFileName(file.FullName)));
					main.FileList.UpdateDirectory();
				}
			}
		} 
	}
}

