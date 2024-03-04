using Unity.Netcode;
using UnityEngine;
using static TestingLib.Attributes;

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
            bool isServer = GameNetworkManager.Instance.localPlayerController.IsServer;
            if(isServer){
                Instances.QMM_Instance.Debug_ToggleTestRoom();
            }
            else{
                // Basically Debug_EnableTestRoomClientRpc, except I'm lazy
                bool shouldEnable = Instances.QMM_Instance.outOfBoundsCollider.enabled == true; // StartOfRound.Instance.testRoom == null;
                var testRoom = GameObject.Find("TestRoom(Clone)");

                for (int i = 0; i < Instances.QMM_Instance.doorGameObjects.Length; i++)
                {
                    Instances.QMM_Instance.doorGameObjects[i].SetActive(!shouldEnable);
                }
                Instances.QMM_Instance.outOfBoundsCollider.enabled = !shouldEnable;
                if (shouldEnable)
                {
                    if(testRoom == null){
                        Plugin.Logger.LogInfo($"Could not find testroom!");
                        return;
                    }
                    // We do this so testRoom is not null in the next few frames so we can teleport asap
                    StartOfRound.Instance.testRoom = testRoom;

                    StartOfRound.Instance.StartCoroutine(StartOfRound.Instance.SetTestRoomDebug(testRoom.GetComponent<NetworkObject>()));
                }
            }

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