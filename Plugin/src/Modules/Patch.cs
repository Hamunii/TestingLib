using UnityEngine;
using HarmonyLib;

namespace TestingLib {
    /// <summary>
    /// This module contains methods to patch methods and variables in the game.
    /// </summary>
    public class Patch {
        private static bool shouldDebug = false;
        internal static bool shouldSkipSpawnPlayerAnimation = false;
        /// <summary>
        /// Patches all methods in this module:
        /// <br/>
        /// <br/><c>isEditor()</c>
        /// <br/><c>SkipSpawnPlayerAnimation()</c>
        /// <br/><c>InfiniteSprint()</c>
        /// </summary>
        public static void All(){
            isEditor();
            SkipSpawnPlayerAnimation();
            InfiniteSprint();
        }
        /// <summary>
        /// Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.
        /// </summary>
        public static void isEditor(){
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
        /// Skips the spawn player animation so you can start moving and looking around as soon as you spawn.
        /// </summary>
        public static void SkipSpawnPlayerAnimation(){
            shouldSkipSpawnPlayerAnimation = true;
        }
        /// <summary>
        /// Patches the game to allow infinite sprinting by always setting SprintMeter to full.
        /// </summary>
        public static void InfiniteSprint(){
            // Make sure we don't subscribe to event twice
            On.GameNetcodeStuff.PlayerControllerB.Update -= PlayerControllerB_Update;
            On.GameNetcodeStuff.PlayerControllerB.Update += PlayerControllerB_Update;
        }
        private static void PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            orig(self);
            self.sprintMeter = 1;
        }
    }
}