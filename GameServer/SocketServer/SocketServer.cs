using DomoticzAPILibrary;
using DomoticzAPILibrary.Models;
using GameServer.DAL;
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

    public static class SocketServer
    {
        // Server info
        public static IPAddress ipAddress;
        public static int portnumber;

        // Lists with connected clients and logged in players
        public static List<(string, TcpClient)> connectedClients = new List<(string, TcpClient)>();
        public static List<Player> loggedInPlayers = new List<Player>();

        // TCPListener
        private static TcpListener listener;

        public static void PrepareSocket(string _ipaddress, int _portnumber)
        {
            ipAddress = IPAddress.Parse(_ipaddress);
            portnumber = _portnumber;
        }

        public static void StartListening()
        {
            // Create a TCP/IP socket.  
            listener = new TcpListener(ipAddress, portnumber);

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

        public static void StopListening()
        {
            if(connectedClients.Count > 0)
            {
                foreach((string ip, TcpClient socket) in connectedClients)
                {
                    socket.Close();
                }
            }
            listener.Stop();
        }

        public static void GetConnectedClients()
        {
            if (connectedClients.Count > 0)
            {
                CustomConsole.CustomLogWrites.LogWriter("##### CONNECTED TCPCLIENTS #####");
                foreach ((string clientip, TcpClient socket) in connectedClients)
                {
                    Console.WriteLine("IP: " + clientip);
                }
                CustomConsole.CustomLogWrites.LogWriter("################################");
            }
            else
            {
                CustomConsole.CustomLogWrites.LogWriter("No clients connected!");
            }
        }

        public static TcpClient GetSingleClient(string ip)
        {
            if (connectedClients.Count > 0)
            {
                CustomConsole.CustomLogWrites.LogWriter("Searching clients...");
                foreach ((string clientip, TcpClient socket) in connectedClients)
                {
                    if(clientip == ip)
                    {
                        CustomConsole.CustomLogWrites.LogWriter("Client found!");
                        return socket;
                    }
                }
                CustomConsole.CustomLogWrites.LogWriter("No client found!");
                return null;
            }
            CustomConsole.CustomLogWrites.LogWriter("No clients to connect to!");
            return null;
        }

        public static Player GetSinglePlayer(string username)
        {
            if(loggedInPlayers.Count > 0)
            {
                foreach(Player player in loggedInPlayers)
                {
                    if(player.Playername == username)
                    {
                        return player;
                    }
                }
                return null;
            }
            else
            {
                return null;
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
                connectedClients.Add((client.Client.RemoteEndPoint.ToString(), client));
                while (client.Connected)
                {
                    string request = ReadCallBack(stream);
                    Console.WriteLine("Client send: " + request);
                    CallbackHandler(client, request);
                }
                connectedClients.Remove((client.Client.RemoteEndPoint.ToString(), client));
                
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
                    
                    Player loginPlayer = DataAccessLayer.GetUser(jsonConvert["clientname"].ToString(), jsonConvert["password"].ToString());
                    if(loginPlayer != null)
                    {
                        loginPlayer.Socket = clientSocket;
                        loggedInPlayers.Add(loginPlayer);
                        Send(loginPlayer.Socket, "{'connectionType':'loginResponse', 'loginStatus':'success'}");
                    }
                    else
                    {
                        Send(loginPlayer.Socket, "{'connectionType':'loginResponse', 'loginStatus':'failed'}");
                    }
                    break;
                case "logout":
                    CustomConsole.CustomLogWrites.LogWriter("User " + jsonConvert["username"].ToString() + " logging out.");
                    Send(clientSocket, "{'connectionType':'logoutResponse', 'logoutStatus':'success'}");
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
                    CustomConsole.CustomLogWrites.LogWriter("Accessing game of player");
                    Player playerToAccess = GetSinglePlayer(jsonConvert["accessplayer"].ToString());
                    if(playerToAccess != null)
                    {
                        GameSession sessionToAccess = GameLogic.GameLogic.GetSessionByUser(playerToAccess);
                        Player accesserPlayer = GetSinglePlayer(jsonConvert["playeraccesser"].ToString());

                        if(accesserPlayer != null)
                        {
                            if (sessionToAccess.Seeker == null)
                            {
                                sessionToAccess.Seeker = accesserPlayer;
                                Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'success','gameaccessDescription':'player is seeker'}");
                            }
                            else
                            {
                                sessionToAccess.Hider = accesserPlayer;
                                Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'succeess','gameaccessDescription':'player is seeker'}");
                            }
                        }
                        else
                        {
                            Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'failed','gameaccessDescription':'Player not found'}");
                        }
                    }
                    else
                    {
                        Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'failed','gameaccessDescription':'Player_to_access not found'}");
                    }
                    break;
                case "roomenter":
                    Console.WriteLine("Entering room in game");
                    CustomConsole.CustomLogWrites.LogWriter("Player " + clientSocket.Client.RemoteEndPoint + " going into room " + jsonConvert["roomnum"].ToString());
                    (List<Floorplan> rooms, string areRooms) = DomoticzAPI.GetRoomsbyFloor("");

                    if (areRooms == null)
                    {
                        foreach(Floorplan room in rooms)
                        {
                            if(room.Name == jsonConvert["roomnum"].ToString())
                            {
                                (List<Device> devices,string isRooms) = DomoticzAPI.GetDevicesByRoom(room.Idx.ToString());
                                if(isRooms == null)
                                {
                                    foreach(Device device in devices)
                                    {
                                        if (device.Name.Contains("Sensor"))
                                        {
                                            DomoticzAPI.TriggerMotionDetector(device.Idx.ToString(), true);
                                        }
                                    }
                                }
                            }
                        }
                    }

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
                    CustomConsole.CustomLogWrites.LogWriter("Client " + clientSocket.Client.RemoteEndPoint.ToString() + " trying to connect");
                    Send(clientSocket, "{'connectionType':'testResponse'}");
                    break;
            }
            Console.WriteLine("End of function...");

        }
    }
}

