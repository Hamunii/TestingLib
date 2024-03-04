using System.Collections.Generic;
using static TestingLib.Attributes;

namespace TestingLib {
    /// <summary>
    /// Methods that can be helpful, but do nothing by themselves.
    /// </summary>
    [DevTools(Visibility.MenuOnly, Available.PlayerSpawn)]
    internal class Helper {
        [DevTools(Visibility.MenuOnly, Available.PlayerSpawn)]
        private static void ListEnemiesOnCurrentLevel(){
            ListAllEnemies(true);
        }
        /// <summary>
        /// Lists all enemies in current level.
        /// </summary>
        [DevTools(Visibility.Blacklist)]
        internal static void ListAllEnemies(bool nameOnly){
            #if DEBUG
            nameOnly = false;
            #endif
            Plugin.Logger.LogInfo("-- Inside Enemies ---");
            PrintListEnemy(RoundManager.Instance.currentLevel.Enemies, nameOnly);
            Plugin.Logger.LogInfo("-- Outside Enemies --");
            PrintListEnemy(RoundManager.Instance.currentLevel.OutsideEnemies, nameOnly);
            Plugin.Logger.LogInfo("-- Daytime Enemies --");
            PrintListEnemy(RoundManager.Instance.currentLevel.DaytimeEnemies, nameOnly);
        }
        private static void PrintListEnemy(List<SpawnableEnemyWithRarity> listOfEnemies, bool nameOnly){
            foreach (var enemy_ in listOfEnemies){
                var alteredName = enemy_.enemyType.enemyName.Replace(' ','_');
                alteredName = alteredName.Replace('-', '_');
                if(nameOnly)
                    Plugin.Logger.LogInfo(enemy_.enemyType.enemyName);
                else
                    Plugin.Logger.LogInfo($"public const string {alteredName} = \"{enemy_.enemyType.enemyName}\";");
            }
            Plugin.Logger.LogInfo("---------------------");
        }
        /// <summary>
        /// Lists all items.
        /// </summary>
        [DevTools(Visibility.Blacklist)]
        internal static void ListAllItems(bool nameOnly){
            #if DEBUG
            nameOnly = false;
            #endif
            Plugin.Logger.LogInfo("-- Items ------------");
            foreach (var item_ in StartOfRound.Instance.allItemsList.itemsList){
                var alteredName = item_.itemName.Replace(' ','_');
                alteredName = alteredName.Replace('-', '_');
                if(nameOnly)
                    Plugin.Logger.LogInfo(item_.itemName);
                else
                    Plugin.Logger.LogInfo($"public const string {alteredName} = \"{item_.itemName}\";");
            }
            Plugin.Logger.LogInfo("---------------------");
        }
    }
}