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
				if (!string.IsNullOrEmpty(platform))
				{
					switch (platform.ToLowerInvariant())
					{
						case "gtk":
							Generator.Initialize(Generators.GtkAssembly);
							break;
						case "winforms":
						case "win":
							Generator.Initialize(Generators.WinAssembly);
							break;
						case "wpf":
							Generator.Initialize(Generators.WpfAssembly);
							break;
						case "osx":
						case "mac":
							Generator.Initialize(Generators.MacAssembly);
							break;
						case "auto":
							Generator.Initialize(Generator.Detect);
							break;
						default:
							throw new ArgumentException(string.Format("Platform '{0}' is not recognized. Must be one of (gtk|winforms|wpf)", platform));
					}
				}
				else
				{
					Generator.Initialize(Generator.Detect);
				}
			}
			catch (Exception ex)
			{
				args.Writer.WriteLine("Error initializing platform: {0}", ex);
#if DEBUG
				throw;
#endif
			}

			mainthread = Generator.Current.ThreadStart();

			args.Writer.WriteLine("Using {0} platform", Generator.Current.ID);
			return false;
		}
	}
}
