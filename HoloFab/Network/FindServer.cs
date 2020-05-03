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

		private static ThreadInterface deviceUpdater;

		public FindServer() {
			try {
				FindServer.receiver = new UDPReceive(this.localPortOverride);
				FindServer.receiver.OnReceive = OnDeviceReceived;
				FindServer.deviceUpdater = new ThreadInterface();
				FindServer.deviceUpdater.threadAction = UpdateDevices;
			} catch {}
		}
		~FindServer() { 
			if (FindServer.receiver != null)
				FindServer.receiver.Disconnect();
			if (FindServer.deviceUpdater != null)
				FindServer.deviceUpdater.Stop();
		}
        
		// A Function to start searching for devices
		public void StartScanning() {
			if (FindServer.receiver != null)
				FindServer.receiver.Connect();
			if (FindServer.deviceUpdater != null)
				FindServer.deviceUpdater.Start();
		}

		private void OnDeviceReceived() {
			string clientAddress, clientRequest;

			clientAddress = FindServer.receiver.connectionHistory.Dequeue();// connectionHistory[FindServer.receiver.connectionHistory.Count - 1].ToString();
			clientRequest = FindServer.receiver.dataMessages.Dequeue();// [FindServer.receiver.dataMessages.Count - 1];

			Console.WriteLine("Recived {0} from {1}, sending response", clientRequest, clientAddress);
			if (!this.devices.ContainsKey(clientAddress))
				this.devices.Add(clientAddress, new HoloDevice(clientAddress, clientRequest));
			else
				this.devices[clientAddress].lastCall = DateTime.Now;
			RefreshList();
		}
		private void UpdateDevices() {
			RefreshList();
			Thread.Sleep(1000); // Don't update continuously
		}
		private void RefreshList(){
			bool flagUpdate = false;
			// Check if any of devices have to be excluded.
			List<string> removeList = new List<string>();
			lock (this.devices){
				try { 
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
				catch { }
			}
		}
	}
}