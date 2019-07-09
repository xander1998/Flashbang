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
        private string[] Animation = new string[] { "anim@heists@ornate_bank@thermal_charge", "cover_eyes_intro" };

        public Main()
        {
            EventHandlers.Add("Flashbang:Explode", new Action<float, float, float, int, int, float>(FB_Explode));
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

        private async void FB_Explode(float x, float y, float z, int stunTime, int afterTime, float radius)
        {
            int stunRefTime = stunTime * 1000;
            int afterRefTime = afterTime * 1000;
            int finishTime = 0;
            Ped ped = Game.Player.Character;
            Vector3 pos = new Vector3(x, y, z);

            PlayParticles(pos);

            float distance = World.GetDistance(ped.Position, pos);
            if (distance <= radius)
            {
                Screen.Effects.Start(ScreenEffect.DontTazemeBro, 0, true);
                GameplayCamera.Shake(CameraShake.Hand, 15f);
                await ped.Task.PlayAnimation(Animation[0], Animation[1], -8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly | AnimationFlags.AllowRotation, 8f);
                finishTime = Game.GameTime + stunRefTime;
                while (Game.GameTime < finishTime)
                {
                    Game.Player.DisableFiringThisFrame();
                    await Delay(0);
                }
                ped.Task.ClearAnimation(Animation[0], Animation[1]);
                GameplayCamera.ShakeAmplitude = 10f;
                finishTime = Game.GameTime + afterRefTime;
                while (Game.GameTime < finishTime)
                {
                    await Delay(0);
                }
                GameplayCamera.StopShaking();
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
