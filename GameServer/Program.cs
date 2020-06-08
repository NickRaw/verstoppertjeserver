using System;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using GameServer.SocketServer;
using GameServer.CustomConsole;
using DomoticzAPILibrary;
using DomoticzAPILibrary.Models;
using System.Collections.Generic;
using GameServer.DAL;
using System.Net.Sockets;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Console.WriteLine("Verstoppertje server");
            Console.WriteLine("Version: "+ fileVersionInfo.FileVersion);
            

            /*
             STARTUP
             */

            Dictionary<String, String> settings = DataAccessLayer.getSettings();

            // SOCKETSERVER IP AND PORT
            String ipaddress = settings["SERVER_IP"];
            int portnumber = Int32.Parse(settings["SERVER_PORT"]);

            // DOMOTICZSERVER IP AND TESTCONNECTION
            DomoticzAPI domoticz = new DomoticzAPI(settings["DOMOTICZ_IP"]);
            (string responseCategory, string responseMessage) = domoticz.TestConnection();
            if(responseCategory == "success")
            {
                CustomLogWrites.LogWriter(responseMessage);
            }
            else if (responseCategory == "log")
            {
                CustomLogWrites.ErrorWriter(responseMessage);
                CustomLogWrites.DomoticzLogWriter(DomoticzAPI.GetLogInfo());
            }
            else if (responseCategory == "error")
            {
                CustomLogWrites.ErrorWriter(responseMessage);
                Console.WriteLine("Druk op enter om te stoppen...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            // STARTING SOCKETSERVER
            CustomLogWrites.LogWriter("Socket server aanmaken");
            SocketServer.SocketServer socketserver = new SocketServer.SocketServer(ipaddress, portnumber);
            CustomLogWrites.LogWriter("Socket server aangemaakt op ipaddress: " + ipaddress + " en poort " + portnumber.ToString());


            // Console for executing commands

            // List with all the executable functions (FUNCTIONNAME, DESCRIPTION, FUNCTION TO EXECUTE)
            List<(string, string, Action)> commands = new List<(string, string, Action)>()
            { ("quit", "Stopt de server", () => Environment.Exit(0)),
              ("domoticzlog","Haalt log van Domoticz Server op", () => CustomLogWrites.DomoticzLogWriter(DomoticzAPI.GetLogInfo())),
              ("getfloors","Geeft vloerplannen van de afdelingen van Domoticz terug", () => GetFloors()),
              ("getrooms","Geeft vloerplannen van de kamer van Domoticz terug", () => GetRoomsByFloor()),
              ("get_devices_by_room","Geeft devices van de kamer in Domoticz terug", () => GetDevicesByRoom()),
              ("triggerSensor","Triggert een sensor", () => SensorSwitch(true)),
              ("sensorOn","Zet sensor aan", () => SensorSwitch(true)),
              ("sensorOff","Zet sensor uit", () => SensorSwitch(false)),
              ("startSocket","Zet socket server aan", () => socketserver.StartListening()),
              ("stopSocket","Zet socket server uit", () => socketserver.StopListening()),
              ("getClients","Geeft alle verbonden clients terug", () => socketserver.GetAllClients()),
              ("sendTestMessage","Stuurt bericht naar client", () => SendTestMessage(socketserver)),
              ("createDatabase", "Maak een nieuwe database", () => DataAccessLayer.DatabaseInitializer()),
              ("getUser", "Haal gebruiker op", () => GetAUser()),
              ("newUser", "Maak een nieuwe gebruiker aan", () => MakeNewUser()),
              ("changeUser", "Pas een bestaande gebruiker aan", () => ChangeAUser()),
              ("deleteUser", "Verwijder een bestaande gebruiker", () => DeleteAUser())

            };

            bool done = false;
            while(!done)
            {
                Console.Write("Geef een commando. Typ 'help' voor hulp.\n: ");
                string commandResponse = Console.ReadLine();

                if (commandResponse == "help")
                {
                    Console.WriteLine("COMMANDO'S\n-----------------------");
                    foreach ((string functionName, string description, Action function) in commands)
                    {
                        Console.WriteLine("{0} : {1}", functionName, description);
                    }
                    Console.WriteLine();
                }
                else
                {
                    foreach ((string functionName, string description, Action function) in commands)
                    {
                        if(commandResponse == functionName)
                        {
                            function.Invoke();
                        }
                    }
                }
            }



            /*
            CUSTOM FUNCTIONS
            These functions are for running the server
            */

            // DOMOTICZ FUNCTIONS
            static void GetDevicesByRoom()
            {
                Console.WriteLine("Geef IDx van kamer");
                (List<Device>, string) deviceResults =  DomoticzAPI.GetDevicesByRoom(Console.ReadLine());

                if(deviceResults.Item1 != null)
                {
                    Console.WriteLine("IDx : Naam");
                    foreach (Device dev in deviceResults.Item1)
                    {
                        Console.WriteLine("{0} : {1}", dev.Devidx, dev.Name);
                    }
                }
                else
                {
                    CustomLogWrites.LogWriter("Geen devices gevonden!");
                }
                
                Console.WriteLine();
            }

            static void GetFloors()
            {
                (List<Floorplan>, string) floorResults = DomoticzAPI.GetFloors();

                if(floorResults.Item1 != null)
                {
                    Console.WriteLine("IDx : Naam");
                    foreach (Floorplan dev in floorResults.Item1)
                    {
                        Console.WriteLine("{0} : {1}", dev.Idx, dev.Name);
                    }
                    Console.WriteLine();
                }
                else
                {
                    CustomLogWrites.LogWriter("Geen floors gevonden!");
                }

            }

            static void GetRoomsByFloor()
            {
                Console.WriteLine("Geef naam van floor");
                (List<Floorplan>, string) roomResults = DomoticzAPI.GetRoomsbyFloor(Console.ReadLine());

                if(roomResults.Item1 != null)
                {
                    Console.WriteLine("IDx : Naam");
                    foreach (Floorplan room in roomResults.Item1)
                    {
                        Console.WriteLine("{0} : {1}", room.Idx, room.Name);
                    }
                    Console.WriteLine();
                }
                else
                {
                    CustomLogWrites.LogWriter("Geen rooms gevonden!");
                }

            }

            static void SensorSwitch(bool switchState)
            {
                Console.WriteLine("Geef IDx van device");
                CustomLogWrites.LogWriter(DomoticzAPI.TriggerMotionDetector(Console.ReadLine(), switchState));
            }

            /*
             SOCKETSERVER COMMANDS
             */

            static void SendTestMessage(SocketServer.SocketServer serverSocket)
            {
                Console.WriteLine("Geef aan welke client je wil testen. (Voorbeeld: Client 0)");
                String clientname = Console.ReadLine();
                Console.WriteLine("Welk bericht wil je versturen");
                String message = Console.ReadLine();
                Socket socket = SocketServer.SocketServer.GetClient(clientname);
                SocketServer.SocketServer.Send(socket, message);
            }

            /*
             USER FUNCTIONS
             */

            static void GetAUser()
            {
                Console.WriteLine("Geef gebruikersnaam");
                string username = Console.ReadLine();
                Console.WriteLine("Geef wachtwoord");
                string password = Console.ReadLine();
                DataAccessLayer.GetUser(username, password);
            }

            static void MakeNewUser()
            {
                Console.WriteLine("Geef gebruikersnaam");
                string username = Console.ReadLine();
                Console.WriteLine("Geef wachtwoord");
                string password = Console.ReadLine();
                DataAccessLayer.NewUser(username, password);
            }

            static void ChangeAUser()
            {
                Console.WriteLine("Geef gebruikersnaam");
                string username = Console.ReadLine();
                Console.WriteLine("Geef wachtwoord");
                string password = Console.ReadLine();
                Console.WriteLine("Geef nieuwe username");
                string new_username = Console.ReadLine();
                DataAccessLayer.ChangeUser(username, password, new_username);
            }

            static void DeleteAUser()
            {
                Console.WriteLine("Geef gebruikersnaam");
                string username = Console.ReadLine();
                Console.WriteLine("Geef wachtwoord");
                string password = Console.ReadLine();
                DataAccessLayer.DeleteUser(username, password);
            }
        }
    }
}
