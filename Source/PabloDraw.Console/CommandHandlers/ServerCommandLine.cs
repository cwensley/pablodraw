using Pablo;
using Eto;
using System.Xml;
using Pablo.Network;
using System;
using System.IO;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;
using log4net;

namespace PabloDraw.Console.CommandHandlers
{
	public class ServerCommandLine : CommandLineHandler, IClientDelegate
	{
	    private Timer _autoSaveTimer;
	    private int _autosaveInterval;
	    private bool _backup = false;
	    private int _port;
	    private byte _autoSaveRetryCounter;
	    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	    
        public override string Name { get { return "Server"; } }

		public bool EditMode { get { return true; } }

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			args.WriteOption("--server, -s", "Starts a pablo server");
			args.WriteOption("--file=<FILE>, [input.ext]", "Initial file to load (or create) after starting the server");
			args.WriteOption("--port=<PORT>", "Specifies the port to use (default:14400)");
			args.WriteOption("--nat=[true|false]", "Map the port on the router using UPnP or NAT-PMP");
			args.WriteOption("--password, -pw=<PASS>", "Sets the password for regular users");
			args.WriteOption("--oppassword, -op=<PASS>", "Sets the password for operator access");
			args.WriteOption("--userlevel, -ul=[viewer|editor|operator]", "Sets the user level for users (default:viewer)");
			args.WriteOption("--autosave=<seconds>", "Interval to save the file in seconds, or 0 to turn off");
			args.WriteOption("--backup", "Backup existing file before saving");
		}

		public DocumentInfoCollection DocumentInfos { get; private set; }

		public Document Document { get; private set; }

		public Handler Handler { get; private set; }

		public Client Client { get; private set; }

		public Server Server { get; private set; }

		public bool EnableBackups { get; set; }

		public ServerCommandLine()
		{
			DocumentInfos = DocumentInfoCollection.Default;
		}

		IEnumerable<ICommand> Commands
		{
			get
			{
				yield return new Commands.NewFile(this);
				yield return new Commands.EditFile(this);
				if (Handler != null)
					foreach (var command in Handler.Commands)
						yield return command;
			}
		}

		IEnumerable<ICommand> ServerCommands
		{
			get
			{
				if (Handler != null)
					foreach (var command in Handler.ServerCommands)
						yield return command;
			}
		}

		public void SetDocument(Document document)
		{
			Document = document;

			Handler = document.CreateHandler();
			Handler.ClientDelegate = this;
			if (Client != null)
				Client.SetCommands(Commands);
			
			if (Server != null)
				Server.SetCommands(ServerCommands);
			
			Handler.PreLoad(null, document.LoadedFormat);
			Handler.Loaded();
		}

		public void LoadFile(string fileName, System.IO.Stream stream, bool editMode, Format format = null)
		{
			format = format ?? DocumentInfos.FindFormat(fileName) ?? DocumentInfos.DefaultFormat;
			if (format == null)
				throw new  ArgumentOutOfRangeException("fileName", "Could not find format of the file");
			var doc = format.Info.Create();
			doc.FileName = fileName;
			doc.Load(stream, format, null);
			doc.EditMode = editMode;
			SetDocument(doc);
		}

		public void StartServer(int port, string password, string operatorPassword, bool nat, UserLevel defaultUserLevel, TextWriter writer)
		{
			Client = new Client
			{
				Port = port,
				Delegate = this,
				Password = operatorPassword ?? password,
				Headless = true
			};
			Client.SetCommands(Commands);
			
			Client.CurrentUser.Alias = "admin";
			Client.CurrentUser.Level = UserLevel.Operator;
			
			Client.Message += (sender, e) => writer.WriteLine(e.DisplayMessage);
			
			Server = new Server
			{
				Port = port,
				Delegate = this,
				Password = password,
				OperatorPassword = operatorPassword,
				AutoMapPort = nat,
				DefaultUserLevel = defaultUserLevel,
				Client = Client
			};
			Server.SetCommands(ServerCommands);
			Server.Start();
			Client.Start();
		}

		public void StopServer()
		{
			Client.Stop();
			Server.Stop();
		}

		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			if (command.GetBool("server", "s") != true)
				return false;

			EnableBackups = command.GetBool("backup") ?? true;
			var password = command.GetValue("password", "pw");
			var operatorPassword = command.GetValue("op", "oppassword", "oppw");
			_port = command.GetInt("port") ?? 14400;
			var nat = command.GetBool("nat") ?? false;
			var userLevel = command.GetEnum<UserLevel>("userlevel", "ul") ?? UserLevel.Viewer;

			var fileName = command.GetValue("file", "generic:0");
			if (!string.IsNullOrEmpty(fileName))
			{
				if (File.Exists(fileName))
				{
					using (var stream = File.OpenRead(fileName))
					{
						LoadFile(fileName, stream, true);
					}
				}
				else
				{
					var format = DocumentInfos.FindFormat(fileName);
					if (format == null)
						throw new InvalidOperationException(string.Format("Unrecognized format for file name '{0}'", fileName));
					var doc = format.Info.Create();
					doc.FileName = fileName;
					doc.EditMode = true;
					SetDocument(doc);
				}
			}

            _autosaveInterval = command.GetInt("autosave") ?? 0;
			if (_autosaveInterval > 0)
			{
                if (!Directory.Exists(_port.ToString()))
			    {
			        Directory.CreateDirectory(_port.ToString( )); // Store AutoSave in its port number folder so that multiple instances don't overwrite each other.
                }
			    _autosaveInterval *= 1000; // Convert to seconds.
                _autoSaveTimer = new Timer(TimerCallback, null, _autosaveInterval, Timeout.Infinite);
            }

            _backup = command.GetBool("backup") ?? false;
			

			args.Writer.WriteLine("Starting PabloDraw server on port {0}...", _port);
			StartServer(_port, password, operatorPassword, nat, userLevel, args.Writer);

			args.Writer.WriteLine("Started! Press any key to stop");
			System.Console.Read();

			args.Writer.WriteLine("Stopping...");
			StopServer();
			return true;
		}

        // TODO - It seems that some Document properties are not set when a new doc is created on the server (iceColors, IsModifier, ...)
        // TODO - It would be better to set these properties (Document Creation, Editing, ...)
	    private void TimerCallback(Object state)
	    {
            try
            {
                if (Handler != null) Handler.Document.IsModified = true; // TODO - IsModified is always false, even if document is changed

                if (Handler != null && Handler.Document.IsModified) 
                {
                    string path;
	                string fileName = "";

	                if (Document.Info.ID == "rip")
	                {
	                    fileName = "AutoSave.rip";
	                }
	                else if (Document.Info.ID == "character")
	                {
	                    fileName = "AutoSave.ans";
	                    if (((Pablo.Formats.Character.CharacterHandler) Handler).CurrentPage.Palette.Count > 16) // TODO - Find a better way to detect/set this.
                        {
	                        ((Pablo.Formats.Character.CharacterDocument) Document).ICEColours = true; 

	                    }
                    }
	                else
                    {
                        string msg = string.Format("File format was not detected - {0} at {1}", Document.Info.ID, DateTime.Now);
                        System.Console.WriteLine(msg);
                        Log.Warn(msg);

                        return; // Undetected file format.
                    }

	                Document.FileName = fileName;
                    path = Path.Combine(_port.ToString(), fileName);
                    
                    if (_backup)
	                {
	                    Handler.SaveWithBackup(path, Document.LoadedFormat);
	                    string msg = string.Format("AutoSave - {0} was saved at {1} with Backup", path, DateTime.Now);
                        System.Console.WriteLine(msg);
                        Log.Info(msg);
                    }
	                else
	                {
	                    Handler.Save(path, Document.LoadedFormat);
	                    string msg = string.Format("AutoSave - {0} was saved at {1}", path, DateTime.Now);
                        System.Console.WriteLine(msg);
                        Log.Info(msg);
                    }

	                Handler.Document.IsModified = false;
	                _autoSaveRetryCounter = 0;

	            }
	        }
	        catch (Exception ex)
	        {
	            Log.ErrorFormat("An error happened: {0}", ex);

	            if (_autoSaveRetryCounter == 5)
	            {
//#if DEBUG
	                throw new Exception(ex.Message, ex);
	                //#endif
	            }

	            _autoSaveRetryCounter++;

            }
            finally
	        {

	            // Make sure timer is set again.
	            _autoSaveTimer.Change(_autosaveInterval, Timeout.Infinite);
	        }
	    }

	}
}
