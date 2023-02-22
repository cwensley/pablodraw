using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;

namespace Pablo.Formats.Character.Actions
{
	public partial class ExportAsIcon : Command
	{
		public const string ActionID = "character_exportIcon";
		CharacterHandler handler;

		public ExportAsIcon(CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			MenuText = "Export as Icon...";
			ToolBarText = "Export Icon";
			ToolTip = "Exports the file as an image suitable to use as an Icon";
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);

			var page = handler.CurrentPage;
			var canvasSize = page.Canvas.Size;
			var fontSize = page.Font.Size;

			// export square size
			var exportSize = canvasSize * fontSize;
			if (exportSize.Width > exportSize.Height)
				exportSize.Width = exportSize.Height;
			else
				exportSize.Height = exportSize.Width;


			// var oldAllowEditing = handler.AllowEditing;
			// handler.AllowEditing = false;
			// var bmp = handler.Generate(new Rectangle(Point.Empty, exportSize));
			// handler.AllowEditing = oldAllowEditing;
			Rectangle? area = new Rectangle(Point.Empty, exportSize);
			var rectDraw = area != null ? area.Value : new RectangleF(0, 0, handler.Size.Width, handler.Size.Height);
			handler.CalculateRect(Rectangle.Round(rectDraw), out var rectScreen, out var rectGenerate);

			//Console.WriteLine("{0}, {1}, {2}", Size, rectScreen, rectGenerate);
			var generator = new TransparentBackgroundRegionGenerator();
			var bmp = new Bitmap(rectScreen.Width, rectScreen.Height, PixelFormat.Format32bppRgba);
			page.GenerateRegion(bmp, rectGenerate, page.Font.Size, null, null, handler.CharacterDocument.ICEColours, false, null, generator);
			
			
			var resized = new Bitmap(bmp, 32, 32, ImageInterpolation.High);
			var saveDialog = new SaveFileDialog
			{
				Filters = {
					new FileFilter("PNG files (*.png)", ".png"),
					new FileFilter("JPEG files (*.jpeg)", ".jpeg"),
					new FileFilter("GIF files (*.gif)", ".gif")
				}
			};

			var result = saveDialog.ShowDialog(handler.ViewerControl);
			if (result == DialogResult.Ok)
			{
				var fmt = saveDialog.CurrentFilterIndex == 0
					? ImageFormat.Png
					: saveDialog.CurrentFilterIndex == 1
					? ImageFormat.Jpeg
					: ImageFormat.Gif;
					
				resized.Save(saveDialog.FileName, fmt);
			}

		}
	}
}

