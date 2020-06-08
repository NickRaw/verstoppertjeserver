using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class RoomDevices
    {
        [JsonProperty("result")]
        public Device[] Devices { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
