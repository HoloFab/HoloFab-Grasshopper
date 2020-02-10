using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif

namespace HoloFab {
	// UDP sender.
	public class UDPSend {
		// A port for UDP communication to send to.
		int remotePort = 12121;
        
		// Network Objects:
		#if WINDOWS_UWP
		// Connection Object Reference.
		private DatagramSocket client;
		#else
		// Connection Object Reference.
		private UdpClient client;
		#endif
		// History:
		// - Debug History.
		public List<string> debugMessages = new List<string>();
        
		// Constructor.
		public UDPSend(int _remotePort=12121){
			this.remotePort = _remotePort;
		}
        
		#if WINDOWS_UWP
		// Start a connection and send given byte array.
		public async void Send(byte[] sendBuffer, string remoteIP) {
			// Reset.
			this.debugMessages = new List<string>();
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await client.GetOutputStreamAsync(new HostName(remoteIP), this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				// Acknowledge.
				this.debugMessages.Add("UDPSend: UWP: Data Sent!");
			} catch (Exception exception) {
				// Exception.
				this.debugMessages.Add("UDPSend: UWP: Exception: " + exception.ToString());
			}
		}
		#else
		// Start a connection and send given byte array.
		public void Send(byte[] sendBuffer, string remoteIP) {
			// Reset.
			this.debugMessages = new List<string>();
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new UdpClient(remoteIP, this.remotePort);
				// Write.
				this.client.Send(sendBuffer, sendBuffer.Length);
				// Close.
				this.client.Close();
				// Acknowledge.
				this.debugMessages.Add("UDPSend: Data Sent!");
			} catch (Exception exception) {
				// Exception.
				this.debugMessages.Add("UDPSend: Exception: " + exception.ToString());
			}
		}
		#endif
	}
}