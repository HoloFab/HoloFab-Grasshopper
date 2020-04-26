#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Threading;

using HoloFab;

namespace HoloFab {
	// TCP sender.
	public class TCPSend {
		// An IP and a port for TCP communication to send to.
		private string remoteIP;
		private int remotePort;
        
		public bool flagSuccess = false;
        
		private TcpClient client;
		private NetworkStream stream;
		public List<string> debugMessages = new List<string>();
		public bool connected;
		private string sourceName = "TCP Send Interface";

		// Main Constructor
		public TCPSend(string _remoteIP, int _remotePort=11111) {
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			// Reset.
			Disconnect();
			this.debugMessages = new List<string>();
		}
		// Destructor.
		~TCPSend() {
			Disconnect();
		}
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
					this.connected = false;
					return false;
				}
				this.stream = this.client.GetStream();
				this.connected = true;
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
				this.connected = false;
				return false;
			} catch (SocketException exception) {
				// Exception.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.connected = false;
				return false;
			} catch (Exception exception) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "UnhandledException: " + exception.ToString(), ref this.debugMessages);
				#endif
				this.connected = false;
				return false;
			}
		}
		////// Start a connection and send given byte array.
		private void Send(byte[] sendBuffer) {
			try {
				if (!this.client.Connected) {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Client Disconnected!", ref this.debugMessages);
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
			DebugUtilities.UniversalDebug(this.sourceName, "Disconnected.", ref this.debugMessages);
			#endif
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
	}
}