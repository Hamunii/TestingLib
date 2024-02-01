using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

namespace TestingLib {
    /// <summary>
    /// Contains macros for things to happen at certain moments. 
    /// </summary>
    public class Macro {
        /// <summary>
        /// Macros that happen as soon as the player spawns. 
        /// </summary>
        public class OnPlayerSpawn {
            /// <summary>
            /// Event for OnPlayerSpawn.
            /// </summary>
            public static event Action OnEvent;
            private static QuickMenuManager QMM_Instance;
            private static bool shouldToggleTestRoom = false;
            /// <summary>
            /// Specify the Teleport Location in the test level.
            /// </summary>
            public enum TeleportLocation {
                /// <summary>
                /// Teleports you inside the testing maze.
                /// </summary>
                Inside = 1,
                /// <summary>
                /// Teleports you outside in the testing level.
                /// </summary>
                Outside = 2,
            }

            private static TeleportLocation tpLocation = 0;
            private static string spawnEnemyName;
            internal static void Init(){
                On.QuickMenuManager.Debug_SetAllItemsDropdownOptions += QuickMenuManager_Debug_SetAllItemsDropdownOptions;
                On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation += PlayerControllerB_SpawnPlayerAnimation;
            }
            private static void QuickMenuManager_Debug_SetAllItemsDropdownOptions(On.QuickMenuManager.orig_Debug_SetAllItemsDropdownOptions orig, QuickMenuManager self)
            {
                orig(self);
                QMM_Instance = self;
            }
            /// <summary>
            /// Toggles the testing level from the debug menu.
            /// </summary>
            public static void ToggleTestRoom() {
                shouldToggleTestRoom = true;
            }
            /// <summary>
            /// Teleports you to the location specified in the test level.
            /// </summary>
            /// <param name="location"></param>
            public static void TeleportSelf(TeleportLocation location = 0) {
                tpLocation = location;
            }
            /// <summary>
            /// Will find the enemy by name, and spawn it. Limitation: will only spawn one enemy.
            /// <br/><br/>
            /// Previously was somewhat broken: enemy might have appeared invisible. No idea if I fixed it or not.
            /// </summary>
            /// <param name="enemyName"></param>
            public static void SpawnEnemyInFrontOfSelf(string enemyName){
                spawnEnemyName = enemyName;
            }
            // This is OnPlayerSpawn
            private static void PlayerControllerB_SpawnPlayerAnimation(On.GameNetcodeStuff.PlayerControllerB.orig_SpawnPlayerAnimation orig, GameNetcodeStuff.PlayerControllerB self)
            {
                if(Patch.shouldSkipSpawnPlayerAnimation) {
                    Plugin.Logger.LogInfo("Patch: SkipSpawnPlayerAnimation");
                }
                else{
                    orig(self);
                }

                if(shouldToggleTestRoom) {
                    Plugin.Logger.LogInfo("Macro: Toggle Debug testroom");
                    QMM_Instance.Debug_ToggleTestRoom();
                }

                _ = WaitAndOnPlayerSpawn(self);
            }
            private static async Task WaitAndOnPlayerSpawn(PlayerControllerB self) {
                // ISSUE: Sometimes, maybe a chance of 1/8, enemies don't get rendered until player steps on the ship.
                // I can't find where in the game's logic this is, so I'm waiting 20 ms to make sure the game has
                // registered that the player is on the ship.
                await Task.Delay(20);
                Teleport(self);
                if(spawnEnemyName != null){
                    SpawnEnemy(self.transform);
                }
            }

            private static void Teleport(PlayerControllerB self) {
                switch(tpLocation){
                    case 0:
                        // By default, tpLocation is 0.
                        break;
                    case TeleportLocation.Inside:
                        Plugin.Logger.LogInfo("Macro: Teleport Inside");
                        self.transform.position = new Vector3(8f, -173.6f, -32f);
                        break;
                    case TeleportLocation.Outside:
                        Plugin.Logger.LogInfo("Macro: Teleport Outside");
                        self.transform.position = new Vector3(50f, -5.4f, 0f);
                        break;
                }
                OnEvent?.Invoke();
            }

            private static void SpawnEnemy(Transform selfPos) {
                Plugin.Logger.LogInfo($"Macro: SpawnEnemyInFrontOfSelf");
                Vector3 spawnPosition = selfPos.position - Vector3.Scale(new Vector3(-5, 0, -5), selfPos.forward);
                // This might be bad code
                var allEnemiesList = new List<SpawnableEnemyWithRarity>();
                allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
                allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
                allEnemiesList.AddRange(RoundManager.Instance.currentLevel.DaytimeEnemies);
                var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Contains(spawnEnemyName)).enemyType;
                RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, enemyToSpawn);
            }
        }
    }
}