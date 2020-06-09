using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data.SQLite;
using GameServer.GameLogic.Models;

namespace GameServer.DAL
{
    public static class DataAccessLayer
    {
        public static Dictionary<string, string> getSettings()
        {
            Dictionary<string, string> settings = new Dictionary<String, String>();

            string[] lines = File.ReadAllLines("Settings.txt");
            foreach(string line in lines)
            {
                string[] splitLines = line.Split("=");
                settings.Add(splitLines[0], splitLines[1]);
                
            }
                        
            return settings;
        }

        public static void DatabaseInitializer()
        {
            SQLiteConnection.CreateFile("users.db");
            using (var conn = new SQLiteConnection("Data Source = users.db; Version = 3; "))
            {
                conn.Open();
                string createUserTable = "create table users (id varchar(255), username varchar(255), password varchar(255))";
                SQLiteCommand cmd = new SQLiteCommand(createUserTable, conn);
                cmd.ExecuteNonQuery();
            }

        }

        public static bool NewUser(string username, string password) 
        {
            using (var conn = new SQLiteConnection("Data Source = users.db; Version = 3; "))
            {
                conn.Open();
                // Check if username already exists
                string getUser = "SELECT count(*) FROM users WHERE username = @username";
                using var cmd = new SQLiteCommand(getUser, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if(Int32.Parse(rdr[0].ToString()) == 0)
                    {
                        Console.WriteLine("User does not exist. Creating new user");
                        string setUser = "INSERT INTO users (id, username, password) VALUES (@id, @username, @password)";
                        using (var cmd_2 = new SQLiteCommand(setUser, conn))
                        {
                            string newId = System.Guid.NewGuid().ToString();
                            cmd_2.Parameters.AddWithValue("id", newId);
                            cmd_2.Parameters.AddWithValue("username", username);
                            cmd_2.Parameters.AddWithValue("password", password);
                            cmd_2.ExecuteNonQuery();
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public static Player GetUser(string username, string password) 
        {
            using (var conn = new SQLiteConnection("Data Source = users.db; Version = 3; "))
            {
                conn.Open();
                string getUser = "SELECT * FROM users WHERE username = @username AND password = @password";
                using var cmd = new SQLiteCommand(getUser, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                using SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read()) 
                {
                    CustomConsole.CustomLogWrites.LogWriter(rdr[0].ToString() + " - " + rdr[1].ToString() + " - " + rdr[2].ToString());
                    return new Player(null, rdr[1].ToString(), rdr[2].ToString(), rdr[0].ToString());
                }
                return null;
            }
        }

        public static void ChangeUser(string old_username, string old_password, string new_username = null, string new_password = null) 
        {
            using (var conn = new SQLiteConnection("Data Source = users.db; Version = 3; "))
            {
                conn.Open();
                string getUser = "SELECT username,password FROM users WHERE username = @username AND password = @password";
                using var cmd = new SQLiteCommand(getUser, conn);
                cmd.Parameters.AddWithValue("@username", old_username);
                cmd.Parameters.AddWithValue("@password", old_password);

                using SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string changeSQL = new_username == null ? "UPDATE users SET password = @newPassword WHERE username = @username AND password = @password" : "UPDATE users SET username = @newUsername WHERE username = @username AND password = @password";
                    using var cmd_2 = new SQLiteCommand(changeSQL, conn);
                    cmd_2.Parameters.AddWithValue(new_username == null ? "@newPassword" : "@newUsername", new_username == null ? new_password : new_username);
                    cmd_2.Parameters.AddWithValue("@username", old_username);
                    cmd_2.Parameters.AddWithValue("@password", old_password);
                    cmd_2.ExecuteNonQuery();
                    CustomConsole.CustomLogWrites.LogWriter("User " + new_username == null ? old_username : new_username + " changed");
                }

            }
        }

        public static void DeleteUser(string username, string password)
        {
            using(var conn = new SQLiteConnection("Data Source = users.db; Version = 3; "))
            {
                conn.Open();
                string deleteUser = "DELETE FROM users WHERE username = @username AND password = @password;";
                using var cmd = new SQLiteCommand(deleteUser, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
                CustomConsole.CustomLogWrites.LogWriter("User " + username + " has been deleted!");
            }
        }

    }
}
