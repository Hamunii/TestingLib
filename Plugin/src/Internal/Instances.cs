namespace TestingLib {
    /// <summary>
    /// Contains instances of some things in the game. 
    /// </summary>
    internal class Instances {
        /// <summary>
        /// Instance of Quick Menu Manager.
        /// </summary>
        internal static QuickMenuManager QMM_Instance;
        internal static void Init(){
            On.QuickMenuManager.Start += QuickMenuManager_Start;
        }

        private static void QuickMenuManager_Start(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
        {
            QMM_Instance = self;
            orig(self);
        }
    }
}