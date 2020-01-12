using System;
using System.Collections.Generic;

using System.Net.Sockets;

using HoloFab;

namespace HoloFab
{
    // TCP sender.
    public class TCPSend
    {
        private TcpClient client;
        private NetworkStream stream;
        private static int remotePort = 11111;
        public static List<string> debugMessages = new List<string>();
        public bool connected;

        public TCPSend()
        {
            // Reset.
            TCPSend.debugMessages = new List<string>();
            if (this.client != null)
            {
                this.client.Close();
            }
            if (this.stream != null)
            {
                this.stream.Close();
            }
        }

        public bool connect(string remoteIP)
        {
            // Reset.
            if (this.client != null)
            {
                this.client.Close();
            }
            if (this.stream != null)
            {
                this.stream.Close();
            }
            this.client = new TcpClient();
            try
            {
                // Open.
                if (!this.client.ConnectAsync(remoteIP, TCPSend.remotePort).Wait(2000))
                {
                    // connection failure
                    this.connected = false;
                    return false;
                }
                this.stream = this.client.GetStream();
                this.connected = true;
                // Acknowledge.
                TCPSend.debugMessages.Add("TCPSend: Connection Stablished!");
                return true;
            }
            catch (ArgumentNullException exception)
            {
                // Exception.
                TCPSend.debugMessages.Add("TCPSend: ArgumentNullException: " + exception.ToString());
                this.connected = false;
                return false;
            }
            catch (SocketException exception)
            {
                // Exception.
                TCPSend.debugMessages.Add("TCPSend: SocketException: " + exception.ToString());
                this.connected = false;
                return false;
            }
            catch (Exception e)
            {
                TCPSend.debugMessages.Add("TCPSend: UnhandledException: " + e.ToString());
                this.connected = false;
                return false;
            }
        }

        ~TCPSend()
        {
            // Reset.
            if (this.client != null)
            {
                this.client.Close();
            }
            if (this.stream != null)
            {
                this.stream.Close();
            }
        }

        // Start a connection and send given byte array.
        public string Send(byte[] sendBuffer)
        {
            try
            {
                if (!this.client.Connected)
                {
                    return "Client Disconnected!";
                }

                // Write.
                this.stream.Write(sendBuffer, 0, sendBuffer.Length);
                // Acknowledge.
                TCPSend.debugMessages.Add("TCPSend: Data Sent!");
                
                return "Sent";
            }
            catch (ArgumentNullException exception)
            {
                // Exception.
                TCPSend.debugMessages.Add("TCPSend: ArgumentNullException: " + exception.ToString());
                return exception.ToString();
            }
            catch (SocketException exception)
            {
                // Exception.
                TCPSend.debugMessages.Add("TCPSend: SocketException: " + exception.ToString());
                return exception.ToString();
            }
        }

        public void disconnect()
        {
            // Reset.
            if (this.client != null)
            {
                this.client.Close();
            }
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null; // Good Practice?
            }
        }
    }
}