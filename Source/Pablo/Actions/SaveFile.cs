using System;
using System.Collections;
using System.IO;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

namespace Pablo.Actions
{
	public class SaveFile : Command
	{
		readonly Handler handler;
		public const string ActionID = "save";

		public SaveFile(Handler handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.MenuText = "&Save";
			this.ToolBarText = "Save";
			this.ToolTip = "Saves the current file";
			this.Image = ImageCache.IconFromResource("Pablo.Icons.save.ico");
			this.Shortcut = PabloCommand.CommonModifier | Keys.S;
		}

		protected override void OnExecuted(EventArgs e)
		{
			var file = handler.Document.FileName;
			var shouldSave = string.IsNullOrEmpty(file) || !File.Exists(file) || File.GetAttributes(file).HasFlag(FileAttributes.ReadOnly);
			bool useCurrentFile = false;
#if MAS
			useCurrentFile = !shouldSave;
			shouldSave |= !handler.Document.HasSavePermission;
#endif
			if (shouldSave)
			{
				SaveAs.Activate(handler, useCurrentFile);
			}
			else
			{
				try
				{
					handler.SaveWithBackup(file, handler.Document.LoadedFormat);
					handler.Document.IsModified = false;
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("Error saving file: {0}", ex.Message), MessageBoxButtons.OK, MessageBoxType.Error);
					#if DEBUG
					throw;
					#endif
				}
			}
		}
	}
}

