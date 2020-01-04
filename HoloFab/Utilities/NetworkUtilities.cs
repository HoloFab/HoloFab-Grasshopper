using System;
using System.Net;
using System.Net.Sockets;

namespace HoloFab {
	// Tools for working with netwrok.
	public static class NetworkUtilities {
		// Get Local IP address.
		public static string LocalIPAddress() {
			string localIP = "0.0.0.0";
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					localIP = ip.ToString();
					break;
				}
			}
			return localIP;
		}
	}
}