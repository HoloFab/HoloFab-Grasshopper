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

namespace HoloFab
{
    public class FindServer
    {

        // Thread Object Reference.
        private Thread receiveThread = null;
        // Client List
        public List<string> clients = new List<string>();
        public List<HoloDevice> devices = new List<HoloDevice>();

        public FindServer()
        {
        }

        private void discoverClients()
        {
            IPEndPoint ClientEp = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var Server = new UdpClient(8888);
                Server.Client.ReceiveTimeout = 700;
                try
                {
                    var ClientRequestData = Server.Receive(ref ClientEp);
                    string ClientRequest = Encoding.ASCII.GetString(ClientRequestData);
                    string deviceIP = ClientEp.Address.ToString();
                    
                    if (!devices.Exists(d => d.remoteIP == deviceIP))
                    {
                        devices.Add(new HoloDevice(deviceIP, ClientRequest));
                        Grasshopper.Instances.InvalidateCanvas();
                    }
                    else
                    {
                        devices.Find(d => d.remoteIP == deviceIP).lastCall = DateTime.Now;
                    }
                }
                catch
                {
                }
                finally
                {
                    Server.Close();
                }
                if (devices.RemoveAll(d => DateTime.Now - d.lastCall > TimeSpan.FromSeconds(4)) > 0) Grasshopper.Instances.InvalidateCanvas(); ;
            }
        }

        public void StartScanning()
        {
            if (receiveThread == null || !receiveThread.IsAlive)
            {
                // Reset.
                // Start the thread.
                receiveThread = new Thread(new ThreadStart(discoverClients));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
        }
    }
}