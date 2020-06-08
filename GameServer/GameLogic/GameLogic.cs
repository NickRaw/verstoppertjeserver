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

        public static GameSession GetSessionByUser(Player player)
        {
            foreach(GameSession session in activeSessions)
            {
                if(session.Hider == player || session.Seeker == player)
                {
                    return session;
                }
            }
            return null;
        }
    }
}
