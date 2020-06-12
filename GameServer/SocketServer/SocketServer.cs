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

        public static bool isHidden = false;

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

        public static async Task StartListening()
        {
            // Create a TCP/IP socket.  
            listener = new TcpListener(ipAddress, portnumber);

            await Task.Run(() =>
            {
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
            });
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
                    String request = ReadCallBack(stream);
                    Console.WriteLine("Client send: " + request);
                    String singleRequest = "";
                    if (request.Contains("}{"))
                    {
                        String[] multiRequests = request.Split("}");
                        singleRequest = multiRequests[multiRequests.Length-1];
                        foreach(String str in request.Split("}"))
                        {
                            Console.WriteLine("str: " + str);
                        }
                        if(singleRequest == "")
                        {
                            singleRequest = multiRequests[multiRequests.Length - 2];
                            singleRequest += "}";
                        }
                        //Console.WriteLine("SINGLEREQUEST IS: "+singleRequest);
                        request = singleRequest;
                    }
                    CallbackHandler(client, request);
                }
                Player foundPlayer = null;
                foreach(Player player in loggedInPlayers)
                {
                    if(player.Socket == client)
                    {
                        foundPlayer = player;
                    }
                }
                if(foundPlayer != null)
                {
                    GameSession session = GameLogic.GameLogic.GetSessionByUser(foundPlayer.Playername);
                    if(session != null)
                    {
                        GameLogic.GameLogic.ActiveSessions.Remove(session);
                    }
                    loggedInPlayers.Remove(foundPlayer);
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
            Console.WriteLine("CONTENT: "+content);
            JObject jsonConvert = ConvertCallbackMsg(content);
            Console.WriteLine(jsonConvert["connectionType"].ToString());
            switch (jsonConvert["connectionType"].ToString())
            {
                case "login":
                    CustomConsole.CustomLogWrites.LogWriter("User wants to login\nUsername: " + jsonConvert["username"].ToString() + " Password: " + jsonConvert["password"].ToString());
                    
                    Player loginPlayer = DataAccessLayer.GetUser(jsonConvert["username"].ToString(), jsonConvert["password"].ToString());
                    if(loginPlayer != null)
                    {
                        loginPlayer.Socket = clientSocket;
                        loggedInPlayers.Add(loginPlayer);
                        Send(loginPlayer.Socket, "{'connectionType':'loginResponse', 'loginStatus':'success'}");
                    }
                    else
                    {
                        Send(clientSocket, "{'connectionType':'loginResponse', 'loginStatus':'failed'}");
                    }
                    break;
                case "logout":
                    CustomConsole.CustomLogWrites.LogWriter("User " + jsonConvert["username"].ToString() + " logging out.");
                    Player player = GetSinglePlayer(jsonConvert["username"].ToString());
                    if (player != null)
                    {
                        GameSession session = GameLogic.GameLogic.GetSessionByUser(player.Playername);
                        if (session != null)
                        {
                            GameLogic.GameLogic.ActiveSessions.Remove(session);
                        }
                        loggedInPlayers.Remove(player);
                    }
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
                    Player loggedInPlayer = DAL.DataAccessLayer.GetUser(jsonConvert["username"].ToString(), jsonConvert["password"].ToString());
                    loggedInPlayer.Socket = clientSocket;
                    if (loggedInPlayer != null)
                    {
                        GameSession newSession = new GameSession(loggedInPlayer);
                        GameLogic.GameLogic.AddNewSession(newSession);
                        Send(clientSocket, "{ 'connectionType':'createResponse','roomId':" + newSession.Id.ToString() + "}");
                        CustomConsole.CustomLogWrites.LogWriter("Created new room with number " + GameLogic.GameLogic.GetSessionByUser(loggedInPlayer.Playername).Id.ToString() + " for player " + loggedInPlayer.Playername);
                    }
                    break;
                case "gameaccess":
                    if(jsonConvert["gamestart"].ToString() == "true")
                    {
                        Player playerToAccess = GetSinglePlayer(jsonConvert["username"].ToString()); // wil ik mee verbinden
                        if(playerToAccess != null)
                        {
                            Send(playerToAccess.Socket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'gamestart'}");
                            CustomConsole.CustomLogWrites.LogWriter("Game started");
                        }
                    }
                    else
                    {
                        CustomConsole.CustomLogWrites.LogWriter("Accessing game of player");
                        Player playerToAccess = GetSinglePlayer(jsonConvert["username"].ToString());
                        if (playerToAccess != null)
                        {
                            CustomConsole.CustomLogWrites.LogWriter("Accessing session");
                            GameSession sessionToAccess = GameLogic.GameLogic.GetSessionByUser(playerToAccess.Playername);
                            if(sessionToAccess != null)
                            {
                                Player accesserPlayer = GetSinglePlayer(jsonConvert["playeraccesser"].ToString());

                                if (accesserPlayer != null)
                                {
                                    if (sessionToAccess.Seeker == null)
                                    {
                                        CustomConsole.CustomLogWrites.LogWriter(sessionToAccess.Id.ToString());
                                        sessionToAccess.Seeker = accesserPlayer;
                                        Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'success','playerType':'seeker','gameid':'" + sessionToAccess.Id.ToString() + "'}");
                                        Send(sessionToAccess.Hider.Socket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'newPlayer','playerName':'" + accesserPlayer.Playername + "'}");
                                    }
                                    else
                                    {
                                        sessionToAccess.Hider = accesserPlayer;
                                        Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'success','playerType':'hider','gameid':'" + sessionToAccess.Id.ToString() + "'}");
                                        Send(sessionToAccess.Seeker.Socket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'newPlayer','playerName':'" + accesserPlayer.Playername + "'}");
                                    }
                                }
                                else
                                {
                                    Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'failed','gameaccessDescription':'Player not found'}");
                                }
                            }
                            else
                            {
                                Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'failed','gameaccessDescription':'Session not found'}");
                            }
                        }
                        else
                        {
                            Send(clientSocket, "{'connectionType':'gameaccessResponse','gameaccessStatus':'failed','gameaccessDescription':'Player_to_access not found'}");
                        }
                    }
                    
                    break;
                case "roomenter":
                    if(isHidden == false)
                    {
                        Console.WriteLine("Entering room in game");
                        CustomConsole.CustomLogWrites.LogWriter("Player " + clientSocket.Client.RemoteEndPoint + " going into room " + jsonConvert["roomnum"].ToString());
                        (List<Floorplan> rooms, string areRooms) = DomoticzAPI.GetRoomsbyFloor("");
                        Console.WriteLine(areRooms);
                        if (areRooms == null)
                        {
                            foreach (Floorplan room in rooms)
                            {
                                if (room.Name == "Room " + jsonConvert["roomnum"].ToString())
                                {
                                    //Console.WriteLine("Getting gamesession...");
                                    GameSession gameToUploadRooms = GameLogic.GameLogic.GetSessionById(Int32.Parse(jsonConvert["gameid"].ToString()));
                                    gameToUploadRooms.AddToRunRooms(jsonConvert["roomnum"].ToString());
                                    //Console.WriteLine("Added to gamesession");

                                    Console.WriteLine("Roomidx: "+room.Idx);
                                    (List<Device> devices, string isRooms) = DomoticzAPI.GetDevicesByRoom(room.Idx.ToString());
                                    if (isRooms == null)
                                    {
                                        //Console.WriteLine("Triggering devices...");
                                        foreach (Device device in devices)
                                        {
                                            if (device.Name.Contains("Sensor"))
                                            {
                                                Console.WriteLine("Device " + device.Devidx + " triggered");
                                                DomoticzAPI.TriggerMotionDetector(device.Devidx.ToString(), true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    

                    break;
                case "hiderhidden":
                    Console.WriteLine("Hider is hidden");
                    GameSession gameToGet = GameLogic.GameLogic.GetSessionById(Int32.Parse(jsonConvert["gameid"].ToString()));
                    Player seekerPlayer = GetSinglePlayer(gameToGet.Seeker.Playername);
                    if (seekerPlayer != null)
                    {
                        Console.WriteLine(gameToGet.GetRooms());
                        Send(seekerPlayer.Socket, "{'connectionType':'hiddenResponse','runRooms':'" + gameToGet.GetRooms() + "','lastRoom':'" + jsonConvert["lastroom"].ToString() + "'}");
                        isHidden = true;
                    }
                    break;
                case "specialpower":
                    Console.WriteLine("Powerup activated");
                    break;

                case "seekerdone":
                    Console.WriteLine("Seeker done seeking");
                    GameSession endSession = GameLogic.GameLogic.GetSessionById(Int32.Parse(jsonConvert["gameid"].ToString()));
                    Console.WriteLine(jsonConvert["gameid"].ToString()+" is my gameid");
                    string seekeranswer = jsonConvert["seekerAnswer"].ToString();
                    Send(endSession.Hider.Socket, "{'connectionType':'seekerResponse','seekerAnswer':'" + seekeranswer + "', 'playerType':'hider'}");
                    Send(endSession.Seeker.Socket, "{'connectionType':'seekerResponse','playerType':'seeker'}");
                    break;
                case "changeplayertype":
                    Console.WriteLine("Room changing type");
                    GameSession gameSession = GameLogic.GameLogic.GetSessionById(Int32.Parse(jsonConvert["gameid"].ToString()));
                    gameSession.SwitchPlayerType();

                    Console.WriteLine(gameSession.Hider);
                    Console.WriteLine(gameSession.Seeker);

                    if(gameSession.Hider != null)
                    {
                        Console.WriteLine("Sending to hider");
                        Send(gameSession.Hider.Socket, "{'connectionType':'changePlayerResponse','changePlayerStatus':'changed'}");
                    }
                    if (gameSession.Seeker != null)
                    {
                        Console.WriteLine("Sending to seeker");
                        Send(gameSession.Seeker.Socket, "{'connectionType':'changePlayerResponse','changePlayerStatus':'changed'}");
                    }
                    break;
                case "connectionTest":
                    CustomConsole.CustomLogWrites.LogWriter("Client " + clientSocket.Client.RemoteEndPoint.ToString() + " trying to connect");
                    Send(clientSocket, "{'connectionType':'testResponse'}");
                    break;
            }

        }
    }
}

