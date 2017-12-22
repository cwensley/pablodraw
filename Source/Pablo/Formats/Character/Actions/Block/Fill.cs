using System;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Controls;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class FillAttributes
	{
		public Controls.FillMode Mode { get; set; }
		public Rectangle Rectangle { get; set; }
		public Attribute Attribute { get; set; }
		public Character Character { get; set; }
	}
	
	public class Fill : PabloCommand
	{
		public FillAttributes Attributes { get; set; }
		
		Selection tool;
		
		public Fill (IClientSource handler)
			: base(handler)
		{
			ID = "character_Fill";
			MenuText = "&Fill...";
			ToolTip = "Fill the selected region|Fills the selected region";
			Name = "Fill";
			Shortcut = Keys.F;
		}
		
		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Selecting; }
			set { base.Enabled = value; }
		}

		public Fill (Selection tool) : this(tool.Handler)
		{
			this.tool = tool;
		}
		
		public override int CommandID { get { return (int)NetCommands.BlockFill; } }
		
		public override UserLevel Level { get { return UserLevel.Editor; } }
		
		FillAttributes GetAttribs()
		{
			var attribs = this.Attributes;
			if (attribs == null) {
				var handler = this.Handler as CharacterHandler;
				var dialog = new FillDialog (handler);
				var result = dialog.ShowModal((Control)handler.Viewer);
				if (!result) return null;
				attribs = new FillAttributes {
					Mode = dialog.FillMode,
					Attribute = dialog.Attribute,
					Character = dialog.Character,
					Rectangle = tool.SelectedRegion.Value
				};
			}
			return attribs;
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = this.Handler as CharacterHandler;
			var attribs = GetAttribs();
			if (attribs != null) {
				Do (handler.CursorPosition, attribs);
				if (tool != null) {
					handler.InvalidateCharacterRegion (attribs.Rectangle, true, true);
					tool.ClearSelected = false;
					tool.DrawMode = DrawMode.Normal;
					handler.CursorPosition = attribs.Rectangle.TopLeft;
				}
				else {
					handler.InvalidateCharacterRegion (attribs.Rectangle, true, false);
				}
			}
		}
		
		void Do (Point? cursorPosition, FillAttributes attributes)
		{
			var handler = this.Handler as CharacterHandler;
			var rect = attributes.Rectangle;
			rect.Normalize ();

			handler.Undo.Save (cursorPosition, cursorPosition, rect);
			
			if (attributes.Mode.HasFlag (Controls.FillMode.Attribute))
				handler.CurrentPage.Canvas.Fill (rect, attributes.Attribute);
			else if (attributes.Mode.HasFlag(Controls.FillMode.Background))
				handler.CurrentPage.Canvas.FillBackground (rect, attributes.Attribute.Background);
			else if (attributes.Mode.HasFlag(Controls.FillMode.Foreground))
				handler.CurrentPage.Canvas.FillForeground (rect, attributes.Attribute.Foreground);

			if (attributes.Mode.HasFlag (Controls.FillMode.Character))
				handler.CurrentPage.Canvas.Fill (rect, attributes.Character);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			var handler = this.Handler as CharacterHandler;
			base.Send (args);

			var attribs = GetAttribs ();
			if (attribs == null) return false;
			
			args.Message.Write (attribs.Rectangle);
			args.Message.WriteEnum<Controls.FillMode> (attribs.Mode);
			args.Message.Write (attribs.Attribute);
			args.Message.Write ((int)attribs.Character);
			
			if (tool != null) {
				tool.ClearSelected = false;
				tool.DrawMode = DrawMode.Normal;
				//tool.SelectedRegion = Rectangle.Empty;
				handler.CursorPosition = attribs.Rectangle.TopLeft;
			}
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var handler = this.Handler as CharacterHandler;
			var attribs = new FillAttributes{
				Rectangle = args.Message.ReadRectangle (),
				Mode = args.Message.ReadEnum<Controls.FillMode>(),
				Attribute = args.Message.ReadAttribute (),
				Character = args.Message.ReadInt32 ()
			};
			args.Invoke (delegate {
				Do (null, attribs);
				handler.InvalidateCharacterRegion (attribs.Rectangle, true);
			});
		}
	}
}

