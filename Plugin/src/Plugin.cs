using BepInEx;
using HarmonyLib;
using BepInEx.Logging;

namespace TestingLib {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public static Harmony _harmony;
        public const string ModGUID = "testinglib";//MyPluginInfo.PLUGIN_GUID;
        internal static new ManualLogSource Logger;
        private void Awake() {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(typeof(Patch));

            Macro.OnPlayerSpawn.Init();
        }
    }
}