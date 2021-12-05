using Eto;
using Microsoft.Win32.SafeHandles;
using Pablo;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PabloDraw
{
	public interface ICommandLineHandler
	{
		string Name { get; }

		void GetHelp(ProcessCommandLineArgs args);

		bool Process(ProcessCommandLineArgs args);
	}

	public class ProcessCommandLineArgs
	{
		IndentedTextWriter _writer;
		
		public IndentedTextWriter Writer
		{
			get
			{
				if (_writer == null)
				{
					ShowConsole();
					_writer = new IndentedTextWriter(System.Console.Out, "  ");
				}
				return _writer;
			}
		}
		public CommandLine Command { get; set; }
		public int MaxWidth { get; set; }
		public IEnumerable<ICommandLineHandler> Handlers { get; set; }

		public int CommentPosition { get; set; }

		public ProcessCommandLineArgs()
		{
			MaxWidth = 79;
			CommentPosition = 28;
		}

		private const int ATTACH_PARENT_PROCESS = -1;
		private const int ERROR_INVALID_HANDLE = 6;
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(int dwProcessId);
		[DllImport("kernel32.dll")]
		static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		static extern bool FreeConsole();

		static bool StartConsole()
		{
			if (!AttachConsole(ATTACH_PARENT_PROCESS)) // try connecting to an existing console  
			{
				if (Marshal.GetLastWin32Error() == ERROR_INVALID_HANDLE) // we don't have a console yet  
				{
					if (!AllocConsole()) // couldn't create a new console, either  
						return false;
				}
				else
					return false; // some other error
			}
			return true;
		}
		
		static bool started;
		public static void ShowConsole()
		{
			if (EtoEnvironment.Platform.IsWindows)
			{
				if (!started)
				{
					started = true;
					StartConsole();
					// AttachConsole(ATTACH_PARENT_PROCESS);
				}
				// AllocConsole();
				// ShowWindow(GetConsoleWindow(), show ? 1 : 0);
			}
		}
		public void Write(string message)
		{
			ShowConsole();
			Writer.Write(message);
		}

		public void WriteOption(string option, string comment)
		{
			ShowConsole();
			Writer.Write(option);
			int offsetLen;
			if (option.Length >= CommentPosition)
			{
				Writer.WriteLine();
				offsetLen = CommentPosition;
			}
			else
				offsetLen = CommentPosition - option.Length;

			for (int i = 0; i < offsetLen; i++)
				Writer.Write(" ");

			if (!string.IsNullOrEmpty(comment))
			{
				var commentWidth = MaxWidth - CommentPosition - Writer.Indent * 2;
				while (!string.IsNullOrEmpty(comment))
				{
					if (comment.Length > commentWidth)
					{
						var split = comment.LastIndexOf(" ", commentWidth, StringComparison.Ordinal);
						Writer.WriteLine(comment.Substring(0, split).Trim());
						for (int i = 0; i < CommentPosition; i++)
							Writer.Write(" ");
						comment = comment.Substring(split);
					}
					else
					{
						Writer.WriteLine(comment.Trim());
						comment = null;
					}
				}
			}
			else
				Writer.WriteLine();

		}
	}

	public abstract class CommandLineHandler : ICommandLineHandler
	{
		public abstract string Name { get; }

		public abstract void GetHelp(ProcessCommandLineArgs args);

		public abstract bool Process(ProcessCommandLineArgs args);



	}
}
