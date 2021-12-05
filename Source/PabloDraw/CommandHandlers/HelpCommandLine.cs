using Eto;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PabloDraw.CommandHandlers
{
	public class HelpCommandLine : CommandLineHandler
	{
		public override string Name => "Help";

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			args.WriteOption("--help", "Show options and help");
		}

		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			var version = Assembly.GetEntryAssembly().GetName();

			if (command.GetBool("help", "h", "?") == true)
			{
				ProcessCommandLineArgs.ShowConsole();
				args.Writer.WriteLine("{0} v{1}", version.Name, version.Version);
				var ext = EtoEnvironment.Platform.IsWindows ? ".exe" : "";
				args.Writer.WriteLine($"Usage: PabloDraw{ext} [options] [input.ext] [output.ext]");
				foreach (var handler in args.Handlers)
				{
					args.Writer.WriteLine();
					args.Writer.WriteLine("{0}:", handler.Name);
					args.Writer.Indent++;
					handler.GetHelp(args);
					args.Writer.Indent--;
				}
				return true;
			}
			return false;
		}
	}
}
