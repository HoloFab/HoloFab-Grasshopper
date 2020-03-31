using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;
using HoloFab.CustomData;


namespace HoloFab
{
    public static class FindServer
    {

        // Thread Object Reference.
        private static Thread receiveThread = null;
        // Client List
        public static List<string> clients = new List<string>();
        public static List<HoloDevice> devices = new List<HoloDevice>();

        private static void discoverClients()
        {
            var Server = new UdpClient(8888);
            var ResponseData = Encoding.ASCII.GetBytes("HolloWorld");

            while (true)
            {
                IPEndPoint ClientEp = new IPEndPoint(IPAddress.Any, 0);
                var ClientRequestData = Server.Receive(ref ClientEp);
                string ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

                Console.WriteLine("Recived {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                if (!FindServer.clients.Contains(ClientEp.Address.ToString()))
                {
                    FindServer.clients.Add(ClientEp.Address.ToString());
                    FindServer.devices.Add(new HoloDevice(ClientEp, ClientRequest));
                }
            }
        }

        public static void StartScanning()
        {
            if (FindServer.receiveThread == null || !FindServer.receiveThread.IsAlive)
            {
                // Reset.
                // Start the thread.
                FindServer.receiveThread = new Thread(new ThreadStart(discoverClients));
                FindServer.receiveThread.IsBackground = true;
                FindServer.receiveThread.Start();
            }
        }
    }
}