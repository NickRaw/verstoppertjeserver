using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSocket
{
    public class ClientSocketConnector
    {
        private int socketport;
        private IPAddress serverip;
        private bool socketIsConnected = true;
        private Socket client;


        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String response = String.Empty;
        // Boolean to check if there is a full responcemessage
        private static bool fullMessageReceived = false;
        public static List<String> responseQueue = new List<string>();

        public Socket Client { get => client; set => client = value; }

        public ClientSocketConnector(string _ipaddress, int _socketport) 
        {
            serverip = IPAddress.Parse(_ipaddress);
            socketport = _socketport;
        }

        public string PrepareSendMessage(string message)
        {
            return message + "<EOF>";
        }

        public void StopCLient()
        {
            socketIsConnected = false;
        }

        public async Task StartClient() 
        {
            // Connect to remote device
            try
            {
                // Create remote endpoint for the socket. Where you want to connect to.
                IPEndPoint remoteEP = new IPEndPoint(serverip,socketport);

                IPAddress clientIpAddress;

                // Get own ipaddress
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    sock.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = sock.LocalEndPoint as IPEndPoint;
                    clientIpAddress = endPoint.Address;
                }

                // Create a TCP/IP socket. Make sure you use the same parameters as the server.
                client = new Socket(clientIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to remote endpoint
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Send test data to remote device
                Send(client, PrepareSendMessage("{'connectionType':'connectionTest'}"));

                await Task.Run(() =>
                {
                    while (true)
                    {
                        // Receive the response to the remote device.
                        Receive(client);
                        receiveDone.WaitOne();
                    }
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("STARTSOCKET\n", e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Receieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            } catch (Exception e)
            {
                Console.WriteLine("CONNECTCALLBACK\n", e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("Recieving a callback");
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {

                    Console.WriteLine("Reading...");
                    // There might be more data, so store the data so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    string content = state.sb.ToString();
                    if(content.IndexOf("<EOF>") > -1)
                    {
                        String stringContent = content.Substring(0, content.Length - 5);
                        Console.WriteLine("Done reading...");

                        // All the data has arrived, so let's put it in response.
                        if (state.sb.Length > 1)
                        {
                            response = state.sb.ToString();
                            responseQueue.Add(response);
                        }
                        // Signal that all bytes have been received.
                        receiveDone.Set();
                        fullMessageReceived = true;
                    }
                    else
                    {
                        // Get the rest of the data.
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }

                }

            } catch (Exception e)
            {
                Console.WriteLine("RECEIVECALLBACK\n", e.ToString());
            }
        }

        public static void Send(Socket sockclient, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            sockclient.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), sockclient);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine("SENDCALLBACK\n", e.ToString());
            }
        }

    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

}
