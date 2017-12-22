using System;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using System.Diagnostics;

namespace Pablo
{
	public class ImageViewer : Drawable
	{
		ViewerPane viewerPane;
		GenerateActionArgs viewerArgs;
		
		Handler ViewHandler {
			get { return viewerPane.ViewHandler; }
		}
		
		public ImageViewer(ViewerPane viewerPane, bool largeCanvas)
#if MOBILE
			: base(largeCanvas)
#endif
		{
			this.Style = "imageViewer";
			CanFocus = true;
			this.viewerPane = viewerPane;
			viewerArgs = new GenerateActionArgs(this);
		}
		
		public override void OnMouseDown(MouseEventArgs e)
		{
			ViewHandler.OnMouseDown(e);
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			ViewHandler.OnMouseUp(e);
		}

		public override void OnMouseMove(MouseEventArgs e)
		{
			ViewHandler.OnMouseMove(e);
		}
		
		public void HookupEvents()
		{
			var handler = ViewHandler;
			
			Document document = handler.Document;
			// hook up events we want to handle
			handler.RegionUpdated += handler_UpdateRegion;
			handler.InvalidateRegion += handler_InvalidateRegion;
			handler.InvalidateVisible += delegate {
				this.Invalidate ();
			};
			handler.SizeChanged += handler_SizeChanged;
			handler.ZoomChanged += handler_ZoomChanged;

			document.Initialized += delegate {
				ResetSize ();
			};
		}
		
		
		void handler_InvalidateRegion(Object sender, Rectangle rect)
		{
#if DEBUG
			//Console.WriteLine ("handler_InvalidateRegion {0}, visible: {1}", rect, viewerPane.VisibleRect);
#endif
			/**
			UpdateRegion(rect);
			/**/
#if DESKTOP
			rect.Restrict (viewerPane.VisibleRect);
#endif
			if (rect.Width != 0 && rect.Height != 0) {
				Application.Instance.AsyncInvoke (delegate {
					this.Invalidate(rect);
				});
			}
			/**/
		}

		public Document Document
		{
			get { return ViewHandler.Document; }
		}

		void ResetSize()
		{
			this.Size = DrawSize;
			this.Visible = (this.Size.Width > 0 || this.Size.Height > 0);
			Invalidate ();
		}

		public Size DrawSize
		{
			get { return ViewHandler.DrawSize; }
		}


		public override void OnPaint(PaintEventArgs pe)
		{
			try
				//lock (this)
			{
				Rectangle rectScreen;
				Rectangle rectGenerate;

				ViewHandler.CalculateRect (pe.ClipRectangle, out rectScreen, out rectGenerate);

				if (rectGenerate.Width > 0 && rectGenerate.Height > 0) {
					/*
					 *
					Region r = pe.Graphics.ClipRegion;
					r.Reset();
					pe.Graphics.ClipRegion = r;
					/*
						 
					Graphics g = this.CreateGraphics();
					Rectangle rect = rectScreen;
					rect.Inflate(1,1);
					g.DrawRectangle(Color.FromArgb(0xff, 0, 0), rect);
					g.Dispose();
					/*
					 */
					ViewHandler.GenerateRegion (pe.Graphics, rectGenerate, rectScreen);
					/*
					 */
				}
			} catch (Exception ex) {
				Debug.WriteLine ("Error generating region: {0}", ex);
			}
				
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown (e);
			if (!e.Handled)
				ViewHandler.OnKeyDown(e);
		}

		void handler_SizeChanged(object sender, EventArgs e)
		{
			Application.Instance.Invoke (delegate {
				ResetSize ();
				base.OnSizeChanged (EventArgs.Empty);
			});
		}

		void handler_ZoomChanged(object sender, EventArgs e)
		{
			ResetSize();
			base.OnSizeChanged(EventArgs.Empty);
		}
	
		//delegate void UpdateRegionHandler(Rectangle rect);

		void UpdateRegion(Rectangle rect)
		{
			//Console.WriteLine("UpdateRegion {0}", rect);

			rect = Rectangle.Ceiling((RectangleF)rect * ViewHandler.ZoomRatio);
#if DESKTOP
			rect.Restrict (viewerPane.VisibleRect);
#endif
			/*
			 *
			if (Current.IsWindows) {
				//Eto.Platform.Windows.Generator.GeneratorID) {
				// faster to do drawing in thread with swf!
				Rectangle rectScreen;
				Rectangle rectGenerate;
				ViewHandler.CalculateRect(rect, out rectScreen, out rectGenerate);

				if (rectGenerate.Width > 0 && rectGenerate.Height > 0)
				{
					//Generator.ExecuteOnMainThread(delegate {
						using (var graphics = this.CreateGraphics())
						{
							ViewHandler.GenerateRegion(graphics, rectGenerate, rectScreen);
						}
					//});
				}
			}
			else
			/*
			 */
			{
				// faster to invalidate and let main thread do drawing in gtk! (and works well in osx too)
				Application.Instance.AsyncInvoke (delegate {
					this.Invalidate(rect);
				});
			}
			/*
			 */
		}

		void handler_UpdateRegion(object sender, Rectangle rect)
		{
			// this is called from another thread!  Invoke method that actually does work
			/*
			 *
			if (Generator is Eto.Platform.Windows.Generator) UpdateRegion(rect);
			else this.Invoke(new UpdateRegionHandler(UpdateRegion), rect);
			/*
			 */
			UpdateRegion(rect);
			/*
			 */
			
		}


		public void RunIteration()
		{
#if !MOBILE
			Application.Instance.RunIteration();
#endif
		}
		
		public void GenerateActions(GenerateActionArgs args)
		{
			viewerArgs.Clear();
			viewerArgs.CopyArguments(args);
			viewerArgs.Arguments["area"] = "viewer";
			viewerArgs.Arguments["control"] = this;
			ViewHandler.GenerateActions(viewerArgs);
			args.Merge(viewerArgs);
		}
	}
}
