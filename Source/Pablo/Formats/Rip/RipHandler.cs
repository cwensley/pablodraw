using Eto.Drawing;
using Eto.Forms;
using System;
using Pablo.BGI;
using System.Collections.Generic;
using Eto;
using System.Linq;

namespace Pablo.Formats.Rip
{
	public partial class RipHandler : Handler
	{
		List<RipTool> tools;
		Messages.SendCommands sendCommands;
		LimitedStack<RipCommand> redoBuffer = new LimitedStack<RipCommand>(500);

		public List<RipTool> Tools
		{
			get
			{ 
				if (tools == null)
				{
					tools = new List<RipTool>
					{
						new Tools.Pixel(),
						new Tools.PixelBrush(),
			
						new Tools.Line(),
						new Tools.PolyLine(),
			
						new Tools.DrawRectangle(),
						new Tools.Bar(),

						new Tools.Polygon(),
						new Tools.FilledPolygon(),

						new Tools.Bezier(),
						new Tools.Fill(),

						new Tools.Oval(),
						new Tools.FilledOval(),
						
						new Tools.Arc(),
						new Tools.Pie(),
			
						new Tools.Copy(),
						new Tools.Paste(),
						
						new Tools.Text(),
						new Tools.InkDropper()
					};
				}
				return tools;
			}
		}

		public LimitedStack<RipCommand> RedoBuffer
		{
			get { return redoBuffer; }
		}

		public override IEnumerable<Pablo.Network.ICommand> Commands
		{
			get
			{
				foreach (var command in base.Commands)
					yield return command;
				yield return new Actions.Undo(this);
				yield return new Actions.Redo(this);
				yield return new Messages.SendCommands(this);
			}
		}

		public override IEnumerable<Pablo.Network.ICommand> ServerCommands
		{
			get
			{
				foreach (var command in base.ServerCommands)
					yield return command;
				yield return new Actions.Undo(this);
				yield return new Actions.Redo(this);
			}
		}

		public void SelectTool<T>()
			where T: RipTool
		{
			SelectedTool = Tools.FirstOrDefault(r => r.GetType() == typeof(T));
		}

		public event EventHandler<EventArgs> ToolChanged;

		protected virtual void OnToolChanged(EventArgs e)
		{
			if (ToolChanged != null)
				ToolChanged(this, e);
		}

		public RipTool SelectedTool
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

		public RipHandler(RipDocument doc) : base(doc)
		{
			/* Disable dos aspect here */
			if (doc.EditMode)
				EnableZoom = false;
			/**/
		}

		protected override void OnZoomChanged(EventArgs e)
		{
			base.OnZoomChanged(e);
			if (BGI != null)
				BGI.Scale = new SizeF(1 / this.ZoomRatio.Width, 1 / this.ZoomRatio.Height);
		}

		public RipDocument RipDocument
		{
			get { return (RipDocument)Document; }
		}

		public BGICanvas BGI
		{
			get { return RipDocument.BGI; }
		}

		public override bool CanEdit
		{
			get { return true; }
		}

		public override Size Size
		{
			get { return new Size(640, 350); }
		}

		public override SizeF Ratio
		{
			get
			{
				if (!EnableZoom)
					return base.Ratio;
				if (RipDocument.Info.DosAspect)
					return new SizeF(1.0F, (640.0F / 350.0F) / (640.0F / 480.0F));
				else
					return base.Ratio;
			}
		}

		public override void GenerateRegion(Graphics graphics, Rectangle rectSource, Rectangle rectDest)
		{
			if (this.ZoomRatio.Width == 1 && this.ZoomRatio.Height == 1)
				graphics.ImageInterpolation = ImageInterpolation.None;
			else
				graphics.ImageInterpolation = ImageInterpolation.Default;

			if (BGI != null)
			{
				BGI.DrawRegion(graphics, rectSource, rectDest);
			}
			else if (RipDocument.Image != null)
			{
				graphics.DrawImage(RipDocument.Image, rectSource, rectDest);
			}
		}

		public override void GenerateCommands(GenerateCommandArgs args)
		{
			base.GenerateCommands(args);

			string area = args.Area;
			
			if (area == "viewer")
			{
				var control = args.Control;
				var actionDos = new CheckCommand { 
					ID = "dosAspect", MenuText = "Emulate Legacy &Aspect", ToolTip = "Stretch image vertically to emulate DOS",
					Checked = !RipDocument.EditMode && RipDocument.Info.DosAspect,
					Enabled = !RipDocument.EditMode
				};
				actionDos.CheckedChanged += actionDos_CheckedChanged;

				var aiView = args.Menu.Items.GetSubmenu("&View", 500);
				var aiEdit = args.Menu.Items.GetSubmenu("&Edit", 200);
	
				aiView.Items.Add(actionDos, 500);
				
				if (Generator.IsMac)
				{
					control.MapPlatformCommand("undo", new Actions.Undo(this));
					control.MapPlatformCommand("redo", new Actions.Redo(this));
				}
				else
				{
					aiEdit.Items.Add(new Actions.Undo(this), 100);
					aiEdit.Items.Add(new Actions.Redo(this), 100);
				}
			}
		}

		protected override Control CreateViewerControl()
		{
			var control = new ViewerPane(this, false);
			
#if DESKTOP
			if (this.Document.EditMode)
				control.Viewer.Cursor = new Cursor(CursorType.Crosshair);
#endif
			control.SizeChanged += delegate
			{
				//Console.WriteLine ("What the f!");
				//if (BGI != null) 
				//	BGI.ResetGraphics ();
			};
			/*if (Generator.IsMac) {
				// ugh
				control.Scroll += delegate {
					if (BGI != null) 
						BGI.ResetGraphics ();
				};
			}*/
			return control;
		}

		private void actionDos_CheckedChanged(object sender, EventArgs e)
		{
			var action = (CheckCommand)sender;
			RipDocument.Info.DosAspect = action.Checked;
			OnSizeChanged(EventArgs.Empty);
			BGI.Scale = new SizeF(1 / this.ZoomRatio.Width, 1 / this.ZoomRatio.Height);
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			if (SelectedTool != null)
				SelectedTool.OnMouseDown(e);
			
			if (!e.Handled)
				base.OnMouseDown(e);
			
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			if (SelectedTool != null)
				SelectedTool.OnMouseUp(e);

			if (!e.Handled)
				base.OnMouseUp(e);
		}

		public override void OnMouseMove(MouseEventArgs e)
		{
			if (SelectedTool != null)
				SelectedTool.OnMouseMove(e);

			if (!e.Handled)
				base.OnMouseMove(e);
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			var allowToolSelection = true;
			if (SelectedTool != null)
			{
				SelectedTool.OnKeyDown(e);
				allowToolSelection = SelectedTool.AllowToolShortcuts;
			}

			if (!e.Handled && allowToolSelection)
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
			if (!e.Handled)
				base.OnKeyDown(e);
		}

		public override void Loaded()
		{
			base.Loaded();
			BGI.ResetGraphics();
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (this.Document.EditMode)
			{
				BGI.DelayDraw = false;
				this.LinePattern = BGI.GetLinePattern(BGICanvas.LineStyle.User);
				this.FillPattern = BGI.GetFillPattern(BGICanvas.FillStyle.User);
				SelectTool<Rip.Tools.Line>();
			}
		}

		public override void GeneratePads(GeneratePadArgs args)
		{
			base.GeneratePads(args);
			if (Document.EditMode && (this.Client == null || this.Client.CurrentUser.Level >= Pablo.Network.UserLevel.Editor))
			{
				var layout = new DynamicLayout { Padding = new Padding(5) };
				layout.BeginVertical(Padding.Empty, Size.Empty);
				layout.Add(new Controls.ColourPad(this));
				layout.EndVertical();
				layout.Add(new Controls.ToolboxPad(this));
				args.LeftPads.Add(layout);
			}
			else
				this.SelectedTool = null;
		}

		public void ApplyIfNeeded<T>(IList<Rectangle> updates = null)
			where T: RipOptionalCommand
		{
			var cmd = RipDocument.Create<T>();
			ApplyIfNeeded(cmd, updates);
		}

		public void ApplyIfNeeded(RipOptionalCommand command, IList<Rectangle> updates = null)
		{
			command.Set(this, false);
			if (Client != null)
				AddCommand(command as RipCommand, updates);
			else
			{
				if (command.ShouldApply(this.BGI))
				{
					AddCommand(command, updates);
				}
			}
		}

		public void FlushCommands(IList<Rectangle> updates = null)
		{
			if (sendCommands != null)
			{
				//Console.WriteLine ("Sending commands");
				this.Client.SendCommand(sendCommands);
				sendCommands = null;
			}
			if (updates != null)
				this.BGI.UpdateRegion(updates);
		}

		public void AddCommand(RipCommand command, IList<Rectangle> updates = null, bool direct = false)
		{
			if (this.Client != null && !direct)
			{
				if (sendCommands == null)
					sendCommands = new Messages.SendCommands(this);
				//Console.WriteLine ("Prepping command {0}", command);
				sendCommands.Commands.Add(command);
			}
			else
			{
				if (command is RipOptionalCommand && !RipDocument.OptionalApplied.Contains(command.OpCode))
				{
					RipDocument.OptionalApplied.Add(command.OpCode);
				}
				//Console.WriteLine ("Adding command {0}", command);
				command.Apply(updates);
				RipDocument.Commands.Add(command);
				RipDocument.IsModified = true;
				if (!direct)
					RedoBuffer.Clear();
			}
		}

		public void Redraw(IList<Rectangle> updates = null)
		{
			var tempUpdates = new List<Rectangle>();
			if (this.SelectedTool != null)
				this.SelectedTool.RemoveDrawing(tempUpdates);
			this.BGI.GraphDefaults(tempUpdates);
			foreach (var command in RipDocument.Commands)
			{
				tempUpdates.Clear();
				command.Apply(tempUpdates);
			}
			if (this.SelectedTool != null)
				this.SelectedTool.ApplyDrawing(tempUpdates);
			
			var rect = new Rectangle(this.BGI.WindowSize);
			if (updates != null)
				updates.Add(rect);
			else
				this.BGI.UpdateRegion(rect);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (BGI != null)
				BGI.Control = null;
		}
	}
}
