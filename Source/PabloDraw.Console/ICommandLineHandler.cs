using Pablo;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PabloDraw.Console
{
	public interface ICommandLineHandler
	{
		string Name { get; }

		void GetHelp(ProcessCommandLineArgs args);

		bool Process(ProcessCommandLineArgs args);
	}

	public class ProcessCommandLineArgs : EventArgs
	{
		public IndentedTextWriter Writer { get; set; }
		public CommandLine Command { get; set; }
		public int MaxWidth { get; set; }
		public IEnumerable<ICommandLineHandler> Handlers { get; set; }

		public int CommentPosition { get; set; }

		public ProcessCommandLineArgs()
		{
			MaxWidth = 79;
			CommentPosition = 28;
		}

		public void WriteOption(string option, string comment)
		{
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
