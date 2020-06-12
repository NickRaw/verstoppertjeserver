using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.GameLogic.Models
{
    public class GameSession
    {
        private int id;
        private Player hider = null;
        private Player seeker = null;
        private List<string> runRooms = new List<string>();

        public Player Hider { get => hider; set => hider = value; }
        public Player Seeker { get => seeker; set => seeker = value; }
        public int Id { get => id; }
        public string GetRooms() 
        {
            string strRooms = "";
            Console.WriteLine("RUNROOMS: " + runRooms.Count);
            strRooms = strRooms + runRooms[0];

            for(int i = 1; i < runRooms.Count; i++)
            {
                strRooms += "," + runRooms[i];
            }
            
            return strRooms;
        }
        public void AddToRunRooms(string room)
        {
            Console.WriteLine("Added room " + room + "to the list");
            runRooms.Add(room);
        }

        public GameSession(Player player) 
        {
            this.hider = player;
            Random random = new Random();
            string num1 = random.Next(0, 10).ToString();
            string num2 = random.Next(0, 10).ToString();
            string num3 = random.Next(0, 10).ToString();
            string num4 = random.Next(0, 10).ToString();
            string roomnum = num1 + num2 + num3 + num4;
            this.id = Int32.Parse(roomnum);
        }

        public void SwitchPlayerType()
        {
            Player helpVar = seeker;
            seeker = hider;
            hider = helpVar;
        }
    }
}
