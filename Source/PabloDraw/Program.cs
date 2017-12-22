using System;
using Pablo;
using Pablo.Network;
using Eto;

namespace PabloDraw
{
	public static class Program
	{

		public static void Run()
		{
			var command = new CommandLine(Environment.CommandLine);

			var platform = command.GetValue("platform", "p");
			//#if DEBUG
			// use winforms by default on windows
			if (platform == null && EtoEnvironment.Platform.IsWindows)
				platform = "win";
			//#endif
			if (!string.IsNullOrEmpty(platform))
			{
				try
				{
					switch (platform.ToLowerInvariant())
					{
						case "direct2d":
						case "d2d":
							Platform.Initialize(Platforms.Direct2D);
							break;
						case "gtk":
						case "gtk2":
							Platform.Initialize(Platforms.Gtk2);
							break;
						case "gtk3":
							Platform.Initialize(Platforms.Gtk3);
							break;
						case "winforms":
						case "win":
							Platform.Initialize(Platforms.WinForms);
							break;
						case "wpf":
							Platform.Initialize(Platforms.Wpf);
							break;
						default:
							throw new ArgumentOutOfRangeException("platform", platform, "Platform is not recognized. Must be one of (gtk|winforms|wpf)");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error initializing platform: {0}", ex);
				}
			}

			if (Platform.Detect.IsWinForms)
			{
				//Eto.WinForms.Forms.ApplicationHandler.BubbleMouseEvents = false;
			}
			if (EtoEnvironment.Platform.IsLinux)
			{
				// enforce case insensitivity
				Environment.SetEnvironmentVariable("MONO_IOMAP", "case");
			}

			var app = new Pablo.Interface.PabloApplication();

			string fileName = command.GetValue("file", "f") ?? command.GenericCommand;

			if (string.IsNullOrEmpty(fileName))
			{
				var activationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
				if (activationArguments != null)
				{
					var args = activationArguments.ActivationData;
					if (args != null && args.Length > 0)
					{
						if (!args[0].EndsWith(".application", StringComparison.InvariantCultureIgnoreCase))
							fileName = args[0];
					}
				}
			}
			if (!string.IsNullOrEmpty(fileName))
			{
				app.Initialized += delegate
				{
					//ClickOnceUpdate.CheckForUpdate ();
					app.Main.LoadFile(fileName, true);
				};
			}
			/*
			else {
				app.Initialized += delegate
				{
					ClickOnceUpdate.CheckForUpdate ();
				};
			}*/
			app.Run();
		}
	}

}

