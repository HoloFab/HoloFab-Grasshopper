#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
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
	// UDP sender.
	public class UDPSend {
		// An IP and a port for UDP communication to send to.
		private string remoteIP;
		private int remotePort;
        
		public bool flagSuccess = false;
        
		// Network Objects:
		#if WINDOWS_UWP
		private string sourceName = "UDP Send Interface UWP";
		// Connection Object Reference.
		private DatagramSocket client;
		private string broadcastAddress = "255.255.255.255";
		#else
		private string sourceName = "UDP Send Interface";
		// Connection Object Reference.
		private UdpClient client;
		#endif
		// History:
		// - Debug History.
		public List<string> debugMessages = new List<string>();
        
		// Constructor.
		public UDPSend(string _remoteIP, int _remotePort=12121){
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			this.debugMessages = new List<string>();
		}
		~UDPSend() {
			Disconnect();
		}

		public void Connect() {
			StartSending();
		}
		public void Disconnect() {
			StopSending();
		}
        
		#if WINDOWS_UWP
		// Start a connection and send given byte array.
		public async void Send(byte[] sendBuffer) {
			this.flagSuccess = false;
			// Stop client if set previously.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open new one.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(this.remoteIP),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				this.client = null; // Good Practice?
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Broadcast Message to everyone.
		public void Broadcast(byte[] sendBuffer) {
			// Reset.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(UDPSend.broadcastAddress),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Broadcast Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		#else
		// Start a connection and send given byte array.
		public void Send(byte[] sendBuffer) {
			this.flagSuccess = false;
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new UdpClient(this.remoteIP, this.remotePort);
				// Write.
				this.client.Send(sendBuffer, sendBuffer.Length);
				// Close.
				this.client.Close();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Broadcast Message to everyone.
		public void Broadcast(byte[] sendBuffer) {
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new UdpClient(new IPEndPoint(IPAddress.Broadcast, this.remotePort));
				// Write.
				this.client.Send(sendBuffer, sendBuffer.Length);
				// Close.
				this.client.Close();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Broadcast Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		////////////////////////////////////////////////////////////////////////////////////////////
		// Queue behavior.
		private Thread sendThread;
		// Queue of buffers to send.
		private Queue<byte[]> sendQueue = new Queue<byte[]>();
		// Accessor to check if there is data in queue
		public bool IsNotEmpty {
			get {
				return this.sendQueue.Count > 0;
			}
		}
		// Enqueue data.
		public void QueueUpData(byte[] newData) {
			lock (this.sendQueue) { 
				this.sendQueue.Enqueue(newData);
			}
		}
		// Infinite Loop to continuously check the loop and try send it.
		public void SendLoop() {
			while (true) {
				try {
					if (this.IsNotEmpty) {
						lock (this.sendQueue) {
							// Peek message to send
							Send(this.sendQueue.Dequeue());
							//// If no exception caught and data sent successfully - remove from queue.
							//if (this.flagSuccess)
							//	this.sendQueue.Dequeue();
						}
					}
				} catch (Exception exception) { 
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Queue Exception: " + exception.ToString(), ref this.debugMessages);
					#endif
					this.flagSuccess = false;
				}
			}
		}
		private void StartSending(){
			// if queue not set create it.
			if (this.sendQueue == null)
				this.sendQueue = new Queue<byte[]>();
			// Start the thread.
			this.sendThread = new Thread(new ThreadStart(SendLoop));
			this.sendThread.IsBackground = true;
			this.sendThread.Start();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Queue Started.", ref this.debugMessages);
			#endif
		}
		// Disable connection.
		public void StopSending() {
			// TODO: Should we reset queue?
			// Reset.
			if (this.sendThread != null) {
				this.sendThread.Abort();
				this.sendThread = null; // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Thread.", ref this.debugMessages);
				#endif
			}
		}
		#endif
	}
}