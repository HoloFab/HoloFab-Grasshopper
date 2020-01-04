using System;

namespace HoloFab {
    // Structure to hold Custom data types holding data to be sent.
    namespace CustomData {
        // A struct holding network info for other components.
        public struct Connection {
            public string remoteIP;
            public bool status;
            public Connection(string _remoteIP, bool _status) {
                this.remoteIP = _remoteIP;
                this.status = _status;
            }
        }
    }
}