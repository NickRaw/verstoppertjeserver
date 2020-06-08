using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomoticzAPILibrary.Models
{
    public class FloorplanResults
    {
        [JsonProperty("ActiveRoomOpacity")]
        public long ActiveRoomOpacity { get; set; }

        [JsonProperty("AnimateZoom")]
        public long AnimateZoom { get; set; }

        [JsonProperty("FullscreenMode")]
        public long FullscreenMode { get; set; }

        [JsonProperty("InactiveRoomOpacity")]
        public long InactiveRoomOpacity { get; set; }

        [JsonProperty("PopupDelay")]
        public long PopupDelay { get; set; }

        [JsonProperty("RoomColour")]
        public string RoomColour { get; set; }

        [JsonProperty("ShowSceneNames")]
        public long ShowSceneNames { get; set; }

        [JsonProperty("ShowSensorValues")]
        public long ShowSensorValues { get; set; }

        [JsonProperty("ShowSwitchValues")]
        public long ShowSwitchValues { get; set; }

        [JsonProperty("result")]
        public Floorplan[] Floorplans { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
