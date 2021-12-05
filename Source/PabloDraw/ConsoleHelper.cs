using System;
using System.Threading;

namespace PabloDraw
{
	public static class ConsoleHelper
	{
		internal static void Run(ProcessCommandLineArgs args)
		{
			args.Writer.WriteLine("Press Ctrl+C to stop...");
			var autoResetEvent = new AutoResetEvent(false);
			System.Console.CancelKeyPress += (sender, eventArgs) =>
			{
				// cancel the cancellation to allow the program to shutdown cleanly
				eventArgs.Cancel = true;
				autoResetEvent.Set();
			};

			// block until ctrl+c is pressed
			autoResetEvent.WaitOne();
		}
	}
}