using Pablo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom.Compiler;

namespace PabloDraw.Console
{
	static class Program
	{
		public static IEnumerable<ICommandLineHandler> GetHandlers()
		{
#if UNIVERSAL
			yield return new CommandHandlers.PlatformCommandLine();
#endif
			yield return new CommandHandlers.ServerCommandLine();
			yield return new CommandHandlers.ConvertCommandLine();
			yield return new CommandHandlers.HelpCommandLine();
		}

		public static int Run ()
		{
#if !UNIVERSAL
			EngineInternal.Initialize();
#endif
			var command = new CommandLine (Environment.CommandLine);

			var args = new ProcessCommandLineArgs
			{
				Command = command,
				Handlers = GetHandlers().ToArray(),
				Writer = new IndentedTextWriter(System.Console.Out, "  ")
			};
			foreach (var handler in args.Handlers)
			{
				if (handler.Process(args))
					return 0;
			}
			System.Console.WriteLine();
			return -1;
		}
	}
}
