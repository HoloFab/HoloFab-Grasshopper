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
		private int localPortOverride = 8888;
		// Thread Object Reference.
		private static UDPReceive receiver;
		// Client List
		public Dictionary<string, HoloDevice> devices = new Dictionary<string, HoloDevice>();
		//private int expireDelay = 700;
		private int expireDeivceDelay = 4000;
        
		public FindServer() {
			try {
				FindServer.receiver = new UDPReceive(this.localPortOverride);
				FindServer.receiver.OnReceive = UpdateDevices;
			} catch {}
		}
        
		// A Function to start searching for devices
		public void StartScanning() {
			if (FindServer.receiver != null)
				FindServer.receiver.Connect();
		}
        
		public void UpdateDevices() {
			string clientAddress, clientRequest;
            
			clientAddress = FindServer.receiver.connectionHistory[FindServer.receiver.connectionHistory.Count-1].ToString();
			clientRequest = FindServer.receiver.dataMessages[FindServer.receiver.dataMessages.Count-1];
            
			Console.WriteLine("Recived {0} from {1}, sending response", clientRequest, clientAddress);
			if (!this.devices.ContainsKey(clientAddress))
				this.devices.Add(clientAddress, new HoloDevice(clientAddress, clientRequest));
			else
				this.devices[clientAddress].lastCall = DateTime.Now;
			RefreshList();
		}
		public void RefreshList(){
			bool flagUpdate = false;
			// Check if any of devices have to be excluded.
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, HoloDevice> item in this.devices)
				if (DateTime.Now - item.Value.lastCall > TimeSpan.FromMilliseconds(this.expireDeivceDelay)) {
					removeList.Add(item.Key);
					flagUpdate = true;
				}
			// Check if solution need to update.
			if (flagUpdate) {
				for (int i = 0; i < removeList.Count; i++)
					this.devices.Remove(removeList[i]);
				Grasshopper.Instances.InvalidateCanvas();
			}
		}
	}
}