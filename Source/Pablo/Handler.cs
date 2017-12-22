using System;
using System.IO;
using Eto;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Network;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pablo
{
	public abstract class Handler : IDisposable, IClientSource
	{
		IViewer viewer;
		Control viewerControl;
		float zoom = 1;
		bool enableZoom = true;
		readonly Document document;

		public event EventHandler<EventArgs> ActionsChanged;
		public event EventHandler<EventArgs> InvalidateVisible;
		public event UpdateEventHandler InvalidateRegion;
		public event EventHandler<EventArgs> SizeChanged;
		public event EventHandler<EventArgs> ZoomChanged;
		public event UpdateEventHandler RegionUpdated;

		public IClientDelegate ClientDelegate
		{
			get;
			set;
		}

		public Platform Generator
		{
			get { return document.Generator; }
		}

		#if DEBUG
		~Handler ()
		{
			Dispose(false);
		}
		#endif

		protected Handler(Document document)
		{
			this.document = document;
		}

		public Client Client
		{
			get;
			set;
		}

		public virtual IEnumerable<ICommand> Commands
		{
			get
			{
				yield break;
			}
		}

		public virtual IEnumerable<ICommand> ServerCommands
		{
			get
			{
				yield break;
			}
		}

		public Document Document
		{
			get { return document; }
		}

		public IViewer Viewer
		{
			get
			{ 
				if (viewer == null)
				{
					viewer = CreateViewer();
				}
				return viewer;
			}
		}

		public bool HasViewer
		{
			get { return viewer != null; }
		}

		public bool HasViewerControl
		{
			get { return viewerControl != null; }
		}

		public Control ViewerControl
		{
			get
			{
				if (viewerControl == null)
				{
					viewerControl = CreateViewerControl();
					viewer = viewerControl as IViewer;
				}
				return viewerControl;
			}
		}

		public virtual void PostLoad()
		{
			document.PostLoad(this);
			if (viewer != null)
				viewer.PostLoad();
		}

		public virtual void PreLoad(Stream stream, Format format)
		{
			document.PreLoad(stream, format, this);
			if (viewer != null)
				viewer.PreLoad();
		}

		public virtual void Loaded()
		{
			if (viewer != null)
				viewer.DocumentLoaded();
		}
		// todo: move to an AnimatedHandler
		public virtual void BackgroundLoaded()
		{
			if (viewer != null)
				viewer.BackgroundLoaded();
		}

		protected virtual void OnUpdateRegion(Rectangle rect)
		{
			if (RegionUpdated != null)
				RegionUpdated(this, rect);
			else if (InvalidateRegion != null)
				InvalidateRegion(this, rect);
		}

		protected virtual void OnInvalidateRegion(Rectangle rect)
		{
			if (InvalidateRegion != null)
				InvalidateRegion(this, rect);
			else if (RegionUpdated != null)
				RegionUpdated(this, rect);
		}

		protected virtual void OnInvalidateVisible(EventArgs e)
		{
			if (InvalidateVisible != null)
				InvalidateVisible(this, e);
		}

		public void TriggerActionsChanged()
		{
			OnActionsChanged(EventArgs.Empty);
		}

		public void UpdateActionState()
		{
			/* TODO: Fix for menus
			if (actions != null)
			{
				foreach (var action in actions)
				{
					action.Enabled = action.Enabled;
				}
			}*/
		}

		public void TriggerInvalidateVisible()
		{
			OnInvalidateVisible(EventArgs.Empty);
		}

		public abstract Size Size { get; }

		protected virtual void OnSizeChanged(EventArgs e)
		{
			if (SizeChanged != null)
				SizeChanged(this, e);
		}

		protected void OnActionsChanged(EventArgs e)
		{
			if (ActionsChanged != null)
				ActionsChanged(this, e);
		}

		public virtual SizeF Ratio
		{
			get { return new SizeF(1, 1); }
		}

		public virtual float Zoom
		{
			get { return (EnableZoom) ? zoom : 1; }
			set
			{
				zoom = value;
				OnZoomChanged(EventArgs.Empty);
			}
		}

		protected virtual void OnZoomChanged(EventArgs e)
		{
			if (ZoomChanged != null)
				ZoomChanged(this, e);
		}

		public bool EnableZoom
		{
			get	{ return enableZoom; }
			set
			{
				enableZoom = value;
				OnZoomChanged(EventArgs.Empty);
			}
		}

		public Size DrawSize
		{
			get
			{
				SizeF ratio = ZoomRatio;
				Size size = Size;
				return new Size((int)Math.Round((float)size.Width * ratio.Width), (int)Math.Round((float)size.Height * ratio.Height));
			}
		}

		public SizeF DrawSizeF
		{
			get
			{
				SizeF ratio = ZoomRatio;
				SizeF size = Size;
				return new SizeF(size.Width * ratio.Width, size.Height * ratio.Height);
			}
		}

		public SizeF ZoomRatio
		{
			get
			{
				SizeF ratio = Ratio;
				return new SizeF(Zoom * ratio.Width, Zoom * ratio.Height);
			}
		}

		public void Load(Stream stream, Format format)
		{
			Document.Load(stream, format, this);
		}

		public void Save(Stream stream, Format format)
		{
			Document.Save(stream, format, this);
		}

		public void Load(string fileName, Format format = null)
		{
			format = format ?? DocumentInfoCollection.Default.FindFormat(fileName);
			Document.Load(fileName, format, this);
		}

		public void Save(string fileName, Format format = null)
		{
			format = format ?? DocumentInfoCollection.Default.FindFormat(fileName);
			Document.Save(fileName, format, this);
		}

		public void SaveWithBackup(string fileName, Format format)
		{
			if (ClientDelegate != null && ClientDelegate.EnableBackups && File.Exists(fileName))
			{
				var tempFile = Path.GetTempFileName();

				using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
				{
					Save(stream, format);
				}
				var backupFile = GetBackupFile(fileName);
				File.Move(fileName, backupFile);
				File.Move(tempFile, fileName);
			}
			else
			{
				using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
				{
					Save(stream, format);
				}
			}
		}

		static string GetBackupFile(string fileName)
		{
			var ext = Path.GetExtension(fileName);
			var baseFile = Path.GetFileNameWithoutExtension(fileName);
			baseFile = Regex.Replace(baseFile, "(.+?)([.]\\d+)", "$1", RegexOptions.Singleline);
			baseFile = Path.Combine(Path.GetDirectoryName(fileName), baseFile);
			var count = 1;
			string currentFile;
			do
			{
				currentFile = string.Format("{0}.{1:000}{2}", baseFile, count, ext);
				count++;
			} while (File.Exists(currentFile));
			return currentFile;
		}

		public virtual void OnKeyDown(KeyEventArgs e)
		{
			// no default implementation
			if (args != null)
			{
				var cmd = args.KeyboardCommands.FirstOrDefault(r => r.Shortcut == e.KeyData && r.Enabled);
				if (cmd != null)
				{
					cmd.Execute();
					e.Handled = true;
				}
			}
		}

		public virtual void OnMouseDown(MouseEventArgs e)
		{
			// no default implementation
		}

		public virtual void OnMouseMove(MouseEventArgs e)
		{
			// no default implementation
		}

		public virtual void OnMouseUp(MouseEventArgs e)
		{
			// no default implementation
		}

		public void CalculateRect(Rectangle rectDraw, out Rectangle rectScreen, out Rectangle rectGenerate)
		{
			RectangleF rect = rectDraw;
			SizeF ratio = ZoomRatio;
			
			//rect = rectScreen;
			rect /= ratio;
			rect.Align(1, 1);
			rect.Inflate(1, 1);
			Size size = Size;
			while (rect.Top > 0 && ((rect.Top * ratio.Height) % 1.0F) > 0.2F)
				rect.Top--;
			while (rect.Left > 0 && ((rect.Left * ratio.Width) % 1.0F) > 0.2F)
				rect.Left--;
			 
			while (rect.InnerBottom < size.Height && ((rect.InnerBottom * ratio.Height) % 1.0F) > 0.2F)
				rect.InnerBottom++;
			while (rect.InnerRight < size.Width && ((rect.InnerRight * ratio.Width) % 1.0F) > 0.2F)
				rect.InnerRight++;
			rect.Restrict(PointF.Empty, Size);
			rectGenerate = Rectangle.Round(rect);

			rect = rectGenerate;
			rect *= ratio;
			rectScreen = Rectangle.Round(rect);
			
			/*
			 */
			//Console.WriteLine("{0} - {1}", rectScreen, rectGenerate);
		}

		public Bitmap Generate(Rectangle? area = null)
		{
			
			Rectangle rectScreen;
			Rectangle rectGenerate;
			RectangleF rectDraw;
			rectDraw = area != null ? area.Value : new RectangleF(0, 0, Size.Width, Size.Height);
			rectDraw *= ZoomRatio;
			CalculateRect(Rectangle.Round(rectDraw), out rectScreen, out rectGenerate);
							  
			//Console.WriteLine("{0}, {1}, {2}", Size, rectScreen, rectGenerate);
			var bmp = new Bitmap(rectScreen.Width, rectScreen.Height, PixelFormat.Format32bppRgb);
			GenerateRegion(bmp, rectGenerate, rectScreen);
			return bmp;
		}

		protected virtual void GenerateRegion(Bitmap bitmap, Rectangle rectGenerate, Rectangle rectScreen)
		{
			using (var graphics = new Graphics(bitmap))
			{
				GenerateRegion(graphics, rectGenerate, rectScreen);
				graphics.Flush();
			}
		}

		protected virtual Control CreateViewerControl()
		{
			return new ViewerPane(this, true);
		}

		protected virtual IViewer CreateViewer()
		{
			return ViewerControl as IViewer;
		}

		public abstract bool CanEdit { get; }

		public abstract void GenerateRegion(Graphics graphics, Rectangle rectSource, Rectangle rectDest);

		GenerateCommandArgs args;
		public virtual void GenerateCommands(GenerateCommandArgs args)
		{
			Document.GenerateActions(args);
			if (args.Area == "viewer")
			{
				this.args = args;
			}
			//actions = args.Actions;

			string area = args.Area;
			if (area == "main")
			{
				var aiFile = args.Menu.Items.GetSubmenu("&File");
				
				if (Document.EditMode)
					aiFile.Items.Add(new Actions.SaveFile(this), 500);
				aiFile.Items.Add(new Actions.SaveAs(this), 500);
				
				if (Document.EditMode)
					args.ToolBar.Items.Add(new Actions.SaveFile(this), 500);
				
				Viewer.GenerateCommands(args);
			}
			
		}

		public virtual void GeneratePads(GeneratePadArgs args)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Debug.Print(string.Format("{0}: {1}", disposing ? "Dispose" : "GC", GetType().Name));
		}
	}
}
