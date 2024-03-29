﻿using static TestingLib.Attributes;

namespace TestingLib {
    /// <summary>
    /// Contains actions that can be executed.
    /// </summary>
    [DevTools(Visibility.Whitelist, Available.PlayerSpawn)]
    public class Execute {
        /// <summary>
        /// Toggles the testing level from the debug menu.<br/>
        /// Requires <c>Patch.IsEditor</c> to be active for this to work.
        /// <br/><br/>
        /// Should be ran on <c>OnEvent.PlayerSpawn</c> or later.
        /// </summary>
        [DevTools(Visibility.ConfigOnly)]
        public static void ToggleTestRoom() {
            Plugin.Logger.LogInfo("Execute: Toggle Test Room");
            Instances.QMM_Instance.Debug_ToggleTestRoom();
            if(StartOfRound.Instance.testRoom){
                On.StartOfRound.SetPlayerSafeInShip += StartOfRound_SetPlayerSafeInShip;
            }
            else{
                On.StartOfRound.SetPlayerSafeInShip -= StartOfRound_SetPlayerSafeInShip;
            }
        }

        private static void StartOfRound_SetPlayerSafeInShip(On.StartOfRound.orig_SetPlayerSafeInShip orig, StartOfRound self) {
            // The game might think we are inside the ship with doors closed and will not render enemies.
            // This is a hacky way to get around that.
            // This is mostly a problem on the test level because we don't actually open the doors there.
            bool isInShip = GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
            if(isInShip){
                GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            }
            orig(self);
            if(isInShip){
                GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = isInShip;
            }
        }
    }
}