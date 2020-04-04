using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;

using HoloFab.CustomData;

namespace HoloFab {
	public static class FindServer {
		// Network Settings
		private static int localPort = 8888;
		// Thread Object Reference.
		private static Thread receiveThread = null;
		// Client List
		public static Dictionary<string, HoloDevice> devices = new Dictionary<string, HoloDevice>();
        
        
		public static void StartScanning() {
			if (FindServer.receiveThread == null || !FindServer.receiveThread.IsAlive) {
				// Start the thread.
				FindServer.receiveThread = new Thread(new ThreadStart(DiscoverClients));
				FindServer.receiveThread.IsBackground = true;
				FindServer.receiveThread.Start();
			}
		}
        
		private static void DiscoverClients() {
			UdpClient server = new UdpClient(FindServer.localPort);
			// byte[] responseData = Encoding.ASCII.GetBytes("HolloWorld");
            
			// Start continuously looking for devices requesting in the network.
			IPEndPoint clientEP;
			string clientAddress, clientRequest;
			byte[] clientRequestData;
			while (true) {
				clientEP = new IPEndPoint(IPAddress.Any, 0);
				clientAddress = clientEP.Address.ToString();
				clientRequestData = server.Receive(ref clientEP);
				clientRequest = Encoding.ASCII.GetString(clientRequestData);
                
				Console.WriteLine("Recived {0} from {1}, sending response", clientRequest, clientAddress);
				if (!FindServer.devices.ContainsKey(clientAddress)) {
					FindServer.devices.Add(clientAddress, new HoloDevice(clientEP, clientRequest));
				}
			}
		}
	}
}