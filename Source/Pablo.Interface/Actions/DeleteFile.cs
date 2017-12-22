using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Eto;

namespace Pablo.Interface.Actions
{
	public class DeleteFile : ButtonAction
	{
		Main main;
		
		public const string ActionID = "deleteFile";

		public DeleteFile (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Delete|Delete|Delete the selected file";
			this.Enabled = main.FileList.SelectedFile != null;
			if (main.Generator.IsMac) this.Accelerator = Command.CommonModifier | Key.Backspace;
			else this.Accelerator = Command.CommonModifier | Key.Delete;
			
			main.FileList.SelectedIndexChanged += fileList_Changed;
		}
		
		protected override void OnRemoved (EventArgs e)
		{
			base.OnRemoved (e);
			main.FileList.SelectedIndexChanged -= fileList_Changed;
		}

		void fileList_Changed(object sender, EventArgs e)
		{
			this.Enabled = main.FileList.SelectedFile != null;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
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

