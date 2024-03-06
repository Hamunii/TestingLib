using System;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using MonoMod.RuntimeDetour;
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

            Instances.Init();
            OnEvent.Init();
        }
    }
}