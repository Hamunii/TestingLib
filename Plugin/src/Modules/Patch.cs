﻿using UnityEngine;
using HarmonyLib;

namespace TestingLib {
    /// <summary>
    /// Contains patching methods that can be run at any time or are time sensitive.
    /// </summary>
    public class Patch {
        /// <summary>
        /// Contains patching methods that can be run at any time.
        /// </summary>
        public class AnyTime {
            private static bool shouldDebug = false;
            internal static bool shouldSkipSpawnPlayerAnimation = false;

            /// <summary>
            /// Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.
            /// </summary>
            public static void IsEditor(){
                shouldDebug = true;
            }
            [HarmonyPatch(typeof(Application), "isEditor", MethodType.Getter)]
            [HarmonyPostfix]
            private static void Application_isEditor_Postfix(ref bool __result)
            {
                if(!shouldDebug) return;
                __result = true;
            }

            /// <summary>
            /// Patches the game to allow infinite sprinting by always setting SprintMeter to full.
            /// </summary>
            public static void InfiniteSprint(){
                // Make sure we don't subscribe to event twice
                On.GameNetcodeStuff.PlayerControllerB.Update -= InfiniteSprint_PlayerControllerB_Update;
                On.GameNetcodeStuff.PlayerControllerB.Update += InfiniteSprint_PlayerControllerB_Update;
            }
            private static void InfiniteSprint_PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
            {
                orig(self);
                self.sprintMeter = 1;
            }

            /// <summary>
            /// Skips the spawn player animation so you can start moving and looking around as soon as you spawn.
            /// </summary>
            public static void SkipSpawnPlayerAnimation(){
                shouldSkipSpawnPlayerAnimation = true;
            }

            /// <summary>
            /// Instead of dying, set health to full instead.
            /// <br/><br/>
            /// This helps with testing taking damage from your enemy, without death being a concern.
            /// </summary>
            public static void OnDeathHeal(){
                On.GameNetcodeStuff.PlayerControllerB.KillPlayer -= PlayerControllerB_KillPlayer;
                On.GameNetcodeStuff.PlayerControllerB.KillPlayer += PlayerControllerB_KillPlayer;
            }
            private static void PlayerControllerB_KillPlayer(On.GameNetcodeStuff.PlayerControllerB.orig_KillPlayer orig, GameNetcodeStuff.PlayerControllerB self, Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
            {   
                self.health = 100;
                self.MakeCriticallyInjured(enable: false);
                HUDManager.Instance.UpdateHealthUI(self.health, hurtPlayer: false);
                // Lazy hacky method to get rid of the broken glass effect.
                self.DamagePlayer(damageNumber: 0, hasDamageSFX: false);
            }
        }
        
        /// <summary>
        /// Contains patching methods that can't be run at any moment, mostly too early.
        /// <br/><br/>
        /// Every method in this class has information on when it can/should be run.
        /// </summary>
        public class TimeSensitive {
            /// <summary>
            /// Toggles the testing level from the debug menu.
            /// <br/><br/>
            /// Should be ran on <c>OnEvent.PlayerSpawn</c> or later.
            /// </summary>
            public static void ToggleTestRoom() {
                Plugin.Logger.LogInfo("Patch.TimeSensitive: Toggle Test Room");
                Instances.QMM_Instance.Debug_ToggleTestRoom();
                if(StartOfRound.Instance.testRoom){
                    On.StartOfRound.SetPlayerSafeInShip += StartOfRound_SetPlayerSafeInShip;
                }
                else{
                    On.StartOfRound.SetPlayerSafeInShip -= StartOfRound_SetPlayerSafeInShip;
                }
            }

            private static void StartOfRound_SetPlayerSafeInShip(On.StartOfRound.orig_SetPlayerSafeInShip orig, StartOfRound self) {
                bool isInShip = GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
                if(isInShip){
                    Plugin.Logger.LogInfo("ToggleTestRoom Hack: Prevent disabling of enemy meshes in StartOfRound.SetPlayerSafeInShip()");
                    GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
                }
                orig(self);
                if(isInShip){
                    Plugin.Logger.LogInfo("Restore isInHangarShipRoom");
                    GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = isInShip;
                }
            }
        }

        /// <summary>
        /// Patches all methods in <c>Patch.AnyTime</c> and <c>Patch.TimeSensitive</c>:
        /// <br/>
        /// <br/><c>IsEditor()</c>
        /// <br/><c>InfiniteSprint()</c>
        /// <br/><c>SkipSpawnPlayerAnimation()</c>
        /// <br/><c>OnDeathHeal()</c>
        /// <br/><c>ToggleTestRoom()</c> // runs on <c>OnEvent.PlayerSpawn</c>
        /// </summary>
        public static void All() {
            AnyTime.IsEditor();
            AnyTime.InfiniteSprint();
            AnyTime.SkipSpawnPlayerAnimation();
            AnyTime.OnDeathHeal();
            // TimeSensitive methods
            OnEvent.PlayerSpawn -= OnEvent_PlayerSpawn;
            OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;
        }
        private static void OnEvent_PlayerSpawn() {
            TimeSensitive.ToggleTestRoom();
        }
    }
}