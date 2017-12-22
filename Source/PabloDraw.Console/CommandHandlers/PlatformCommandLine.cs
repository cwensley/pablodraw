using Eto;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PabloDraw.Console.CommandHandlers
{
	public class PlatformCommandLine : CommandLineHandler
	{
		public override string Name { get { return "Platform"; } }

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			args.WriteOption("--platform, -p=[gtk|win|wpf|mac|auto]", "Platform to load (default:auto)");
		}

		static object mainthread;

		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			var platform = command.GetValue("platform", "p");
#if DEBUG
			//platform = "win";
#endif
			try
			{
				EngineInternal.Initialize(platform);
			}
			catch (Exception ex)
			{
				args.Writer.WriteLine("Error initializing platform: {0}", ex);
#if DEBUG
				throw;
#endif
			}

			mainthread = Platform.Instance.ThreadStart();

			args.Writer.WriteLine("Using {0} platform", Platform.Instance.ID);
			return false;
		}
	}
}
