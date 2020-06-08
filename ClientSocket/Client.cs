using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocket
{
    public class Client
    {
        public ClientSocketConnector clientSocket;
        
        public Client()
        {
            clientSocket = new ClientSocketConnector("192.168.2.201", 34000);
            clientSocket.StartClient();
            //ResponseListener();
        }

        private async Task ResponseListener()
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

        public void testSend(string testdata)
        {
            ClientSocketConnector.Send(clientSocket.Client, testdata);
        }

        public string changeplayertype(int gameid)
        {
            throw new NotImplementedException();
        }

        public string gameaccess(string username)
        {
            throw new NotImplementedException();
        }

        public string gamecreate(string username, string password)
        {
            throw new NotImplementedException();
        }

        public string hiderhidden(int gameid)
        {
            throw new NotImplementedException();
        }

        public void login(string username, string password)
        {
            ClientSocketConnector.Send(clientSocket.Client, clientSocket.PrepareSendMessage("{'connectionType':'login','username':'" + username + "','password':'" + password + "'}"));
        }

        public string register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public static string roomenter(int gameid, int roomnum)
        {
            throw new NotImplementedException();
        }

        public string seekerdone(int gameid)
        {
            throw new NotImplementedException();
        }

        public string specialpower(int gameid, int roomnum)
        {
            throw new NotImplementedException();
        }

    }
}
