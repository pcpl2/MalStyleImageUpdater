using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalStyleImageUpdater.Models
{
    class SettingsModel
    {
        [JsonProperty]
        public string username { get; set; }

        [JsonProperty]
        public string animeImagePath { get; set; }

        [JsonProperty]
        public string mangaImagePath { get; set; }

        [JsonProperty]
        public Boolean minFileActivate { get; set; }

        [JsonProperty]
        public string animeImageMinPath { get; set; }

        [JsonProperty]
        public string mangaImageMinPath { get; set; }
    }
}
