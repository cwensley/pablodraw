using System;
using Pablo;
using Pablo.Network;
using Eto;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace PabloDraw
{
	public static class Program
	{

		public static void Run()
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => UnhandledExceptionReporter(e.ExceptionObject);

				var command = new CommandLine(Environment.CommandLine);

				var platform = command.GetValue("platform", "p");

#if SPECIFY_PLATFORM
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
								Platform.Initialize(Platforms.Gtk);
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
#elif WINDOWS
				Platform.Initialize(new Eto.WinForms.Platform());
#elif LINUX
				Platform.Initialize(new Eto.GtkSharp.Platform());
#elif MAC
				Platform.Initialize(new Eto.Mac.Platform());
				MacStyles.Apply();
#endif

				if (Platform.Detect.IsWinForms)
				{
					//Eto.WinForms.Forms.ApplicationHandler.BubbleMouseEvents = false;
				}
				if (EtoEnvironment.Platform.IsMono)
				{
					// enforce case insensitivity
					Environment.SetEnvironmentVariable("MONO_IOMAP", "case");
				}

				var app = new Pablo.Interface.PabloApplication();
				// app.UnhandledException += (sender, e) => UnhandledExceptionReporter(e.ExceptionObject);

				string fileName = command.GetValue("file", "f") ?? command.GenericCommand;
				bool? editMode = null;

				// if (string.IsNullOrEmpty(fileName))
				// {
				// 	var activationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
				// 	if (activationArguments != null)
				// 	{
				// 		var args = activationArguments.ActivationData;
				// 		if (args != null && args.Length > 0)
				// 		{
				// 			if (!args[0].EndsWith(".application", StringComparison.InvariantCultureIgnoreCase))
				// 				fileName = args[0];
				// 		}
				// 	}
				// }
				if (string.IsNullOrEmpty(fileName))
				{
					fileName = command.GetValue("edit", "e");
					editMode = true;
				}

				if (!string.IsNullOrEmpty(fileName))
				{
					app.Initialized += delegate
					{
					//ClickOnceUpdate.CheckForUpdate ();
					app.Main.LoadFile(fileName, true, editMode: editMode);
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
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred.  Please report this information to https://github.com/cwensley/pablodraw/issues:\n {ex}");
				ShowErrorDialog(ex);
				throw;
			}
		}

		private static void UnhandledExceptionReporter(object ex)
		{
			if (Application.Instance == null)
				ShowErrorDialog(ex);
			else
				Application.Instance.Invoke(() => ShowErrorDialog(ex));
		}

		static void ShowErrorDialog(object exceptionObject)
		{
			var dlg = new Dialog();
			dlg.Resizable = true;
			dlg.Title = "Error";

			var textArea = new TextArea { ReadOnly = true, Size = new Size(400, 300), Wrap = false };
			textArea.Text = Convert.ToString(exceptionObject);

			var quitButton = new Button { Text = "Quit" };
			quitButton.Click += (sender, e) => dlg.Close();

			var copyButton = new Button { Text = "Copy to clipboard" };
			copyButton.Click += (sender, e) => new Clipboard().Text = textArea.Text;

			var reportButton = new Button { Text = "Report Issue" };
			reportButton.Click += (sender, e) => Application.Instance.Open("https://github.com/cwensley/pablodraw/issues/new/choose");

			var label = new Label { Text = "PabloDraw encountered an error and will now close.\nPlease report this by copying the error below and pasting into an issue on GitHub.", TextAlignment = TextAlignment.Center };

			var layout = new DynamicLayout { Padding = 10, Spacing = new Size(5, 5) };

			layout.AddSeparateRow(label);
			layout.Add(textArea, yscale: true);

			dlg.Content = layout;

			dlg.PositiveButtons.Add(quitButton);
			dlg.PositiveButtons.Add(reportButton);
			dlg.PositiveButtons.Add(copyButton);

			dlg.ShowModal();
		}
	}

}

