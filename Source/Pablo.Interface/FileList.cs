using Eto.Forms;
using Eto.IO;
using System;
using System.IO;
using System.Linq;
using Pablo;
using System.Collections.Generic;

namespace Pablo.Interface
{
	public class FileList : ListBox
	{
		EtoDirectoryInfo directory;
		List<EtoSystemObjectInfo> items;
		readonly Settings settings;
		EtoDirectoryInfo olddir;
		bool? hasfocus;
		string newFileName;


		public FileList(Main main)
			: base(main.Generator)
		{
			this.settings = main.Settings;
			this.Style = "fileList";
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
				if (obj != null && obj != directory.Parent)
					return obj;
				return null;
			}
		}

		public EtoDirectoryInfo CurrentDirectory
		{
			get { return directory; }
		}

		public void Initialize(string fileName, bool forceReload = false)
		{
			var directoryPath = !File.Exists(fileName) ? fileName : Path.GetDirectoryName(fileName);

			if (!string.IsNullOrEmpty(directoryPath))
			{
				if (directoryPath != fileName)
				{
					newFileName = fileName;
				}
				Initialize(EtoDirectoryInfo.GetDirectory(directoryPath), forceReload);
			}
		}

		public void Initialize(EtoDirectoryInfo directory, bool forceReload = false)
		{
			if (forceReload || this.directory != directory)
			{
				olddir = this.directory;
				UpdateDirectory(directory);
			}
			else
				SetFileName();
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

		public void GoToParent()
		{
			if (directory != null && directory.Parent != null)
				Initialize(directory.Parent);
		}

		void SetFileName()
		{
			if (!string.IsNullOrEmpty(newFileName))
			{
				int index = items.FindIndex(r => r.FullName.Equals(newFileName, StringComparison.InvariantCultureIgnoreCase));
				if (index >= 0)
					SelectedIndex = index;
			}
			newFileName = null;
		}

		void Populate()
		{
			var listitems = new ListItemCollection();
			listitems.AddRange(items.Select(r => r.Equals(directory.Parent) ? ".." : r.Name).Select(r => new ListItem { Text = r }));
			DataStore = listitems;

			if (!string.IsNullOrEmpty(newFileName))
			{
				SetFileName();
			}
			else if (olddir != null && olddir.Parent != null && olddir.Parent.Equals(directory))
			{
				int index = items.IndexOf(olddir);
				if (index >= 0)
					SelectedIndex = index;
			}
			olddir = null;
		}

		void PerformUpdate(object state)
		{
			var directory = (EtoDirectoryInfo)state;
			try
			{
				var newitems = new List<EtoSystemObjectInfo>();

				newitems.AddRange(directory.GetDirectories());
				var formats = settings.Infos.GetFormats();

				var extensions = formats.Values.Where(r => r.CanLoad).SelectMany(s => s.Extensions).Select(r => "*." + r).Distinct();
				newitems.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase));

				var alFiles = directory.GetFiles(extensions).ToList();
				alFiles.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase));
				newitems.AddRange(alFiles);

				if (directory is DiskDirectoryInfo)
				{
					newitems.Insert(0, new Sixteencolors.SixteenBrowser(directory));

					//newitems.Insert (0, new WebDav.WebDavBrowser (directory, "SixteenColors", "https://github.com/sixteencolors/sixteencolors-archive.git"));
					//newitems.Insert (0, new WebDav.WebDavBrowser (directory, "SixteenColors", "http://sixteencolors.picoe.ca"));
				}

				var parent = directory.Parent;
				if (parent != null)
					newitems.Insert(0, parent);

				items = newitems;

				Application.Instance.Invoke(delegate
				{
					Enabled = true;
					this.directory = directory;
					Populate();
					if (hasfocus != null && hasfocus.Value) Focus();
					hasfocus = null;
				});


			}
			catch (Exception ex)
			{
				Application.Instance.Invoke(delegate
				{
					Enabled = true;
					MessageBox.Show(this, string.Format("Error loading directory contents\n{0}", ex));
					Populate();
				});
			}

		}

		public void UpdateDirectory(EtoDirectoryInfo directory = null)
		{
			directory = directory ?? this.directory;
			hasfocus = HasFocus;
			if (Generator.IsWinForms && this.Loaded) // hack since winforms doesn't set focus if disabled
				Enabled = false;
			var listitems = new ListItemCollection();
			listitems.Add(new ListItem { Text = "Loading..." });
			DataStore = listitems;
			var task = new System.Threading.Tasks.Task(new Action<object>(PerformUpdate), directory);
			task.Start();


		}

		public override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			int index = SelectedIndex;
			if (index >= 0)
			{
				var dir = items[index] as EtoDirectoryInfo;
				if (dir != null)
				{
					Initialize(dir);
				}
			}
		}

	}
}
