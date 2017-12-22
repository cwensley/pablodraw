using System;
using System.Net;

namespace Pablo.Network
{
	public class UserBan : INetworkReadWrite
	{
		public string LastUserName { get; set; }
		
		public DateTime LastDenied { get; set; }
		
		public IPAddress Address { get; set; }
		
		public string HostMask { get; set; }
		
		
		public bool Send (Pablo.Network.SendCommandArgs args)
		{
			return false;
		}

		public void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
		}
	}
}

