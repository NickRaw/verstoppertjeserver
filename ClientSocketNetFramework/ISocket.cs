using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ClientSocketNetFramework;
using Newtonsoft.Json.Linq;

public static class ISocket
{
    public static void StartSocket(string ipaddress, int portnumber)
    {
        ClientSocketConnector.PrepareConnector(ipaddress, portnumber);

    }

    public static List<string> GetResponseQueue()
    {
        return ClientSocketConnector.responseQueue;
    }

    public static void RemoveFromResponseQueue(string queueItem)
    {
        ClientSocketConnector.responseQueue.Remove(queueItem);
    }
    
    public static void login(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'login', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    public static void logout(string username)
    {
        ClientSocketConnector.Send("{'connectionType':'logout','username':'" + username + "'}");
    }

    public static void register(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'register', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    public static void gamecreate(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'gamecreate', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    public static void gameaccess(string username, string gameAccesser, string gamestart = "") 
    {
        if(gamestart == "")
        {
            ClientSocketConnector.Send("{'connectionType':'gameaccess', 'username': '" + username + "','playeraccesser':'" + gameAccesser + "','gamestart':'false'}");
        }
        else if (gamestart == "gamestart")
        {
            ClientSocketConnector.Send("{'connectionType':'gameaccess', 'username': '" + username + "','playeraccesser':'" + gameAccesser + "','gamestart':'true'}");
        }
    }

    public static void roomenter(int gameid, int roomnum)
    {
        ClientSocketConnector.Send("{'connectionType':'roomenter', 'gameid': '" + gameid + "', 'roomnum':'" + roomnum + "'}");
    }

    public static void hiderhidden(int gameid, int lastroomnum)
    {
        ClientSocketConnector.Send("{'connectionType':'hiderhidden', 'gameid': '" + gameid + "','lastroom':'" + lastroomnum + "'}");
    }

    public static void specialpower(int gameid, int roomnum)
    {
        ClientSocketConnector.Send("{'connectionType':'specialpower', 'gameid': '" + gameid + "', 'roomnum':'" + roomnum + "'}");
    }

    public static void seekerdone(int gameid, string answer)
    {
        ClientSocketConnector.Send("{'connectionType':'seekerdone', 'gameid': '" + gameid + "','seekerAnswer':'" + answer + "'}");
    }

    public static void changeplayertype(int gameid)
    {
        ClientSocketConnector.Send("{'connectionType':'changeplayertype', 'gameid': '" + gameid + "'}");
    }

    public static void testMessage()
    {
        ClientSocketConnector.Send("{'connectionType':'connectionTest'}");
    }
}
