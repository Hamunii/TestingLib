using GameNetcodeStuff;
using UnityEngine;

namespace TestingLib {
    public class Macro {
        public class OnPlayerSpawn {
            private static QuickMenuManager qmmInstance;
            public enum TeleportLocation {
                Inside = 1,
                Outside = 2,
            }
            private static TeleportLocation tpLocation = 0;
            private static EnemyType spawnEnemyType;
            internal static void Init(){
                On.QuickMenuManager.Debug_SetAllItemsDropdownOptions += QuickMenuManager_Debug_SetAllItemsDropdownOptions;
            }
            private static void QuickMenuManager_Debug_SetAllItemsDropdownOptions(On.QuickMenuManager.orig_Debug_SetAllItemsDropdownOptions orig, QuickMenuManager self)
            {
                orig(self);
                Plugin.Logger.LogInfo("Get QuicKMenuManager instance");
                qmmInstance = self;

            }

            public static void ToggleTestRoom() {
                On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation += PlayerControllerB_SpawnPlayerAnimation;
            }

            public static void TeleportSelf(TeleportLocation location = 0) {
                tpLocation = location;
            }

            public static void SpawnEnemyInFrontOfSelf(EnemyType enemy){
                spawnEnemyType = enemy;
            }

            private static void PlayerControllerB_SpawnPlayerAnimation(On.GameNetcodeStuff.PlayerControllerB.orig_SpawnPlayerAnimation orig, GameNetcodeStuff.PlayerControllerB self)
            {
                //We don't run orig(self) because we don't want the be stuck in an animation when loading anyways
                Plugin.Logger.LogInfo("Toggle Debug testroom");
                qmmInstance.Debug_ToggleTestRoom();

                switch(tpLocation){
                    case TeleportLocation.Inside:
                        self.transform.position = new Vector3(8f, -173.6f, -32f);
                        break;
                    case TeleportLocation.Outside:
                        self.transform.position = new Vector3(50f, -5.4f, 0f);
                        break;
                }

                if(spawnEnemyType != null){
                    Vector3 spawnPosition = self.transform.position - Vector3.Scale(new Vector3(-5, 0, -5), self.transform.forward);
                    RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, spawnEnemyType);
                }
            }
        }
    }
}