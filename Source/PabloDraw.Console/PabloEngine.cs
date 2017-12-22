using System.IO;
using System.Collections.Generic;

namespace PabloDraw
{
	public class ConversionOption
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public ConversionOption()
		{
		}

		public ConversionOption(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}

	public class InputParameters
	{
		public string InputFileName { get; set; }

		public Stream InputStream { get; set; }

		public string InputFormat { get; set; }

		public float Zoom { get; set; }

		public int? MaxWidth { get; set; }

		public IEnumerable<ConversionOption> Options { get; set; }

		public InputParameters()
		{
			Zoom = 1f;
		}
	}

	public class ConvertParameters : InputParameters
	{
		public string OutputFileName { get; set; }

		public Stream OutputStream { get; set; }

		public string OutputFormat { get; set; }
	}

	public class FileInfo
	{
		public int ImageWidth { get; set; }

		public int ImageHeight { get; set; }
	}

	public class PabloEngine
	{
		static PabloEngine()
		{
			Console.Startup.EnsureInternalAssemblies();
		}

		public PabloEngine(string platform = null, string defaultWindowsPlatform = "win")
		{
			EngineInternal.Initialize(platform, defaultWindowsPlatform);
		}

		public string DetectFormat(string fileName)
		{
			return EngineInternal.DetectFormat(fileName);
		}

		public string DetectType(string fileName)
		{
			return EngineInternal.DetectType(fileName);
		}

		public void Convert(ConvertParameters parameters)
		{
			EngineInternal.Convert(parameters);
		}

		public FileInfo GetInfo(InputParameters parameters)
		{
			return EngineInternal.GetInfo(parameters);
		}

		public bool SupportsFile(InputParameters parameters)
		{
			return EngineInternal.SupportsFile(parameters);
		}
	}
}

