using System;

namespace HoloFab
{
    // Structure to hold Custom data types holding data to be sent.
    namespace CustomData
    {
        // A struct holding network info for other components.
        public struct Connection
        {
            public string remoteIP;
            public bool status;
            public TCPSend tcp;
            public Connection(string _remoteIP, bool _status, TCPSend _tcp)
            {
                this.remoteIP = _remoteIP;
                this.status = _status;
                this.tcp = _tcp;
            }
        }
    }
}