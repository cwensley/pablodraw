using System;

namespace PabloDraw
{
	public static class Startup
	{
		[STAThread]
		static void Main()
		{
			EmbeddedAssemblyLoader.Register("PabloDraw.Assemblies");
			Program.Run();
		}
	}
}

