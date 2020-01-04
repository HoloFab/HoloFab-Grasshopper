using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using HoloFab;

namespace HoloFab {
	// UDP receiver.
	// TODO:
	// - Change data received into an event to be raised and subscribed by listeners.
	public static class UDPReceive {
		// Connection Object Reference.
		private static UdpClient client;
		// Thread Object Reference.
		private static Thread receiveThread = null;
		// Local Port
		private static int localPort = 8055;
		// History:
		// - debug
		public static List<string> debugMessages = new List<string>();
		// - received data
		public static List<string> dataMessages = new List<string>();
		// Flag to be raised on data recepcion.
		public static bool flagDataRead = true;
        
		// Enable connection - if not yet open.
		public static void TryStartConnection(int _localPort=8055) {
			// Create a new thread to receive incoming messages.
			if (UDPReceive.receiveThread == null) {
				if (UDPReceive.localPort != _localPort)
					UDPReceive.localPort = _localPort;
				// Reset.
				UDPReceive.debugMessages = new List<string>();
				UDPReceive.dataMessages = new List<string>();
				// Start the thread.
				UDPReceive.receiveThread = new Thread(new ThreadStart(ReceiveData));
				UDPReceive.receiveThread.IsBackground = true;
				UDPReceive.receiveThread.Start();
				UDPReceive.debugMessages.Add("UDPReceive: Thread Started.");
			}
		}
		// Disable connection.
		public static void StopConnection() {
			// Reset.
			if (UDPReceive.receiveThread != null) {
				UDPReceive.receiveThread.Abort();
				UDPReceive.receiveThread = null; // Good Practice?
				UDPReceive.debugMessages.Add("UDPReceive: Stopping Thread.");
			}
			if (UDPReceive.client != null) {
				UDPReceive.client.Close();
				UDPReceive.client = null; // Good Practice?
				UDPReceive.debugMessages.Add("UDPReceive: Stopping Client.");
			}
		}
		// Constantly check for new messages on given port.
		private static void ReceiveData() {
			// Open.
			UDPReceive.client = new UdpClient(UDPReceive.localPort);
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
			// Infinite loop.
			try {
				byte[] data;
				string receiveString;
				while (true) {
					// Receive Bytes.
					data = client.Receive(ref anyIP);
					if (data.Length > 0) {
						// If buffer not empty - decode it.
						receiveString = EncodeUtilities.DecodeData(data);
						if ((UDPReceive.dataMessages.Count == 0) || (UDPReceive.dataMessages[UDPReceive.dataMessages.Count-1] != receiveString)) {
							UDPReceive.debugMessages.Add("UDPReceive: New Data: " + receiveString);
							UDPReceive.dataMessages.Add(receiveString);
							UDPReceive.flagDataRead = false;
						}
					}
				}
			} catch (SocketException exception) {
				// SocketException.
				UDPReceive.debugMessages.Add("UDPReceive: SocketException: " + exception.ToString());
			} catch (Exception exception) {
				// Exception.
				UDPReceive.debugMessages.Add("UDPReceive: Exception: " + exception.ToString());
			} finally {
				UDPReceive.StopConnection();
			}
		}
	}
}