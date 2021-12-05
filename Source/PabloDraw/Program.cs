using System;
using Pablo;
using Pablo.Network;
using Eto;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Linq;

namespace PabloDraw
{
	public static class Program
	{
		public static IEnumerable<ICommandLineHandler> GetHandlers()
		{
#if UNIVERSAL
			yield return new CommandHandlers.PlatformCommandLine();
#endif
			yield return new CommandHandlers.ServerCommandLine();
			yield return new CommandHandlers.ConvertCommandLine();
			yield return new CommandHandlers.HelpCommandLine();
			yield return new CommandHandlers.EditorCommandLine();
		}

		public static int Run()
		{
#if !UNIVERSAL
			EngineInternal.Initialize();
#endif
			var command = new CommandLine(Environment.CommandLine);

			var args = new ProcessCommandLineArgs
			{
				Command = command,
				Handlers = GetHandlers().ToList(),
			};
			foreach (var handler in args.Handlers)
			{
				if (handler.Process(args))
					return 0;
			}
			return -1;
		}


	}

}

