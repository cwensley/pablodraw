using System;

namespace Pablo.Network
{
	public enum NetCommands
	{
		UserList = 1,
		ChatMessage,
		UserStatusChanged,
		LoadDocument,
		LoadFile,

		NewFile,
		EditFile,
		EditSauce,

		FormatCommands = 1000,
		InterfaceCommands = 2000
	}
}

