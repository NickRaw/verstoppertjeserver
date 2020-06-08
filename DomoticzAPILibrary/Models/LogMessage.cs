using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class LogMessage
    {
        [JsonProperty("LastLogTime")]
        public long LastLogTime { get; set; }
        
        [JsonProperty("result")]
        public Log[] Result { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
