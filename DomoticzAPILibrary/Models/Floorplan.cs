using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class Floorplan
    {
        [JsonProperty("Image")]
        public string Image { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Order")]
        public long Order { get; set; }

        [JsonProperty("Plans")]
        public long Plans { get; set; }

        [JsonProperty("ScaleFactor")]
        public string ScaleFactor { get; set; }

        [JsonProperty("idx")]
        public long Idx { get; set; }
    }
}
