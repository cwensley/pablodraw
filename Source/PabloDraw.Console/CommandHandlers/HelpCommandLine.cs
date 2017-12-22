using Eto;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PabloDraw.Console.CommandHandlers
{
	public class HelpCommandLine : CommandLineHandler
	{
		public override string Name { get { return "Help"; } }

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			args.WriteOption("--help", "Show options and help");
		}

		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			var version = Assembly.GetEntryAssembly().GetName();
			args.Writer.WriteLine("{0} v{1}", version.Name, version.Version);

			if (command.GetBool("help") ?? command.IsEmpty)
			{
				args.Writer.WriteLine("Usage: PabloDraw.Console.exe [options] [input.ext] [output.ext]");
				foreach (var handler in args.Handlers)
				{
					args.Writer.WriteLine();
					args.Writer.WriteLine("{0}:", handler.Name);
					args.Writer.Indent++;
					handler.GetHelp(args);
					args.Writer.Indent--;
				}
			}
			return false;
		}
	}
}
