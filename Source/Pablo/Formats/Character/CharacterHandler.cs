using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Actions;
using System.Collections.Generic;
using Pablo.Formats.Character.Undo;
using System.Linq;
using System.Diagnostics;

namespace Pablo.Formats.Character
{
	public enum PasteMode
	{
		Normal,
		Under,
		Transparent
	}

	public class CharacterHandler : Handler
	{

		#region Members

		const int currentPage = 0;
		int characterSet = 5;
		UITimer blinkTimer;
		bool insertMode;
		bool blinkOn;
		bool allowBlink = true;
		Point cursorPosition;
		bool enableUpdate = true;
		bool wrapMode = true;
		List<CharacterTool> tools;
		readonly Queue<Rectangle> queuedUpdates = new Queue<Rectangle>();
		readonly Queue<Rectangle> queuedInvalidates = new Queue<Rectangle>();
		CharacterHandler preview;
		CharacterTool selectedTool;
		CanvasElement drawElement = CanvasElement.Default;
		//Hashtable characterCache = new Hashtable();

		#endregion

		#region Events

		public event EventHandler<EventArgs> CharacterSetChanged;

		protected virtual void OnCharacterSetChanged(EventArgs e)
		{
			if (CharacterSetChanged != null)
				CharacterSetChanged(this, e);
		}

		public event EventHandler<EventArgs> CursorPositionChanged;

		protected virtual void OnCursorPositionChanged(EventArgs e)
		{
			if (CursorPositionChanged != null)
				CursorPositionChanged(this, e);
		}

		public event EventHandler<EventArgs> DrawAttributeChanged;

		protected virtual void OnDrawAttributeChanged(EventArgs e)
		{
			if (DrawAttributeChanged != null)
				DrawAttributeChanged(this, e);
		}

		#endregion

		#region Properties

		public List<CharacterTool> Tools
		{
			get
			{
				if (tools == null)
				{
					tools = new List<CharacterTool>();
					tools.Add(new Tools.Selection { Handler = this });
					tools.Add(new Tools.Brush { Handler = this });
					tools.Add(new Tools.InkDropper { Handler = this });
					tools.Add(new Tools.ColourBrush { Handler = this });
					tools.Add(new Tools.Pencil { Handler = this });
					tools.Add(new Tools.LineTool { Handler = this });
					tools.Add(new Tools.RectangleTool { Handler = this });
					tools.Add(new Tools.EllipseTool { Handler = this });
				}
				return tools;
			}
		}

		public void SelectTool<T>()
			where T : CharacterTool
		{
			SelectedTool = Tools.OfType<T>().FirstOrDefault();
		}

		public event EventHandler<EventArgs> ToolChanged;

		protected virtual void OnToolChanged(EventArgs e)
		{
			if (ToolChanged != null)
				ToolChanged(this, e);

			TriggerActionsChanged();
		}

		public CharacterTool SelectedTool
		{
			get { return selectedTool; }
			set
			{
				if (selectedTool != null)
				{
					selectedTool.Unselected();
				}
				if (selectedTool != value)
				{
					if (value != null)
					{
						value.Handler = this;
						value.Selecting();
					}
					selectedTool = value;
					OnToolChanged(EventArgs.Empty);
				}
			}
		}

		public CanvasElement DrawElement
		{
			get { return drawElement; }
		}

		public Attribute DrawAttribute
		{
			get { return drawElement.Attribute; }
			set
			{
				drawElement.Attribute = value;
				OnDrawAttributeChanged(EventArgs.Empty);
			}
		}

		public UndoManager Undo { get; private set; }

		public bool AllowEditing { get; set; }

		public bool AllowToolSelection { get; set; }

		public CharacterDocument CharacterDocument
		{
			get { return (CharacterDocument)Document; }
		}

		public CharacterDocumentInfo Info
		{
			get { return CharacterDocument.Info; }
		}

		public bool AllowKeyboardEditing
		{
			get { return SelectedTool == null || SelectedTool.AllowKeyboard; }
		}

		public override bool CanEdit
		{
			get { return true; }
		}

		public int ColourStart
		{
			get;
			set;
		}

		public int CharacterSet
		{
			get { return characterSet; }
			set
			{
				if (characterSet != value)
				{
					characterSet = value;
					OnCharacterSetChanged(EventArgs.Empty);
				}
			}
		}

		public bool EnableUpdate
		{
			get { return enableUpdate; }
			set
			{
				enableUpdate = value;
				if (enableUpdate)
				{
					while (queuedUpdates.Count > 0)
						base.OnUpdateRegion(queuedUpdates.Dequeue());
					while (queuedInvalidates.Count > 0)
						base.OnInvalidateRegion(queuedInvalidates.Dequeue());
				}
			}
		}

		public Point CursorPosition
		{
			get { return cursorPosition; }
			set
			{
				SetCursorPosition(value, true);
			}
		}

		public override SizeF Ratio
		{
			get
			{
				if (CharacterDocument.DosAspect)
				{
					return new SizeF(1f, CharacterDocument.Pages[0].Font.LegacyRatio);
				}
				return base.Ratio;
			}
		}

		public float Aspect
		{
			get
			{
				var ratio = Ratio;
				return ratio.Width * ratio.Height;
			}
		}

		public override Size Size
		{
			get
			{
				return Document.Size * CurrentPage.Font.Size;
			}
		}

		public Page CurrentPage
		{
			get { return CharacterDocument.Pages[currentPage]; }
		}

		public event EventHandler<EventArgs> InsertModeChanged;

		protected virtual void OnInsertModeChanged(EventArgs e)
		{
			if (InsertModeChanged != null)
				InsertModeChanged(this, e);
		}

		public bool InsertMode
		{
			get { return insertMode; }
			set
			{
				if (value != insertMode)
				{
					insertMode = value;
					OnInsertModeChanged(EventArgs.Empty);
				}
			}
		}

		public bool WrapMode
		{
			get { return wrapMode; }
			set { wrapMode = value; }
		}

		public override IEnumerable<Pablo.Network.ICommand> Commands
		{
			get
			{
				foreach (var c in base.Commands)
					yield return c;
				foreach (var c in Tools.SelectMany(r => r.Commands))
					yield return c;
				yield return new DrawCharacter(this);
				yield return new InsertLine(this);
				yield return new DeleteLine(this);
				yield return new InsertColumn(this);
				yield return new DeleteColumn(this);
				yield return new SetWidth(this);
				yield return new Backspace(this);
				yield return new NewLine(this);
				yield return new Delete(this);
				yield return new Actions.Undo(this);
				yield return new Redo(this);
				yield return new SetPalette(this);
				yield return new SetColour(this);
				yield return new Actions.Drawing.Brush(this);
				yield return new Actions.Drawing.DrawLine(this);
				yield return new Actions.Drawing.DrawRect(this);
				yield return new Actions.Drawing.DrawEllipse(this);
				yield return new Actions.Drawing.HalfFill(this);
				yield return new ChangeFont(this);
				yield return new ToggleUse9x(this);
				yield return new ToggleIceMode(this);
				yield return new ToggleDosAspect(this);
			}
		}

		public override IEnumerable<Pablo.Network.ICommand> ServerCommands
		{
			get
			{
				foreach (var c in base.Commands)
					yield return c;
				yield return new Actions.Undo(this);
				yield return new Redo(this);
			}
		}

		#endregion

		#region Event Handlers

		void Info_DosAspectChanged(object sender, EventArgs e)
		{
			OnSizeChanged(EventArgs.Empty);
		}

		void Info_iCEColoursChanged(object sender, EventArgs e)
		{
			SetupIceColours();
			OnDrawAttributeChanged(EventArgs.Empty);
			OnSizeChanged(e);
		}

		void document_SizeChanged(Object sender, EventArgs e)
		{
			OnSizeChanged(e);
		}

		void Canvas_Update(object sender, Rectangle rect)
		{
			UpdateRegion(rect);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (blinkTimer != null)
				{
					blinkTimer.Stop();
					blinkTimer.Dispose();
					blinkTimer = null;
				}
			}

			var doc = CharacterDocument;

			doc.Info.DosAspectChanged -= Info_DosAspectChanged;
			doc.Info.iCEColoursChanged -= Info_iCEColoursChanged;

			doc.ICEColoursChanged -= Info_iCEColoursChanged;
			doc.SizeChanged -= document_SizeChanged;

			base.Dispose(disposing);
		}

		public override void PreLoad(System.IO.Stream stream, Format format)
		{
			base.PreLoad(stream, format);

			if (CharacterDocument.AnimateView)
				CurrentPage.Canvas.Update += Canvas_Update;
		}

		public override void Loaded()
		{
			base.Loaded();

			if (Document.EditMode)
			{
				CurrentPage.PaletteChanged += delegate
				{
					var attr = DrawAttribute;
					attr.Foreground = Math.Min(attr.Foreground, CurrentPage.Palette.Count);
					attr.Background = Math.Min(attr.Background, CurrentPage.Palette.Count);
					OnInvalidateVisible(EventArgs.Empty);
				};
			}
		}

		protected override void OnInvalidateRegion(Rectangle rect)
		{
			if (enableUpdate)
			{
				base.OnInvalidateRegion(rect);
			}
			else
			{
				queuedInvalidates.Enqueue(rect);
			}
		}

		protected override void OnUpdateRegion(Rectangle rect)
		{
			if (enableUpdate)
			{
				base.OnUpdateRegion(rect);
			}
			else
				queuedUpdates.Enqueue(rect);
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!AllowEditing)
				return;

			if (SelectedTool != null)
				SelectedTool.OnMouseDown(e);
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (!AllowEditing)
				return;

			if (SelectedTool != null)
				SelectedTool.OnMouseUp(e);
		}

		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!AllowEditing)
				return;

			if (SelectedTool != null)
				SelectedTool.OnMouseMove(e);
		}

		public Point ScreenToCharacter(Point point, bool half = false)
		{
			BitFont font = CurrentPage.Font;
			PointF ptf = point;
			if (half)
				ptf.Y *= 2;
			ptf /= font.Size;
			ptf /= ZoomRatio;
			

			var p = new Point(ptf);
			return p;
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
				return;
			//Console.WriteLine("Pressed {0}", e.KeyData.ToString());
			if (AllowEditing)
			{
				if (!e.Handled && AllowToolSelection)
				{
					foreach (var tool in Tools)
					{
						if (e.KeyData == tool.Accelerator && tool != SelectedTool)
						{
							SelectedTool = tool;
							e.Handled = true;
							break;
						}
					}
				}

				if (!e.Handled && SelectedTool != null)
				{
					SelectedTool.OnKeyDown(e);
				}

				if (!e.Handled && AllowKeyboardEditing && e.IsChar)
				{
					byte[] bytes = CurrentPage.Font.Encoding.GetBytes(new char[] { e.KeyChar });

					byte b = bytes[0];
					if (b < CurrentPage.Font.NumChars)
					{
						InsertCharacter(b);
						e.Handled = true;
					}
				}
			}
		}

		static bool UseInterpolation(float zoom)
		{
			if (Math.Abs(zoom % 2f) < 0.01f || Math.Abs(zoom - 1) < 0.01f)
				return false;
			return true;
		}

		public override void GenerateRegion(Graphics graphics, Rectangle rectSource, Rectangle rectDest)
		{
			Page page = CurrentPage;
			//Debug.WriteLine("Generating {0}, Stack: {1}", rectSource, new System.Diagnostics.StackTrace());
			//Debug.WriteLine("Generating {0}", rectSource);
			Rectangle sourceChunk = rectSource;
			Rectangle destChunk = rectDest;
			int chunkSize = 1024;
			sourceChunk.Height = Math.Min(chunkSize, rectSource.Height);
			destChunk.Height = rectDest.Height * chunkSize / rectSource.Height;
			int destChunkSize = (rectDest.Height * chunkSize / rectSource.Height) - 4;
			chunkSize = rectSource.Height * destChunkSize / rectDest.Height;
			int chunkPos = 0;
			int destChunkPos = 0;
			if (UseInterpolation(ZoomRatio.Width) || UseInterpolation(ZoomRatio.Height))
				graphics.ImageInterpolation = ImageInterpolation.High;
			else
				graphics.ImageInterpolation = ImageInterpolation.None;

			/**/
			using (var bitmap = new Bitmap(sourceChunk.Width, sourceChunk.Height, PixelFormat.Format32bppRgb))
			{
				while (chunkPos < rectSource.Height)
				{
					sourceChunk.Height = Math.Min(sourceChunk.Height, rectSource.Height - chunkPos);
					destChunk.Height = Math.Min(destChunk.Height, rectDest.Height - destChunkPos);
					//Debug.Print("Source: {0} Dest:{1}, Chunk:{2}, Size:{3}", sourceChunk, destChunk, chunkPos, rectSource.Height);

					page.GenerateRegion(bitmap, sourceChunk, page.Font.Size, null, null, CharacterDocument.ICEColours, blinkOn, (AllowEditing) ? (Point?)CursorPosition : null, SelectedTool != null ? SelectedTool.GetGenerator() : null);
					graphics.DrawImage(bitmap, new Rectangle(0, 0, sourceChunk.Width, sourceChunk.Height), destChunk);

					sourceChunk.Y += chunkSize;
					chunkPos += chunkSize;
					destChunk.Y += destChunkSize;
					destChunkPos += destChunkSize;
				}

			}
			/**
			using (var bitmap = new Bitmap (Document.Generator, rectSource.Width, rectSource.Height, PixelFormat.Format32bppRgb))
			{
				page.GenerateRegion(bitmap, rectSource, page.Font.Size, null, null, CharacterDocument.iCEColours, blinkOn, (AllowEditing) ? (Point?)CursorPosition : null, SelectedTool != null ? SelectedTool.GetGenerator() : null);
				graphics.DrawImage(bitmap, new Rectangle(rectSource.Size), rectDest);
			}
			/**/
		}

		protected override void GenerateRegion(Bitmap bitmap, Rectangle rectGenerate, Rectangle rectScreen)
		{
			if (rectGenerate == rectScreen)
			{
				Page page = CurrentPage;

				page.GenerateRegion(bitmap, rectGenerate, page.Font.Size, null, null, CharacterDocument.ICEColours, blinkOn, (AllowEditing) ? (Point?)CursorPosition : null, SelectedTool != null ? SelectedTool.GetGenerator() : null);
			}
			else
				base.GenerateRegion(bitmap, rectGenerate, rectScreen);
		}

		void AddMoveAction(GenerateCommandArgs args, string id, string name, string toolTip, MoveDirection direction, Keys accelerator)
		{
			args.KeyboardCommands.Add(new Move(this, id, direction, accelerator) { Name = name, MenuText = name, ToolTip = toolTip });
			args.KeyboardCommands.Add(new MoveSelect(Tools.OfType<Tools.Selection>().First(), id, direction, Keys.Shift, accelerator) { Name = name, MenuText = name, ToolTip = toolTip });
		}

		GenerateCommandArgs args;

		// bug in Eto:
		static Command s_DisabledCommand = new Command { Enabled = false };

		public override void GenerateCommands(GenerateCommandArgs args)
		{
			base.GenerateCommands(args);

			if (args.Area == "viewer")
			{
				this.args = args;
				var control = args.Control;

				if (Generator.IsMac && control != null)
				{
					control.MapPlatformCommand("cut", s_DisabledCommand);
					control.MapPlatformCommand("copy", s_DisabledCommand);
					control.MapPlatformCommand("paste", s_DisabledCommand);
					control.MapPlatformCommand("selectAll", s_DisabledCommand);
					control.MapPlatformCommand("delete", s_DisabledCommand);
					control.MapPlatformCommand("undo", s_DisabledCommand);
					control.MapPlatformCommand("redo", s_DisabledCommand);
				}

				var file = args.Menu.Items.GetSubmenu("&File");
				file.Items.Add(new ExportAsIcon(this), 500);


				var edit = args.Menu.Items.GetSubmenu("&Edit", 200);
				var view = args.Menu.Items.GetSubmenu("&View", 500);
				view.Items.AddSeparator(600);
				view.Items.Add(Info.GetFontMenu(this, 600));


				view.Items.Add(new ToggleDosAspect(this), 500);
				view.Items.Add(new ToggleUse9x(this), 500);
				view.Items.Add(new ToggleIceMode(this), 500);

				if (AllowEditing)
				{
#if DEBUG
					edit.Items.Add(new StartDrawing(this), 700);
#endif

					edit.Items.AddSeparator(700);
					edit.Items.Add(new SetWidth(this), 700);
					edit.Items.Add(new CharacterSetEditor(this), 700);

					edit.Items.AddSeparator(800);
					edit.Items.Add(new DefaultColour(this), 800);
					edit.Items.Add(new SwitchForegroundBackground(this), 800);


					args.ToolBar.Items.Add(new SetWidth(this), 500);

					if (SelectedTool != null)
						SelectedTool.GenerateCommands(args);

					// block commands

					if (Generator.IsMac && control != null)
					{
						control.MapPlatformCommand("undo", new Actions.Undo(this));
						control.MapPlatformCommand("redo", new Redo(this));
					}
					else
					{
						edit.Items.Add(new Actions.Undo(this), 100);
						edit.Items.Add(new Redo(this), 100);
					}

					edit.Items.Add(new ToggleInsertMode(this), 500);

					args.KeyboardCommands.Add(new SwitchColour(this, -1, 0, Keys.Control | Keys.Up));
					args.KeyboardCommands.Add(new SwitchColour(this, 1, 0, Keys.Control | Keys.Down));
					args.KeyboardCommands.Add(new SwitchColour(this, 0, -1, Keys.Control | Keys.Left));
					args.KeyboardCommands.Add(new SwitchColour(this, 0, 1, Keys.Control | Keys.Right));

					for (int i=0; i<Math.Min (10, CurrentPage.Palette.Count / 2); i++)
					{
						args.KeyboardCommands.Add(new ChangeColour(this, i, null, Keys.Control | (Keys.D0 + i)));
						args.KeyboardCommands.Add(new ChangeColour(this, null, i, Keys.Alt | (Keys.D0 + i)));
					}
					AddMoveAction(args, "up", "Move Up", "Moves the cursor up one row", MoveDirection.Up, Keys.Up);
					AddMoveAction(args, "down", "Move Down", "Moves the cursor down one row", MoveDirection.Down, Keys.Down);
					AddMoveAction(args, "left", "Move Left", "Moves the cursor left one column", MoveDirection.Left, Keys.Left);
					AddMoveAction(args, "right", "Move Right", "Moves the cursor right one column", MoveDirection.Right, Keys.Right);
					AddMoveAction(args, "home", "Home", "Moves the cursor to the beginning of the row", MoveDirection.First, (Generator.IsMac) ? Keys.Application | Keys.Left : Keys.Home);
					AddMoveAction(args, "end", "End", "Moves the cursor to the end of the row", MoveDirection.Last, (Generator.IsMac) ? Keys.Application | Keys.Right : Keys.End);
					AddMoveAction(args, "pageUp", "Page Up", "Moves the cursor up one page", MoveDirection.PageUp, Keys.PageUp);
					AddMoveAction(args, "pageDown", "Page Down", "Moves the cursor down one page", MoveDirection.PageDown, Keys.PageDown);
					AddMoveAction(args, "top", "Move Top", "Moves the cursor to the top of the document", MoveDirection.Top, (Generator.IsMac) ? Keys.Application | Keys.Home : Keys.Control | Keys.PageUp);
					AddMoveAction(args, "bottom", "Move Bottom", "Moves the cursor to the bottom of the document", MoveDirection.Bottom, (Generator.IsMac) ? Keys.Application | Keys.End : Keys.Control | Keys.PageDown);
					args.KeyboardCommands.Add(new SelectAttribute(this));
					args.KeyboardCommands.Add(new SelectAttribute(this) { Shortcut = Keys.Escape });
					args.KeyboardCommands.Add(new InsertColumn(this));
					args.KeyboardCommands.Add(new DeleteColumn(this));
					args.KeyboardCommands.Add(new InsertLine(this));
					args.KeyboardCommands.Add(new DeleteLine(this));
					args.KeyboardCommands.Add(new NewLine(this));
					args.KeyboardCommands.Add(new Delete(this));
					args.KeyboardCommands.Add(new Backspace(this));
					
					args.KeyboardCommands.Add(new UseColour(this));

					for (int i = 0; i < 12; i++)
					{
						args.KeyboardCommands.Add(new DrawCharacterSetChar(this, i, (Keys.F1 + i)));
					}

					for (int i = 0; i < 10; i++)
					{
						args.KeyboardCommands.Add(new SwitchCharacterSet(this, i, Keys.Alt | (Keys.F1 + i)));

						args.KeyboardCommands.Add(new SwitchCharacterSet(this, i + 10, Keys.Control | (Keys.F1 + i)));
					}

				}
			}
		}

		public CharacterHandler(CharacterDocument doc, bool allowBlink = true)
			: base(doc)
		{
			this.Undo = new UndoManager(this);
			this.allowBlink = allowBlink;
			this.AllowEditing = doc.EditMode;
			this.AllowToolSelection = true;

			doc.Info.DosAspectChanged += Info_DosAspectChanged;
			doc.Info.iCEColoursChanged += Info_iCEColoursChanged;

			doc.ICEColoursChanged += Info_iCEColoursChanged;
			doc.SizeChanged += document_SizeChanged;

			SetupIceColours();
		}

		void SetupIceColours()
		{
			if (!allowBlink)
			{
				blinkOn = true;
				ViewerControl.Invalidate();
				return;
			}
			if (CharacterDocument.ICEColours)
			{
				if (blinkTimer != null)
				{
					blinkTimer.Stop();
					blinkTimer.Dispose();
					blinkTimer = null;
					InvalidateBlinkingCharacters();
				}
			}
			else if (blinkTimer == null && HasViewer)
			{
				blinkTimer = new UITimer();
				blinkTimer.Interval = 0.25;
				blinkTimer.Elapsed += delegate
				{
					blinkOn = !blinkOn;
					InvalidateBlinkingCharacters();
				};
				blinkTimer.Start();
			}
		}

		public void InvalidateBlinkingCharacters()
		{
			if (!HasViewer)
				return;
			var rectf = new RectangleF(Viewer.ScrollPosition, Viewer.ViewSize);
			rectf.Inflate(CurrentPage.Font.Size);
			rectf /= ZoomRatio;
			rectf /= CurrentPage.Font.Size;
			var canvas = CurrentPage.Canvas;
			var rect = Rectangle.Round(rectf);
			rect.Restrict(new Rectangle(canvas.Size));
			if (rect.Width > 0 && rect.Height > 0)
			{
				for (int y = rect.Top; y < rect.Bottom; y++)
				{
					//var line = canvas.GetLine (y);
					int? startx = null;
					for (int x = rect.Left; x < rect.Right; x++)
					{
						var ce = canvas[x, y];
						if (ce.Attribute.Blink)
						{
							if (startx == null)
							{
								startx = x;
							}
						}
						else if (startx != null)
						{
							InvalidateCharacterRegion(new Rectangle(startx.Value, y, x - startx.Value, 1), false);
							startx = null;
						}
					}

					if (startx != null)
					{
						InvalidateCharacterRegion(new Rectangle(startx.Value, y, rect.Right - startx.Value, 1), false);
					}
				}
			}
		}

		public void TriggerCharacterSetChanged()
		{
			OnCharacterSetChanged(EventArgs.Empty);
		}

		public void TriggerSizeChanged()
		{
			OnSizeChanged(EventArgs.Empty);
		}

		public void SetCursorPosition(Point value, bool invalidate)
		{
			if (cursorPosition != value)
			{
				Point pos = cursorPosition;
				cursorPosition = value;
				cursorPosition.Restrict(new Rectangle(CurrentPage.Canvas.Size));
				if (SelectedTool != null)
					SelectedTool.OnSetCursorPosition(pos, cursorPosition, invalidate);
				OnCursorPositionChanged(EventArgs.Empty);
			}
		}

		public void UpdateRegion(Rectangle rect)
		{
			BitFont font = CurrentPage.Font;
			var destrect = (RectangleF)rect;
			destrect *= font.Size;
			var w = (ZoomRatio.Width != 1) ? 1 / ZoomRatio.Width : 0;
			var h = (ZoomRatio.Height != 1) ? 1 / ZoomRatio.Height : 0;
			destrect.Inflate(w, h);
			OnUpdateRegion(Rectangle.Ceiling(destrect));
			if (preview != null)
				preview.UpdateRegion(rect);
		}

		public void InvalidateCharacterRegion(Rectangle rect, bool includePreview, bool previewOnly = false, bool halfMode = false)
		{
			rect.Normalize();
			if (halfMode)
			{
				rect = rect.FromHalfMode();
			}
			if (!previewOnly)
			{ 
				BitFont font = CurrentPage.Font;
				RectangleF rectf = rect;
				rectf *= font.Size;
				rectf *= ZoomRatio;
				int w = (ZoomRatio.Width != 1) ? 1 : 0;
				int h = (ZoomRatio.Height != 1) ? 1 : 0;
				rectf.Inflate(w, h);
				OnInvalidateRegion(Rectangle.Ceiling(rectf));
			}
			if (includePreview && preview != null)
				preview.InvalidateCharacterRegion(rect, false);
		}

		public void EnsureVisible(Point point)
		{
			BitFont font = CurrentPage.Font;
			var rect = new RectangleF(point * font.Size, font.Size);
			rect *= ZoomRatio;

			Point scrollPos = Viewer.ScrollPosition;
			Point oldPos = scrollPos;

			Size viewSize = Viewer.ViewSize;
			if (rect.InnerBottom > scrollPos.Y + viewSize.Height)
				scrollPos.Y = (int)rect.InnerBottom - viewSize.Height;
			if (rect.InnerRight > scrollPos.X + viewSize.Width)
				scrollPos.X = (int)rect.InnerRight - viewSize.Width;
			if (rect.Top < scrollPos.Y)
				scrollPos.Y = (int)rect.Top;
			if (rect.Left < scrollPos.X)
				scrollPos.X = (int)rect.Left;
			if (scrollPos.Y < 0)
				scrollPos.Y = 0;
			if (scrollPos.X < 0)
				scrollPos.X = 0;

			if (scrollPos != oldPos)
				Viewer.ScrollPosition = scrollPos;
		}

		DrawCharacter insertCharacter;

		public void InsertCharacter(int character)
		{
			if (insertCharacter == null)
				insertCharacter = new DrawCharacter(this);
			insertCharacter.Activate(character);
		}

		protected override Control CreateViewerControl()
		{
			return new ViewerPane(this, !CharacterDocument.AnimateView);
		}

		public override void GeneratePads(GeneratePadArgs args)
		{
			base.GeneratePads(args);
			if (Document.EditMode)
			{
				var canEdit = Client == null || Client.CurrentUser.Level >= Pablo.Network.UserLevel.Editor;
				if (canEdit)
				{
					var layout = new TableLayout
					{
						Padding = new Padding(5),
						Rows = {
							new Controls.ColourPad(this),
							new Controls.ToolboxPad(this)
						}
					};
					args.LeftPads.Add(layout);
				}
				
				{
					var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };
					
					layout.BeginHorizontal();
					if (canEdit)
						layout.Add(new Controls.CharacterPad(this));
					layout.Add(new Controls.FlagsPad(this), xscale:true);
					layout.Add(new Controls.PositionPad(this));
					layout.EndHorizontal();
					
					args.BottomPads.Add(layout);
				}
				
				
				/* VGA Preview
				 */
				if (preview == null)
				{
					preview = new CharacterHandler(CharacterDocument, false);
					preview.AllowToolSelection = false;
					var v = (ViewerPane)preview.ViewerControl;
					var zi = new ZoomInfo { Zoom = 0.25F, FitWidth = true };
					v.ZoomInfo = zi;
					preview.ViewerControl.ID = "preview";
					/*CurrentPage.Canvas.Update += delegate(object sender, Rectangle rect) {
						h2.InvalidateCharacterRegion (rect);
					};*/
					CurrentPage.Canvas.SizeChanged += preview.document_SizeChanged;
	
					// must be after creating the viewer
					preview.PreLoad(null, null);
					preview.Loaded();
					preview.PostLoad();
				}
				
				var dl = new Panel { Size = new Size(165, 100), Padding = new Padding(5, 0, 0, 0), Content = preview.ViewerControl };
				args.RightPads.Add(dl);
				/**/
             
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (Document.EditMode && AllowToolSelection)
			{
				SelectTool<Tools.Selection>();
			}
		}
	}
}
