using GameServer.GameLogic;
using GameServer.GameLogic.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.SocketServer
{
    public class SocketServer
    {
        private int portnumber;
        private IPAddress ipaddress;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static List<Player> loggedInClients = new List<Player>();
        public static List<(string, Socket)> loggedInSockets = new List<(string, Socket)>();
        public static Socket listener;

        public static string PrepareSendMessage(string message)
        {
            return message + "<EOF>";
        }

        public SocketServer(string _ipaddress, int _portnumber)
        {
            ipaddress = IPAddress.Parse(_ipaddress);
            portnumber = _portnumber;
        }

        public async Task StartSocket()
        {
            // Establish local endpoint for the socket.
            IPEndPoint localEndPoint = new IPEndPoint(ipaddress, portnumber);

            // Create TCP/IP Socket
            listener = new Socket(ipaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await Task.Run(() =>
                {
                    Console.WriteLine("Start listening...");
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    while (true)
                    {
                        StartListening();
                    }
                    
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
                        
        }

        public static void StartListening()
        {
            allDone.Reset();
            Console.WriteLine("Listening...");
            // Start an asynchronous socket to listen for connections.
            listener.BeginAccept(
                new AsyncCallback(AcceptCallback),
                listener);

            // Wait until a connection is made before continuing.
            allDone.WaitOne();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("ACCEPT CALLBACK triggered");
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            Console.WriteLine("READ CALLBACK triggered");
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                //Check for end-of-file tag. If it is not there, read more data
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    String stringContent = content.Substring(0, content.Length - 5);
                    JObject jsonObject = JObject.Parse(stringContent);

                    // We found all the data so we can display it all to the console and do something with it.
                    CustomConsole.CustomLogWrites.LogWriter($"Client {handler.RemoteEndPoint}: Sends {content.Length} bytes from socket. \n Data : {content.Replace("<EOF>", "")}");
                    Send(handler, PrepareSendMessage(content));
                    //CallbackHandler(content.Replace("<EOF>", ""), handler);
                    loggedInSockets.Add(("Client"+loggedInSockets.Count, handler));
                    StartListening();
                }
                else
                {
                    // We don't have all the data, so keep looking.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
            }
        }

        public static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both); //PROBLEEM DIE ERVOOR ZORGDE DAT DE CLIENTS NIET MEER MET DE SOCKETS VERBONDEN
                //handler.Close(); //PROBLEEM DIE ERVOOR ZORGDE DAT DE CLIENTS NIET MEER MET DE SOCKETS VERBONDEN

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void GetAllClients() 
        { 
            foreach((string name, Socket client) in loggedInSockets)
            {
                Console.WriteLine(name + " : " + client);
            }
        }
        
        public static Socket GetClient(String _name)
        {
            foreach((string name, Socket client) in loggedInSockets)
            {
                if (_name == name)
                {
                    return client;
                }
            }
            return null;
        }

        public static JObject ConvertCallbackMsg(String msg)
        {
            JObject json = JObject.Parse(msg);
            /*Dictionary<String, String> finalMsg = new Dictionary<string, string>();
            foreach(String mes in msg.Split(","))
            {
                String[] cont = mes.Split(":");
                finalMsg.Add(cont[0], cont[1]);
            }*/
            return json;
        }

        public static void CallbackHandler(string content, Socket clientSocket)
        {
            Console.WriteLine(content);
            JObject jsonConvert = ConvertCallbackMsg(content);
            Console.WriteLine(jsonConvert["connectionType"].ToString());
            switch (jsonConvert["connectionType"].ToString())
            {
                case "login":
                    CustomConsole.CustomLogWrites.LogWriter("User wants to login\nUsername: "+ jsonConvert["clientname"].ToString() +" Password: "+ jsonConvert["password"].ToString());
                    Send(clientSocket, PrepareSendMessage("{'connectionType':'loginResponse'}"));
                    /*Player loginPlayer = GetClient(jsonConvert["clientname"].ToString());
                    if(loginPlayer != null)
                    {
                        loginPlayer.Socket = clientSocket;
                        loggedInClients.Add(loginPlayer);
                        Send(loginPlayer.Socket, PrepareSendMessage("{'connectionType':'loginResponse'}"));
                    }*/
                    break;
                case "register":
                    Console.WriteLine("User wants to register");
                    bool registed = DAL.DataAccessLayer.NewUser(jsonConvert["clientname"].ToString(), jsonConvert["password"].ToString());
                    if (registed)
                    {
                        Send(clientSocket, PrepareSendMessage("{'connectionType':'registerResponse','registerStatus':'success','responseMessage':'You are now registered. Login to start gaming'}"));
                    }
                    else
                    {
                        Send(clientSocket, PrepareSendMessage("{'connectionType':'registerResponse','registerStatus':'usernamefail','responseMessage':'Registration failed! Username not correct'}"));
                    }
                    break;
                case "gamecreate":
                    // GET PLAYER THATS WANTS TO CREATE A NEW GAME
                    Console.WriteLine("Creating new game for player");
                    Player loggedInPlayer = DAL.DataAccessLayer.GetUser(jsonConvert["clientname"].ToString(), jsonConvert["password"].ToString());
                    if(loggedInPlayer != null)
                    {
                        GameSession newSession = new GameSession(loggedInPlayer);
                        GameLogic.GameLogic.AddNewSession(newSession);
                        Send(loggedInPlayer.Socket, "{ 'roomId':"+ newSession.Id.ToString()+ "}");
                        CustomConsole.CustomLogWrites.LogWriter("Created new room with number " + GameLogic.GameLogic.GetSessionByUser(loggedInPlayer).Id.ToString() + " for player " + loggedInPlayer.Playername);
                    }
                    break;
                case "gameaccess":
                    // GET PLAYER THATS OWNS THE GAME
                    // GET PLAYER THATS WANTS TO ACCESS THE GAME
                    Console.WriteLine("Accessing game of player");

                    /*GameSession playerToAccess = GameLogic.GameLogic.GetSessionByUser(GetClient(jsonConvert["accessplayer"].ToString()));
                    Player accesserPlayer = GetClient(jsonConvert["playeraccesser"].ToString());
                    if(playerToAccess.Seeker == null)
                    {
                        playerToAccess.Seeker = accesserPlayer;
                    }
                    else
                    {
                        playerToAccess.Hider = accesserPlayer;
                    }*/
                    break;
                case "roomenter":
                    Console.WriteLine("Entering room in game");
                    break;
                case "hiderhidden":
                    Console.WriteLine("Hider is hidden");
                    break;
                case "specialpower":
                    Console.WriteLine("Powerup activated");
                    break;
                case "seekerdone":
                    Console.WriteLine("Seeker done seeking");
                    break;
                case "changeplayertype":
                    Console.WriteLine("Room changing type");
                    GameSession gameSession = GameLogic.GameLogic.GetSessionById(Int32.Parse(jsonConvert["roomId"].ToString()));
                    gameSession.SwitchPlayerType();
                    break;
                case "connectionTest":
                    Console.WriteLine("Client trying to connect");
                    loggedInSockets.Add(("Client"+loggedInSockets.Count,clientSocket));
                    Send(clientSocket, PrepareSendMessage("{'connectionType':'testResponse'}"));
                    break;
            }
        }

    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

}
