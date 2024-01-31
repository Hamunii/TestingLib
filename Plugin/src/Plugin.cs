using BepInEx;
using HarmonyLib;
using BepInEx.Logging;

namespace TestingLib {
    /// <summary>
    /// Plugin class.
    /// </summary>
    [BepInPlugin(ModGUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        internal static Harmony _harmony;
        /// <summary>
        /// The Mod GUID of TestingLib.
        /// </summary>
        public const string ModGUID = "hamunii.testinglib";//MyPluginInfo.PLUGIN_GUID;
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