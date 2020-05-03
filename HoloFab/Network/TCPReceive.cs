// #define DEBUG
// #define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using UnityEngine;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloFab {
	public class TCPReceive {
		#if WINDOWS_UWP
		private string sourceName = "TCP Receive Interface UWP";
		// Connection Object References.
		private StreamSocketListener listener;
		// Task Object Reference.
		private CancellationTokenSource connectionCancellationTokenSource;
		private Task connectionReceiver;
		private DataReader reader;
		#else
		private string sourceName = "TCP Receive Interface";
		// Connection Object References.
		public TcpListener listener;
		public TcpClient client;
		public NetworkStream stream;
		// Thread Object Reference.
		private Thread connectionReceiver = null;
		#endif
        
		// Local Variables.
		// Local Port
		private int localPort;
		// Size of buffer to try to read at once.
		private uint bufferSize = 8096;
		// Local buffer
		private string currentHistory = string.Empty;
		// Flag raised when server is found
		public bool flagConnectionFound;
		// History:
		// - debug
		public List<string> debugMessages = new List<string>();
		// - received data
		public Queue<string> dataMessages = new Queue<string>();
        
		public TCPReceive(int _localPort=11111) {
			this.localPort = _localPort;
			this.debugMessages = new List<string>();
			this.dataMessages = new Queue<string>();
			this.flagConnectionFound = false;
			Disconnect();
		}
		~TCPReceive(){
			Disconnect();
		}
        
		// Enable connection - if not yet open.
		public void Connect() {
			// Create a new thread to receive incoming messages.
			if (this.connectionReceiver == null)
				StartListenning(this.localPort);
		}
		// When splitter is found - react to it.
		private void ReactToMessage(){
			string[] messages = this.currentHistory.Split(new string[] { EncodeUtilities.messageSplitter },
			                                              StringSplitOptions.RemoveEmptyEntries);
			int index = (messages.Length > 1) ? messages.Length-2 : 0;
			string receiveString = messages[index];
			this.currentHistory = (messages.Length > 1) ? messages[index+1] : string.Empty;
			if (!string.IsNullOrEmpty(receiveString)) {
				#if DEBUG2
				DebugUtilities.UniversalDebug(this.sourceName, "Total Data found: " + receiveString, ref this.debugMessages);
				#endif
				this.dataMessages.Enqueue(receiveString);
			}
		}
		//////////////////////////////////////////////////////////////////////////
		#if WINDOWS_UWP
		private async void StartListenning(int _localPort){
			if (this.localPort != _localPort)
				this.localPort = _localPort;
			// Start the thread.
			this.connectionCancellationTokenSource = new CancellationTokenSource();
			this.connectionReceiver = new Task(() => ReceiveConnection(), this.connectionCancellationTokenSource.Token);
			this.connectionReceiver.Start();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Client receivng thread Started.", ref this.debugMessages);
			#endif
		}
		// Disable connection.
		public void Disconnect() {
			// Reset.
			if (this.connectionReceiver != null) {
				this.connectionCancellationTokenSource.Cancel();
				this.connectionReceiver.Wait(1);
				this.connectionCancellationTokenSource.Dispose();
				this.connectionReceiver = null;     // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Connection Reception Task.", ref this.debugMessages);
				#endif
			}
			if (this.listener != null) {
				this.listener.Dispose();
				this.listener = null;     // Good Practice?
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Listener.", ref this.debugMessages);
				#endif
			}
			if (this.reader != null) {
				this.reader.DetachStream();
				this.reader = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Reader.", ref this.debugMessages);
				#endif
			}
		}
		// Constantly check for new messages on given port.
		private async void ReceiveConnection(){
			try {
				// Open.
				this.listener = new StreamSocketListener();
				this.listener.ConnectionReceived += OnClientFound;
				await this.listener.BindServiceNameAsync(this.localPort.ToString());
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Started Listening for Incoming Connections.", ref this.debugMessages);
				#endif
			} catch (Exception exception) {
				// Exception.
				SocketErrorStatus webErrorStatus = SocketError.GetStatus(exception.GetBaseException().HResult);
				string webError = (webErrorStatus.ToString() != "Unknown") ? webErrorStatus.ToString() :
				                                                             exception.Message;
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + webError, ref this.debugMessages);
				#endif
			}
		}
		private async void OnClientFound(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args){
			this.flagConnectionFound = true;
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "New Client Found!", ref this.debugMessages);
			#endif
			uint dataLengthToRead;
            
			this.reader = new DataReader(args.Socket.InputStream);
			this.reader.InputStreamOptions = InputStreamOptions.Partial;
			this.reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Starting infinite loop reading data.", ref this.debugMessages);
			#endif
			try {
				while (true) {
					// Try to read a byte - if timed out - will raise an exceptiion.
					// CancellationTokenSource timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
					// uint temp = await this.reader.LoadAsync(1).AsTask(timeoutSource.Token);
                    
					dataLengthToRead = await this.reader.LoadAsync(this.bufferSize);
					if ((dataLengthToRead != 0) && (!this.currentHistory.Contains(EncodeUtilities.messageSplitter))) {
						#if DEBUG2
						DebugUtilities.UniversalDebug(this.sourceName, "Data found in stream.", ref this.debugMessages);
						#endif
						this.currentHistory += this.reader.ReadString(dataLengthToRead);
						// Check if there is more data in stream.
						dataLengthToRead = await this.reader.LoadAsync(bufferSize);
					}
					// if found a message splitter - process first and remove from history.
					if (this.currentHistory.Contains(EncodeUtilities.messageSplitter))
						ReactToMessage();
					// await Task.Delay(TimeSpan.FromMilliseconds(100));
					// DebugUniversal("Keeping going . . .");
				}
			} catch(TaskCanceledException) {
				// timeout
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Connection timed out!", ref this.debugMessages);
				#endif
			} catch(Exception exception) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Receiving Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
			// TODO: Shouldn't it close in case of error?
			// finally {
			// 	this.flagConnectionFound = false;
			// 	this.reader.DetachStream();
			// }
		}
		//////////////////////////////////////////////////////////////////////////
		#else
		private void StartListenning(int _localPort){
			// Start the thread.
			this.connectionReceiver = new Thread(new ThreadStart(this.ReceiveData));
			this.connectionReceiver.IsBackground = true;
			this.connectionReceiver.Start();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Thread Started.", ref this.debugMessages);
			#endif
		}
		// Disable connection.
		public void Disconnect() {
			// Reset.
			if (this.listener != null) {
				this.listener.Stop();
				this.listener = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Listener.", ref this.debugMessages);
				#endif
			}
			if (this.client != null) {
				this.client.Close();
				this.client = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Client.", ref this.debugMessages);
				#endif
			}
			if (this.connectionReceiver != null) {
				this.connectionReceiver.Abort();
				this.connectionReceiver = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Thread.", ref this.debugMessages);
				#endif
			}
		}
		// Constantly check for new messages on given port.
		private void ReceiveData(){
			try {
				// Open.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, this.localPort);
				this.listener = new TcpListener(anyIP);
				this.listener.Start();
                
				this.currentHistory = "";
				// Infinite loop.
				while (true) {
					if (this.client == null) {
						#if DEBUG
						DebugUtilities.UniversalDebug(this.sourceName, "Listening for a Client!", ref this.debugMessages);
						#endif
						this.client = this.listener.AcceptTcpClient();
						this.stream = this.client.GetStream();
					} else {
						try {
							if (this.stream.DataAvailable) {
								OnClientFound();
								#if DEBUG
								DebugUtilities.UniversalDebug(this.sourceName, "Reading Data: " + this.client.Available.ToString(), ref this.debugMessages);
								#endif
							} else {
								if (this.IsConnected) {
									#if DEBUGWARNING
									DebugUtilities.UniversalWarning(this.sourceName, "No Data Available!", ref this.debugMessages);
									#endif
								} else {
									#if DEBUGWARNING
									DebugUtilities.UniversalWarning(this.sourceName, "Client Disconnected", ref this.debugMessages);
									#endif
									this.stream.Close();
									this.client.Close();
									this.client = null;
								}
							}
                            
                            
						} catch (Exception e) {
							this.stream.Close();
							this.client.Close();
							this.client = null;
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
			// TODO: Shouldn't it close in case of error?
			// finally {
			// 	this.Disconnect();
			// }
		}
		private void OnClientFound(){
			this.flagConnectionFound = true;
			byte[] buffer = new byte[bufferSize];
            
			// Receive Bytes.
			int dataLengthToRead = this.stream.Read(buffer, 0, buffer.Length);
			while ((dataLengthToRead != 0) && (!this.currentHistory.Contains(EncodeUtilities.messageSplitter))) {
				this.currentHistory += EncodeUtilities.DecodeData(buffer, 0, dataLengthToRead);
				dataLengthToRead = this.stream.Read(buffer, 0, buffer.Length);
			}
			if (this.currentHistory.Contains(EncodeUtilities.messageSplitter))
				ReactToMessage();
		}
		// Check if Server Disconnected.
		public bool IsConnected {
			get {
				try {
					if (this.client != null && this.client.Client != null && this.client.Client.Connected) {
						/* pear to the documentation on Poll:
						 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
						 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
						 * -or- true if data is available for reading;
						 * -or- true if the connection has been closed, reset, or terminated;
						 * otherwise, returns false
						 */
                        
						// Detect if client disconnected
						if (this.client.Client.Poll(1000, SelectMode.SelectRead)) {
							byte[] buff = new byte[1];
							if (this.client.Client.Receive(buff, SocketFlags.Peek) == 0) {
								// Client disconnected
								return false;
							} else {
								return true;
							}
						}
						return true;
					} else {
						return false;
					}
				} catch {
					return false;
				}
			}
		}
		#endif
	}
}