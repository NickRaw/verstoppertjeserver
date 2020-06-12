using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using DomoticzAPILibrary.Models;
using System.Linq;

namespace DomoticzAPILibrary
{
    public class DomoticzAPI
    {
        private static string url;
        public static string Url { set => url = value; }

        public DomoticzAPI(string _url) { url = _url; }

        public (string, string) TestConnection()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                try
                {
                    HttpResponseMessage response = client.GetAsync("json.htm?type=command&param=getversion").Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JObject.Parse(result);
                    if (data.status == "OK")
                    {
                        return ("success", "Server is ready to go!");
                    }
                    else
                    {
                        string domoticzErrorReturnLog = "Status is niet OK.\nZoeken log bestanden van Domoticz server\n";
                        return ("log", domoticzErrorReturnLog);
                    }
                }
                catch (AggregateException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n!!!!!!De server kan niet verder werken. Los probleem op en start opnieuw op!!!!!!");
                    Console.ResetColor();
                    return ("error", e.ToString());
                }

            }
        }
        public static (List<Floorplan>, string) GetFloors()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                try
                {
                    HttpResponseMessage response = client.GetAsync("json.htm?type=floorplans&order=name&used=true").Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;

                    var list = JsonConvert.DeserializeObject<FloorplanResults>(result);

                    return (list.Floorplans.ToList<Floorplan>(), null);
                }
                catch (Exception e)
                {
                    return (null, e.ToString());
                }
            }
        }

        public static (List<Floorplan>, string) GetRoomsbyFloor(string floorName)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                try
                {
                    HttpResponseMessage response = client.GetAsync("json.htm?type=plans&order=name&used=true").Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;

                    var list = JsonConvert.DeserializeObject<FloorplanResults>(result);

                    List<Floorplan> allRooms = list.Floorplans.ToList<Floorplan>();
                    List<Floorplan> floorRooms = new List<Floorplan>();

                    foreach(Floorplan room in allRooms)
                    {
                        if (room.Name.Contains("Room"))
                        {
                            floorRooms.Add(room);
                        }
                        
                    }

                    return (floorRooms, null);
                }
                catch (Exception e)
                {
                    return (null, e.ToString());
                }
            }
        }

        public static (List<Device>, string) GetDevicesByRoom(string roomIDx)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                try
                {
                    HttpResponseMessage response = client.GetAsync("json.htm?type=command&param=getplandevices&idx=" + roomIDx).Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;

                    var list = JsonConvert.DeserializeObject<RoomDevices>(result);

                    if (list.Devices is null)
                    {
                        return (null, null);
                    }
                    else
                    {
                        return (list.Devices.ToList<Device>(), null);
                    }

                }
                catch (Exception e)
                {
                    return (null, e.ToString());
                }
            }
        }

        public static string TriggerMotionDetector(string deviceIDx, bool On)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                String onStatus = "";
                if (On)
                {
                    onStatus = "On";
                }
                else
                {
                    onStatus = "Off";
                }
                HttpResponseMessage response = client.GetAsync("/json.htm?type=command&param=switchlight&idx=" + deviceIDx + "&switchcmd=" + onStatus).Result;
                response.EnsureSuccessStatusCode();
                string logresult = response.Content.ReadAsStringAsync().Result;
                return logresult;
            }
        }

        public static List<(long, string)> GetLogInfo()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri(url);
                HttpResponseMessage response = client.GetAsync("json.htm?type=command&param=getlog&lastlogtime=0&loglevel=268435455").Result;
                response.EnsureSuccessStatusCode();
                string logresult = response.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<LogMessage>(logresult);

                List<(long, string)> finalResult = new List<(long, string)>();
                foreach (Log dat in list.Result)
                {
                    finalResult.Add((dat.Level, dat.Message));
                }
                return finalResult;
            }
        }
    }
}
