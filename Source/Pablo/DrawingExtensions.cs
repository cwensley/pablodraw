using Eto.Drawing;

namespace Pablo
{
	static class DrawingExtensions
	{
		public static Rectangle FromHalfMode(this Rectangle rect)
		{
			rect.Normalize();
			rect.Height = (rect.Height + 1 + rect.Y % 2) / 2;
			rect.Y /= 2;
			return rect;
		}
	}
}

