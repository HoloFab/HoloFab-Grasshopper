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

namespace HoloFab {
	// UDP receiver.
	// TODO:
	// - Change data received into an event to be raised and subscribed by listeners.
	public class UDPReceive {
		// Local Port
		private int localPort = 8055;
		// Force the messages depite history.
		public bool flagForce = false;
        
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
		private Thread receiveThread = null;
		#endif
		// History:
		// - debug
		public List<string> debugMessages = new List<string>();
		// - received data
		public List<string> dataMessages = new List<string>();
		// - addresses of incomiing connections (corresponding to data)
		public List<string> connectionHistory = new List<string>();
		// Flag to be raised on data recepcion.
		public bool flagDataRead = true;
        
		public Action OnReceive;
        
		// Constructor.
		public UDPReceive(int _localPort=8055){
			this.flagDataRead = true;
			this.localPort = _localPort;
			this.debugMessages = new List<string>();
			this.dataMessages = new List<string>();
			this.connectionHistory = new List<string>();
			Disconnect();
		}
		~UDPReceive(){
			Disconnect();
		}
        
		// Enable connection - if not yet open.
		public void Connect() {
			#if WINDOWS_UWP
			StartReceiving();
			#else
			// Create a new thread to receive incoming messages.
			if (this.receiveThread == null)
				StartReceiving();
			#endif
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
				if ((this.dataMessages.Count == 0) ||
				    (this.flagForce || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
					this.dataMessages.Add(receiveString);
					this.connectionHistory.Add(args.RemoteAddress.RawName);
					this.flagDataRead = false;
					if (OnReceive != null)
						OnReceive();
				} else {
					#if DEBUG2
					DebugUtilities.UniversalDebug(this.sourceName, "Message already added.", ref this.debugMessages);
					#endif
				}
			}
		}
		//////////////////////////////////////////////////////////////////////////
		#else
		private void StartReceiving(){
			// Start the thread.
			this.receiveThread = new Thread(new ThreadStart(ReceiveData));
			this.receiveThread.IsBackground = true;
			this.receiveThread.Start();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Client receivng thread Started.", ref this.debugMessages);
			#endif
		}
		// Disable connection.
		public void Disconnect() {
			// Reset.
			if (this.receiveThread != null) {
				this.receiveThread.Abort();
				this.receiveThread = null; // Good Practice?
				this.debugMessages.Add("UDPReceive: Stopping Thread.");
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Connection Reception Thread.", ref this.debugMessages);
				#endif
			}
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Client.", ref this.debugMessages);
				#endif
			}
		}
		// Constantly check for new messages on given port.
		private void ReceiveData(){
			// Open.
			this.client = new UdpClient(this.localPort);
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
			// Infinite loop.
			byte[] data;
			string receiveString;
	        while (true) {
                try {
				    // Receive Bytes.
				    data = client.Receive(ref anyIP);
				    if (data.Length > 0) {
					    // If buffer not empty - decode it.
					    receiveString = EncodeUtilities.DecodeData(data);
					    // If string not empty and not read yet - react to it.
					    if (!string.IsNullOrEmpty(receiveString)) {
						    #if DEBUG2
						    DebugUtilities.UniversalDebug(this.sourceName, "Total Data found: " + receiveString, ref this.debugMessages);
						    #endif
						    if ((this.dataMessages.Count == 0) ||
							    (this.flagForce || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
							    this.dataMessages.Add(receiveString);
							    this.connectionHistory.Add(anyIP.Address.ToString());
							    this.flagDataRead = false;
							    if (OnReceive != null)
								    OnReceive();
						    } else {
							    #if DEBUG2
							    DebugUtilities.UniversalDebug(this.sourceName, "Message already added.", ref this.debugMessages);
							    #endif
						    }
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
            //this.Disconnect();
        }
		#endif
	}
}