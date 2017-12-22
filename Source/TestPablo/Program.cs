using System;
using System.Text;
using System.IO;
using Pablo;
using Pablo.Sauce;
using Pablo.Formats.Character;
using Eto;
using Eto.Drawing;
using Eto.Misc;
using System.Collections.Generic;

namespace PabloDraw
{
	class Program
	{
		static DocumentInfoCollection GetFormats()
		{
			var formats = new DocumentInfoCollection();
		
			formats.Add(new Pablo.Formats.Character.CharacterDocumentInfo());
			//info.Add(new Pablo.Formats.Character.Character24DocumentInfo());
			formats.Add(new Pablo.Formats.Rip.RipDocumentInfo());
			formats.Add(new Pablo.Formats.Image.ImageDocumentInfo());
			formats.DefaultFormat = formats["character"].Formats["ansi"];
			return formats;
			
		}
		
		public static void Start(string[] args)
		{
			if (args == null || args.Length < 2)
			{
				Console.WriteLine("Usage: TestPablo.exe <InputFile.ext> <OutputFile.ext> [<option>=<value>, ...]");
				Console.WriteLine();
				Console.WriteLine("Global options:");
				Console.WriteLine("\tzoom = zoom factor, where 1.0 is full size.  (default is 1.0)");
				Console.WriteLine();
				Console.WriteLine("Character format options:");
				Console.WriteLine("\tdosaspect = true|false (default is false)");
				Console.WriteLine("\tuse9x = true|false  (default is false)");
				Console.WriteLine();
				Console.WriteLine("RIP format options:");
				Console.WriteLine("\tdosaspect = true|false  (default is false)");
				return;
			}
			string fileToLoad = args[0];
			string fileToSave = args[1];
			
			var options = new Dictionary<string, string>();
			if (args.Length > 2)
			{
				for (int i=2; i<args.Length; i++)
				{
					string[] split = args[i].Split('=');
					if (split.Length != 2)
					{
						Console.WriteLine("Invalid option {0}", args[i]);
						return;
					}
					options[split[0]] = split[1];
				}
			}
			
			var start = DateTime.Now;
			
			// use appropriate api
			Generator generator;
			/*if (Platform.IsUnix) generator = new Eto.Platform.GtkSharp.Generator(); // hm, gtk doesn't like working without a GUI..
			else*/
			generator = new Eto.Platform.Windows.Generator();
			
			var formats = GetFormats();
			
			var sourceFormat = formats.FindFormat(fileToLoad);
			if (sourceFormat == null)
			{
				Console.WriteLine("Error: Cannot find format to use for file '{0}'", fileToLoad);
				return;
			}
			
			var destinationFormat = sourceFormat.Info.GetCompatibleDocuments().FindFormat(fileToSave);
			if (destinationFormat == null)
			{
				Console.WriteLine("Error: Cannot find format conversion from '{0}.{1}' format to file '{2}'", sourceFormat.Info.ID, sourceFormat.ID, fileToSave);
				return;
			}
			
			var sourceDoc = sourceFormat.Info.Create(generator);
			sourceDoc.Info.SetOption("animation", "false");
			var sourceHandler = sourceDoc.CreateHandler();
			foreach (var option in options)
			{
				if (string.Equals(option.Key, "zoom", StringComparison.InvariantCultureIgnoreCase))
				{
					sourceHandler.Zoom = float.Parse(option.Value);
				}
				else if (!sourceDoc.Info.SetOption(option.Key, option.Value))
				{
					Console.WriteLine("Option '{0}' is invalid or could not be set to '{1}'", option.Key, option.Value);
					return;
				}
			}
			sourceDoc.Load(fileToLoad, sourceFormat, sourceHandler);
			
			using (var destinationStream = new FileStream(fileToSave, FileMode.Create, FileAccess.Write))
			{
				sourceHandler.Save(destinationStream, destinationFormat);
			}
			var elapsed = DateTime.Now - start;
			
			Console.Write("Succesfully converted '{0}' ('{1}.{2}') to '{3}' ('{4}.{5}')", fileToLoad, sourceFormat.Info.ID, sourceFormat.ID, fileToSave, destinationFormat.Info.ID, destinationFormat.ID);
			Console.WriteLine (" in {0} seconds", elapsed.TotalSeconds);
			
		}
	}
}
