using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using Pablo.Interface.Dialogs;
using System.Collections.Generic;

namespace Pablo.Interface.Actions
{
	public class OpenFile : ButtonAction
	{
		Main main;
		
		public const string ActionID = "openFile";
		
		public OpenFile(Main main)
		{
			this.main = main;
			base.ID = ActionID;
			this.Text = "&Open|Open|Open a file";
			this.Image = Icon.FromResource("Pablo.Interface.Icons.open.ico");
			this.Accelerator = Command.CommonModifier | Key.O;
			this.Enabled = main.Client == null || main.Client.CurrentUser.Level >= Pablo.Network.UserLevel.Operator;
		}
		
		
		protected override void OnActivated(EventArgs e)
		{
			var ofd = new OpenFileDialog(main.Generator);
			ofd.Title = "Select the file to open";
			var filters = new List<IFileDialogFilter>();
			var formats = main.Settings.Infos.GetFormats();
			var allFormats = new List<string>();
			foreach (Format format in formats.Values)
			{
				if (format.CanLoad)
				{
					var extensions = from ex in format.Extensions select "." + ex;
					allFormats.AddRange (extensions);
					filters.Add(new FileDialogFilter{ Name = format.Name, Extensions = extensions.ToArray () });
				}
			}
			filters.Insert(0, new FileDialogFilter{ Name = "All Formats", Extensions = allFormats.ToArray() });
			ofd.Filters = filters;

			var dr = ofd.ShowDialog(main);
			if (dr == DialogResult.Ok)
			{
				if (FileModifiedDialog.Show(main) == DialogResult.Ok) {
					Format format = (ofd.CurrentFilterIndex > 0) ? formats.Values.ElementAtOrDefault(ofd.CurrentFilterIndex-1) : null;
					if (format == null) format = formats.Find(ofd.FileName, string.Empty);
					if (format == null) MessageBox.Show(main.Generator, main, "Cannot determine format to open based on file extension.");
	
					if (format != null)
					{
						var stream = System.IO.File.OpenRead(ofd.FileName);
						main.LoadFile(ofd.FileName, stream, format, main.EditMode, true, true);
						// don't close stream as it could be used in background..
					}
					
				}
			}
		}
	}
}
