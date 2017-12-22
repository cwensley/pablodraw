using System;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.BGI
{
	/// <summary>
	/// Summary description for IBGIFont.
	/// </summary>
	public interface IBGIFont
	{
		float DrawCharacter (BGICanvas bgi, float x, float y, BGICanvas.Direction dir, int size, byte character, IList<Rectangle> updates);

		Size GetTextSize (string str, BGICanvas.Direction dir, int size);

		Size GetRealTextSize (string str, BGICanvas.Direction dir, int size);

		Size GetMaxCharacterSize (BGICanvas.Direction dir, int size);
	}
}
