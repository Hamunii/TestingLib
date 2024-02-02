using System;

namespace TestingLib {
    /// <summary>
    /// Contains Events that can be subscribed to.
    /// </summary>
    public class OnEvent {
        /// <summary>
        /// Event for when player spawns.
        /// </summary>
        public static event Action PlayerSpawn;
        internal static void Init(){
            On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation += PlayerControllerB_SpawnPlayerAnimation;
        }

        private static void PlayerControllerB_SpawnPlayerAnimation(On.GameNetcodeStuff.PlayerControllerB.orig_SpawnPlayerAnimation orig, GameNetcodeStuff.PlayerControllerB self)
        {
            PlayerSpawn?.Invoke();
            if(Patch.AnyTime.shouldSkipSpawnPlayerAnimation) {
                Plugin.Logger.LogInfo("Patch.AnyTime: SkipSpawnPlayerAnimation");
            }
            else{
                orig(self);
            }
        }
    }
}