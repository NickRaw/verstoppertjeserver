using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClientSocket;

public interface ISocket
{
    public static void StartSocket(string ipaddress, int portnumber)
    {
        ClientSocketConnector.PrepareConnector(ipaddress, portnumber);
        ResponseListener();

    }

    public static async Task ResponseListener()
    {
        await Task.Run(() =>
        {
            while (true)
            {
                if (ClientSocketConnector.responseQueue.Count > 0)
                {
                    for (int i = 0; i < ClientSocketConnector.responseQueue.Count; i++)
                    {
                        Console.WriteLine(ClientSocketConnector.responseQueue[i]);
                        ClientSocketConnector.responseQueue.RemoveAt(i);
                    }
                }
            }
        });
    }
    static void login(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'login', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    static void logout(string username)
    {
        ClientSocketConnector.Send("{'connectionType':'logout','username':'" + username + "'}");
    }

    static void register(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'register', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    static void gamecreate(string username, string password) 
    {
        ClientSocketConnector.Send("{'connectionType':'gamecreate', 'username': '" + username + "', 'password':'" + password + "'}");
    }

    static void gameaccess(string username) 
    {
        ClientSocketConnector.Send("{'connectionType':'gameaccess', 'username': '" + username + "'}");
    }

    static void roomenter(int gameid, int roomnum)
    {
        ClientSocketConnector.Send("{'connectionType':'roomenter', 'gameid': '" + gameid + "', 'roomnum':'" + roomnum + "'}");
    }

    static void hiderhidden(int gameid)
    {
        ClientSocketConnector.Send("{'connectionType':'hiderhidden', 'gameid': '" + gameid + "'}");
    }

    static void specialpower(int gameid, int roomnum)
    {
        ClientSocketConnector.Send("{'connectionType':'specialpower', 'gameid': '" + gameid + "', 'roomnum':'" + roomnum + "'}");
    }

    static void seekerdone(int gameid)
    {
        ClientSocketConnector.Send("{'connectionType':'seekerdone', 'gameid': '" + gameid + "'}");
    }

    static void changeplayertype(int gameid)
    {
        ClientSocketConnector.Send("{'connectionType':'changeplayertype', 'gameid': '" + gameid + "'}");
    }

    static void testMessage()
    {
        ClientSocketConnector.Send("{'connectionType':'connectionTest'}");
    }
}
