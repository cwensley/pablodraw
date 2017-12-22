using System;
using Pablo;
using System.IO;
using Eto;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Linq;

namespace PabloDraw.Console.CommandHandlers
{
	public class ConvertCommandLine : CommandLineHandler
	{
		public override string Name { get { return "Conversion"; } }

		public override void GetHelp(ProcessCommandLineArgs args)
		{
			var writer = args.Writer;
			args.WriteOption("--convert=<FILE>, [input.ext]", "Input file to convert from");
			args.WriteOption("--to=<FILE>, [output.ext]", "Output file to convert to");
			args.WriteOption("--zoom, -z=<ZOOM>", "Sets the zoom factor (default:1.0)");
			foreach (var format in DocumentInfoCollection.Default.Values)
			{
				var options = format.Options.ToArray();
				if (options.Length > 0)
				{
					writer.WriteLine();
					writer.WriteLine("{0} Conversion:", format.Description);
					writer.Indent++;
					var formatId = format.OptionID ?? format.ID;
					foreach (var option in options)
					{
						var name = string.Format("--{0}-{1}", formatId, option.ID);
						if (option.Values != null && option.Values.Length > 0)
						{
							name = string.Format("{0}=[{1}]", name, string.Join("|", option.Values));
						}
						args.WriteOption(name, option.Comment);
					}
					writer.Indent--;
				}
			}
		}
		public override bool Process(ProcessCommandLineArgs args)
		{
			var command = args.Command;
			var inFile = command.GetValue("convert", "generic:0");
			var outFile = command.GetValue("out", "output", "generic:1");

			if (string.IsNullOrEmpty(inFile) && string.IsNullOrEmpty(outFile))
				return false;
			if (string.IsNullOrEmpty(inFile))
			{
				args.Writer.WriteLine("Input file name not specified");
				return true;
			}
			if (string.IsNullOrEmpty(outFile))
			{
				args.Writer.WriteLine("Output file name not specified");
				return true;
			}

			var formats = DocumentInfoCollection.Default;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var sourceFormat = formats.FindFormat(inFile);
			if (sourceFormat == null)
			{
				args.Writer.WriteLine("Error: Cannot find format to use for file '{0}'", inFile);
				return false;
			}

			var destinationFormat = sourceFormat.Info.GetCompatibleDocuments().FindFormat(outFile);
			if (destinationFormat == null)
			{
				args.Writer.WriteLine("Error: Cannot find format conversion from '{0}.{1}' format to file '{2}'", sourceFormat.Info.ID, sourceFormat.ID, outFile);
				return false;
			}

			args.Writer.WriteLine("Converting '{0}' ({1}.{2}) to '{3}' ({4}.{5})", inFile, sourceFormat.Info.ID, sourceFormat.ID, outFile, destinationFormat.Info.ID, destinationFormat.ID);

			var sourceDoc = sourceFormat.Info.Create(Generator.Current);
			sourceDoc.Info.SetOption("animation", "false");
			var sourceHandler = sourceDoc.CreateHandler();
			sourceHandler.Zoom = command.GetFloat("zoom", "z") ?? 1.0f;
			var formatId = sourceDoc.Info.OptionID ?? sourceDoc.Info.ID;
			foreach (var option in sourceDoc.Info.Options)
			{
				var val = command.GetValue(string.Format("{0}-{1}", formatId, option.ID));
				if (val != null)
				{
					sourceDoc.Info.SetOption(option.ID, val);
				}
			}
			/**
			var iters = 300;
			var sw2 = new Stopwatch ();
			sw2.Start ();
			for (int i = 0; i < iters; i++) {
			/**/
			sourceDoc.Load(inFile, sourceFormat, sourceHandler);
			/**
			}
			sw2.Stop ();
			args.Writer.WriteLine ("Convertion rate: {0}, total seconds: {1}", iters / sw2.Elapsed.TotalSeconds, sw2.Elapsed.TotalSeconds);
			/**/

			using (var destinationStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
			{
				sourceHandler.Save(destinationStream, destinationFormat);
			}
			stopwatch.Stop();

			args.Writer.WriteLine("Succesfully converted in {0} seconds", stopwatch.Elapsed.TotalSeconds);
			return true;
		}
	}
}

