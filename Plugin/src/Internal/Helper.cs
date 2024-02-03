using System.Collections.Generic;

namespace TestingLib {
    /// <summary>
    /// Methods that can be helpful, but do nothing by themselves.
    /// </summary>
    internal class Helper {
        /// <summary>
        /// Lists all enemies in current level.
        /// </summary>
        internal static void ListAllEnemies(){
            Plugin.Logger.LogInfo("-- Inside Enemies ---");
            PrintListEnemy(RoundManager.Instance.currentLevel.Enemies);
            Plugin.Logger.LogInfo("-- Outside Enemies --");
            PrintListEnemy(RoundManager.Instance.currentLevel.OutsideEnemies);
            Plugin.Logger.LogInfo("-- Daytime Enemies --");
            PrintListEnemy(RoundManager.Instance.currentLevel.DaytimeEnemies);
        }
        private static void PrintListEnemy(List<SpawnableEnemyWithRarity> listOfEnemies){
            foreach (var enemy_ in listOfEnemies){
                var alteredName = enemy_.enemyType.enemyName.Replace(' ','_');
                alteredName = alteredName.Replace('-', '_');
                Plugin.Logger.LogInfo($"public static readonly string {alteredName} = \"{enemy_.enemyType.enemyName}\";");
            }
            Plugin.Logger.LogInfo("---------------------");
        }
        /// <summary>
        /// Lists all items.
        /// </summary>
        internal static void ListAllItems(){
            Plugin.Logger.LogInfo("-- Items ------------");
            foreach (var item_ in StartOfRound.Instance.allItemsList.itemsList){
                var alteredName = item_.itemName.Replace(' ','_');
                alteredName = alteredName.Replace('-', '_');
                Plugin.Logger.LogInfo($"public static readonly string {alteredName} = \"{item_.itemName}\";");
            }
            Plugin.Logger.LogInfo("---------------------");
        }
    }
}