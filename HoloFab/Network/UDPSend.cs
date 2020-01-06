using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif

using HoloFab;

namespace HoloFab {
	// UDP sender.
	public static class UDPSend {
		#if WINDOWS_UWP
		// Connection Object Reference.
		private static DatagramSocket client;
		#else
		// Connection Object Reference.
		private static UdpClient client;
		#endif
		// Debug History.
		public static List<string> debugMessages = new List<string>();
        
		#if WINDOWS_UWP
		// Start a connection and send given byte array.
		public static async void Send(byte[] sendBuffer, string remoteIP, int remotePort=12121) {
			// Reset.
			UDPSend.debugMessages = new List<string>();
			if (UDPSend.client != null) {
				UDPSend.client.Dispose();
				UDPSend.client = null; // Good Practice?
			}
			try {
				// Open.
				UDPSend.client = new DatagramSocket();
				// Write.
				using (var stream = await client.GetOutputStreamAsync(new HostName(remoteIP), remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				UDPSend.client.Dispose();
				// Acknowledge.
				UDPSend.debugMessages.Add("UDPSend: UWP: Data Sent!");
			} catch (Exception exception) {
				// Exception.
				UDPSend.debugMessages.Add("UDPSend: UWP: Exception: " + exception.ToString());
			}
		}
		#else
		// Start a connection and send given byte array.
		public static void Send(byte[] sendBuffer, string remoteIP, int remotePort=12121) {
			// Reset.
			UDPSend.debugMessages = new List<string>();
			if (UDPSend.client != null) {
				UDPSend.client.Close();
				UDPSend.client = null; // Good Practice?
			}
			try {
				// Open.
				UDPSend.client = new UdpClient(remoteIP, remotePort);
				// Write.
				UDPSend.client.Send(sendBuffer, sendBuffer.Length);
				// Close.
				UDPSend.client.Close();
				// Acknowledge.
				UDPSend.debugMessages.Add("UDPSend: Data Sent!");
			} catch (Exception exception) {
				// Exception.
				UDPSend.debugMessages.Add("UDPSend: Exception: " + exception.ToString());
			}
		}
		#endif
	}
}