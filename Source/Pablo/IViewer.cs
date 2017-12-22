using System;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo
{
	public interface IViewer
	{
		Size ViewSize { get; }

		Point ScrollPosition { get; set; }

		void EnsureVisible(Rectangle rect);

		void GenerateCommands(GenerateCommandArgs args);

		void PreLoad();

		void DocumentLoaded();

		void BackgroundLoaded();

		void PostLoad();

		float Zoom { get; }
	}
}

