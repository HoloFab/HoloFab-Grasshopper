using System.Net;

namespace HoloFab
{
    namespace CustomData
    {
        public struct HoloDevice
        {
            public string remoteIP;
            public string name;
            public HoloDevice(IPEndPoint ClientEp, string _name)
            {
                this.remoteIP = ClientEp.Address.ToString();
                this.name = _name;
                
            }
        }
    }
}