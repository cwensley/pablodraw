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

namespace PabloDraw.Console
{
	static class Startup
	{
		[STAThread]
		static int Main()
		{
			// ensure we run everything using the ThreadPool (monomac will use its own if we don't set it here)
			SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

			return Program.Run();
		}
	}
}

