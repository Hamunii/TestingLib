using UnityEngine;
using System.Collections.Generic;
using GameNetcodeStuff;
using System.Collections;
using TestingLib.Attributes;
using TestingLib.Internal;

namespace TestingLib.Modules;
/// <summary>
/// Contains helpful methods for testing.
/// </summary>
[DevTools(Visibility.Whitelist, Available.PlayerSpawn)]
public static class Tools {
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

    [DevTools(Visibility.MenuOnly, Available.PlayerSpawn)]
    private static void TPSelfOutside(){
        TeleportSelf(TeleportLocation.Outside);
    }

    [DevTools(Visibility.MenuOnly, Available.PlayerSpawn)]
    private static void TPSelfInside(){
        TeleportSelf(TeleportLocation.Inside);
    }

    /// <summary>
    /// Teleports you to the location specified in the test level.
    /// <br/><br/>
    /// Valid values are: <c>TeleportLocation.Inside = 1</c>, <c>TeleportLocation.Outside = 2</c>
    /// </summary>
    /// <param name="location"></param>
    [DevTools(Visibility.ConfigOnly, Available.PlayerSpawn)]
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
    /// Teleport yourself to entrance.
    /// </summary>
    [DevTools(Visibility.MenuOnly, Available.PlayerSpawn)]
    public static void TeleportSelfToEntrance()
    {
        var self = StartOfRound.Instance.localPlayerController;
        int id = 0; // Main entrance
        var entrances = GameObject.FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None);
        foreach (var entrance in entrances)
        {
            if (entrance.entranceId != id)
                continue;
                
            // IF inside, set outside, or vice-versa.
            if (self.isInsideFactory != entrance.isEntranceToBuilding)
            {
                entrance.TeleportPlayer(); // Teleport self
                return;
            }
        }
    }

    /// <summary>
    /// Will find the enemy by name, and spawn it.<br/>
    /// If name is invalid, prints all valid enemy names to console.
    /// </summary>
    /// <param name="enemyName"></param>
    [DevTools(Visibility.ConfigOnly, Available.PlayerSpawn, Permission.HostOnly)]
    public static void SpawnEnemyInFrontOfSelf(string enemyName) {
        Plugin.Logger.LogInfo($"Tools: SpawnEnemyInFrontOfSelf");
        Vector3 spawnPosition = GameNetworkManager.Instance.localPlayerController.transform.position - Vector3.Scale(new Vector3(-5, 0, -5), GameNetworkManager.Instance.localPlayerController.transform.forward);
        // This might be bad code
        var allEnemiesList = new List<SpawnableEnemyWithRarity>();
        allEnemiesList.AddRange(RoundManager.Instance.currentLevel.Enemies);
        allEnemiesList.AddRange(RoundManager.Instance.currentLevel.OutsideEnemies);
        allEnemiesList.AddRange(RoundManager.Instance.currentLevel.DaytimeEnemies);
        var enemyToSpawn = allEnemiesList.Find(x => x.enemyType.enemyName.Equals(enemyName));
        if (enemyToSpawn == null){
            Helper.ListAllEnemies(nameOnly: true);
            return;
        }
        RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, enemyToSpawn.enemyType);
    }

    /// <summary>
    /// Will find item by name, and give it to your inventory.<br/>
    /// If name is invalid, prints all valid item names to console.
    /// </summary>
    [DevTools(Visibility.ConfigOnly, Available.PlayerSpawn, Permission.HostOnly)]
    public static void GiveItemToSelf(string itemName) {
        var itemToGive = StartOfRound.Instance.allItemsList.itemsList.Find(x => x.itemName.Equals(itemName));
        if (itemToGive == null){
            Helper.ListAllItems(nameOnly: true);
            return;
        }
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
    [DevTools(Visibility.Blacklist)]
    [System.Obsolete("Please use Patch.PatchAll() instead.")]
    public static void RunAllPatchAndExecuteMethods() {
        // Patch.IsEditor();
        // Patch.SkipSpawnPlayerAnimation();
        // Patch.OnDeathHeal();
        // Patch.MovementCheat();
        // Patch.InfiniteSprint();
        // Patch.InfiniteCredits();
        // Patch.InfiniteShotgunAmmo();
        // Execute methods
        // OnEvent.PlayerSpawn -= OnEvent_PlayerSpawn;
        // OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;
    }
    private static void OnEvent_PlayerSpawn() {
        Execute.ToggleTestRoom();
    }
}