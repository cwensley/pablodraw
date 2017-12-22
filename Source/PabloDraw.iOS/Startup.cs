using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Eto.Forms;
using Eto.Platform.iOS.Forms;
using System.IO;
using Eto;
using Eto.Platform.iOS.Forms.Controls;

namespace PabloDraw.iOS
{
	public class Startup
	{
		static void Main (string[] args)
		{
			CopyDb();
			Style.Add<ScrollableHandler> ("viewerPane", handler => handler.Control.IndicatorStyle = UIScrollViewIndicatorStyle.White);
			Style.Add<ApplicationHandler> ("pablo", handler => {
				//handler.DelegateClassName = "AppDelegate";
			});

			//UIApplication.CheckForIllegalCrossThreadCalls = false;

			var app = new Pablo.Mobile.PabloApplication ();

			app.Initialized += HandleAppInitialized;
			app.Run();
		}

		static void HandleAppInitialized (object sender, EventArgs e)
		{
			UINavigationBar.Appearance.TintColor = UIColor.Black;
		}
		
		static void CopyDb ()
		{
			string destPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // This goes to the documents directory for your app
			 
			string sourcePath = Environment.CurrentDirectory;  // This is the package such as MyApp.app/
			
			//Console.WriteLine("source: {0} dest: {1}", sourcePath, destPath);
			var dir = Eto.IO.EtoDirectoryInfo.GetDirectory (sourcePath);
			 
			foreach (var file in dir.GetFiles (new string[] { "*.ans", "*.rip" }))
			{
				var filePath = Path.Combine(destPath, file.Name);
				if (!File.Exists(filePath)) {
					File.Copy(file.FullName, filePath);
				}
			}
		 }		
	}
}

