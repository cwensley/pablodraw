using System;

namespace Pablo.Formats.Rip
{
	public enum NetCommands
	{
		SendCommands = (int)Pablo.Network.NetCommands.FormatCommands,
		
		Undo,
		Redo
		
	}
}

