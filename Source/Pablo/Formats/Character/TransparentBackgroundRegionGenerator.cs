using Eto.Drawing;

namespace Pablo.Formats.Character
{
	class TransparentBackgroundRegionGenerator : IGenerateRegion
	{
		public CanvasElement? GetElement(Point point, Canvas canvas)
		{
			return null;
		}

		public void TranslateColour(Point point, ref CanvasElement ce, ref int foreColour, ref int backColour)
		{
			if (ce.Background == 0)
				backColour = 0;
		}
	}
}

