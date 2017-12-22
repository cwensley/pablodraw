using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using System.Collections.Generic;

namespace Pablo
{
	public class ViewerPane : Scrollable, IViewer
	{
		static readonly float[] ZOOM_LEVELS = { 2, 1.5F, 1, .75F, .50F, .25F, .125F };

		#region Members

		readonly ImageViewer viewer;
		CheckAction actionZoomFitWidth;
		CheckAction actionZoomFitHeight;
		CheckAction actionAllowGrow;
		readonly Dictionary<float, RadioAction> zoomLevels = new Dictionary<float, RadioAction>();
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
		public override void OnMouseMove (MouseEventArgs e)
		{
			//base.OnMouseMove (e);
			viewer.OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location - adjust ));
		}
		*/

		public override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			UpdateSizes();
		}

		#region Properties

		public ImageViewer Viewer { get { return viewer; } }

		public float Zoom { get { return ZoomInfo.Zoom; } }

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
				if (ZoomInfo.FitWidth && ViewHandler.Size.Width > 0)
				{
					float val = (float)clientSize.Width / (float)ViewHandler.Size.Width / ViewHandler.Ratio.Width;
					if (val < 0.05F)
						val = 0.05F;
					zoom = val;
				}
				if (ZoomInfo.FitHeight && ViewHandler.Size.Height > 0)
				{
					float val = (float)clientSize.Height / (float)ViewHandler.Size.Height / ViewHandler.Ratio.Height;
					if (val < 0.05F)
						val = 0.05F;
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
			if (Generator.IsWinForms && ScrollPosition.Y + clientSize.Height > ushort.MaxValue)
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

			scrollIncrement = 0.12;
			scrollPosition = 0;
			scrollAmount = 0;
			const int max = 4;
			const double interval = 0.020;
			var startTime = DateTime.Now;
			double seconds = 0;
			oldScrollPosition = ScrollPosition;
			if (scrollTimer == null)
			{
				scrollTimer = new UITimer();
				scrollTimer.Interval = 0.010;
				scrollTimer.Elapsed += delegate
				{
					var pt = ScrollPosition;
					var maxPosition = ScrollSize.Height - ClientSize.Height;
					var newTime = DateTime.Now;
					seconds += (newTime - startTime).TotalSeconds;
					var count = (int)(seconds / interval);
					seconds -= (count * interval);
					startTime = newTime;
					for (int i = 0; i < count; i++)
					{
						//Console.WriteLine("Scroll Size: {0}, Client Size: {1}", this.ScrollSize, this.ClientSize);
						if (Math.Round(scrollPosition) >= maxPosition)
						{
							if (Math.Abs(scrollAmount) <= 2)
								scrollTimer.Stop();
							else
							{
								scrollAmount = -(scrollAmount * 0.5);
								scrollIncrement = 0.2;
								oldScrollPosition = pt;
							}
						}

						if (pt.Y == oldScrollPosition.Y)
						{
							scrollPosition += scrollAmount;
							scrollPosition = Math.Min(maxPosition, scrollPosition);
							if (scrollAmount < max)
								scrollAmount += scrollIncrement;
							pt.Y = (int)Math.Round(scrollPosition);
							oldScrollPosition = pt;
						}
						else
						{
							scrollTimer.Stop();
						}
					}
					ScrollPosition = pt;
				};
			}
			scrollTimer.Start();
		}

		void UpdateUI()
		{
			if (actionAllowGrow != null)
				actionAllowGrow.Checked = ZoomInfo.AllowGrow;
			if (actionZoomFitWidth != null)
				actionZoomFitWidth.Checked = ZoomInfo.FitWidth;
			if (actionZoomFitHeight != null)
				actionZoomFitHeight.Checked = ZoomInfo.FitHeight;
			if (zoomLevels != null)
			{
				RadioAction raction;
				if (zoomLevels.TryGetValue(ZoomInfo.Zoom, out raction))
					raction.Checked = true;
			}
		}

		public override bool HasFocus { get { return base.HasFocus || Viewer.HasFocus; } }

		public override void Focus()
		{
			Viewer.Focus();
		}

		public void GenerateActions(GenerateActionArgs args)
		{
			Viewer.GenerateActions(args);

			args.Actions.Add(new Actions.Autoscroll(this));
			args.Actions.Add(actionZoomFitWidth = new Actions.FitWidth(this));
			args.Actions.Add(actionZoomFitHeight = new Actions.FitHeight(this));
			args.Actions.Add(actionAllowGrow = new Actions.AllowGrow(this));

			ActionItemSubMenu smView = args.Menu.GetSubmenu("&View");

			smView.Actions.Add(Actions.Autoscroll.ActionID);

			ActionItemSubMenu smZoom = smView.Actions.GetSubmenu("&Zoom");

			smZoom.Actions.Add(Actions.FitWidth.ActionID);
			smZoom.Actions.Add(Actions.FitHeight.ActionID);
			smZoom.Actions.Add(Actions.AllowGrow.ActionID);

			smZoom.Actions.AddSeparator();


			RadioAction controller = null;
			zoomLevels.Clear();
			foreach (float zoomLevel in ZOOM_LEVELS)
			{
				var raction = args.Actions.AddRadio(controller, "zoom" + zoomLevel, string.Format("{0}%", zoomLevel * 100));
				raction.Activated += (sender, e) =>
				{
					var action = sender as RadioAction;
					ZoomInfo.Zoom = (float)action.Tag;
					UpdateSizes();
				};
				if (controller == null)
					controller = raction;
				raction.Tag = zoomLevel;
				smZoom.Actions.Add(raction.ID);
				zoomLevels.Add(zoomLevel, raction);
			}
			UpdateUI();
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
