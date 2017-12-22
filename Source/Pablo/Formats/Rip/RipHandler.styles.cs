using System;
using System.Linq;
using Pablo.BGI;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip
{
	public partial class RipHandler
	{
		RipTool selectedTool;
		byte foreground = 15;
		byte background = 7;
		BGICanvas.LineStyle lineStyle = BGICanvas.LineStyle.Solid;
		BGICanvas.FillStyle fillStyle = BGICanvas.FillStyle.Solid;
		byte[] fillPattern = new byte[8];
		int lineThickness = 1;
		uint linePattern = 0xFFFF;
		BGICanvas.FontType fontType = BGICanvas.FontType.Small;
		BGICanvas.Direction textDirection = BGICanvas.Direction.Horizontal;
		int fontSize = 4;
		
		public event EventHandler<EventArgs> LineStyleChanged;
		
		protected virtual void OnLineStyleChanged (EventArgs e)
		{
			if (LineStyleChanged != null)
				LineStyleChanged (this, e);
			RedrawTool ();
		}
		
		public BGICanvas.LineStyle LineStyle {
			get { return lineStyle; }
			set {
				if (lineStyle != value) {
					lineStyle = value;
					OnLineStyleChanged (EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler<EventArgs> LineThicknessChanged;
		
		protected virtual void OnLineThicknessChanged (EventArgs e)
		{
			if (LineThicknessChanged != null)
				LineThicknessChanged (this, e);
			RedrawTool ();
		}
		
		public int LineThickness {
			get { return lineThickness; }
			set { 
				if (lineThickness != value) {
					lineThickness = value;
					OnLineThicknessChanged (EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler<EventArgs> LinePatternChanged;
		
		protected virtual void OnLinePatternChanged (EventArgs e)
		{
			if (LinePatternChanged != null)
				LinePatternChanged (this, e);
			RedrawTool ();
		}
		
		public uint LinePattern {
			get { return linePattern; }
			set {
				if (linePattern != value) {
					linePattern = value;
					OnLinePatternChanged (EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler<EventArgs> FillStyleChanged;
		
		protected virtual void OnFillStyleChanged (EventArgs e)
		{
			if (FillStyleChanged != null)
				FillStyleChanged (this, e);
			RedrawTool ();
		}
		
		public BGICanvas.FillStyle FillStyle {
			get { return fillStyle; }
			set {
				if (fillStyle != value) {
					fillStyle = value;
					OnFillStyleChanged (EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> FillPatternChanged;
		
		protected virtual void OnFillPatternChanged (EventArgs e)
		{
			if (FillPatternChanged != null)
				FillPatternChanged (this, e);
			RedrawTool ();
		}
		
		public byte[] FillPattern {
			get { return fillPattern; }
			set {
				if (!fillPattern.SequenceEqual (value)) {
					fillPattern = value;
					OnFillPatternChanged (EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler<EventArgs> AttributeChanged;
		
		protected virtual void OnAttributeChanged (EventArgs e)
		{
			if (AttributeChanged != null)
				AttributeChanged (this, e);
			RedrawTool ();
		}
		
		public byte Foreground {
			get { return foreground; }
			set {
				if (value != foreground) {
					foreground = value;
					OnAttributeChanged (EventArgs.Empty);
				}
			}
		}
		
		public byte Background {
			get { return background; }
			set {
				if (value != background) {
					background = value;
					OnAttributeChanged (EventArgs.Empty);
				}
			}
		}
		
		public int FontSize {
			get { return this.fontSize; }
			set {
				if (value != fontSize) {
					fontSize = value;
					RedrawTool ();
				}
			}
		}
		
		public BGICanvas.FontType FontType {
			get { return this.fontType; }
			set {
				if (value != this.FontType) {
					this.fontType = value;
					RedrawTool ();
				}
			}
		}
		
		public BGICanvas.Direction TextDirection {
			get { return this.textDirection; }
			set { 
				if (value != textDirection) {
					textDirection = value;
					RedrawTool ();
				}
			}
		}
		
		void RemoveTool (IList<Rectangle> updates) {
			if (SelectedTool != null)
				SelectedTool.RemoveDrawing(updates);
			
		}
		void ApplyTool (IList<Rectangle> updates) {
			if (SelectedTool != null) {
				SelectedTool.ApplyDrawing (updates);
				this.BGI.UpdateRegion (updates);
			}
			
		}
		
		void RedrawTool ()
		{
			if (SelectedTool != null)
				SelectedTool.Redraw ();
		}
		
	}
}

