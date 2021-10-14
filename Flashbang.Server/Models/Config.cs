using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flashbang.Server.Models
{
    [JsonObject]
    public class Config
    {
        [JsonProperty("stuntime")]
        public int StunTime { get; protected set; } = 8;
        [JsonProperty("aftertime")]
        public int AfterTime { get; protected set; } = 8;
        [JsonProperty("range")]
        public float Range { get; protected set; } = 8f;
        [JsonProperty("damage")]
        public int Damage { get; protected set; } = 25; 
        [JsonProperty("lethalrange")]
        public float LethalRange { get; protected set; } = 1.6f; 
        public Config() { }
    }
}
