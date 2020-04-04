using System.Net;

namespace HoloFab {
	namespace CustomData {
		// Structure to hold Device info
		public class HoloDevice {
			public string remoteIP;
			public string name;
            
			public HoloDevice(IPEndPoint _clientEP, string _name) {
				this.remoteIP = _clientEP.Address.ToString();
				this.name = _name;
			}
			// Encode information into String.
			public string ToString(){
				return this.name + "(" + this.remoteIP + ")";
			}
		}
	}
}