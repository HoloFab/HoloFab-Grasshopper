using System.Net;
using System;

namespace HoloFab {
	namespace CustomData {
		// Structure to hold Device info
		public class HoloDevice {
			public string remoteIP;
			public string name;
			public DateTime lastCall { get; set; }
            
			public HoloDevice(string _address, string _name) {
				this.remoteIP = _address;
				this.name = _name;
				this.lastCall = DateTime.Now;
			}
			// Encode information into String.
			public override string ToString(){
				return this.name + "(" + this.remoteIP + ")";
			}
		}
	}
}