using System;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		// A struct holding network info for other components.
		public struct Connection {
			public string remoteIP;
			public bool status;
			public UDPSend udpSender;
			public UDPReceive udpReceiver;
			public TCPSend tcpSender;
            
			public Connection(string _remoteIP, bool _status, UDPSend _udpSender, UDPReceive _udpReceiver, TCPSend _tcpSender){
				this.remoteIP = _remoteIP;
				this.status = _status;
                
				this.udpSender = _udpSender;
				this.udpReceiver = _udpReceiver;
				this.tcpSender = _tcpSender;
			}
		}
	}
}