using Eto.Forms;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Actions
{
	public class SaveAs : Command
	{
		readonly Handler handler;

		public SaveAs(Handler handler)
		{
			this.handler = handler;
			this.ID = "saveas";
			this.MenuText = "&Save As...";
			this.ToolBarText = "Save As";
			this.ToolTip = "Saves a copy of the current file";
			//this.Accelerator = Key.Alt | Key.S;
			this.Shortcut = PabloCommand.CommonModifier | Keys.Shift | Keys.S;
		}

		public static void Activate(Handler handler, bool setCurrent = false)
		{
			var ofd = new SaveFileDialog();
			ofd.Title = "Specify the file to save";
			var compatibleFormats = handler.Document.Info.GetCompatibleDocuments().GetFormats();
			var formats = compatibleFormats.Values.Where(r => r.CanSave).ToList();
			var allFormats = new List<string>();
			foreach (Format format in formats)
			{
				allFormats.AddRange(format.Extensions);
				ofd.Filters.Add(new FileFilter{ Name = format.Name, Extensions = format.Extensions });
			}
			ofd.Filters.Insert(0, new FileFilter{ Name = "Auto Detect", Extensions = allFormats.ToArray() });
			if (!handler.Generator.IsMac || setCurrent)
			{
				if (!string.IsNullOrEmpty(handler.Document.FileName) && handler.Document.HasSavePermission)
				{
					if (Path.IsPathRooted(handler.Document.FileName))
					{
						var dir = Path.GetDirectoryName(handler.Document.FileName);
						if (!string.IsNullOrEmpty(dir))
							ofd.Directory = new Uri(dir);
					}
				}
			}
			if (setCurrent)
			{
				ofd.FileName = Path.GetFileName(handler.Document.FileName);
				if (handler.Document.LoadedFormat != null)
				{
					ofd.CurrentFilterIndex = formats.FindIndex(r => r.ID == handler.Document.LoadedFormat.ID) + 1;
				}
			}
			
			var dr = ofd.ShowDialog(handler.ViewerControl.ParentWindow ?? Application.Instance.MainForm);
			if (dr == DialogResult.Ok)
			{
				var fileName = ofd.FileName;
				Format format = (ofd.CurrentFilterIndex > 0) ? formats[ofd.CurrentFilterIndex - 1] : null;
				//Console.WriteLine("Saving as format {0}, index:{1}", format, ofd.CurrentFilterIndex);
				if (format == null)
				{
					format = compatibleFormats.Find(fileName);
					if (format == null)
						format = handler.Document.LoadedFormat;
					if (format == null)
						format = handler.Document.Info.DefaultFormat;
					
					if (format != null && string.IsNullOrEmpty(Path.GetExtension(fileName).Trim('.')))
					{
						fileName = fileName + "." + format.Extensions[0];
						
					}
				}
				//Console.WriteLine("Saving as format {0}", format);
				if (format == null)
					MessageBox.Show("Cannot find format to save based on file extension");
				else
				{
					try
					{
						handler.SaveWithBackup(fileName, format);

						if (format.Info == handler.Document.Info && format.CanLoad)
						{
							handler.Document.FileName = fileName;
							handler.Document.HasSavePermission = true;
							handler.Document.LoadedFormat = format;
						}
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

		protected override void OnExecuted(EventArgs e)
		{
			Activate(handler);
		}
	}
}

