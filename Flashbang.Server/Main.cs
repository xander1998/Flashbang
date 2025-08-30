global using CitizenFX.Core;
global using CitizenFX.Core.Native;
using Flashbang.Server.Models;
using Flashbang.Shared;
using Newtonsoft.Json;
using System;

namespace Flashbang.Server
{
    public class Main : BaseScript
    {
        private Config _config = new();
        
        public Main()
        {
            _config.Load();

            EventHandlers["Flashbang:DispatchExplosion"] += new Action<string>(OnFlashbangMessageAsync);
        }

        private void OnFlashbangMessageAsync(string jsonMessage)
        {
            FlashbangMessage flashbangMessage = JsonConvert.DeserializeObject<FlashbangMessage>(jsonMessage);
            flashbangMessage.StunDuration = _config.StunDuration;
            flashbangMessage.AfterStunDuration = _config.AfterStunDuration;
            flashbangMessage.Range = _config.Range;
            flashbangMessage.Damage = _config.Damage;
            flashbangMessage.LethalRadius = _config.LethalRadius;
            
            string jsonFlashbangMessage = JsonConvert.SerializeObject(flashbangMessage);
            TriggerClientEvent("Flashbang:Explode", jsonFlashbangMessage);
        }
    }
}
