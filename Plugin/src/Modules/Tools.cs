using UnityEngine;
using System.Collections.Generic;

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
                    break;
                case TeleportLocation.Outside:
                    Plugin.Logger.LogInfo("Tools: Teleport Outside");
                    GameNetworkManager.Instance.localPlayerController.transform.position = new Vector3(50f, -5.4f, 0f);
                    break;
            }
        }

        /// <summary>
        /// Will find the enemy by name, and spawn it. Limitation: will only spawn one enemy.
        /// <br/><br/>
        /// Previously was somewhat broken: enemy might have appeared invisible. No idea if I fixed it or not.
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
            var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Contains(enemyName)).enemyType;
            RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, enemyToSpawn);
        }     
    }
}