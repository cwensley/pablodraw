using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.IO;
using Pablo;
using Pablo.Network;
using Pablo.Interface.Dialogs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Pablo.Interface
{
	public class Main : Form, IClientDelegate, IClientSource
	{
		const string TITLE = "PabloDraw";
		Handler handler;
		readonly FileList fileList;
		readonly Settings settings = new Settings();
		Server server;
		Client client;
		Format currentFormat;
		EtoFileInfo currentFile;
		readonly Splitter splitter;
		readonly PadPanel padpanel;
		bool shouldReload = true;
		bool lastEditMode;
		GenerateCommandArgs args;

		#region Events

		public event EventHandler<EventArgs> ServerChanged;

		protected virtual void OnServerChanged(EventArgs e)
		{
			if (ServerChanged != null)
				ServerChanged(this, e);
		}

		public event EventHandler<EventArgs> ClientChanged;

		protected virtual void OnClientChanged(EventArgs e)
		{
			if (ClientChanged != null)
				ClientChanged(this, e);

			if (handler != null)
				handler.Client = Client;

			GeneratePads();
			GenerateActions();
		}

		public event EventHandler<EventArgs> DocumentChanged;

		protected virtual void OnDocumentChanged(EventArgs e)
		{
			if (DocumentChanged != null)
				DocumentChanged(this, e);
		}

		#endregion

		#region Properties

		public IEnumerable<ICommand> Commands
		{
			get
			{
				yield return new Actions.EditFile(this);
				yield return new Actions.NewFile(this);
				yield return new Actions.EditSauce(this);

				if (handler != null)
					foreach (var command in handler.Commands)
						yield return command;
			}
		}

		public IEnumerable<ICommand> ServerCommands
		{
			get
			{
				if (handler != null)
					foreach (var command in handler.ServerCommands)
						yield return command;
			}
		}

		public new Control Content
		{
			get { return padpanel.Content; }
		}

		public ChatPanel ChatPanel
		{
			get;
			private set;
		}

		public string SettingsFile
		{
			get
			{
				var dir = EtoEnvironment.GetFolderPath(EtoSpecialFolder.ApplicationSettings);
				dir = Path.Combine(dir, "PabloDraw");
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				return Path.Combine(dir, "PabloDraw.config");
			}
		}

		public Document Document
		{
			get { return handler != null ? handler.Document : null; }
		}

		public Handler ViewHandler
		{
			get { return handler; }
		}

		public FileList FileList
		{
			get { return fileList; }
		}

		public Settings Settings
		{
			get { return settings; }
		}

		public Server Server
		{
			get { return server; }
			set
			{
				if (server != value)
				{
					server = value;
					OnServerChanged(EventArgs.Empty);
				}
			}
		}

		public Client Client
		{
			get { return client; }
			set
			{
				if (client != value)
				{
					client = value;
					if (client != null)
					{
						client.CurrentUserChanged += delegate
						{
							GenerateActions();
							GeneratePads();
						};
						client.Disconnected += delegate (object sender, DisconnectedArgs e)
						{
							Client = null;
							if (!string.IsNullOrEmpty(e.Reason))
								MessageBox.Show(this, e.Reason, "Disconnected from server");
						};
					}
					OnClientChanged(EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> EditModeChanged;

		protected virtual void OnEditModeChanged(EventArgs e)
		{
			if (EditModeChanged != null)
				EditModeChanged(this, e);

			SetDocument(Document);

			SetupEditMode(false, true);
		}

		void SetupEditMode(bool reload, bool focus)
		{
			if (lastEditMode != EditMode)
			{
				if (lastEditMode)
					Settings.EditFileSplit = splitter.Position;
				else
					Settings.FileSplit = splitter.Position;

				if (EditMode)
				{
					splitter.Position = Settings.EditFileSplit;
					if (reload)
						ReloadFile(false, true, false);
				}
				else
				{
					splitter.Position = Settings.FileSplit;
					if (reload)
						ReloadFile(false, false, false);
				}
				lastEditMode = EditMode;
			}

			if (focus)
			{
				if (EditMode)
					Content.Focus();
				else
					FileList.Focus();
			}
		}

		public bool EditMode
		{
			get { return Document != null && Document.EditMode; }
			set
			{
				var doc = Document;
				if (doc != null && value != doc.EditMode)
				{
					var stream = loadingStream;
					if (stream != null)
					{
						doc.EditMode = value;

						var fileName = doc.FileName;
						SetDocument(null);
						stream.Seek(0, SeekOrigin.Begin);
						LoadFile(fileName, stream, doc.LoadedFormat, value, false, doc.HasSavePermission);
						stream.Dispose();
						loadingStream = null;
					}
					else
						doc.EditMode = value;
					OnEditModeChanged(EventArgs.Empty);
				}
			}
		}

		public bool EnableBackups
		{
			get { return settings.EnableBackups; }
			set { settings.EnableBackups = value; }
		}

		#endregion

		public void WriteXml()
		{
			try
			{
				if (EditMode)
					Settings.EditFileSplit = splitter.Position;
				else
					Settings.FileSplit = splitter.Position;


				var doc = new XmlDocument();
				var head = doc.CreateElement("pablo");

				Settings.WriteXml(head);


				var elem = doc.CreateElement("main");
				if (FileList.CurrentDirectory != null)
				{
					var dir = FileList.CurrentDirectory;
					while (dir is VirtualDirectoryInfo)
					{
						var file = ((VirtualDirectoryInfo)dir).FileInfo;
						if (file == null)
							break;
						dir = file.Directory;
					}
					elem.SetAttribute("path", dir.FullName);
				}
				head.AppendChild(elem);

				head.WriteChildXml("main-window", new WindowStateSaver(this));


				doc.AppendChild(head);
				doc.Save(SettingsFile);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error! {0}", ex);
			}

		}

		class WindowStateSaver : IXmlReadable
		{
			public Window Window { get; private set; }

			public bool LoadMinimized { get; set; }

			public bool LoadPosition { get; set; }

			public WindowStateSaver(Window window)
			{
				this.Window = window;
				LoadPosition = true;
			}

			#region IXmlReadable implementation

			public void ReadXml(XmlElement element)
			{
				var startx = element.GetIntAttribute("startx");
				var starty = element.GetIntAttribute("starty");
				var width = element.GetIntAttribute("width");
				var height = element.GetIntAttribute("height");

				if (width != null && height != null && width > 0 && height > 0)
				{
					Window.Size = new Size(width.Value, height.Value);
				}

				var state = element.GetEnumAttribute<WindowState>("state") ?? WindowState.Normal;

				if (startx != null && starty != null && (LoadPosition || state != WindowState.Normal))
				{
					Window.Location = new Point(startx.Value, starty.Value);
				}

				if (state == WindowState.Maximized)
					Window.Maximize();
				else if (LoadMinimized && state == WindowState.Minimized)
					Window.Minimize();
			}

			public void WriteXml(XmlElement element)
			{
				var restoreBounds = Window.RestoreBounds;
				element.SetAttribute("width", restoreBounds.Width);
				element.SetAttribute("height", restoreBounds.Height);
				if (LoadPosition || Window.WindowState != WindowState.Normal)
				{
					element.SetAttribute("startx", restoreBounds.X);
					element.SetAttribute("starty", restoreBounds.Y);

				}
				element.SetAttribute("state", Window.WindowState);
			}

			#endregion
		}

		public void ReadXml()
		{
			if (File.Exists(SettingsFile))
			{
				try
				{
					var doc = new XmlDocument();
					doc.Load(SettingsFile);
					var head = (XmlElement)doc.SelectSingleNode("pablo");

					Settings.ReadXml(head);

					var elem = (XmlElement)head.SelectSingleNode("main");
					if (elem != null)
					{
						var dir = elem.GetAttribute("path");
						if (Directory.Exists(dir))
							FileList.Initialize(dir);
					}

					head.ReadChildXml("main-window", new WindowStateSaver(this));
				}
				catch (XmlException)
				{
					// ignore config errors.
				}
			}
		}

		public Main()
		{
			Title = TITLE;
			Size = new Size(950, 650);
			this.Style = "main";

			this.Icon = ImageCache.IconFromResource("Pablo.Interface.Icons.PabloDraw.ico");

			padpanel = new PadPanel();

			splitter = new Splitter();

			fileList = new FileList(this);
			fileList.Activated += delegate
			{
				if (fileList.SelectedFile != null)
				{
					currentFormat = null;

					ReloadFile(!fileList.SelectedFile.ReadOnly, true, true);
				}
			};
			fileList.SelectedIndexChanged += delegate
			{
				if (!this.EditMode)
				{
					if (fileList.SelectedFile != null)
					{
						currentFormat = null;
						ReloadFile(!fileList.SelectedFile.ReadOnly, false, false);
					}
				}
			};
			string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			fileList.Initialize(initialDirectory);


			padpanel.Content = new Panel { BackgroundColor = Colors.Black };
			splitter.Panel1 = fileList;
			splitter.Panel2 = padpanel;

			base.Content = splitter;

			GenerateActions();
			ReadXml();

			splitter.Position = Settings.FileSplit;
			fileList.Focus();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (args != null)
			{
				var cmd = args.KeyboardCommands.FirstOrDefault(r => r.Shortcut == e.KeyData);
				if (cmd != null)
				{
					if (cmd.Enabled)
						cmd.Execute();

					// allow default behaviour
					if (!(cmd is PabloCommand pc && pc.AllowKeyboardFallback))
					{
						e.Handled = true;
						return;
					}
				}

				// winforms doesn't support shortcuts without modifiers.. hm.
				// if (Platform.IsWinForms)
				{
					var menuItem = FindMenu(args.Menu, e.KeyData);
					if (menuItem != null)
					{
						// validate the menu item
						menuItem.PerformValidate();

						// winforms does not support shortcuts with no modifiers.. currently
						if (menuItem.Enabled)
						{
							menuItem.PerformClick();
							e.Handled = true;
						}
					}
				}
			}
			base.OnKeyDown(e);
		}

		private MenuItem FindMenu(ISubmenu menu, Keys keyData)
		{
			foreach (var item in menu.Items)
			{
				if (item.Shortcut == keyData)
				{
					return item;
				}
				if (item is ISubmenu submenu)
				{
					var child = FindMenu(submenu, keyData);
					if (child != null)
						return child;
				}
			}
			return null;
		}

		IEnumerable<MenuItem> AllItems(MenuItem item)
		{
			yield return item;
			var submenu = item as ISubmenu;
			if (submenu != null)
			{
				foreach (var child in submenu.Items.SelectMany(r => AllItems(r)))
				{
					yield return child;
				}
			}
		}

		public void GenerateActions()
		{
			bool canEdit = (handler != null && handler.CanEdit);

			if (args != null)
			{
				var disposableMenuItems = args.Menu.Items.SelectMany(r => AllItems(r)).Select(r => r.Command).OfType<IDisposable>().ToList();
				disposableMenuItems.AddRange(args.KeyboardCommands.OfType<IDisposable>());
				foreach (var item in disposableMenuItems)
					item.Dispose();
				var disposableToolbarItems = args.ToolBar.Items.Select(r => r.Command).OfType<IDisposable>().Where(r => !disposableMenuItems.Contains(r));
				foreach (var item in disposableToolbarItems)
					item.Dispose();
			}

			args = new GenerateCommandArgs { EditMode = EditMode, Area = "main" };

			if (Platform.IsMac && ToolBar != null)
			{
				// needed when running on big sur, but not needed until .NET 6
				args.ToolBar = ToolBar;
				args.ToolBar.Items.Clear();
			}

			//GC.Collect ();

			args.KeyboardCommands.Add(new Actions.FocusChatView(this));

			var prevCommand = new Command { ID = "prev", MenuText = "Open &Previous", ToolTip = "Opens the previous file in the list", Shortcut = Keys.Alt | Keys.Up };
			prevCommand.Executed += (sender, e) => fileList.GoToPrevious(); ;
			var nextCommand = new Command { ID = "next", MenuText = "Open &Next", ToolTip = "Opens the next file in the list", Shortcut = Keys.Alt | Keys.Down };
			nextCommand.Executed += (sender, e) => fileList.GoToNext(); ;

			var aiFile = args.Menu.Items.GetSubmenu("&File", 100);
			args.Menu.Items.GetSubmenu("&View", 300);
			args.Menu.Items.GetSubmenu("Options", 500);
			var aiNetwork = args.Menu.Items.GetSubmenu("&Network", 600);
			var aiHelp = args.Menu.Items.GetSubmenu("&Help", 1000);


			args.Menu.AboutItem = new Actions.About(this);

			if (Platform.IsMac)
			{
				var quitCommand = new Command { MenuText = "Quit", ToolTip = "Close the application", Shortcut = Keys.Q | Keys.Application };
				quitCommand.Executed += (sender, e) => ExitApplication();
				args.Menu.QuitItem = quitCommand;
			}
			else
			{
				args.Menu.QuitItem = new Actions.Exit(this);
			}


			aiFile.Items.Add(new Actions.NewFile(this), 100);
			aiFile.Items.Add(new Actions.OpenFile(this), 100);
			aiFile.Items.Add(new Actions.EnclosingFolder(this), 100);
			if (!EditMode)
			{
				aiFile.Items.Add(nextCommand, 100);
				aiFile.Items.Add(prevCommand, 100);

				aiFile.Items.AddSeparator(300);
				aiFile.Items.Add(new Actions.DeleteFile(this), 300);
				aiFile.Items.Add(new Actions.RenameFile(this), 300);
				aiFile.Items.Add(new Actions.MoveFile(this), 300);
			}
			aiFile.Items.AddSeparator(400);
			aiFile.Items.Add(new Actions.EditSauce(this), 500);

			if (canEdit)
				aiFile.Items.Add(new Actions.EditFile(this), 500);

			aiFile.Items.Add(new Actions.EnableBackups(this), 500);

			// network
			//#if DEBUG
			aiNetwork.Items.Add(new Actions.ServerStart(this), 500);
			aiNetwork.Items.Add(new Actions.ClientConnect(this), 500);
			aiNetwork.Items.Add(new Actions.ServerStop(this), 500);
			//#endif

			// help
			aiHelp.Items.Add(new Actions.Readme(this), 500);
			aiHelp.Items.Add(new Actions.Homepage(), 500);

			args.ToolBar.Items.Add(new Actions.NewFile(this), 100);
			args.ToolBar.Items.Add(new Actions.OpenFile(this), 100);
			args.ToolBar.Items.Add(new Actions.EditSauce(this), 100);

			if (Platform.IsMac)
			{
				args.ToolBar.Items.AddSeparator(800, SeparatorToolItemType.FlexibleSpace);
				args.ToolBar.Items.Add(new Actions.ViewFile(this), 900);
				args.ToolBar.Items.Add(new Actions.EditFileRadio(this), 900);
			}
			else
			{
				args.ToolBar.Items.AddSeparator(200);
				args.ToolBar.Items.Add(new Actions.EditFile(this), 200);
			}

			if (handler != null)
				handler.GenerateCommands(args);

			var oldmenu = Menu;
			var oldtoolbar = ToolBar;

			Menu = args.Menu;
			ToolBar = args.ToolBar;

#if DEBUG
			Application.Instance.AsyncInvoke(() =>
			{
				/*if (oldtoolbar != null)
					oldtoolbar.Dispose();

				if (oldmenu != null)
					oldmenu.Dispose();*/
				GC.Collect();
				GC.WaitForPendingFinalizers();
			});
#endif
		}

		public void ExitApplication()
		{
			var ce = new CancelEventArgs();
			OnClosing(ce);
			if (!ce.Cancel)
			{
				Application.Instance.Quit();
			}
		}

		public void SetDocument(Document document, bool setFocus = false)
		{
			InternalSetDocument(document, true, null, null);
			if (setFocus)
				padpanel.Content.Focus();
			SetupEditMode(false, false);
			GenerateActions();
		}

		void InternalSetDocument(Document document, bool postLoad, Stream stream, Format format)
		{
			loadingStream = stream;
			Document olddoc = null;
			// unlink old document
			if (handler != null)
			{
				olddoc = handler.Document;
				handler.ActionsChanged -= handler_ActionsChanged;

				if (olddoc != null && olddoc != document)
				{
					olddoc.Saved -= Document_Saved;
					olddoc.Loaded -= Document_Loaded;
					olddoc.Dispose();
				}
				padpanel.Content = new Panel { BackgroundColor = Colors.Black };
				handler.Dispose();

				handler = null;
			}
#if DEBUG
			GC.Collect();
			GC.WaitForPendingFinalizers();
#endif

			// new document
			if (document != null)
			{
				var docName = string.IsNullOrEmpty(document.FileName) ? "<New Document>" : Path.GetFileName(document.FileName);
				Title = string.Format("{0} - {1}", TITLE, docName);

				if (olddoc != document)
				{
					document.Saved += Document_Saved;
					document.Loaded += Document_Loaded;
				}

				handler = document.CreateHandler();
				handler.ClientDelegate = this;
				handler.Client = Client;
				handler.ActionsChanged += handler_ActionsChanged;

				handler.PreLoad(stream, format);

				// TODO: RIP needs this PreLoad, WPF/ansi animation needs post pre-load
				var viewerControl = handler.ViewerControl;
				padpanel.Content = viewerControl;

				if (Client != null)
					Client.SetCommands(Commands);

				if (Server != null)
					Server.SetCommands(ServerCommands);

				if (postLoad)
				{
					handler.Loaded();
				}

			}
			else
			{
				Title = TITLE;
				GenerateActions();
			}

			if (postLoad)
				PostLoad();

			OnDocumentChanged(EventArgs.Empty);
		}

		void GeneratePads()
		{
			var padArgs = new GeneratePadArgs();

			if (handler != null)
			{
				handler.GeneratePads(padArgs);
			}

			if (Client != null)
			{
				if (ChatPanel == null)
					ChatPanel = new ChatPanel(this);
				padArgs.BottomPads.Insert(0, ChatPanel);
			}
			else
				ChatPanel = null;

			padpanel.SetPads(padArgs);
		}

		void PostLoad()
		{
			SetupEditMode(false, false);
			GeneratePads();
			if (loadingStream != null)
				GenerateActions(); // background loading

			if (handler != null)
				handler.PostLoad();
		}

		Stream loadingStream;

		public bool LoadFile(string fileName, Stream stream, Format format = null, bool editMode = false, bool setFileList = true, bool hasSavePermissions = true)
		{
			if (format == null)
			{
				format = Settings.Infos.FindFormat(fileName, "character", "ansi");
				if (format == null)
					return false;
			}
			if (Client != null)
			{
				var doc = format.Info.Create(Platform);
				if (editMode && doc.Info.CanEdit)
				{
					doc.EditMode = editMode;
					doc.FileName = fileName;
					doc.Load(stream, format, null);
					doc.HasSavePermission = hasSavePermissions;
					Client.SetDocument(doc);
				}
				else
				{
					using (var ms = new MemoryStream((int)stream.Length))
					{
						stream.CopyTo(ms);
						ms.Seek(0, SeekOrigin.Begin);
						Client.SetFile(fileName, ms, format, editMode);
					}
				}
				return true;
			}
			InternalLoadFile(fileName, stream, format, editMode, setFileList, hasSavePermissions);
			return true;
		}

		void InternalLoadFile(string fileName, Stream stream, Format format, bool editMode, bool setFileList, bool hasSavePermission)
		{
			try
			{
				var doc = format.Info.Create(Platform);
				if (doc.Info.CanEdit)
					doc.EditMode = editMode;
				doc.HasSavePermission = hasSavePermission;
				doc.FileName = fileName;
				InternalSetDocument(doc, false, stream, format);

				/**
				var bufferedStream = new MemoryStream();
				stream.WriteTo(bufferedStream);
				stream.Close();
				bufferedStream.Position = 0;
				/**/
				var bufferedStream = new BufferedStream(stream, 20 * 1024);

				//Console.WriteLine ("Loading file {0}, format: {1}", fileName, format.GetType ());
				currentFormat = format;
				handler.Load(bufferedStream, format);
				if (setFileList)
				{
					shouldReload = false;
					FileList.Initialize(fileName);
					shouldReload = true;
				}

				PostLoad();
			}
			catch (Exception e)
			{
				SetDocument(null);
				MessageBox.Show(this, string.Format("Unable to load the selected file ({0})", e));
#if DEBUG
				Debug.Print("Error loading: {0}", e);
#endif
			}
		}

		void handler_ActionsChanged(Object sender, EventArgs e)
		{
			GenerateActions();
		}

		void Document_Loaded(object sender, EventArgs e)
		{
			if (loadingStream != null)
			{
				loadingStream.Dispose();
				loadingStream = null;
				PabloApplication.Instance.Invoke(delegate
				{
					GenerateActions();
				});
			}
		}

		void Document_Saved(object sender, EventArgs e)
		{
			var document = sender as Document;
			if (document != null)
			{
				fileList.Initialize(document.FileName, true);
				Title = string.Format("{0} - {1}", TITLE, Path.GetFileName(document.FileName));
			}
		}

		public bool LoadFile(string fileName, bool hasSavePermissions, bool setFileList = true, bool? editMode = null)
		{
			if (FileModifiedDialog.Show(this) != DialogResult.Ok)
				return true;

			currentFormat = null;
			currentFile = null;

			if (File.Exists(fileName))
			{
				// do not dispose, we may need to load in the background for animated documents.
				var stream = File.OpenRead(fileName);
				return LoadFile(fileName, stream, null, editMode ?? EditMode, setFileList, hasSavePermissions);
			}
			return false;
		}

		public void ReloadFile(bool hasSavePermissions, bool focus, bool checkModifications)
		{
			if (!shouldReload)
				return;

			if (checkModifications && FileModifiedDialog.Show(this) != DialogResult.Ok)
				return;

			EtoFileInfo file = fileList.SelectedFile;
			if (file != null)
			{
				//Console.WriteLine ("Reloading file: {0}, format:{1}", file.Name, currentFormat != null ? currentFormat.GetType () : null);
				if (file != currentFile)
					currentFormat = null;
				currentFile = file;
				Format format = currentFormat ?? Settings.Infos.FindFormat(file.FullName, "character", "ansi");
				if (format != null)
				{
					try
					{
						var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
						LoadFile(file.FullName, stream, format, EditMode, false, hasSavePermissions);
						if (focus)
							padpanel.Content.Focus();
					}
					catch (Exception ex)
					{
						SetDocument(null);
						MessageBox.Show(this, string.Format("Error loading file '{0}'.  {1}", file.FullName, ex.Message));
#if DEBUG
						Debug.Print("Error loading: {0}", ex);
						throw;
#endif
					}
				}
			}
		}

		#region IClientDelegate implementation

		DocumentInfoCollection IClientDelegate.DocumentInfos
		{
			get
			{
				return Settings.Infos;
			}
		}

		void IClientDelegate.SetDocument(Document document)
		{
			SetDocument(document, false);
		}

		void IClientDelegate.LoadFile(string fileName, Stream stream, bool editMode, Format format)
		{
			InternalLoadFile(fileName, stream, format, editMode, false, false);
		}

		#endregion

		public bool PromptSave()
		{
			if (Document?.IsModified == true)
			{
				var result = MessageBox.Show(this, "The current file has been modifieded. Do you wish to save?", MessageBoxButtons.YesNoCancel, MessageBoxType.Warning);
				if (result == DialogResult.Yes)
				{
					var saveFile = new Pablo.Actions.SaveFile(ViewHandler);
					saveFile.Execute();
				}
				if (result == DialogResult.Cancel)
				{
					return false;
				}
			}

			return true;
		}
	}
}
