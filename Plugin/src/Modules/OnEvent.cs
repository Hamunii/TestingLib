using System;
using System.Threading.Tasks;

namespace TestingLib {
    /// <summary>
    /// Contains Events that can be subscribed to.
    /// </summary>
    public class OnEvent {
        /// <summary>
        /// Event for when player spawns.
        /// </summary>
        public static event Action PlayerSpawn;
        /// <summary>
        /// Event 20 milliseconds after when player spawns.
        /// <br/><br/>
        /// <b>Why:</b> Sometimes, what feels like a chance of 1/8, enemies don't get rendered until player steps on the ship.<br/>
        /// This is if we teleport from the ship immediately, and that causes the game to sometimes not register that the player ever stood on the ship.
        /// So as a hacky workaround for now, we are waiting 20 ms to make sure the game has registered that the player is on the ship.
        /// <br/><br/>
        /// If you can find the part in the game's code which causes this (and how to make it think the player was on the ship), please open an issue or a pull request.
        /// </summary>
        public static event Action PlayerSpawn_20ms_delay;
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
            if(PlayerSpawn_20ms_delay != null) _ = Wait20ms();
        }
        private static async Task Wait20ms() {
            await Task.Delay(20);
            PlayerSpawn_20ms_delay?.Invoke();
        }
    }
}