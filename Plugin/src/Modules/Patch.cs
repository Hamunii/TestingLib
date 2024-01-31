using UnityEngine;
using HarmonyLib;

namespace TestingLib {
    /// <summary>
    /// This module contains methods to patch methods and variables in the game.
    /// </summary>
    public class Patch {
        private static bool shouldDebug = false;
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
    }
}