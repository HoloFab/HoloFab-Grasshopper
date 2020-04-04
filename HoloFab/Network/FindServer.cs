using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;

using HoloFab.CustomData;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace HoloFab {
	public class FindServer {
		// Network Settings
		private int localPort = 8888;
		// Thread Object Reference.
		private Thread receiveThread = null;
		// Client List
		public Dictionary<string, HoloDevice> devices = new Dictionary<string, HoloDevice>();
		private int expireDelay = 700;
		private int expireDeivceDelay = 4000;
        
		// A Function to start searching for devices
		public void StartScanning() {
			if (this.receiveThread == null || !this.receiveThread.IsAlive) {
				// Start the thread.
				this.receiveThread = new Thread(new ThreadStart(DiscoverClients));
				this.receiveThread.IsBackground = true;
				this.receiveThread.Start();
			}
		}
		// An infinite loop looking for incoming connections and noting or expiring old ones.
		private void DiscoverClients() {
			// Start continuously looking for devices requesting in the network.
			IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
			UdpClient server;
			string clientAddress, clientRequest;
			byte[] clientRequestData;
			while (true) {
				server = new UdpClient(this.localPort);
				server.Client.ReceiveTimeout = this.expireDelay;
                
				try {
					clientAddress = clientEP.Address.ToString();
					clientRequestData = server.Receive(ref clientEP);
					clientRequest = Encoding.ASCII.GetString(clientRequestData);
                    
					Console.WriteLine("Recived {0} from {1}, sending response", clientRequest, clientAddress);
					if (!this.devices.ContainsKey(clientAddress)) {
						this.devices.Add(clientAddress, new HoloDevice(clientEP, clientRequest));
						Grasshopper.Instances.InvalidateCanvas();
					} else {
						this.devices[clientAddress].lastCall = DateTime.Now;
					}
				} catch {} finally {
					server.Close();
				}
				if (this.devices.RemoveAll(device =>
				                           DateTime.Now - device.lastCall > TimeSpan.FromMilliseconds(this.expireDeivceDelay)) > 0)
					Grasshopper.Instances.InvalidateCanvas();
			}
		}
	}
}