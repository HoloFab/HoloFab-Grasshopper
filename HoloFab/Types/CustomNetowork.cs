using System;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		// A struct holding network info for other components.
		public class Connection {
			public string remoteIP;
			public bool status;
			public UDPSend udpSender;
			public UDPReceive udpReceiver;
			public TCPSend tcpSender;
            
			public Connection(string _remoteIP){
				this.remoteIP = _remoteIP;
				this.status = false;
				this.udpSender = new UDPSend(_remoteIP);
				this.udpReceiver = new UDPReceive();
				this.tcpSender = new TCPSend(_remoteIP);
			}
			~Connection() {
				Disconnect();
			}
			// public Connection(string _remoteIP, bool _status, UDPSend _udpSender, UDPReceive _udpReceiver, TCPSend _tcpSender) {
			// 	this.remoteIP = _remoteIP;
			// 	this.status = _status;
			// 	this.udpSender = _udpSender;
			// 	this.udpReceiver = _udpReceiver;
			// 	this.tcpSender = _tcpSender;
			// }
			public bool Connect(){
				if (!this.tcpSender.Connect())
					return false;
				this.udpReceiver.Connect();
				this.udpSender.Connect();
				return true;
			}
			public void Disconnect(){
				this.tcpSender.Disconnect();
				this.udpReceiver.Disconnect();
				this.udpSender.Disconnect();
			}

			public bool PendingMessages {
				get {
					return (this.tcpSender.IsNotEmpty || this.udpSender.IsNotEmpty);
				}
			}

			public void TransmitIP() { 
				// Send local IPAddress for device to communicate back.
				byte[] bytes = EncodeUtilities.EncodeData("IPADDRESS", NetworkUtilities.LocalIPAddress(), out string currentMessage);
				this.udpSender.QueueUpData(bytes);
				//bool success = connect.udpSender.flagSuccess;
			}
		}
	}
}