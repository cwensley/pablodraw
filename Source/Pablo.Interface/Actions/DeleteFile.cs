using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Eto;

namespace Pablo.Interface.Actions
{
	public class DeleteFile : Command, IDisposable
	{
		Main main;

		static int count = 0;
		int current;
		public const string ActionID = "deleteFile";

		public DeleteFile (Main main)
		{
			current = count++;
			this.main = main;
			base.ID = ActionID;
			this.MenuText = "&Delete";
			this.ToolTip = "Delete the selected file";
			this.Enabled = main.FileList.SelectedFile != null;
			if (Platform.Instance.IsMac) this.Shortcut = PabloCommand.CommonModifier | Keys.Backspace;
			else this.Shortcut = PabloCommand.CommonModifier | Keys.Delete;
			
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
			if (main.Document == null) return;
			var file = main.Document.FileName;
			if (File.Exists(file)) {
				var result = MessageBox.Show(main, "Are you sure you want to delete the selected file?", MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes)
				{
					File.Delete(file);
					main.FileList.UpdateDirectory();
				}
			}
			
		} 
	}
}

