using System;
using Lidgren.Network;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Network;

namespace Pablo.Formats.Rip
{
	public static class LidgrenExtensions
	{
		public static void Write (this NetOutgoingMessage message, IEnumerable<RipCommand> commands)
		{
			var stream = new MemoryStream ();
			var writer = new RipWriter (stream);
			
			foreach (var command in commands) {
				command.Write (writer);
			}
			stream.Flush ();
			stream.Position = 0;
			message.WriteStream ((Stream)stream);
		}

		public static List<RipCommand> ReadCommands (this NetIncomingMessage message, RipDocument document)
		{
			var commands = new List<RipCommand>();
			ReadCommands (message, document, commands);
			return commands;
		}
		
		public static void ReadCommands (this NetIncomingMessage message, RipDocument document, IList<RipCommand> commands)
		{
			var stream = message.ReadStream ();
			var reader = new BinaryReader (stream);
			try {
				while (true) {
					char b = (char)reader.ReadRipByte ();
					if (b == '|') {
					
						string op = ((char)reader.ReadRipByte ()).ToString ();
						if (op == "1")
							op += (char)reader.ReadRipByte ();
						
						var command = RipCommands.Create (op, document);
						if (command != null) {
							command.Read (reader);
							if (command.Store)
								commands.Add (command);
						}
					}
				}
			} catch (EndOfStreamException) {
			}
		}
	}
}

