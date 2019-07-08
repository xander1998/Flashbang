using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Flashbang.Client
{
    public class Main : BaseScript
    {
        private const string AnimDict = "core";
        private const string AnimName = "ent_anim_paparazzi_flash";
        private bool FlashbangEquipped = false;
        private const string WeaponModel = "w_ex_grenadesmoke";

        public Main()
        {
            EventHandlers.Add("Flashbang:Explode", new Action<float, float, float, int, float>(FB_Explode));
            Tick += FB_Tick;
        }

        private async void FB_Thrown(int prop)
        {
            Prop flashbang = (Prop)Entity.FromHandle(prop);
            await Delay(2500);
            Vector3 flashbangPos = flashbang.Position;
            World.AddExplosion(flashbangPos, ExplosionType.ProgramAR, 0f, 1f, null, true, true);
            TriggerServerEvent("Flashbang:DispatchExplosion", flashbangPos.X, flashbangPos.Y, flashbangPos.Z);
            flashbang.Delete();
        }

        private async void FB_Explode(float x, float y, float z, int time, float radius)
        {
            int refTime = time * 1000;
            int finishTime = Game.GameTime + refTime;
            Ped ped = Game.Player.Character;
            Vector3 pos = new Vector3(x, y, z);

            PlayParticles(pos);

            float distance = World.GetDistance(ped.Position, pos);
            if (distance <= radius)
            {
                Screen.Effects.Start(ScreenEffect.DontTazemeBro, 0, true);
                while (Game.GameTime < finishTime)
                {
                    Game.Player.Character.Ragdoll(0, RagdollType.Normal);
                    await Delay(0);
                }
                Screen.Effects.Stop(ScreenEffect.DontTazemeBro);
            }
        }

        private async Task FB_Tick()
        {
            if (!FlashbangEquipped)
            {
                if (Game.Player.Character.Weapons.Current.Hash == (WeaponHash)API.GetHashKey("WEAPON_FLASHBANG"))
                {
                    FlashbangEquipped = true;
                }
            } else
            {
                if (Game.Player.Character.IsShooting)
                {
                    FlashbangEquipped = false;

                    await Delay(100);

                    Vector3 pos = Game.Player.Character.Position;
                    int handle = API.GetClosestObjectOfType(pos.X, pos.Y, pos.Z, 50f, (uint)API.GetHashKey(WeaponModel), false, false, false);

                    if (handle != 0)
                    {
                        FB_Thrown(handle);
                    }
                }
            }
            await Task.FromResult(0);
        }

        private async void PlayParticles(Vector3 pos)
        {
            API.RequestNamedPtfxAsset(AnimDict);
            while (!API.HasNamedPtfxAssetLoaded(AnimDict))
            {
                await Delay(0);
            }
            API.UseParticleFxAssetNextCall(AnimDict);
            API.StartParticleFxLoopedAtCoord(AnimName, pos.X, pos.Y, pos.Z, 0f, 0f, 0f, 25f, false, false, false, false);
        }
    }
}
