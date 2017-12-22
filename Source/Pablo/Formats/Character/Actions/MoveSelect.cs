using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions
{
	public class MoveSelect : Move
	{
		Selection tool;
		public MoveSelect(Selection tool, string id, string name, string text, MoveDirection direction, Key modifier, params Key[] accelerators)
			: base(tool.Handler, id, name, text, direction, accelerators)
		{
			this.tool = tool;
			this.ID = this.ID + "_Select";
			for (int i = 0; i<Accelerators.Length; i++)
			{
				Accelerators[i] |= modifier;
			}
		}
		
		protected override void Execute(CommandExecuteArgs args)
		{
			CharacterHandler handler = Handler as CharacterHandler;
			Point pos = handler.CursorPosition;

			base.Execute(args); // move to new position!
			
			// now we want to select from where we were to the new position
			// if already selecting a region, set new destination point to current position
			if (handler.Info.ShiftSelect && (tool.DrawMode == DrawMode.Normal))
			{
				tool.DrawMode = DrawMode.Selecting;
				tool.SelectedRegion = new Rectangle(pos, handler.CursorPosition);
			}
		}
	}
}
