using System;
using System.Collections.Generic;

using System.Net.Sockets;

using HoloFab;

namespace HoloFab {
	// TCP sender.
	public class TCPSend {
		// An IP and a port for TCP communication to send to.
		private string remoteIP;
		private int remotePort;
        
		public bool success = false;
        
		private TcpClient client;
		private NetworkStream stream;
		public List<string> debugMessages = new List<string>();
		public bool connected;
        
		// Main Constructor
		public TCPSend(string _remoteIP, int _remotePort=11111) {
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			// Reset.
			Disconnect();
			this.debugMessages = new List<string>();
		}
		// Destructor.
		~TCPSend() {
			Disconnect();
		}
		////////////////////////////////////////////////////////////////////////
		// Establish Connection
		public bool Connect() {
			// Reset.
			Disconnect();
			this.client = new TcpClient();
			try {
				// Open.
				if (!this.client.ConnectAsync(this.remoteIP, this.remotePort).Wait(2000)) {
					// connection failure
					this.connected = false;
					return false;
				}
				this.stream = this.client.GetStream();
				this.connected = true;
				// Acknowledge.
				this.debugMessages.Add("TCPSend: Connection Stablished!");
				return true;
			} catch (ArgumentNullException exception) {
				// Exception.
				this.debugMessages.Add("TCPSend: ArgumentNullException: " + exception.ToString());
				this.connected = false;
				return false;
			} catch (SocketException exception) {
				// Exception.
				this.debugMessages.Add("TCPSend: SocketException: " + exception.ToString());
				this.connected = false;
				return false;
			} catch (Exception exception) {
				this.debugMessages.Add("TCPSend: UnhandledException: " + exception.ToString());
				this.connected = false;
				return false;
			}
		}
		// Start a connection and send given byte array.
		public void Send(byte[] sendBuffer) {
			try {
				if (!this.client.Connected) {
					this.debugMessages.Add("TCPSend: Client Disconnected!");
					success = false;
				}
                
				// Write.
				this.stream.Write(sendBuffer, 0, sendBuffer.Length);
				// Acknowledge.
				this.debugMessages.Add("TCPSend: Data Sent!");
				success = true;
				return;
			} catch (ArgumentNullException exception) {
				// Exception.
				this.debugMessages.Add("TCPSend: ArgumentNullException: " + exception.ToString());
				success = false;
			} catch (SocketException exception) {
				// Exception.
				this.debugMessages.Add("TCPSend: SocketException: " + exception.ToString());
				success = false;
			}
			success = false;
		}
		// Stop Connection.
		public void Disconnect() {
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null;
			}
			if (this.stream != null) {
				this.stream.Close();
				this.stream = null; // Good Practice?
			}
		}
	}
}