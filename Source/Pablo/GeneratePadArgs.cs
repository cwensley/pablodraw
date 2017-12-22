using System;
using System.Collections.Generic;
using Eto.Forms;

namespace Pablo
{
	public class GeneratePadArgs
	{
		public IList<Control> BottomPads { get; private set; }
		public IList<Control> RightPads { get; private set; }
		public IList<Control> TopPads { get; private set; }
		public IList<Control> LeftPads { get; private set; }
		
		public GeneratePadArgs()
		{
			BottomPads = new List<Control>();
			RightPads = new List<Control>();
			TopPads = new List<Control>();
			LeftPads = new List<Control>();
		}
	}
}

