using GameServer.GameLogic;
using GameServer.GameLogic.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.SocketServer
{
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

    public class SocketServer
    {
        public static List<(string, TcpClient)> loggedInSockets = new List<(string, TcpClient)>();
        private static TcpListener listener;
        public SocketServer()
        {
        }

        public static void StartListening()
        {
            

            IPAddress ipAddress = IPAddress.Parse("192.168.2.8");

            // Create a TCP/IP socket.  
            listener = new TcpListener(ipAddress, 34000);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Start();
                
                while (true)
                {
                    Console.WriteLine("Listening...");
                    // Listen to tcpclients
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client " + client.Client.RemoteEndPoint + " connected");

                    ThreadPool.QueueUserWorkItem(ThreadProc, client);

                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static string ReadCallBack(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);
            int recv = 0;
            foreach (byte b in buffer)
            {
                if (b != 0)
                {
                    recv++;
                }
            }
            return Encoding.UTF8.GetString(buffer, 0, recv);
        }

        private static void ThreadProc(object obj)
        {
            var client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(client.GetStream());
            StreamWriter writer = new StreamWriter(client.GetStream());
            try
            {
                while (client.Connected)
                {
                    string request = ReadCallBack(stream);
                    Console.WriteLine("Client send: " + request);
                    CallbackHandler(client, request);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(TcpClient client, string data)
        {
            StreamWriter writer = new StreamWriter(client.GetStream());
            writer.WriteLine(data);
            writer.Flush();
        }

        public static JObject ConvertCallbackMsg(String msg)
        {
            JObject json = JObject.Parse(msg);
            return json;
        }

        public static void CallbackHandler(TcpClient clientSocket, string content)
        {
            Console.WriteLine(content);
            JObject jsonConvert = ConvertCallbackMsg(content);
            Console.WriteLine(jsonConvert["connectionType"].ToString());
            switch (jsonConvert["connectionType"].ToString())
            {
                case "login":
                    CustomConsole.CustomLogWrites.LogWriter("User wants to login\nUsername: " + jsonConvert["clientname"].ToString() + " Password: " + jsonConvert["password"].ToString());
                    Send(clientSocket, "{'connectionType':'loginResponse'}");
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
                        Send(clientSocket, "{'connectionType':'registerResponse','registerStatus':'success','responseMessage':'You are now registered. Login to start gaming'}");
                    }
                    else
                    {
                        Send(clientSocket, "{'connectionType':'registerResponse','registerStatus':'usernamefail','responseMessage':'Registration failed! Username not correct'}");
                    }
                    break;
                case "gamecreate":
                    // GET PLAYER THATS WANTS TO CREATE A NEW GAME
                    Console.WriteLine("Creating new game for player");
                    Player loggedInPlayer = DAL.DataAccessLayer.GetUser(jsonConvert["clientname"].ToString(), jsonConvert["password"].ToString());
                    if (loggedInPlayer != null)
                    {
                        GameSession newSession = new GameSession(loggedInPlayer);
                        GameLogic.GameLogic.AddNewSession(newSession);
                        Send(clientSocket, "{ 'roomId':" + newSession.Id.ToString() + "}");
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
                    //loggedInSockets.Add(("Client" + loggedInSockets.Count, clientSocket));
                    Send(clientSocket, "{'connectionType':'testResponse'}");
                    break;
            }
            Console.WriteLine("End of function...");

        }
    }
}

