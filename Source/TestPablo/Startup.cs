using System;
using System.Reflection;
using Eto.Misc;
using Eto;

namespace PabloDraw
{
	public static class Startup
	{
		static Startup() {
			
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
				string resourceName = "PabloDraw.Assemblies." + new AssemblyName(args.Name).Name + ".dll";
				//Console.WriteLine("Resource: {0}", resourceName);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
					byte[] assemblyData = new byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};		
		}
		
		[STAThread]
		static void Main(string[] args)
		{
			Program.Start(args);
		}
	}
}

