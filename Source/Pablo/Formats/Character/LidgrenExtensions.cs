using System;
using Lidgren.Network;
using Eto.Drawing;

namespace Pablo.Formats.Character
{
	public static class LidgrenExtensions
	{
		public static Canvas ReadCanvas (this NetIncomingMessage message, Palette pal)
		{
			return Types.Pablo.Receive (message, pal);
		}
		
		public static void Write (this NetOutgoingMessage message, Canvas canvas, Palette pal)
		{
			Types.Pablo.Send (message, canvas, pal);
		}
		
		public static CanvasElement ReadCanvasElement (this NetIncomingMessage message)
		{
			return new CanvasElement (
				message.ReadVariableInt32 (),
				message.ReadAttribute ()
				);
		}
		
		public static void Write (this NetOutgoingMessage message, CanvasElement element)
		{
			message.WriteVariableInt32 (element.Character);
			message.Write (element.Attribute);
		}
		
		public static Attribute ReadAttribute (this NetIncomingMessage message)
		{
			return new Attribute (
				message.ReadVariableInt32 (),
				message.ReadVariableInt32 ()
				);
		}
		
		public static void Write (this NetOutgoingMessage message, Attribute attribute)
		{
			message.WriteVariableInt32 (attribute.Foreground);
			message.WriteVariableInt32 (attribute.Background);
		}
	}
}

