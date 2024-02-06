using UnityEngine;
using System.Collections.Generic;
using GameNetcodeStuff;
using System.Collections;

namespace TestingLib {
    /// <summary>
    /// Contains helpful methods for testing.
    /// </summary>
    public class Tools {
        /// <summary>
        /// Specify the Teleport Location in the test level.
        /// </summary>
        public enum TeleportLocation {
            /// <summary>
            /// Teleports you inside the testing maze.
            /// </summary>
            Inside = 1,
            /// <summary>
            /// Teleports you outside in the testing level.
            /// </summary>
            Outside = 2,
        }

        /// <summary>
        /// Teleports you to the location specified in the test level.
        /// </summary>
        /// <param name="location"></param>
        public static void TeleportSelf(TeleportLocation location = 0) {
            switch(location){
                case TeleportLocation.Inside:
                    Plugin.Logger.LogInfo("Tools: Teleport Inside");
                    GameNetworkManager.Instance.localPlayerController.transform.position = new Vector3(8f, -173.6f, -32f);
                    // Some Enemy AI targetting methods might not work as intended unless we do this
                    GameNetworkManager.Instance.localPlayerController.isInsideFactory = true;
                    break;
                case TeleportLocation.Outside:
                    Plugin.Logger.LogInfo("Tools: Teleport Outside");
                    GameNetworkManager.Instance.localPlayerController.transform.position = new Vector3(50f, -5.4f, 0f);
                    break;
            }
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
        }

        /// <summary>
        /// Will find the enemy by name, and spawn it.
        /// </summary>
        /// <param name="enemyName"></param>
        public static void SpawnEnemyInFrontOfSelf(string enemyName) {
            Plugin.Logger.LogInfo($"Tools: SpawnEnemyInFrontOfSelf");
            Vector3 spawnPosition = GameNetworkManager.Instance.localPlayerController.transform.position - Vector3.Scale(new Vector3(-5, 0, -5), GameNetworkManager.Instance.localPlayerController.transform.forward);
            // This might be bad code
            var allEnemiesList = new List<SpawnableEnemyWithRarity>();
            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
            allEnemiesList.AddRange(RoundManager.Instance.currentLevel.DaytimeEnemies);
            var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Equals(enemyName)).enemyType;
            RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, enemyToSpawn);
        }

        /// <summary>
        /// Give an item to yourself.
        /// </summary>
        public static void GiveItemToSelf(string itemName) {
            var itemToGive = StartOfRound.Instance.allItemsList.itemsList.Find(x => x.itemName.Equals(itemName));
            GameObject obj = Object.Instantiate(itemToGive.spawnPrefab, GameNetworkManager.Instance.localPlayerController.transform.position, Quaternion.identity, StartOfRound.Instance.propsContainer);
            GrabbableObject _obj = obj.GetComponent<GrabbableObject>();
            _obj.fallTime = 0f;
            _obj.NetworkObject.Spawn();
            _obj.StartCoroutine(WaitAndGrabObject(obj, _obj));
        }
        private static IEnumerator WaitAndGrabObject(GameObject obj, GrabbableObject _obj){
            // Stuff happens on the object's Start() method, so well have to wait for it to run first.
            yield return new WaitForEndOfFrame();
            _obj.InteractItem();
            PlayerControllerB self = GameNetworkManager.Instance.localPlayerController;
            if(GameNetworkManager.Instance.localPlayerController.FirstEmptyItemSlot() == -1){
                Plugin.Logger.LogInfo("GiveItemToSelf: Could not grab item, inventory full!");
                yield break;
            }
            self.twoHanded = _obj.itemProperties.twoHanded;
            self.carryWeight += Mathf.Clamp(_obj.itemProperties.weight - 1f, 0f, 10f);
            self.grabbedObjectValidated = true;
            self.GrabObjectServerRpc(_obj.NetworkObject);
            _obj.GrabItemOnClient();
            _obj.parentObject = self.localItemHolder;
            self.isHoldingObject = true;
			_obj.hasBeenHeld = true;
            _obj.EnablePhysics(false);
        }

        /// <summary>
        /// Runs all methods in <c>TestingLib.Patch</c> and <c>TestingLib.Execute</c>:
        /// <br/>
        /// <br/><c>Patch.IsEditor()</c>
        /// <br/><c>Patch.SkipSpawnPlayerAnimation()</c>
        /// <br/><c>Patch.OnDeathHeal()</c>
        /// <br/><c>Patch.MovementCheat()</c>
        /// <br/><c>Patch.InfiniteSprint()</c>
        /// <br/><c>Patch.InfiniteCredits()</c>
        /// <br/><c>Patch.InfiniteShotgunAmmo()</c>
        /// <br/><c>Execute.ToggleTestRoom()</c> // runs on <c>OnEvent.PlayerSpawn</c>
        /// </summary>
        public static void RunAllPatchAndExecuteMethods() {
            Patch.IsEditor();
            Patch.SkipSpawnPlayerAnimation();
            Patch.OnDeathHeal();
            Patch.MovementCheat();
            Patch.InfiniteSprint();
            Patch.InfiniteCredits();
            Patch.InfiniteShotgunAmmo();
            // Execute methods
            OnEvent.PlayerSpawn -= OnEvent_PlayerSpawn;
            OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;
        }
        private static void OnEvent_PlayerSpawn() {
            Execute.ToggleTestRoom();
        }
    }
}