using Pablo;
using Eto;
using System.Xml;
using Pablo.Network;
using System;
using System.IO;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Threading;

namespace PabloDraw.Console.CommandHandlers
{
	public class ServerCommandLine : CommandLineHandler, IClientDelegate
	{
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
			doc.Load(stream, format, null);
			doc.FileName = fileName;
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
			if (command.GetBool("server") != true)
				return false;

			EnableBackups = command.GetBool("backup") ?? true;
			var password = command.GetValue("password", "pw");
			var operatorPassword = command.GetValue("op", "oppassword", "oppw");
			var port = command.GetInt("port") ?? 14400;
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

			var autosaveInterval = command.GetInt("autosave") ?? 0;
			if (autosaveInterval > 0)
			{
				// turn on autosaving
			}

			var backup = command.GetBool("backup") ?? false;
			if (backup)
			{
				// turn on backups
			}

			args.Writer.WriteLine("Starting PabloDraw server on port {0}...", port);
			StartServer(port, password, operatorPassword, nat, userLevel, args.Writer);

			args.Writer.WriteLine("Started! Press any key to stop");
			System.Console.Read();

			args.Writer.WriteLine("Stopping...");
			StopServer();
			return true;
		}
	}
}
