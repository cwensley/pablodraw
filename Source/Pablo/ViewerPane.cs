using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using System.Collections.Generic;
using System.Timers;

namespace Pablo
{
    public class ViewerPane : Scrollable, IViewer
    {
        const float MinZoom = 0.125f;
        static readonly float[] ZOOM_LEVELS = { 7, 6.5F, 6, 5.5F, 5, 4.5F, 4, 3.5F, 3, 2, 1.5F, 1, .75F, .50F, .25F, .125F };
        static readonly float[] PREVIEW_ZOOM_LEVELS = { 1, .75F, .50F, .25F, .125F };

        #region Members

        readonly ImageViewer viewer;
        CheckCommand actionZoomFitWidth;
        CheckCommand actionZoomFitHeight;
        CheckCommand actionAllowGrow;
        CheckCommand actionPreviewZoomFitWidth;
        CheckCommand actionPreviewZoomFitHeight;
        CheckCommand actionPreviewAllowGrow;
        readonly Dictionary<float, RadioCommand> zoomLevels = new Dictionary<float, RadioCommand>();
        readonly Dictionary<float, RadioCommand> previewZoomLevels = new Dictionary<float, RadioCommand>();
        readonly PixelLayout layout;
        UITimer scrollTimer;
        Point oldScrollPosition;
        double scrollIncrement;
        double scrollAmount;
        double scrollPosition;
        ZoomInfo zoomInfo;
        private ZoomInfo previewZoomInfo;
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

        protected override void OnSizeChanged(EventArgs e)
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

        public float PreviewZoom { get { return PreviewZoomInfo.Zoom; } }

        public ZoomInfo PreviewZoomInfo
        {
            get { return previewZoomInfo ?? ViewHandler.Document.Info.PreviewZoomInfo; }
            set { previewZoomInfo = value; }
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
            float previewZoom = PreviewZoomInfo.Zoom;

            if (ViewHandler != null)
            {
                if (!ViewHandler.EnableZoom)
                    return 1.0f;

                Size clientSize = ClientSize;
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
                RadioCommand raction;
                if (zoomLevels.TryGetValue(ZoomInfo.Zoom, out raction))
                    raction.Checked = true;
            }

            if (actionPreviewAllowGrow != null)
                actionPreviewAllowGrow.Checked = PreviewZoomInfo.AllowGrow;
            if (actionPreviewZoomFitWidth != null)
                actionPreviewZoomFitWidth.Checked = PreviewZoomInfo.FitWidth;
            if (actionPreviewZoomFitHeight != null)
                actionPreviewZoomFitHeight.Checked = PreviewZoomInfo.FitHeight;
            if (previewZoomLevels != null)
            {
                RadioCommand raction;
                if (previewZoomLevels.TryGetValue(PreviewZoomInfo.Zoom, out raction))
                    raction.Checked = true;
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

            smZoom.Items.Add(actionZoomFitWidth = new Actions.FitWidth(this), 500);
            smZoom.Items.Add(actionZoomFitHeight = new Actions.FitHeight(this), 500);
            smZoom.Items.Add(actionAllowGrow = new Actions.AllowGrow(this), 500);

            smZoom.Items.AddSeparator(500);


            RadioCommand controller = null;
            zoomLevels.Clear();
            foreach (float zoomLevel in ZOOM_LEVELS)
            {
                var raction = new RadioCommand { Controller = controller, ID = "zoom" + zoomLevel, MenuText = string.Format("{0}%", zoomLevel * 100) };
                raction.Executed += (sender, e) =>
                {
                    var action = sender as RadioCommand;
                    ZoomInfo.Zoom = (float)action.Tag;
                    UpdateSizes();
                };
                if (controller == null)
                    controller = raction;
                raction.Tag = zoomLevel;
                smZoom.Items.Add(raction);
                zoomLevels.Add(zoomLevel, raction);
            }

            //--------------------------------------
            var smPreviewZoom = smView.Items.GetSubmenu("&Preview Zoom", 500);

            smPreviewZoom.Items.Add(actionPreviewZoomFitWidth = new Actions.FitWidth(this), 500);
            smPreviewZoom.Items.Add(actionPreviewZoomFitHeight = new Actions.FitHeight(this), 500);
            smPreviewZoom.Items.Add(actionPreviewAllowGrow = new Actions.AllowGrow(this), 500);

            smPreviewZoom.Items.AddSeparator(500);


            RadioCommand previewController = null;
            previewZoomLevels.Clear();
            foreach (float previewZoomLevel in PREVIEW_ZOOM_LEVELS)
            {
                var raction = new RadioCommand { Controller = controller, ID = "zoom" + previewZoomLevel, MenuText = string.Format("{0}%", previewZoomLevel * 100) };
                raction.Executed += (sender, e) =>
                {
                    var action = sender as RadioCommand;
                    PreviewZoomInfo.Zoom = (float)action.Tag;
                    UpdateSizes();
                };
                if (previewController == null)
                    previewController = raction;
                raction.Tag = previewZoomLevel;
                smPreviewZoom.Items.Add(raction);
                previewZoomLevels.Add(previewZoomLevel, raction);
            }
            //--------------------------------------
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
