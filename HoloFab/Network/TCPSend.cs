// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Threading;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// TCP sender.
	public class TCPSend {
		// An IP and a port for TCP communication to send to.
		public string remoteIP;
		private int remotePort;
        
		public bool flagSuccess = false;
		public bool flagConnected;
        
		// Network Objects:
		#if WINDOWS_UWP
		private string sourceName = "TCP Send Interface UWP";
		#else
		private string sourceName = "TCP Send Interface";
		// Connection Object Reference.
		private TcpClient client;
		private NetworkStream stream;
		#endif
		// History:
		// - Debug History.
		public List<string> debugMessages = new List<string>();
		// Queuing:
		// Interface to keep checking queue on background and send it.
		ThreadInterface sender;
		// Queue of buffers to send.
		private Queue<byte[]> sendQueue = new Queue<byte[]>();
		// Accessor to check if there is data in queue
		public bool IsNotEmpty {
			get {
				return this.sendQueue.Count > 0;
			}
		}
        
		// Main Constructor
		public TCPSend(string _remoteIP, int _remotePort=11111) {
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			this.debugMessages = new List<string>();
			this.sender = new ThreadInterface();
			this.sender.threadAction = SendFromQueue;
			// Reset.
			Disconnect();
		}
		// Destructor.
		~TCPSend() {
			Disconnect();
		}
		////////////////////////////////////////////////////////////////////////
		// Queue Functions.
		// Start the thread to send data.
		private void StartSending(){
			// if queue not set create it.
			if (this.sendQueue == null)
				this.sendQueue = new Queue<byte[]>();
			// Start the thread.
			this.sender.Start();
		}
		// Disable Sending.
		public void StopSending() {
			// TODO: Should we reset queue?
			// Reset.
			this.sender.Stop();
		}
		// Enqueue data.
		public void QueueUpData(byte[] newData) {
			lock (this.sendQueue) {
				this.sendQueue.Enqueue(newData);
			}
		}
		// Check the queue and try send it.
		public void SendFromQueue() {
			try {
				if (this.IsNotEmpty) {
					byte[] currentData;
					lock (this.sendQueue) {
						currentData = this.sendQueue.Dequeue();
					}
					// Peek message to send
					Send(currentData);
					//// if no exception caught and data sent successfully - remove from queue.
					//if (!this.flagSuccess)
					//	lock (this.sendQueue) {
					//		this.sendQueue.Enqueue(currentData);
					//	}
				}
			} catch (Exception exception) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Queue Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagSuccess = false;
			}
		}
		#if WINDOWS_UWP
		////////////////////////////////////////////////////////////////////////
		// Establish Connection
		public async void Connect() {}
		// Start a connection and send given byte array.
		private async void Send(byte[] sendBuffer) {}
		// Stop Connection.
		public async void Disconnect() {}
		#else
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
					this.flagConnected = false;
					return false;
				}
				this.stream = this.client.GetStream();
				this.flagConnected = true;
				StartSending();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Connection Stablished!", ref this.debugMessages);
				#endif
				return true;
			} catch (ArgumentNullException exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "ArgumentNullException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagConnected = false;
				return false;
			} catch (SocketException exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagConnected = false;
				return false;
			} catch (Exception exception) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "UnhandledException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagConnected = false;
				return false;
			}
		}
		// Start a connection and send given byte array.
		private void Send(byte[] sendBuffer) {
			try {
				if (!this.client.Connected) {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Client DisflagConnected!", ref this.debugMessages);
					#endif
					this.flagSuccess = false;
				}
                
				// Write.
				this.stream.Write(sendBuffer, 0, sendBuffer.Length);
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (ArgumentNullException exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "ArgumentNullException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagSuccess = false;
			} catch (SocketException exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagSuccess = false;
			} catch (Exception exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.flagSuccess = false;
			}
			this.flagSuccess = false;
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
			StopSending();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "DisflagConnected.", ref this.debugMessages);
			#endif
		}
		#endif
	}
}