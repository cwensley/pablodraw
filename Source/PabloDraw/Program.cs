using System;
using Pablo;
using Pablo.Network;
using Eto;

namespace PabloDraw
{
	public static class Program
	{
		
		public static void Run ()
		{
			var command = new CommandLine (Environment.CommandLine);

			var platform = command.GetValue("platform", "p");
//#if DEBUG
			// use winforms by default on windows
			if (platform == null && EtoEnvironment.Platform.IsWindows)
				platform = "win";
//#endif
			if (!string.IsNullOrEmpty (platform)) {
				try {
					switch (platform.ToLowerInvariant ()) {
					case "gtk":
						Generator.Initialize (Generators.GtkAssembly);
						break;
					case "winforms":
					case "win":
						Generator.Initialize (Generators.WinAssembly);
						break;
					case "wpf":
						Generator.Initialize (Generators.WpfAssembly);
						break;
					default:
						throw new ArgumentOutOfRangeException ("platform", platform, "Platform is not recognized. Must be one of (gtk|winforms|wpf)");
					}
				} catch (Exception ex) {
					Console.WriteLine ("Error initializing platform: {0}", ex);
				}
			}

			if (Generator.Detect.IsWinForms)
			{
				Eto.Platform.Windows.ApplicationHandler.BubbleMouseEvents = false;
			}
			if (EtoEnvironment.Platform.IsLinux)
			{
				// enforce case insensitivity
				Environment.SetEnvironmentVariable("MONO_IOMAP", "all");
			}
			
			var app = new Pablo.Interface.Application ();
			
			string fileName = command.GetValue("file", "f") ?? command.GenericCommand;
			
			if (string.IsNullOrEmpty (fileName)) {
				var activationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
				if (activationArguments != null) {
					var args = activationArguments.ActivationData;
					if (args != null && args.Length > 0) {
						if (!args [0].EndsWith (".application", StringComparison.InvariantCultureIgnoreCase)) 
							fileName = args [0];
					}
				}
			}
			if (!string.IsNullOrEmpty (fileName)) {
				app.Initialized += delegate
				{
					//ClickOnceUpdate.CheckForUpdate ();
					app.Main.LoadFile (fileName, true);
				};
			}
			/*
			else {
 				app.Initialized += delegate
				{
					ClickOnceUpdate.CheckForUpdate ();
				};
			}*/
			app.Run (null);
		}
	}
	
}

