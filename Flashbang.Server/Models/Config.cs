using Newtonsoft.Json;

namespace Flashbang.Server.Models
{
    public class Config
    {
        internal Config _config;

        [JsonProperty("stunDuration")]
        public int StunDuration { get; protected set; } = 8;
        
        [JsonProperty("afterStunDuration")]
        public int AfterStunDuration { get; protected set; } = 8;

        [JsonProperty("range")]
        public float Range { get; protected set; } = 8f;

        [JsonProperty("damage")]
        public int Damage { get; protected set; } = 25;

        [JsonProperty("lethalRadius")]
        public float LethalRadius { get; protected set; } = 1.6f;

        [JsonProperty("maxUpdateRange")]
        public float MaxUpdateRange { get; protected set; } = 50f;

        public Config Load()
        {
            if (_config != null) return _config;
            
            string json = API.LoadResourceFile(API.GetCurrentResourceName(), "config.json");
            if (string.IsNullOrEmpty(json))
            {
                Debug.WriteLine("^1Flashbang Config file not found, using default values.");
                return _config = new Config();
            }

            return _config = JsonConvert.DeserializeObject<Config>(json);
        }
    }
}
