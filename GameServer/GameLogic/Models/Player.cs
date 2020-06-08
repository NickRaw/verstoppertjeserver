using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace GameServer.GameLogic.Models
{
    public class Player
    {
        private string id;
        private Socket socket;
        private string playername;
        private string password;

        public string Id { get => id; }
        public Socket Socket { get => socket; set => socket = value; }
        public string Playername { get => playername; set => playername = value; }
        public string Password { get => password; set => password = value; }


        public Player(Socket _socket, string _playername, string _password, String _id = null)
        {
            this.socket = _socket;
            this.playername = _playername;
            this.password = _password;
            this.id = _id == null ? System.Guid.NewGuid().ToString() : _id;
        }

    }
}
