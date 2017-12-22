using System;
using System.IO;
using Pablo;
using System.Linq;
using Eto;
using System.Collections.Generic;

namespace PabloDraw
{
	static class EngineInternal
	{
		static Format GetFormat(string formatId, string fileName)
		{
			Format format = null;
			if (!string.IsNullOrEmpty(formatId))
				format = DocumentInfoCollection.Default.GetFormats().Values.FirstOrDefault(r => string.Equals(r.ID, formatId, StringComparison.OrdinalIgnoreCase));
			if (format == null)
				format = DocumentInfoCollection.Default.FindFormat(fileName);
			return format;
		}

		public static void Convert(ConvertParameters parameters)
		{
			Stream inStream = null;
			Stream outStream = null;
			try
			{
				using (var b = Platform.Instance.ThreadStart())
				{
					inStream = parameters.InputStream;
					if (inStream == null)
						inStream = File.OpenRead(parameters.InputFileName);

					var inFormat = GetFormat(parameters.InputFormat, parameters.InputFileName);
					if (inFormat == null)
						throw new PabloException(string.Format("Error: Cannot find format to use for file '{0}'", parameters.InputFileName));

					var outFormat = GetFormat(parameters.OutputFormat, parameters.OutputFileName);
					if (outFormat == null)
						throw new PabloException(string.Format("Error: Cannot find format conversion from '{0}.{1}' format to file '{2}'", inFormat.Info.ID, inFormat.ID, parameters.OutputFileName));

					using (var sourceDoc = inFormat.Info.Create(Platform.Instance))
					{
						sourceDoc.Info.SetOption("animation", "false");
						using (var sourceHandler = sourceDoc.CreateHandler())
						{
							var formatId = sourceDoc.Info.OptionID ?? sourceDoc.Info.ID;
							if (parameters.Options != null)
							{
								foreach (var option in sourceDoc.Info.Options)
								{
									var optionId = string.Format("{0}-{1}", formatId, option.ID);
									var val = parameters.Options.FirstOrDefault(r => string.Equals(r.Name, optionId, StringComparison.OrdinalIgnoreCase));
									if (val != null)
									{
										sourceDoc.Info.SetOption(option.ID, val.Value);
									}
								}
							}

							sourceDoc.FileName = parameters.InputFileName;
							sourceDoc.Load(inStream, inFormat, sourceHandler);

							sourceHandler.Zoom = parameters.Zoom;
							var drawSize = sourceHandler.DrawSize;
							if (parameters.MaxWidth != null && drawSize.Width > parameters.MaxWidth.Value)
							{
								sourceHandler.Zoom = (float)parameters.MaxWidth.Value / (float)sourceHandler.Size.Width;
							}

							outStream = parameters.OutputStream;
							if (outStream == null)
								outStream = File.Create(parameters.OutputFileName);
							sourceHandler.Save(outStream, outFormat);
						}
					}
				}
			}
			catch
			{
				// clean up file if conversion failed
				if (parameters.OutputStream == null && File.Exists(parameters.OutputFileName))
				{
					if (outStream != null)
					{
						outStream.Close();
						outStream.Dispose();
						outStream = null;
					}
					File.Delete(parameters.OutputFileName);
				}
				throw;
			}
			finally
			{
				if (inStream != null && parameters.InputStream == null)
					inStream.Dispose();
				if (outStream != null && parameters.OutputStream == null)
					outStream.Dispose();

			}
		}

		public static FileInfo GetInfo(InputParameters parameters)
		{
			Stream inStream = null;
			try
			{
				using (var b = Platform.Instance.ThreadStart())
				{
					inStream = parameters.InputStream;
					if (inStream == null)
						inStream = File.OpenRead(parameters.InputFileName);

					var inFormat = GetFormat(parameters.InputFormat, parameters.InputFileName);
					if (inFormat == null)
						throw new PabloException(string.Format("Error: Cannot find format to use for file '{0}'", parameters.InputFileName));

					using (var sourceDoc = inFormat.Info.Create(Platform.Instance))
					{
						sourceDoc.Info.SetOption("animation", "false");
						sourceDoc.Info.SetOption("autoresize", "true");
						using (var sourceHandler = sourceDoc.CreateHandler())
						{
							var formatId = sourceDoc.Info.OptionID ?? sourceDoc.Info.ID;
							if (parameters.Options != null)
							{
								foreach (var option in sourceDoc.Info.Options)
								{
									var optionId = string.Format("{0}-{1}", formatId, option.ID);
									var val = parameters.Options.FirstOrDefault(r => string.Equals(r.Name, optionId, StringComparison.OrdinalIgnoreCase));
									if (val != null)
									{
										sourceDoc.Info.SetOption(option.ID, val.Value);
									}
								}
							}

							sourceDoc.FileName = parameters.InputFileName;
							sourceDoc.Load(inStream, inFormat, sourceHandler);

							sourceHandler.Zoom = parameters.Zoom;
							var drawSize = sourceHandler.DrawSize;
							if (parameters.MaxWidth != null && drawSize.Width > parameters.MaxWidth.Value)
							{
								sourceHandler.Zoom = (float)parameters.MaxWidth.Value / (float)sourceHandler.Size.Width;
							}

							drawSize = sourceHandler.DrawSize;
							return new FileInfo { ImageWidth = drawSize.Width, ImageHeight = drawSize.Height };
						}
					}
				}
			}
			finally
			{
				if (inStream != null && parameters.InputStream == null)
					inStream.Dispose();
			}
		}

		public static bool SupportsFile(InputParameters parameters)
		{
			return GetFormat(parameters.InputFormat, parameters.InputFileName) != null;
		}

		public static void Initialize(string platform, string defaultWindowsPlatform = null)
		{
			if (Platform.Instance != null)
				return;
			if (platform == null && EtoEnvironment.Platform.IsWindows)
				platform = defaultWindowsPlatform;
			if (!string.IsNullOrEmpty(platform))
			{
				switch (platform.ToLowerInvariant())
				{
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
					case "osx":
					case "mac":
						Platform.Initialize(Platforms.Mac);
						break;
					case "auto":
						Platform.Initialize(Platform.Detect);
						break;
					default:
						throw new ArgumentException(string.Format("Platform '{0}' is not recognized. Must be one of (gtk|winforms|wpf)", platform));
				}
			}
			else
			{
				Platform.Initialize(Platform.Detect);
			}
		}

		public static string DetectFormat(string fileName)
		{
			var format = DocumentInfoCollection.Default.FindFormat(fileName);
			return format != null ? format.ID : null;
		}

		public static string DetectType(string fileName)
		{
			var info = DocumentInfoCollection.Default.Find(fileName);
			return info != null ? info.ID : null;
		}
	}
}
