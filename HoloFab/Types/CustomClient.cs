using System.Net;
using System;

namespace HoloFab
{
    namespace CustomData
    {
        public class HoloDevice
        {
            public string remoteIP;
            public string name;
            public DateTime lastCall { get; set; }
            public HoloDevice(string ClientEp, string name)
            {
                remoteIP = ClientEp;
                this.name = name;
                lastCall = DateTime.Now;
            }
        }
    }
}