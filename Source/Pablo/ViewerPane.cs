using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace Pablo
{
	public interface IUpdatableCommand
	{
		void UpdateState();
	}
	
	public class ViewerPane : Scrollable, IViewer
	{
		const float MinZoom = 0.125f;
		public static readonly float[] ZOOM_LEVELS = { 7, 6.5F, 6, 5.5F, 5, 4.5F, 4, 3.5F, 3, 2, 1.5F, 1, .75F, .50F, .25F, .125F };

		#region Members

		readonly ImageViewer viewer;
		readonly List<IUpdatableCommand> updatableCommands = new List<IUpdatableCommand>();
		readonly PixelLayout layout;
		UITimer scrollTimer;
		Point oldScrollPosition;
		double scrollIncrement;
		double scrollAmount;
		double scrollPosition;
		ZoomInfo zoomInfo;
		bool initialized;
#if DESKTOP
		Point adjust;
		int updateResize;
#endif

		public Handler ViewHandler { get; private set; }

		#endregion

		/*
		~ViewerPane()
		{
			Debug.Print(string.Format("GC: {0}", this.GetType().FullName));
		}*/

		public ViewerPane(Handler handler, bool largeCanvas)
		{
			this.ViewHandler = handler;
			this.Style = "viewerPane";

#if MOBILE
			this.MinimumZoom = 0.25f;
			this.MaximumZoom = 3.0f;
			ExpandContentWidth = ExpandContentHeight = false;
#endif

			BackgroundColor = handler.Document.EditMode ? Color.FromArgb(48, 48, 48) : Colors.Black;

			Border = BorderType.None;
			layout = new PixelLayout();

			viewer = new ImageViewer(this, largeCanvas);
			viewer.SizeChanged += viewer_SizeChanged;
			layout.Add(viewer, 0, 0);
			viewer.Visible = false;

			Content = layout;

		}

		/*
		protected override void OnMouseMove (MouseEventArgs e)
		{
			//base.OnMouseMove (e);
			viewer.OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location - adjust ));
		}
		*/

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// when switching to fullscreen we don't get updated sizes properly with OnSizeChanged.. hrm.
			if (Platform.IsMac)
				ParentWindow.SizeChanged += ParentWindow_SizeChanged;
		}

		protected override void OnUnLoad(EventArgs e)
		{
			base.OnUnLoad(e);
			if (Platform.IsMac)
				ParentWindow.SizeChanged -= ParentWindow_SizeChanged;
		}

		private void ParentWindow_SizeChanged(object sender, EventArgs e)
		{
			UpdateSizes();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			UpdateSizes();
		}

		#region Properties

		public ImageViewer Viewer { get { return viewer; } }

		public new float Zoom { get { return ZoomInfo.Zoom; } }

		public ZoomInfo ZoomInfo
		{
			get { return zoomInfo ?? ViewHandler.Document.Info.ZoomInfo; }
			set { zoomInfo = value; }
		}

		#endregion

		//float lastZoom;
		public void UpdateSizes()
		{
#if DESKTOP
			if (updateResize > 3) //== 0)
				return; //lastZoom = -1;

			//if (updateResize < 1)
			{
				// update twice, as the first time doesn't have scrollbars, but second time we may..
				// this will make it so that the smaller of the two sizes is chosen.
				updateResize++;
				if (ViewHandler != null)
				{
					float zoom = CalculateZoom();
					//if (lastZoom < zoom && lastZoom != -1)
					//	zoom = lastZoom;
					if (zoom > 0 && zoom != ViewHandler.Zoom/* && (updateResize < 2 || zoom < lastZoom) && (zoom != lastZoom || lastZoom == -1)*/)
					{
						//Console.WriteLine ("Zoom: {0}, last: {1}, count: {2}", zoom, lastZoom, updateResize);
						//if (zoom < lastZoom || lastZoom == -1) lastZoom = zoom;
						//lastZoom = zoom;
						ViewHandler.Zoom = zoom;
					}
				}
				UpdateScroll();

				updateResize--;
			}
#else
			base.Zoom = CalculateZoom ();
			UpdateScroll ();
#endif
		}

		float CalculateZoom()
		{
			float zoom = ZoomInfo.Zoom;
			if (ViewHandler != null)
			{
				if (!ViewHandler.EnableZoom)
					return 1.0f;

				Size clientSize = ClientSize;
				if (Platform.IsMac && clientSize.IsEmpty)
                {
                    // bug in eto?
					clientSize = Size;
                }
				if (ZoomInfo.FitWidth && ViewHandler.Size.Width > 0)
				{
					float val = (float)clientSize.Width / (float)ViewHandler.Size.Width / ViewHandler.Ratio.Width;
					if (val < MinZoom)
						val = MinZoom;
					zoom = val;
				}
				if (ZoomInfo.FitHeight && ViewHandler.Size.Height > 0)
				{
					float val = (float)clientSize.Height / (float)ViewHandler.Size.Height / ViewHandler.Ratio.Height;
					if (val < MinZoom)
						val = MinZoom;
					if (val < zoom)
						zoom = val;
				}
				if (!ZoomInfo.AllowGrow && zoom > ZoomInfo.Zoom)
					zoom = ZoomInfo.Zoom;
			}
			return zoom;
		}

		void UpdateScroll()
		{
#if DESKTOP
			Size clientSize = ClientSize;
			if (Platform.IsMac && clientSize.IsEmpty)
			{
				// bug in eto?
				clientSize = Size;
			}
			if (Platform.IsWinForms && ScrollPosition.Y + clientSize.Height > ushort.MaxValue)
			{
				//ScrollPosition = Point.Empty;
			}
			adjust = Point.Empty;
			Size size = viewer.Size;
			adjust.X = (size.Width < clientSize.Width) ? ((clientSize.Width - size.Width) / 2) : 0;
			adjust.Y = (size.Height < clientSize.Height) ? ((clientSize.Height - size.Height) / 2) : 0;
			layout.Move(viewer, adjust);
			//Debug.WriteLine ("Adjusting: {0}, size: {1}, ClientSize: {2}", adjust, size, clientSize);
#endif
			UpdateScrollSizes();
		}

		void viewer_SizeChanged(object sender, EventArgs e)
		{
            if (this.Loaded)
    			UpdateSizes();
		}

		void StartAutoScroll()
		{
			if (scrollTimer != null)
				scrollTimer.Stop();
			if (!ViewHandler.Document.Info.AutoScroll || ViewHandler.Document.EditMode)
				return;

			if (ScrollSize.Height <= ClientSize.Height)
				return;

			scrollIncrement = 0.08;
			scrollPosition = 0;
			scrollAmount = 0;
			const int maxSpeed = 4;
			const double interval = 0.016;
			var startTime = DateTime.UtcNow;
			double seconds = 0;
			oldScrollPosition = ScrollPosition;
			if (scrollTimer == null)
			{
				scrollTimer = new UITimer();
				scrollTimer.Interval = 0.010;
				scrollTimer.Elapsed += (sender, e) =>
				{
					var pt = ScrollPosition;
					if (pt.Y != oldScrollPosition.Y)
					{
						// user scrolled
						scrollTimer.Stop();
						return;
					}
					var maxPosition = ScrollSize.Height - ClientSize.Height;
					var newTime = DateTime.UtcNow;
					seconds += (newTime - startTime).TotalSeconds;
					var count = (seconds / interval);
					seconds -= (count * interval);
					startTime = newTime;
					while (count > 0)
					{
						var inc = Math.Min(count, 1);

						if (scrollPosition >= maxPosition)
						{
							if (Math.Abs(scrollAmount) <= 1)
							{
								// reached end, stop
								scrollPosition = maxPosition;
								scrollTimer.Stop();
								break;
							}
							scrollAmount = -(scrollAmount * 0.5);
						}

						scrollPosition = Math.Min(maxPosition, scrollPosition + scrollAmount * inc);
						scrollAmount = Math.Min(maxSpeed, scrollAmount + scrollIncrement * inc);

						count -= inc;
					}
					pt.Y = (int)Math.Round(scrollPosition);
					oldScrollPosition = pt;
					ScrollPosition = pt;
				};
			}
			scrollTimer.Start();
		}

		internal void UpdateMenuItems()
		{
			foreach (var item in updatableCommands)
			{
				item.UpdateState();
			}
		}

		public override bool HasFocus { get { return base.HasFocus || Viewer.HasFocus; } }

		public override void Focus()
		{
			Viewer.Focus();
		}

		public void GenerateCommands(GenerateCommandArgs args)
		{
			Viewer.GenerateCommands(args);

			var smView = args.Menu.Items.GetSubmenu("&View");

			smView.Items.Add(new Actions.Autoscroll(this), 500);

			var smZoom = smView.Items.GetSubmenu("&Zoom", 500);

			updatableCommands.Clear();
			CreateZoomMenu(smZoom);
			
			UpdateMenuItems();
		}


		private void CreateZoomMenu(ButtonMenuItem menu)
		{
			menu.Items.Add(new Actions.ZoomIn(this));
			menu.Items.Add(new Actions.ZoomOut(this));
			menu.Items.Add(new Actions.ZoomReset(this));

			menu.Items.AddSeparator();

			Actions.ZoomLevel controller = null;
			foreach (float zoomLevel in ZOOM_LEVELS)
			{
				var zoomLevelCommand = new Actions.ZoomLevel(controller, this, zoomLevel);
				if (controller == null)
					controller = zoomLevelCommand;
				menu.Items.Add(zoomLevelCommand);
			}
			menu.Items.AddSeparator();

			menu.Items.Add(new Actions.FitWidth(this));
			menu.Items.Add(new Actions.FitHeight(this));
			menu.Items.Add(new Actions.AllowGrow(this));
			
			updatableCommands.AddRange(menu.Items.Select(r => r.Command).OfType<IUpdatableCommand>());
		}

		protected override void Dispose(bool disposing)
		{
			if (scrollTimer != null)
			{
				scrollTimer.Dispose();
				scrollTimer = null;
			}

#if MOBILE
			if (this.ViewHandler != null) {
				Console.WriteLine ("Disposing viewer pane");
				if (this.ViewHandler.Document != null) {
					this.ViewHandler.Document.Dispose ();
				}
				this.ViewHandler.Dispose ();
				//this.ViewHandler = null;
			}
#endif

			base.Dispose(disposing);
		}

		#region IViewer implementation

		public Size ViewSize
		{
			get { return ClientSize; }
		}

		public void EnsureVisible(Rectangle rectangle)
		{

		}

		public new void PreLoad()
		{

		}

		public void PostLoad()
		{
			UpdateScrollSizes();
			StartAutoScroll();
		}

		public void DocumentLoaded()
		{
			if (!initialized)
			{
				Viewer.HookupEvents();
				initialized = true;
			}
		}

		public virtual void BackgroundLoaded()
		{
			if (!initialized)
			{
				Viewer.HookupEvents();
				initialized = true;
			}
		}

		#endregion
	}
}
