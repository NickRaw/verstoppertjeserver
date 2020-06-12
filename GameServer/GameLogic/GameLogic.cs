using GameServer.GameLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.GameLogic
{
    public static class GameLogic
    {
        private static List<GameSession> activeSessions = new List<GameSession>();

        public static List<GameSession> ActiveSessions { get => activeSessions; }
        public static void AddNewSession(GameSession session) { activeSessions.Add(session); }
        
        public static GameSession GetSessionById(int id)
        {
            foreach(GameSession session in activeSessions)
            {
                if(session.Id == id)
                {
                    return session;
                }
            }
            return null;
        }

        public static GameSession GetSessionByUser(string player)
        {
            //Console.WriteLine(activeSessions.Count+" amount of session to look through");
            foreach(GameSession session in activeSessions)
            {
                if(session.Hider.Playername == player)
                {
                    return session;
                }
                else if(session.Seeker.Playername == player)
                {
                    return session;
                }
            }
            return null;
        }
    }
}
