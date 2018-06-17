using System;
using System.Reflection;
using System.Linq;
using Eto;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pablo.Network;
using System.Text;
using Pablo;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using log4net;

namespace PabloDraw.Console
{
	static class Startup
	{
		static EmbeddedAssemblyLoader loader;
	    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal static void EnsureInternalAssemblies()
		{
			if (loader == null)
				loader = EmbeddedAssemblyLoader.Register("PabloDraw.Console.Assemblies");
		}

		[STAThread]
		static int Main()
		{
		    try
		    {
		        Log.InfoFormat("Start PabloDraw.Console {0} Application at {1}", Assembly.GetExecutingAssembly().GetName().Version, DateTime.Now);

                EnsureInternalAssemblies();

		        // ensure we run everything using the ThreadPool (monomac will use its own if we don't set it here)
		        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

		        return Program.Run();

		    }
		    catch (Exception ex)
		    {
		        Log.Fatal("Fatal Error happened", ex);

		        return 0;

		    }

		}
	}
}

