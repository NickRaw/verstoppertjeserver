using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class Device
    {
        [JsonProperty("DevSceneRowID")]
        public long DevSceneRowId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("devidx")]
        public long Devidx { get; set; }

        [JsonProperty("idx")]
        public long Idx { get; set; }

        [JsonProperty("order")]
        public long Order { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }
    }
}
