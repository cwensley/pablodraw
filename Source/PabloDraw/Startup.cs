using System;

namespace PabloDraw
{
	public static class Startup
	{
		[STAThread]
		static void Main()
		{
            EmbedReferences.Init();
			Program.Run();
		}
	}
}

