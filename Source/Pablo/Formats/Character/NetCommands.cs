using System;

namespace Pablo.Formats.Character
{
	public enum NetCommands
	{
		DrawCharacter = (int)Pablo.Network.NetCommands.FormatCommands,
		
		SetWidth,
		SetPalette,
		SetColour,
		
		InsertLine,
		DeleteLine,
		InsertColumn,
		DeleteColumn,
		NewLine,
		Move,
		
		Backspace,
		Delete,
		
		
		BlockDelete,
		BlockFill,
		BlockPaste,
		BlockStamp,
		PasteFromClipboard,
		CutToClipboard,
		
		Undo,
		Redo,
		DrawBrush,
		DrawLine,
		DrawRect,
		DrawEllipse,
		ChangeFont,
		Toggle9x,
		ToggleIceMode,
		ToggleDosAspect
	}
}

