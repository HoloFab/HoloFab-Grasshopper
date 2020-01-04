using System;
using System.Collections.Generic;

using System.Net.Sockets;

using HoloFab;

namespace HoloFab {
	// UDP sender.
	public static class UDPSend {
		// Connection Object Reference.
		private static UdpClient client;
		// Debug History.
		public static List<string> debugMessages = new List<string>();
        
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
			//Console.WriteLine(UDPSend.debugMessages[UDPSend.debugMessages.Count-1]);
		}
	}
}