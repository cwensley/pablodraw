using System;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Controls;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Drawing
{
	public class HalfFillAttributes
	{
		public Rectangle Rectangle => HalfRectangle.FromHalfMode();
		public Rectangle HalfRectangle { get; set; }
		public Attribute Color { get; set; }
		public bool Invert { get; set; }
	}

	public class HalfFill : PabloCommand
	{
		public HalfFillAttributes Attributes { get; set; }

		Selection tool;

		public HalfFill(IClientSource handler)
			: base(handler)
		{
			ID = "character_HalfFill";
			MenuText = "&Half Fill...";
			Name = "HalfFill";
		}

		public override bool Enabled
		{
			get => base.Enabled && tool.DrawMode == DrawMode.Selecting;
			set => base.Enabled = value;
		}

		public HalfFill(Selection tool) : this(tool.Handler)
		{
			this.tool = tool;
		}

		public override int CommandID => (int)NetCommands.HalfFill;

		public override UserLevel Level => UserLevel.Editor;

		HalfFillAttributes GetAttribs()
		{
			var attribs = this.Attributes;
			if (attribs == null)
			{
				var handler = this.Handler as CharacterHandler;
				attribs = new HalfFillAttributes
				{
					Color = handler.DrawAttribute,
					HalfRectangle = tool.SelectedRegion.Value
				};
			}
			return attribs;
		}

		protected override void Execute(CommandExecuteArgs args)
		{
			var handler = this.Handler as CharacterHandler;
			var attribs = GetAttribs();
			if (attribs != null)
			{
				Do(handler.CursorPosition, attribs);
				if (tool != null)
				{
					handler.InvalidateCharacterRegion(attribs.Rectangle, true, true);
					tool.ClearSelected = false;
					tool.DrawMode = DrawMode.Normal;
					handler.CursorPosition = attribs.Rectangle.TopLeft;
				}
				else
				{
					handler.InvalidateCharacterRegion(attribs.Rectangle, true, false);
				}
			}
		}

		void Do(Point? cursorPosition, HalfFillAttributes attributes)
		{
			var handler = this.Handler as CharacterHandler;
			var rect = attributes.Rectangle;
			rect.Normalize();

			handler.Undo.Save(cursorPosition, cursorPosition, rect);

			var halfRect = attributes.HalfRectangle;
			halfRect.Normalize();
			if (attributes.Invert)
				handler.CurrentPage.Canvas.ClearHalfBlocks(halfRect, attributes.Color.Background);
			else
				handler.CurrentPage.Canvas.FillHalfBlocks(halfRect, attributes.Color.Foreground);
		}

		public override bool Send(Pablo.Network.SendCommandArgs args)
		{
			var handler = this.Handler as CharacterHandler;
			base.Send(args);

			var attribs = GetAttribs();
			if (attribs == null) return false;

			args.Message.Write(attribs.HalfRectangle);
			args.Message.Write(attribs.Color);
			args.Message.Write(attribs.Invert);

			if (tool != null)
			{
				tool.ClearSelected = false;
				tool.DrawMode = DrawMode.Normal;
				//tool.SelectedRegion = Rectangle.Empty;
				handler.CursorPosition = attribs.Rectangle.TopLeft;
			}
			return true;
		}

		public override void Receive(Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive(args);
			var handler = this.Handler as CharacterHandler;
			var attribs = new HalfFillAttributes
			{
				HalfRectangle = args.Message.ReadRectangle(),
				Color = args.Message.ReadAttribute(),
				Invert = args.Message.ReadBoolean()
			};
			args.Invoke(delegate
			{
				Do(null, attribs);
				handler.InvalidateCharacterRegion(attribs.Rectangle, true);
			});
		}
	}
}

