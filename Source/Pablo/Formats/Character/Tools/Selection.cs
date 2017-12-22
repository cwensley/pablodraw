using System;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Eto.Forms;

namespace Pablo.Formats.Character.Tools
{
	public enum DrawMode
	{
		Normal,
		Selecting,
		Paste
	}
 
	public class Selection : CenterAspectTool
	{
		DrawMode drawMode = DrawMode.Normal;
		PasteMode pasteMode = PasteMode.Normal;
		Rectangle? selectedRegion;
		Canvas pasteCanvas;

		public override Eto.Drawing.Image Image {
			get { return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Copy.png"); }
		}

		public override string Description {
			get { return "Selection - Select text and position the cursor with the mouse"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.E | (Handler.Generator.IsMac ? Key.Control : Key.Alt);
			}
		}
		
		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.IBeam); }
		}
		
		public override bool AllowKeyboard {
			get { return drawMode == DrawMode.Normal; }
		}
		
		public override void Unselected ()
		{
			base.Unselected ();
			if (selectedRegion != null) {
				Handler.InvalidateCharacterRegion (selectedRegion.Value, false);
			}
			if (pasteCanvas != null) {
				Handler.InvalidateCharacterRegion (new Rectangle (Handler.CursorPosition, pasteCanvas.Size), false);
			}
			// go back to normal
			//this.DrawMode = DrawMode.Normal;
		}
		
		public override void Selecting ()
		{
			base.Selecting ();
			if (selectedRegion != null) {
				Handler.InvalidateCharacterRegion (selectedRegion.Value, false);
			}
			if (pasteCanvas != null) {
				Handler.InvalidateCharacterRegion (new Rectangle (Handler.CursorPosition, pasteCanvas.Size), false);
			}
		}
		
		public DrawMode DrawMode {
			get { return drawMode; }
			set {
				if (drawMode != value) {
					drawMode = value;
					var start = Start;
					switch (drawMode) {
					case DrawMode.Normal:
						SelectedRegion = null;
						Cancel ();
						if (start != null)
							Handler.CursorPosition = start.Value;
						break;
					case DrawMode.Paste:
						pasteMode = PasteMode.Normal;
						Cancel ();
						break;
					case DrawMode.Selecting:
						Begin ();
						break;
					}
					Handler.TriggerActionsChanged ();
				}
			}
		}
		
		public override IEnumerable<Pablo.Network.ICommand> Commands {
			get {
				foreach (var c in base.Commands)
					yield return c;
				yield return new Actions.Block.Paste(this);
				yield return new Actions.Block.Delete(this);
				yield return new Actions.Block.Stamp(this);
				yield return new Actions.Block.PasteFromClipboard(this);
				yield return new Actions.Block.CutToClipboard(this);
				yield return new Actions.Block.Fill(this);
			}
		}
		
		public PasteMode PasteMode {
			get { return pasteMode; }
			set {
				if (pasteMode != value) {
					pasteMode = value;
					if (pasteCanvas != null) {
						var rect = new Rectangle (Handler.CursorPosition, pasteCanvas.Size);
						Handler.InvalidateCharacterRegion (rect, false);
					}
				}
			}
		}
		
		public Rectangle? SelectedRegion {
			get { return selectedRegion; }
			set {
				var rect = selectedRegion;
				selectedRegion = value;
				if (selectedRegion != null) {
					var val = selectedRegion.Value;
					val.Restrict (new Rectangle (Handler.CurrentPage.Canvas.Size));
					selectedRegion = val;
					
					this.ResolvedRectangle = selectedRegion;
					this.Start = selectedRegion.Value.Location;
					this.End = selectedRegion.Value.EndLocation;
	             
					if (rect != null) {
						if (val.Intersects (rect.Value)) {
							rect = Rectangle.Union (rect.Value, selectedRegion.Value);
						} else
							Handler.InvalidateCharacterRegion (selectedRegion.Value, false);
					} else 
						rect = selectedRegion;
				} else {
					this.ResolvedRectangle = null;
					this.Start = this.End = null;
				}
				if (rect != null)
					Handler.InvalidateCharacterRegion (rect.Value, false);
			}
		}
     
		public bool ClearSelected { get; set; }
     
		public Canvas PasteCanvas {
			set {
				Rectangle rect = Rectangle.Empty;
				if (pasteCanvas != null)
					rect = new Rectangle (Handler.CursorPosition, pasteCanvas.Size);
				pasteCanvas = value;
				if (pasteCanvas != null) {
					if (!rect.IsEmpty)
						rect = Rectangle.Union (rect, new Rectangle (Handler.CursorPosition, pasteCanvas.Size));
					else
						rect = new Rectangle (Handler.CursorPosition, pasteCanvas.Size);
				}
                                                           
				if (!rect.IsEmpty) {
					Handler.InvalidateCharacterRegion (rect, false);
				}
             
			}
			get { return pasteCanvas; }
		}
		
		public override void OnSetCursorPosition (Point old, Point cursorPosition, bool invalidate)
		{
			switch (drawMode) {
			case DrawMode.Normal:
				base.OnSetCursorPosition (old, cursorPosition, invalidate);
				break;
			case DrawMode.Paste:
				if (pasteCanvas != null) {
					var rect = new Rectangle (old, pasteCanvas.Size);
					rect = Rectangle.Union (rect, new Rectangle (cursorPosition, pasteCanvas.Size));
					Handler.InvalidateCharacterRegion (rect, false);
				}
				break;
			case DrawMode.Selecting:
				base.OnSetCursorPosition (old, cursorPosition, invalidate);
				/*
				if (selectedRegion != null) {
					var rect = selectedRegion.Value;
					var reg = rect;
					reg.EndLocation = cursorPosition;
					selectedRegion = reg;
					rect = Rectangle.Union (rect, reg);
					if (invalidate)
						Handler.InvalidateCharacterRegion (rect, false);
				}*/
				break;
			}
		}
		
		protected override bool Apply ()
		{
			return false;
		}
		
		protected override void UpdateWithLocation (Rectangle rect, Key modifiers, Point end)
		{
			var updateRect = rect;
			rect.Restrict (new Rectangle (Handler.CurrentPage.Canvas.Size));
			if (selectedRegion != null)
				updateRect = Rectangle.Union (updateRect, selectedRegion.Value);
			selectedRegion = rect;
			UpdatingCursor = true;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion (updateRect, false);
		}
		
		public override void OnMouseDown (Eto.Forms.MouseEventArgs e)
		{
			switch (this.DrawMode) {
			case DrawMode.Paste:
				var command = new Actions.Block.Paste (this);
				command.Activate ();
				break;
         
			case DrawMode.Normal:
			case DrawMode.Selecting:
				base.OnMouseDown (e);
				DrawMode = DrawMode.Selecting;
				/*
				Handler.CursorPosition = Handler.ScreenToCharacter (e.Location);
				var bs = new Actions.Block.BlockSelect (this);
				bs.Activate ();
				*/
				break;
			}
		}
		
		public override void OnMouseUp (Eto.Forms.MouseEventArgs e)
		{
			var pt = Handler.ScreenToCharacter ((Point)e.Location);
			switch (this.DrawMode) {
			case DrawMode.Normal:
				Handler.CursorPosition = pt;
				break;
			case DrawMode.Selecting:
				base.OnMouseUp (e);
				if (this.CurrentRectangle != null) {
					var rect = this.CurrentRectangle.Value;
					if (rect.Width == 1 && rect.Height == 1)
						DrawMode = DrawMode.Normal;
				}
				/*
				Handler.CursorPosition = pt;
				var reg = new Rectangle (this.SelectedRegion.Value.Location, pt);
				this.SelectedRegion = reg;
				if (reg.Width == 1 && reg.Height == 1) {
					DrawMode = DrawMode.Normal;
				}*/
				break;
			}
		}

		public override void OnMouseMove (Eto.Forms.MouseEventArgs e)
		{
			Point pt = Handler.ScreenToCharacter ((Point)e.Location);
			switch (this.DrawMode) {
			case DrawMode.Paste:
				Handler.CursorPosition = pt;
				break;
			case DrawMode.Selecting:
				base.OnMouseMove (e);
				/*
				if (e.Buttons.HasFlag (MouseButtons.Primary)) {
					Handler.CursorPosition = pt;
				}*/
                //this.SelectedRegion = new Rectangle(this.SelectedRegion.Location, pt);
				break;
			}
		}
		
		public override void GenerateActions (Eto.Forms.GenerateActionArgs args)
		{
			base.GenerateActions (args);
			
			var control = args.Arguments ["control"] as Control;
			var edit = args.Menu.GetSubmenu("&Edit");
			
			
			switch (this.DrawMode) {
			case DrawMode.Selecting:
				args.Actions.Add (new Actions.Block.Deselect (this));
				args.Actions.Add (new Actions.Block.Copy (this));
				args.Actions.Add (new Actions.Block.Move (this));
				args.Actions.Add (new Actions.Block.Fill (this));
				args.Actions.Add (new Actions.Block.Delete (this));
				args.Actions.Add (new Actions.Block.CopyToClipboard (this));
				args.Actions.Add (new Actions.Block.CutToClipboard (this));
				args.Actions.Add (new Actions.Block.SelectAll (this));
				
				edit.Actions.AddSeparator (200);
				edit.Actions.Add ("character_deselect", 200);
				if (args.Generator.IsMac) {
					control.MapPlatformAction ("cut", args.Actions.Find (Actions.Block.CutToClipboard.ActionID));
					control.MapPlatformAction ("copy", args.Actions.Find (Actions.Block.CopyToClipboard.ActionID));
					control.MapPlatformAction ("selectAll", args.Actions.Find (Actions.Block.SelectAll.ActionID));
					control.MapPlatformAction ("delete", args.Actions.Find (Actions.Block.Delete.ActionID));
				} else {
					edit.Actions.Add (Actions.Block.CutToClipboard.ActionID, 200);
					edit.Actions.Add (Actions.Block.CopyToClipboard.ActionID, 200);
					edit.Actions.Add (Actions.Block.Delete.ActionID, 200);
					edit.Actions.AddSeparator (300);
					edit.Actions.Add (Actions.Block.SelectAll.ActionID, 300);
				}
				edit.Actions.Add ("character_Move", 200);
				edit.Actions.Add ("character_Copy", 200);
				edit.Actions.Add ("character_Fill", 200);
						
				break;
                         
			case DrawMode.Paste:
				args.Actions.Add (new Actions.Block.Deselect (this));
				args.Actions.Add (new Actions.Block.Stamp (this));
				args.Actions.Add (new Actions.Block.Rotate (this));
				args.Actions.Add (new Actions.Block.FlipX (this));
				args.Actions.Add (new Actions.Block.FlipY (this));
				args.Actions.Add (new Actions.Block.Paste (this));
				args.Actions.Add (new Actions.Block.Transparent (this));
				args.Actions.Add (new Actions.Block.Under (this));
				edit.Actions.AddSeparator (200);
				edit.Actions.Add ("character_deselect", 200);
				edit.Actions.Add (Actions.Block.Paste.ActionID, 200);
				edit.Actions.Add ("character_Stamp", 200);
				edit.Actions.Add ("character_Rotate", 200);
				edit.Actions.Add ("character_FlipX", 200);
				edit.Actions.Add ("character_FlipY", 200);

				edit.Actions.AddSeparator (300);
				edit.Actions.Add (Actions.Block.Transparent.ActionID, 300);
				edit.Actions.Add (Actions.Block.Under.ActionID, 300);
				break;
			case DrawMode.Normal:
				args.Actions.Add (new Actions.Block.BlockSelect (this));
				args.Actions.Add (new Actions.Block.SelectAll (this));
				args.Actions.Add (new Actions.Block.PasteFromClipboard (this));
				
				if (args.Generator.IsMac) {
					control.MapPlatformAction ("paste", args.Actions.Find (Actions.Block.PasteFromClipboard.ActionID));
					control.MapPlatformAction ("selectAll", args.Actions.Find (Actions.Block.SelectAll.ActionID));
				} else {
					edit.Actions.Add (Actions.Block.PasteFromClipboard.ActionID, 200);
					edit.Actions.Add (Actions.Block.SelectAll.ActionID, 300);
				}
				break;
			}
		}
		
		bool inPaste = false;

		public override CanvasElement? GetElement (Point point, Canvas canvas)
		{
			if (pasteCanvas != null && new Rectangle (Handler.CursorPosition, pasteCanvas.Size).Contains (point)) {
				inPaste = true;
				switch (this.pasteMode) {
				default:
				case PasteMode.Normal:
					return pasteCanvas [new Point(point - Handler.CursorPosition)];
				case PasteMode.Transparent: {
						var ce = pasteCanvas [new Point(point - Handler.CursorPosition)];
						if (ce.IsTransparent) {
							ce = canvas [point];
							inPaste = false;
						}
						return ce;
					}
				case PasteMode.Under: {
						var ce = canvas [point];
						if (ce.IsTransparent || (ClearSelected && selectedRegion != null && selectedRegion.Value.Contains (point))) 
							ce = pasteCanvas [new Point(point - Handler.CursorPosition)];
						return ce;
					}
				}
			}
			inPaste = false;
			return null;
			
		}
		
		public override void TranslateColour (Point point, ref uint foreColour, ref uint backColour)
		{
			if (selectedRegion != null && !inPaste && selectedRegion.Value.Contains (point)) {
				if (ClearSelected) {
					foreColour = 0xff000000;
					backColour = 0xff000000;
				} else {
					foreColour = ~foreColour | 0xff000000;
					backColour = ~backColour | 0xff000000;
				}
			}
		}
		
		public override void DeleteLine (int y)
		{
			if (this.SelectedRegion != null && (this.SelectedRegion.Value.Top >= y || this.SelectedRegion.Value.InnerBottom >= y)) {
				var reg = this.SelectedRegion.Value;
				reg.Normalize ();
				if (reg.Y >= y)
					reg.Y --;
				else if (reg.InnerBottom >= y)
					reg.Height --;
				this.SelectedRegion = reg;
			}
		}
		
		public override void InsertLine (int y)
		{
			if (this.SelectedRegion != null && (this.SelectedRegion.Value.Top >= y || this.SelectedRegion.Value.InnerBottom >= y)) {
				var reg = this.SelectedRegion.Value;
				reg.Normalize ();
				if (reg.Y >= y)
					reg.Y ++;
				else if (reg.InnerBottom >= y)
					reg.Height ++;
				this.SelectedRegion = reg;
			}
		}
		
		public override Eto.Forms.Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.Add (Separator ());
			layout.Add (base.GeneratePad ());
			
			return layout;
		}
	}
}

