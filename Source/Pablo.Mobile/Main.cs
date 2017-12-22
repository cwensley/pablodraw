using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.IO;
using Eto;
using System.IO;

namespace Pablo.Mobile
{
	public class Main : Form
	{
		Handler handler;
		
		public Settings Settings { get; set; }
		
		public Panel ViewerPane { get; set; }
		
		Navigation ListNavigation { get; set; }
		
		bool UsingSplitter { get; set; }
		
		public Main()
		{
			Settings = new Settings();

			var listBox = new FileList(this);
			listBox.Initialize(EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents));
			listBox.SelectedIndexChanged += listBox_SelectDirectory;
						
			ListNavigation = new Navigation(listBox, "PabloDraw");
			
			
			if (Splitter.IsSupported)
			{
				ViewerPane = new Panel();
				
				var split = new Splitter();
				split.Panel1 = ListNavigation;
				
				split.Panel2 = ViewerPane;
				UsingSplitter = true;

				this.Content = split;
			}
			else
			{
				this.Content = ListNavigation;
			}
		}
		
		void listBox_SelectDirectory(object sender, EventArgs e)
		{
			var listBox = sender as FileList;
			if (listBox.SelectedDirectory != null)
			{
				var subFileList = new FileList(this);
				subFileList.Initialize(listBox.SelectedDirectory);
				subFileList.SelectedIndexChanged += listBox_SelectDirectory;
				ListNavigation.Push(subFileList, subFileList.CurrentDirectory.Name);
			}
			else if (listBox.SelectedFile != null)
			{
				LoadFile(listBox.SelectedFile);
			}
			
		}
		
		
		void Document_Loaded (object sender, EventArgs e)
		{
			if (loadingStream != null) {
				loadingStream.Dispose ();
				loadingStream = null;
				
				/*if (InvokeRequired)
					Invoke (new Action(GenerateActions));
				else
					GenerateActions ();
				*/
			}
		}
		
		Stream loadingStream;
		public void LoadFile (string fileName, Stream stream, Format format)
		{
			try {
				Document doc = format.Info.Create (this.Generator);
				doc.FileName = fileName;
				//if (doc.Info.CanEdit)
				//	doc.EditMode = true;

				loadingStream = new BufferedStream (stream, 4096);
				if (handler != null) {
					handler.Document.Dispose ();
					handler.Dispose ();
					handler = null;
				}
				handler = doc.CreateHandler ();
				handler.PreLoad (loadingStream, format);
				if (!UsingSplitter)
				{
					ListNavigation.Push(handler.ViewerControl, Path.GetFileName (doc.FileName));
					/*handler.ViewerControl.Disposed += delegate {
						handler.Document.Dispose ();
						handler.Dispose ();
						this.handler = null;
					};*/
				}
				else
				{
					ViewerPane.Content = handler.ViewerControl;
				}
				
				handler.Load (loadingStream, format);
				
				handler.PostLoad();
			} catch (Exception e) {
				MessageBox.Show (Generator, this, string.Format ("Unable to load the selected file ({0})", e.ToString ()));
				Console.WriteLine(e.ToString());
			}
			
			//viewerPane.ScrollPosition = new Point (0, 0);
			//viewerPane.UpdateSizes ();
		}
		
		public bool LoadFile(EtoFileInfo file)
		{
			//if (FileModifiedDialog.Show(this) != DialogResult.Ok) return true;

			Format format = Settings.Infos.FindFormat (file.Name, "character", "ansi");

			if (format != null)
			{
				try
				{
					var stream = file.OpenRead();
					this.LoadFile(file.FullName, stream, format);
					//this.FileList.Initialize(fileName);
					
				}
				catch (Exception e)
				{
					MessageBox.Show (Generator, this, string.Format ("Unable to load the selected file ({0})", e.ToString ()));
					Console.WriteLine(e.ToString());
				}
				return true;
			}
			return false;
		}
		
	}
}

