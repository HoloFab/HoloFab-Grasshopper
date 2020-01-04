using System;
using System.Collections.Generic;

using System.Net.Sockets;

using HoloFab;

namespace HoloFab {
	// TCP sender.
	public static class TCPSend {
		private static TcpClient client;
		private static NetworkStream stream;
		private static int remotePort = 11111;
		public static List<string> debugMessages = new List<string>();
        
		// Start a connection and send given byte array.
		public static void Send(byte[] sendBuffer, string remoteIP) {
			// Reset.
			TCPSend.debugMessages = new List<string>();
			if (TCPSend.client != null) {
				TCPSend.client.Close();
				TCPSend.client = null; // Good Practice?
			}
			if (TCPSend.stream != null) {
				TCPSend.stream.Close();
				TCPSend.stream = null; // Good Practice?
			}
			try {
				// Open.
				TCPSend.client = new TcpClient(remoteIP, TCPSend.remotePort);
				TCPSend.stream = TCPSend.client.GetStream();
				// Write.
				TCPSend.stream.Write(sendBuffer, 0, sendBuffer.Length);
				// Close.
				TCPSend.stream.Close();
				TCPSend.client.Close();
				// Acknowledge.
				TCPSend.debugMessages.Add("TCPSend: Data Sent!");
			} catch (ArgumentNullException exception) {
				// Exception.
				TCPSend.debugMessages.Add("TCPSend: ArgumentNullException: " + exception.ToString());
			} catch (SocketException exception) {
				// Exception.
				TCPSend.debugMessages.Add("TCPSend: SocketException: " + exception.ToString());
			}
			//Console.WriteLine(TCPSend.debugMessages[TCPSend.debugMessages.Count-1]);
		}
	}
}