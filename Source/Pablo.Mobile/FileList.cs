using Eto.Forms;
using Eto.IO;
using System;
using System.IO;
using System.Linq;
using Pablo;
using System.Collections.Generic;
using Eto.Threading;
using System.Threading;
using System.Diagnostics;

namespace Pablo.Mobile
{
	public class FileList : ListBox
	{
		EtoDirectoryInfo _directory;
		List<EtoSystemObjectInfo> items;
		readonly Settings settings;
		string _fileName;
		System.Threading.Thread thread;

		public FileList(Main main) : base(main.Generator)
		{
			settings = main.Settings;
		}

		public EtoFileInfo SelectedFile
		{
			get
			{
				if (SelectedIndex == -1)
					return null;
				var obj = items[SelectedIndex];
				return obj as EtoFileInfo;
			}
		}

		public EtoDirectoryInfo SelectedDirectory
		{
			get
			{
				if (SelectedIndex == -1)
					return null;
				var obj = items[SelectedIndex] as EtoDirectoryInfo;
				return obj != null && obj != _directory.Parent ? obj : null;
			}
		}

		public EtoDirectoryInfo CurrentDirectory
		{
			get { return _directory; }
		}

		public void Initialize(string fileName)
		{
			_directory = null;
			_fileName = fileName;
			LoadItems();
		}

		void LoadItems()
		{
			Enabled = false;
			Items.Clear();
			Items.Add("Loading...");
			thread = new System.Threading.Thread(LoadFile);
			thread.Start(this);
		}

		static void LoadFile(object obj)
		{
			var fileList = (FileList)obj;
			string directoryPath = null;
			if (!string.IsNullOrEmpty(fileList._fileName))
			{
				directoryPath = File.Exists(fileList._fileName) ? Path.GetDirectoryName(fileList._fileName) : fileList._fileName;
				
				if (!string.IsNullOrEmpty(directoryPath))
				{
					fileList._directory = EtoDirectoryInfo.GetDirectory(directoryPath);
				}
			}
			
			if (fileList._directory != null)
			{
				fileList.UpdateDirectory();
				if (directoryPath != fileList._fileName)
				{
					int index = fileList.items.FindIndex(r => r.FullName.Equals(fileList._fileName, StringComparison.InvariantCultureIgnoreCase));
					if (index >= 0)
					{
						Application.Instance.AsyncInvoke(() => fileList.SelectedIndex = index);
					}
				}
			}
			
		}

		public void Initialize(EtoDirectoryInfo directory)
		{
			_fileName = null;
			_directory = directory;
			LoadItems();
		}

		public void GoToNext()
		{
			if (SelectedIndex < Items.Count - 1)
			{
				SelectedIndex++;
			}
		}

		public void GoToPrevious()
		{
			if (SelectedIndex > 0)
			{
				SelectedIndex--;
			}
		}

		public void UpdateDirectory()
		{
			try
			{
				var list = this;
				var newitems = new List<EtoSystemObjectInfo>();
				
				newitems.AddRange(_directory.GetDirectories().Cast<EtoSystemObjectInfo>());
				var formats = settings.Infos.GetFormats();
				var extensions = formats.Values.SelectMany(r => r.Extensions).Select(s => "*." + s);
				newitems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase));
				
				var alFiles = _directory.GetFiles(extensions).ToList();
				alFiles.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase));
				newitems.AddRange(alFiles.Cast<EtoSystemObjectInfo>());
				
				if (_directory is DiskDirectoryInfo)
				{
					//newitems.Insert (0, new WebDav.WebDavBrowser (directory, "SixteenColors", "http://sixteencolors.picoe.ca"));
					newitems.Insert(0, new Sixteencolors.SixteenBrowser(_directory));
				}
	
				items = newitems;
				var theitems = new ListItemCollection();
				theitems.AddRange(items.Select(r => new ListItem { Text = r.Name }).Cast<IListItem>());
				Application.Instance.Invoke(delegate
				{
					if (!disposed)
					{
						DataStore = theitems;
						Enabled = true;
					}
				});
			}
			catch (Exception ex)
			{
				Application.Instance.AsyncInvoke(() => MessageBox.Show(Generator, this, string.Format("Unable to load the selected file ({0})", ex)));
				Console.WriteLine("Cannot load directory {0}", ex);
			}
		}

		bool disposed;

		protected override void Dispose(bool disposing)
		{
			disposed = true;
			base.Dispose(disposing);
		}
	}
}
