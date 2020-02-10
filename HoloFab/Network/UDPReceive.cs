using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloFab {
	// UDP receiver.
	// TODO:
	// - Change data received into an event to be raised and subscribed by listeners.
	public class UDPReceive {
		// Local Port
		private int localPort = 8055;
        
		// Network Objects:
		#if WINDOWS_UWP
		// Connection Object Reference.
		private DatagramSocket client;
		#else
		// Connection Object Reference.
		private UdpClient client;
		// Thread Object Reference.
		private Thread receiveThread = null;
		#endif
		// History:
		// - debug
		public List<string> debugMessages = new List<string>();
		// - received data
		public List<string> dataMessages = new List<string>();
		// Flag to be raised on data recepcion.
		public bool flagDataRead = true;
        
		// Constructor.
		public UDPReceive(int _localPort=8055){
			this.localPort = _localPort;
			TryStartConnection();
		}
        
		// Enable connection - if not yet open.
		public void TryStartConnection() {
			#if WINDOWS_UWP
			StartConnection();
			#else
			// Create a new thread to receive incoming messages.
			if (this.receiveThread == null)
				StartConnection();
			#endif
		}
		//////////////////////////////////////////////////////////////////////////
		#if WINDOWS_UWP
		private async void StartConnection(){
			// Reset.
			this.debugMessages = new List<string>();
			this.dataMessages = new List<string>();
			// Start receiving.
			this.client = new DatagramSocket();
			this.client.MessageReceived += ReceiveData;
			try {
				await client.BindEndpointAsync(new HostName(NetworkUtilities.LocalIPAddress()), this.localPort.ToString());
			} catch (Exception exception) {
				this.debugMessages.Add("UDPReceive:UWP:ERROR "+exception.ToString() + SocketError.GetStatus(exception.HResult).ToString());
			}
		}
		// Disable connection.
		public async void StopConnection() {
			// Reset.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
				this.debugMessages.Add("UDPReceive:UWP: Stopping Client.");
			}
		}
		// Constantly check for new messages on given port.
		private async void ReceiveData(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args){
			//Read the message that was received from the UDP client.
			DataReader reader = args.GetDataReader();
			string receiveString = reader.ReadString(reader.UnconsumedBufferLength).Trim();
			// If string not empty and not read yet - react to it.
			if ((!string.IsNullOrEmpty(receiveString)) && ((this.dataMessages.Count == 0) || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
				this.debugMessages.Add("UDPReceive: New Data: " + receiveString);
				this.dataMessages.Add(receiveString);
				this.flagDataRead = false;
			}
		}
		//////////////////////////////////////////////////////////////////////////
		#else
		private void StartConnection(){
			// Reset.
			this.debugMessages = new List<string>();
			this.dataMessages = new List<string>();
			// Start the thread.
			this.receiveThread = new Thread(new ThreadStart(ReceiveData));
			this.receiveThread.IsBackground = true;
			this.receiveThread.Start();
			this.debugMessages.Add("UDPReceive: Thread Started.");
		}
		// Disable connection.
		public void StopConnection() {
			// Reset.
			if (this.receiveThread != null) {
				this.receiveThread.Abort();
				this.receiveThread = null; // Good Practice?
				this.debugMessages.Add("UDPReceive: Stopping Thread.");
			}
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
				this.debugMessages.Add("UDPReceive: Stopping Client.");
			}
		}
		// Constantly check for new messages on given port.
		private void ReceiveData(){
			// Open.
			this.client = new UdpClient(this.localPort);
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
						if ((!string.IsNullOrEmpty(receiveString)) && ((this.dataMessages.Count == 0) || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
							this.debugMessages.Add("UDPReceive: New Data: " + receiveString);
							this.dataMessages.Add(receiveString);
							this.flagDataRead = false;
						}
					}
				}
			} catch (SocketException exception) {
				// SocketException.
				this.debugMessages.Add("UDPReceive: SocketException: " + exception.ToString());
			} catch (Exception exception) {
				// Exception.
				this.debugMessages.Add("UDPReceive: Exception: " + exception.ToString());
			} finally {
				this.StopConnection();
			}
		}
		#endif
	}
}