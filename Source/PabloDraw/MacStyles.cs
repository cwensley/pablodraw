#if MAC
using Eto;
using Eto.Drawing;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Pablo.Interface;
using System;
using System.Runtime.InteropServices;
using Eto.Mac.Forms.ToolBar;

#if __UNIFIED__
using AppKit;
using ObjCRuntime;
using Foundation;
using CoreGraphics;

#else
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using CGSize = System.Drawing.SizeF;
using CGPoint = System.Drawing.PointF;
using CGRect = System.Drawing.RectangleF;
#endif

namespace PabloDraw
{
	public static class MacStyles
	{
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

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void void_objc_msgSend_IntPtr(IntPtr target, IntPtr sel, IntPtr arg1);


		static NSObject activityToken;
		static NSString ActivityReason { get { return new NSString("networking"); } }

		public static void Apply()
		{
			Style.Add<ApplicationHandler>(null, h => h.AppDelegate = new AppDelegate());

			Style.Add<Main>(null, ctl => ctl.ClientChanged += (sender, e) =>
				{
					if (NSProcessInfo.ProcessInfo.RespondsToSelector(Selector.FromHandle(selBeginActivity)))
					{
						// ensure we don't go to sleep when the server (or client) is running! (10.9)
						if (activityToken != null)
						{
							void_objc_msgSend_IntPtr(NSProcessInfo.ProcessInfo.Handle, selEndActivity, activityToken.Handle);
							activityToken.DangerousRelease();
							activityToken = null;
						}
						bool enabled = ctl.Client != null;
						if (enabled)
						{
							var token = IntPtr_objc_msgSend_Int64_IntPtr(NSProcessInfo.ProcessInfo.Handle, selBeginActivity, (Int64)NSActivityOptions.UserInitiated, ActivityReason.Handle);
							if (token != IntPtr.Zero)
							{
								activityToken = new NSObject(token);
								activityToken.DangerousRetain();
							}
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

			Style.Add<ToolBarHandler>(null, handler =>
				{
					handler.Control.DisplayMode = NSToolbarDisplayMode.Icon;
				});
			/*
			Style.Add<ButtonToolItemHandler>(null, handler =>
				{
				handler.UseStandardButton(false);
				});
				*/
			Style.Add<CheckToolItemHandler>(null, handler =>
				{
					handler.Control.MaxSize = new CGSize(16, 16);
					handler.Tint = Colors.Gray;
				});
		}
	}
}


#endif