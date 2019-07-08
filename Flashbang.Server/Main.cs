using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Flashbang.Server.Models;

namespace Flashbang.Server
{
    public class Main : BaseScript
    {
        private Config config = new Config();

        public Main()
        {
            EventHandlers.Add("Flashbang:DispatchExplosion", new Action<float, float, float>(FB_DispatchExplosion));
            //LoadConfig();
        }

        private void LoadConfig()
        {
            string resourceName = API.GetCurrentResourceName();
            string json = API.LoadResourceFile(resourceName, "config.json");
            config = JsonConvert.DeserializeObject<Config>(json);
        }

        private void FB_DispatchExplosion(float x, float y, float z)
        {
            TriggerClientEvent("Flashbang:Explode", x, y, z, config.Timer, config.Range);
        }
    }
}
