using System;
using Eto.Drawing;

namespace Pablo.Formats.Character
{
	public interface IGenerateRegion
	{
		CanvasElement? GetElement(Point point, Canvas canvas);
		
		void TranslateColour(Point point, ref int foreColour, ref int backColour);
	}
}

