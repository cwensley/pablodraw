using System;

namespace Pablo.Formats.Character.Undo
{
	public enum UndoType
	{
		UndoBuffer,
		UndoColour,
		UndoDeleteCharacter,
		UndoDeleteColumn,
		UndoDeleteLine,
		UndoInsertCharacter,
		UndoInsertColumn,
		UndoInsertLine,
		UndoRect
	}
}

