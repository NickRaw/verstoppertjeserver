using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSocket
{
    public static class ClientSocketConnector
    {
        private static int socketport;
        private static IPAddress serverip;
        private static TcpClient server;
        private static NetworkStream stream;

        // The response from the remote device.
        private static String response = String.Empty;
        // Boolean to check if there is a full responcemessage
        private static bool fullMessageReceived = false;
        public static List<String> responseQueue = new List<string>();
        public static bool haveToListen = true;

        public static void PrepareConnector(string ipaddress, int portnumber)
        {
            server = new TcpClient(ipaddress, portnumber);
            stream = server.GetStream();
            StartListening();

        }

        public static async Task StartListening()
        {
            await Task.Run(() =>
            {
                StreamReader sr = new StreamReader(stream);
                while (haveToListen)
                {
                    string response = sr.ReadLine();
                    Console.WriteLine(response);
                }
            });
        }

        public static void StopListening()
        {
            haveToListen = false;
            stream.Close();
            server.Close();
        }

        public static void Send(string data)
        {
            int bytecount = Encoding.ASCII.GetByteCount(data + 1);
            byte[] sendData = new byte[bytecount];
            sendData = Encoding.ASCII.GetBytes(data);
            stream.Write(sendData, 0, sendData.Length);
        }

        

    }


}
