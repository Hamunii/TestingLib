using UnityEngine;
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
                Plugin.Logger.LogInfo("Patch.TimeSensitive: Toggle Debug testroom");
                Instances.QMM_Instance.Debug_ToggleTestRoom();
            }
        }

        /// <summary>
        /// Patches all methods in <c>Patch.AnyTime</c> and <c>Patch.TimeSensitive</c>:
        /// <br/>
        /// <br/><c>IsEditor()</c>
        /// <br/><c>InfiniteSprint()</c>
        /// <br/><c>SkipSpawnPlayerAnimation()</c>
        /// <br/><c>ToggleTestRoom()</c> // runs on <c>OnEvent.PlayerSpawn</c>
        /// </summary>
        public static void All() {
            AnyTime.IsEditor();
            AnyTime.InfiniteSprint();
            AnyTime.SkipSpawnPlayerAnimation();
            OnEvent.PlayerSpawn -= OnEvent_PlayerSpawn;
            OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;
        }
        private static void OnEvent_PlayerSpawn() {
            TimeSensitive.ToggleTestRoom();
        }
    }
}