using UnityEngine;
using HarmonyLib;

namespace TestingLib {
    public class Patch {
        private static bool shouldDebug = false;
        public static void DebugMenu(){
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