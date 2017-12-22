using Eto;
using Eto.Drawing;
using Eto.Platform.Mac;
using Eto.Platform.Mac.Forms;
using Eto.Platform.Mac.Forms.Controls;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Pablo.Interface;
using MonoMac.Foundation;
using System;
using System.Runtime.InteropServices;

namespace PabloDraw
{
	public static class Startup
	{
		static void Main(string[] args)
		{
			AddStyles();

			var app = new Pablo.Interface.Application(new Eto.Platform.Mac.Generator());

			ApplicationHandler.Instance.AppDelegate = new AppDelegate();
			app.Run(args);
		}

		enum NSActivityOptions : ulong
		{
			IdleDisplaySleepDisabled = (1 << 40),
			IdleSystemSleepDisabled = (1 << 20),
			SuddenTerminationDisabled = (1 << 14),
			AutomaticTerminationDisabled = (1 << 15),
			UserInitiated = (0x00FFFFFF | IdleSystemSleepDisabled),
			UserInitiatedAllowingIdleSystemSleep = (UserInitiated & ~IdleSystemSleepDisabled),
			Background = 0x000000FF,
			LatencyCritical = 0xFF00000000,
		};

		static readonly IntPtr selBeginActivity = Selector.GetHandle("beginActivityWithOptions:reason:");
		static readonly IntPtr selEndActivity = Selector.GetHandle("endActivity:");

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern IntPtr IntPtr_objc_msgSend_Int64_IntPtr(IntPtr target, IntPtr sel, Int64 options, IntPtr reason);

		static IntPtr activityToken = IntPtr.Zero;

		static void AddStyles()
		{
			Style.Add<Main>(null, ctl => ctl.ClientChanged += (sender, e) =>
			{
				if (NSProcessInfo.ProcessInfo.RespondsToSelector(Selector.FromHandle(selBeginActivity)))
				{
					// ensure we don't go to sleep when the server (or client) is running! (10.9)
					if (activityToken != IntPtr.Zero)
					{
						Messaging.void_objc_msgSend_IntPtr(NSProcessInfo.ProcessInfo.Handle, selEndActivity, activityToken);
						activityToken = IntPtr.Zero;
					}
					bool enabled = ctl.Client != null;
					if (enabled)
					{
						activityToken = IntPtr_objc_msgSend_Int64_IntPtr(NSProcessInfo.ProcessInfo.Handle, selBeginActivity, (Int64)NSActivityOptions.Background, new NSString("networking").Handle);
					}
				}
			});

			Style.Add<FormHandler>("main", handler => handler.Control.CollectionBehavior |= NSWindowCollectionBehavior.FullScreenPrimary);
			/*control.WillUseFullScreenPresentationOptions = (window, proposedOptions) => {
					return NSApplicationPresentationOptions.FullScreen | NSApplicationPresentationOptions.AutoHideToolbar | NSApplicationPresentationOptions.AutoHideMenuBar | NSApplicationPresentationOptions.AutoHideDock;
				};*/
						
			Style.Add<ListBoxHandler>("fileList", handler =>
			{
				handler.Scroll.BorderType = NSBorderType.NoBorder;
				handler.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
			});

			Style.Add<ApplicationHandler>("application", handler => handler.EnableFullScreen());

			Style.Add<ScrollableHandler>("viewerPane", handler =>
			{
				if (handler.Control.RespondsToSelector(new Selector("setScrollerKnobStyle:")))
				{
					handler.Control.ScrollerKnobStyle = NSScrollerKnobStyle.Light;
				}
			});

			Style.Add<ToolBarHandler>(null, handler => handler.Control.DisplayMode = NSToolbarDisplayMode.Icon);

			Style.Add<ToolBarButtonHandler>(null, handler => handler.UseStandardButton(false));
			Style.Add<CheckToolBarButtonHandler>(null, handler =>
			{
				handler.Control.MaxSize = new System.Drawing.SizeF(16, 16);
				handler.Tint = Colors.Gray;
			});
		}
	}
}

