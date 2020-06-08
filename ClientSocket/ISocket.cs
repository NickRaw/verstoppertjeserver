using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClientSocket;

public interface ISocket
{
    private static ClientSocketConnector clientSocket;

    public static void StartSocket(string ipaddress, int portnumber)
    {
        clientSocket = new ClientSocketConnector(ipaddress, portnumber);
        ResponseListener();
        clientSocket.StartClient();

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
        ClientSocketConnector.Send(clientSocket.Client, clientSocket.PrepareSendMessage("{'connectionType':'login', 'username': '" + username + "', 'password':'" + password + "'}"));
    }

    static void register(string username, string password) { }

    static void gamecreate(string username, string password) { }

    static void gameaccess(string username) { }

    static void roomenter(int gameid, int roomnum) { }

    static void hiderhidden(int gameid) { }

    static void specialpower(int gameid, int roomnum) { }

    static void seekerdone(int gameid) { }

    static void changeplayertype(int gameid) { }

    static void testMessage()
    {
        Console.WriteLine(clientSocket.PrepareSendMessage("{'connectionType':'connectionTest'}"));
        ClientSocketConnector.Send(clientSocket.Client, clientSocket.PrepareSendMessage("{'connectionType':'connectionTest'}"));
    }
}
