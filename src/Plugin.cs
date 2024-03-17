using BepInEx;
using BepInEx.Logging;
using TestingLib.Internal;
using TestingLib.Modules;

namespace TestingLib {
    /// <summary>
    /// Plugin class.
    /// </summary>
    [BepInPlugin(ModGUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        /// <summary>
        /// The Mod GUID of TestingLib.
        /// </summary>
        public const string ModGUID = "hamunii.testinglib";
        internal static new ManualLogSource Logger = null!;
        private void Awake() {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
            // Patch.SkipSpawnPlayerAnimation.Enabled = true;

            Instances.Init();
            OnEvent.Init();
        }

        // private void Enemies_Terminal_Start(On.LethalLib.Modules.Enemies.orig_Terminal_Start orig, object orig2, Terminal self)
        // {
        //     Logger.LogInfo("Hello1");
        //     orig(orig2, self);
        //     Logger.LogInfo("Hello2");
        // }

        // private static void Plugin_Awake(On.AutoStart.Plugin.orig_Awake orig, BaseUnityPlugin self)
        // {
        //     Logger.LogInfo("Hello from Autostart!");
        //     orig(self);
        // }
    }
}