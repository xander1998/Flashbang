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
        private const float maxShakeAmp = 25.0f;
        private const float maxAfterShakeAmp = 18.0f;
        private float totalFlashShakeAmp = 0.0f;
        private float totalAfterShakeAmp = 0.0f;
        private int flashTimersRunning = 0;
        private bool shakeCamActive = false;
        private int afterTimersRunning = 0;
        private const string WeaponModel = "w_ex_flashbang";


        private string[] Animation = new string[] { "anim@heists@ornate_bank@thermal_charge", "cover_eyes_intro" };

        public Main()
        {
            EventHandlers.Add("Flashbang:Explode", new Action<float, float, float, int, int, float, int, int, float>(FB_Explode));
            Tick += FB_Tick;
            FB_LoadWeaponEntry();
        }

        private void FB_LoadWeaponEntry()
        {
            if (API.IsWeaponValid(4221696920))
            {
                API.AddTextEntry("WT_GNADE_FLSH", "Flashbang");
            }
        }

        private async void FB_Thrown(int prop)
        {
            Prop flashbang = (Prop)Entity.FromHandle(prop);
            await Delay(1500);
            Vector3 flashbangPos = flashbang.Position;
            World.AddExplosion(flashbangPos, ExplosionType.ProgramAR, 0f, 1f, null, true, true);
            TriggerServerEvent("Flashbang:DispatchExplosion", flashbangPos.X, flashbangPos.Y, flashbangPos.Z, prop);
            flashbang.Delete();
        }

        private async void drawLine (float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a, int t) 
        {
            int current = 0;  
            while (current <= t)
             {
                API.DrawLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
                await Delay(1);
                current ++;
             }
        }

        private async void cout (string text) 
        {
            CitizenFX.Core.Debug.WriteLine(text);
        }

        private void enableShakeCam (float amp)
        {
            if (shakeCamActive)
            {
                GameplayCamera.ShakeAmplitude = amp;
            }
            else
            {
                GameplayCamera.Shake(CameraShake.Hand, amp);
                shakeCamActive = true;
            }
        }

        private async void shakeCamFalloff (float amp) 
        {
            float currentAmp = amp;
            float ampInterval = amp/2;
            while (currentAmp > 0.0f)
             {
                currentAmp = currentAmp - ampInterval;
                GameplayCamera.ShakeAmplitude = currentAmp;
                await Delay(500);
             }
            GameplayCamera.StopShaking();
            shakeCamActive = false;
        }

        private async void disableFiring (int time)
        {
            int finTime = Game.GameTime + time;
            while (Game.GameTime < finTime)
            {
                await Delay(0);
                Game.Player.DisableFiringThisFrame();
            }
        }

        private float capFlashShakeAmp (float amp)
        {
            if (amp < 0.0f)
            {
                return (0.0f);
            }
            else
            {
                if (amp > maxShakeAmp) 
                {
                    return (maxShakeAmp);
                }
                else 
                {
                    return (amp);
                }
            }
        }

        private float capAfterShakeAmp (float amp)
        {
            if (amp < 0.0f)
            {
                return (0.0f);
            }
            else
            {
                if (amp > maxAfterShakeAmp) 
                {
                    return (maxAfterShakeAmp);
                }
                else 
                {
                    return (amp);
                }
            }
        }

        private async void afterEffect (float shakeAmp, int time)
        {
            // Buffs
            afterTimersRunning++;
            totalAfterShakeAmp += shakeAmp;
            enableShakeCam(capAfterShakeAmp(totalAfterShakeAmp));

            // Wait
            await Delay(time);

            // Debuffs
            afterTimersRunning--;
            totalAfterShakeAmp -= shakeAmp;

            // Cleanup
            if (flashTimersRunning == 0)
            {
                enableShakeCam(capAfterShakeAmp(totalAfterShakeAmp));
                if ((afterTimersRunning == 0) && (flashTimersRunning == 0))
                {
                    shakeCamFalloff(totalFlashShakeAmp + shakeAmp);
                    API.AnimpostfxStop("Dont_tazeme_bro");
                } 
            }
        }

        private async void flashEffect (float shakeAmp, int time, float afterShakeAmp, int afterTime)
        {   
            Ped ped = Game.Player.Character;

            // Buffs
            flashTimersRunning++;
            totalFlashShakeAmp += shakeAmp;
            disableFiring(time);
            enableShakeCam(capFlashShakeAmp(totalFlashShakeAmp));

            // Animation and screen effect
            if (flashTimersRunning == 1)
            {
                API.AnimpostfxPlay("Dont_tazeme_bro", 0, true);
                await ped.Task.PlayAnimation(Animation[0], Animation[1], -8f, -8f, -1, AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly | AnimationFlags.AllowRotation, 8f);
            }            

            // Wait
            await Delay(time);

            // Debuffs
            flashTimersRunning--;
            totalFlashShakeAmp -= shakeAmp;
            enableShakeCam(capFlashShakeAmp(totalFlashShakeAmp));

            // Cleanup
            if (flashTimersRunning == 0)
            {
                ped.Task.ClearAnimation(Animation[0], Animation[1]);
                afterEffect(afterShakeAmp, afterTime);
            } 
        }

        private async void checkLethalRadius(float lethRange, float x, float y, float z, int damage)
        {   
            Ped ped = Game.Player.Character;
            Vector3 pos = new Vector3(x, y, z);
            float distance = World.GetDistance(ped.Position, pos);

            //cout("Damage: " + damage.ToString());
            //cout("Lethal Range: " + lethRange.ToString());
            //cout("Distance: " + distance.ToString());

            if (distance <= lethRange)
            {
                 API.ApplyDamageToPed(API.GetPlayerPed(-1), damage, false);
                 cout("Applying Damage Amount: " + damage.ToString());
            }
        }

        private async void FB_Explode(float x, float y, float z, int stunTime, int afterTime, float radius, int prop, int damage, float lethalRange)
        {
            bool hit = false;
            int entityHit = 0;
            int result = 1;
            bool playerHit = false;
            Vector3 hitPos = new Vector3();
            Vector3 surfaceNormal = new Vector3();
            Ped ped = Game.Player.Character;
            int pedHandle = API.PlayerPedId();
            Vector3 pedPos = API.GetPedBoneCoords(API.PlayerPedId(), 0x62ac, 0, 0, 0);
            Vector3 pedPos2 = API.GetPedBoneCoords(API.PlayerPedId(), 0x6b52, 0, 0, 0);
            Vector3 pos = new Vector3(x, y, z);
            Vector3 hitReg = new Vector3(pedPos.X-x, pedPos.Y-y, pedPos.Z-z); // Richtungsvektor
            PlayParticles(pos);

            float distance = World.GetDistance(ped.Position, pos);
            float faceDistance = World.GetDistance(pedPos, pos);
            float distanceSq = distance * distance;

            float effectFalloffMultiplier = 0.02f / (radius / 8.0f); // Radius ~< effectFalloffMultiplier
            float stunTimeMultiplier = effectFalloffMultiplier * distanceSq; // Distanz ~> effectFalloffMultiplier
            int effectFalloffStunTime;
            int effectFalloffAfterTime;
            float shakeCamAmp = 15.0f;
            effectFalloffStunTime = (int)(((float)stunTime) * stunTimeMultiplier); // Distanz ~> FalloffStunTime
            effectFalloffAfterTime = (int)(((float)afterTime) * stunTimeMultiplier);
            
            int actualStunTime = (stunTime - effectFalloffStunTime) * 1000;
            int actualAfterTime =  (afterTime - effectFalloffAfterTime) * 1000;
            float stunTimeFallOffMultiplier= ((float) actualStunTime) / ((float)(stunTime * 1000));

            if (actualStunTime <= 0)
            {
                actualStunTime = 1;
            }

             if (actualAfterTime <= 0)
            {
                actualAfterTime = 1;
            }

            //cout("StunTime: " + actualStunTime.ToString());
            //cout("AfterTime: " + actualAfterTime.ToString());
            //drawLine(x, y, z, pedPos.X + (10*hitReg.X), pedPos.Y + (10*hitReg.Y), pedPos.Z + (10*hitReg.Z), 255, 0, 0, 255 ,1000);
            //drawLine(x, y, z, pedPos2.X + (10*hitReg.X), pedPos2.Y + (10*hitReg.Y), pedPos2.Z + (10*hitReg.Z), 255, 0, 0, 255 ,1000);
            int handle = 0;
            handle = API.StartShapeTestLosProbe(x, y, z, pedPos.X + (10*hitReg.X), pedPos.Y + (10*hitReg.Y), pedPos.Z + (10*hitReg.Z), 0b0000_0000_1001_1111, prop, 0b0000_0000_0000_0100);
            
            while (result == 1)
            {
                result = API.GetShapeTestResult(handle, ref hit, ref hitPos, ref surfaceNormal, ref entityHit);
                await Delay(0);
            }
            playerHit = (result == 2) && (entityHit == API.GetPlayerPed(-1));

            handle = 0;
            handle = API.StartShapeTestLosProbe(x, y, z, pedPos2.X + (10*hitReg.X), pedPos2.Y + (10*hitReg.Y), pedPos2.Z + (10*hitReg.Z), 0b0000_0000_1001_1111, prop, 0b0000_0000_0000_0100);
            
            while (handle == 1)
            {
                API.GetShapeTestResult(handle, ref hit, ref hitPos, ref surfaceNormal, ref entityHit);
                await Delay(0);
            }
            playerHit = playerHit || ((result == 2) && (entityHit == API.GetPlayerPed(-1)));

            if ((faceDistance <= radius) && playerHit)
            {
                checkLethalRadius(lethalRange, x, y, z, damage);

                // https://wiki.gtanet.work/index.php?title=Screen_Effects
                //Screen.Effects.Start(ScreenEffect.DontTazemeBro, 0, true);
                
                flashEffect(shakeCamAmp * stunTimeFallOffMultiplier, actualStunTime, 10f * stunTimeFallOffMultiplier, actualAfterTime);
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
