using Pablo;
using Eto;
using System.Xml;
using Pablo.Network;
using System;
using System.IO;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using System.Runtime.InteropServices;

namespace PabloDraw.CommandHandlers
{
	public class EditorCommandLine : CommandLineHandler
	{
		public override string Name => "Editor";

		public bool EditMode => true;

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			args.WriteOption("--file=<FILE>, -f=<FILE>, [input.ext]", "Initial file to view");
			args.WriteOption("--edit=<FILE>, -e=<FILE>", "Initial file to edit");
		}

		
		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			try
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => UnhandledExceptionReporter(e.ExceptionObject);

#if MAC
				MacStyles.Apply();
#endif
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

			return true;
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
			textArea.Selection = Eto.Forms.Range.FromLength(0, 0);

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
