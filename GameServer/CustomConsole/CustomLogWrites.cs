using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.CustomConsole
{
    class CustomLogWrites
    {
        public static void LogWriter(string logtext)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Log {0}: ", DateTime.Now.ToString("h:mm:ss tt"));
            Console.ResetColor();
            Console.WriteLine(logtext);
        }

        public static void ErrorWriter(string errortext)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error {0}: ", DateTime.Now.ToString("h:mm:ss tt"));
            Console.ResetColor();
            Console.WriteLine(errortext);
        }

        public static void DomoticzLogWriter(List<(long, string)> domoticzResult)
        {
            Dictionary<int, string> loglevels = new Dictionary<int, string>() { { 1, "Normal" }, { 2, "Status" }, { 4, "Error" } };
            Dictionary<int, ConsoleColor> logColors = new Dictionary<int, ConsoleColor>() { { 1, ConsoleColor.DarkMagenta }, { 2, ConsoleColor.Cyan }, { 4, ConsoleColor.Red } };

            foreach ((long, string) log in domoticzResult)
            {
                string oldMessage = log.Item2;
                String[] strList = oldMessage.Split(' ');

                string stringTime = strList[0] + " " + strList[1];
                string newMessage = log.Item2.Replace(stringTime, "");

                Console.ForegroundColor = logColors[Convert.ToInt32(log.Item1)];
                Console.Write("{0} {1}: ", loglevels[Convert.ToInt32(log.Item1)], stringTime);
                Console.ResetColor();
                Console.WriteLine(newMessage);
            }

        }
    }
}
