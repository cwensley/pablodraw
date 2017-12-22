using System;
using Lidgren.Network;
using System.Collections.Generic;

namespace Pablo.Network
{
	public static class ICommandExtensions
	{
		public static void Add (this IDictionary<int, ICommand> dictionary, ICommand command)
		{
			dictionary.Add (command.CommandID, command);
		}
	}
	
	public interface INetworkReadWrite
	{
		bool Send (SendCommandArgs args);
		
		void Receive (ReceiveCommandArgs args);
	}
	
	public interface ICommand : INetworkReadWrite
	{
		int CommandID { get; }
		
		UserLevel Level { get; }
		
		NetDeliveryMethod DeliveryMethod { get; }
	}
}