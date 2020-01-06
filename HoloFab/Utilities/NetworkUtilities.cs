using System;

#if WINDOWS_UWP
using Windows.Networking.Connectivity;
using System.Linq;
#else
using System.Net;
using System.Net.Sockets;
#endif

namespace HoloFab {
	// Tools for working with netwrok.
	public static class NetworkUtilities {
		// Get Local IP address.
		public static string LocalIPAddress() {
			string localIP = "0.0.0.0";
			#if WINDOWS_UWP
			ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
			localIP = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
			                                                            hn.IPInformation?.NetworkAdapter != null &&
			                                                            (hn.IPInformation.NetworkAdapter.NetworkAdapterId == connectionProfile.NetworkAdapter.NetworkAdapterId)).ToString();
			#else
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					localIP = ip.ToString();
					break;
				}
			}
			#endif
			return localIP;
		}
	}
}