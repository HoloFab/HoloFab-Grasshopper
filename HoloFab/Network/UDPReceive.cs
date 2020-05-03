// #define DEBUG
// #define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

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

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// UDP receiver.
	// TODO:
	// - Change data received into an event to be raised and subscribed by listeners.
	public class UDPReceive {
		// Local Port
		private int localPort = 8055;
        
		// Network Objects:
		#if WINDOWS_UWP
		private string sourceName = "UDP Receive Interface UWP";
		// Connection Object Reference.
		private DatagramSocket client;
		#else
		private string sourceName = "UDP Receive Interface";
		// Connection Object Reference.
		private UdpClient client;
		// Thread Object Reference.
		private ThreadInterface receiver;
		#endif
		// History:
		// - debug
		public List<string> debugMessages = new List<string>();
		// - received data
		public Queue<string> dataMessages = new Queue<string>();
		//public List<string> dataMessages = new List<string>();
		// - addresses of incomiing connections (corresponding to data)
		public Queue<string> connectionHistory = new Queue<string>();
		//public List<string> connectionHistory = new List<string>();
        
		public Action OnReceive;
        
		// Constructor.
		public UDPReceive(int _localPort=8055){
			this.localPort = _localPort;
			this.debugMessages = new List<string>();
			#if !WINDOWS_UWP
			this.receiver = new ThreadInterface();
			this.receiver.threadAction = ReceiveData;
			#endif
			this.dataMessages = new Queue<string>();
			this.connectionHistory = new Queue<string>();
			//this.dataMessages = new List<string>();
			//this.connectionHistory = new List<string>();
			Disconnect();
		}
		~UDPReceive(){
			Disconnect();
		}
        
		//////////////////////////////////////////////////////////////////////////
		#if WINDOWS_UWP
		private async void StartReceiving(){
			// Reset.
			// Start receiving.
			this.client = new DatagramSocket();
			this.client.MessageReceived += ReceiveData;
			try {
				await client.BindEndpointAsync(new HostName(NetworkUtilities.LocalIPAddress()), this.localPort.ToString());
			} catch (Exception exception) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString() + ":" + SocketError.GetStatus(exception.HResult).ToString(), ref this.debugMessages);
				#endif
			}
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Client receivng thread Started.", ref this.debugMessages);
			#endif
		}
		// Enable Connection
		public void Connect() {
			StartReceiving();
		}
		// Disable connection.
		public async void Disconnect() {
			// Reset.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Client.", ref this.debugMessages);
				#endif
			}
		}
		// Constantly check for new messages on given port.
		private async void ReceiveData(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args){
			//Read the message that was received from the UDP client.
			DataReader reader = args.GetDataReader();
			string receiveString = reader.ReadString(reader.UnconsumedBufferLength).Trim();
			// If string not empty and not read yet - react to it.
			if (!string.IsNullOrEmpty(receiveString)) {
				#if DEBUG2
				DebugUtilities.UniversalDebug(this.sourceName, "Total Data found: " + receiveString, ref this.debugMessages);
				#endif
				this.dataMessages.Enqueue(receiveString);
				this.connectionHistory.Enqueue(args.RemoteAddress.RawName);
				if (OnReceive != null)
					OnReceive();
			}
		}
		//////////////////////////////////////////////////////////////////////////
		#else
		// Enable Connection
		public void Connect(){
			Disconnect();
			this.client = new UdpClient(this.localPort);
			// Start the thread.
			this.receiver.Start();
		}
		// Disable connection.
		public void Disconnect() {
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Client.", ref this.debugMessages);
				#endif
			}
			// Reset.
			this.receiver.Stop();
		}
		// Constantly check for new messages on given port.
		private void ReceiveData() {
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
			byte[] data;
			string receiveString;
			try {
				// Receive Bytes.
				data = this.client.Receive(ref anyIP);
				if (data.Length > 0) {
					// If buffer not empty - decode it.
					receiveString = EncodeUtilities.DecodeData(data);
					// If string not empty and not read yet - react to it.
					if (!string.IsNullOrEmpty(receiveString)) {
						#if DEBUG2
						DebugUtilities.UniversalDebug(this.sourceName, "Total Data found: " + receiveString, ref this.debugMessages);
						#endif
						this.dataMessages.Enqueue(receiveString);
						this.connectionHistory.Enqueue(anyIP.Address.ToString());
						if (OnReceive != null)
							OnReceive();
					}
				}
			} catch (SocketException exception) {
				// SocketException.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		#endif
	}
}