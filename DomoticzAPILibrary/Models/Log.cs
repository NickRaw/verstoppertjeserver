using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class Log
    {
        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
