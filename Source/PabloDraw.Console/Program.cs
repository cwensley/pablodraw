using Pablo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom.Compiler;
using System.Reflection;
using log4net;

namespace PabloDraw.Console
{
	static class Program
	{
	    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<ICommandLineHandler> GetHandlers()
		{
			yield return new CommandHandlers.PlatformCommandLine();
			yield return new CommandHandlers.ServerCommandLine();
			yield return new CommandHandlers.ConvertCommandLine();
			yield return new CommandHandlers.HelpCommandLine();
		}

		public static int Run ()
		{
		    try
		    {
		        var command = new CommandLine(Environment.CommandLine);

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
		    catch (Exception ex)
		    {
                Log.ErrorFormat("An error happened: {0}", ex);
		        return 0;
		    }
		}
	}
}
